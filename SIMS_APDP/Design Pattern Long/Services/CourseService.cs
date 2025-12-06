using SIMS_APDP.Data;
using SIMS_APDP.Models;
using Microsoft.EntityFrameworkCore;

namespace SIMS_APDP.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private const int MAX_STUDENTS_PER_COURSE = 30;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<dynamic> GetCourses(string search = "", int page = 1, int pageSize = 10, string sort = "name_asc")
        {
            var (_, items) = GetCoursesWithPagination(search, page, pageSize, sort);
            return items;
        }

        public (int Total, List<dynamic> Items) GetCoursesWithPagination(string search = "", int page = 1, int pageSize = 10, string sort = "name_asc")
        {
            var query = _context.Courses.Include(c => c.Teacher).AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                query = query.Where(c => c.CourseName.ToLower().Contains(searchTerm));
            }

            // Sort
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(c => c.CourseName),
                "credits_asc" => query.OrderBy(c => c.Credits),
                "credits_desc" => query.OrderByDescending(c => c.Credits),
                _ => query.OrderBy(c => c.CourseName)
            };

            var total = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(c => new
                {
                    id = c.CourseId,
                    code = c.CourseName,
                    name = c.CourseName,
                    credits = (int)c.Credits,
                    departmentName = c.Teacher != null ? c.Teacher.FullName : "N/A",
                    semester = "",
                    notes = c.Description ?? ""
                })
                .Cast<dynamic>()
                .ToList();

            return (total, items);
        }

        public Course GetCourseById(int courseId)
        {
            return _context.Courses.Find(courseId);
        }

        public void AddCourse(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
        }

        public void UpdateCourse(Course course)
        {
            _context.Courses.Update(course);
            _context.SaveChanges();
        }

        public void DeleteCourse(int courseId)
        {
            var course = _context.Courses.Find(courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
        }

        public bool CourseExists(int courseId)
        {
            return _context.Courses.Any(c => c.CourseId == courseId);
        }

        public IEnumerable<dynamic> GetDepartments()
        {
            return _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Teacher")
                .Select(u => new { id = u.UserId, name = u.FullName })
                .Cast<dynamic>()
                .ToList();
        }

        public IEnumerable<dynamic> GetCoursesSimple()
        {
            return _context.Courses
                .Select(c => new { id = c.CourseId, name = c.CourseName })
                .Cast<dynamic>()
                .ToList();
        }

        public int GetStudentCountInCourse(int courseId, string semester = "")
        {
            var query = _context.StudentCourses.Where(sc => sc.CourseId == courseId);
            if (!string.IsNullOrWhiteSpace(semester))
            {
                query = query.Where(sc => sc.Semester == semester);
            }
            return query.Count();
        }

        public (int Total, List<dynamic> Items) GetStudentsInCourse(int courseId, string semester = "", int page = 1, int pageSize = 10)
        {
            var query = _context.StudentCourses
                .Include(sc => sc.User)
                .Where(sc => sc.CourseId == courseId);

            if (!string.IsNullOrWhiteSpace(semester))
            {
                query = query.Where(sc => sc.Semester == semester);
            }

            var total = query.Count();

            var items = query
                .OrderBy(sc => sc.User.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(sc => new
                {
                    userId = sc.User.UserId,
                    studentName = sc.User.FullName,
                    username = sc.User.Username,
                    email = sc.User.Email,
                    enrolledDate = sc.EnrolledDate.ToString("dd/MM/yyyy"),
                    semester = sc.Semester
                })
                .Cast<dynamic>()
                .ToList();

            return (total, items);
        }

        public void EnrollStudentInCourse(int userId, int courseId, string semester)
        {
            if (IsStudentEnrolled(userId, courseId, semester))
            {
                throw new InvalidOperationException("Student is already enrolled in this course.");
            }

            if (!CanEnrollStudent(courseId, semester))
            {
                throw new InvalidOperationException("Course is full. Maximum 30 students allowed.");
            }

            var studentCourse = new StudentCourse
            {
                UserId = userId,
                CourseId = courseId,
                Semester = semester,
                EnrolledDate = DateTime.Now
            };

            _context.StudentCourses.Add(studentCourse);
            _context.SaveChanges();
        }

        public void RemoveStudentFromCourse(int userId, int courseId, string semester)
        {
            var studentCourse = _context.StudentCourses
                .FirstOrDefault(sc => sc.UserId == userId && sc.CourseId == courseId && sc.Semester == semester);

            if (studentCourse != null)
            {
                _context.StudentCourses.Remove(studentCourse);
                _context.SaveChanges();
            }
        }

        public bool IsStudentEnrolled(int userId, int courseId, string semester)
        {
            return _context.StudentCourses
                .Any(sc => sc.UserId == userId && sc.CourseId == courseId && sc.Semester == semester);
        }

        public bool CanEnrollStudent(int courseId, string semester)
        {
            var currentCount = GetStudentCountInCourse(courseId, semester);
            return currentCount < MAX_STUDENTS_PER_COURSE;
        }
    }
}
