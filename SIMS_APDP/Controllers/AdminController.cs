using Microsoft.AspNetCore.Mvc;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using SIMS_APDP.Services;
using SIMS_APDP.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SIMS_APDP.Controllers
{
    /// <summary>
    /// Admin Controller - quản lý Users, Courses, Timetables, Student Enrollment
    /// 
    /// Design Patterns:
    /// - Dependency Injection: Services injected via constructor
    /// - Service Layer: Business logic in services, controller handles HTTP requests
    /// - Validator Pattern: Input validation separated into validator classes
    /// - DTO Pattern: Request/Response objects separate from domain models
    /// - Repository Pattern: Entity Framework for data access
    /// </summary>
    public class AdminController : Controller
    {
        private const int STUDENT_ROLE_ID = 3;
        private const int MAX_STUDENTS_PER_COURSE = 30;

        // DTOs for request/response handling
        public class CreateCourseRequest
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int Credits { get; set; }
            public string DepartmentId { get; set; }
            public string Semester { get; set; }
            public string Notes { get; set; }
        }

        public class EnrollStudentRequest
        {
            public int CourseId { get; set; }
            public int StudentId { get; set; }
            public string Semester { get; set; }
        }

        public class CreateTimetableRequest
        {
            public int CourseId { get; set; }
            public string DayOfWeek { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public int? RoomId { get; set; }
            public string Semester { get; set; }
        }

        public class UserUpdateRequest
        {
            public int UserId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Gender { get; set; }
            public string DateOfBirth { get; set; }
            public string Address { get; set; }
            public int RoleId { get; set; }
        }

        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ITimetableService _timetableService;
        private readonly ICourseValidator _courseValidator;
        private readonly ITimetableValidator _timetableValidator;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService,
            ICourseService courseService,
            ITimetableService timetableService,
            ICourseValidator courseValidator,
            ITimetableValidator timetableValidator,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _courseService = courseService;
            _timetableService = timetableService;
            _courseValidator = courseValidator;
            _timetableValidator = timetableValidator;
            _context = context;
            _logger = logger;
        }

        // ===== USER MANAGEMENT =====

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserManager()
        {
            var users = _userService.GetAllUsers();
            return View(users);
        }

        [HttpPost]
        public IActionResult AddUser(User user, string confirmPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                    return RedirectToAction("UserManager");

                if (user.Password != confirmPassword)
                {
                    ModelState.AddModelError("", "Password confirmation does not match!");
                    return RedirectToAction("UserManager");
                }

                if (_userService.UsernameExists(user.Username))
                {
                    ModelState.AddModelError("", "Username already exists!");
                    return RedirectToAction("UserManager");
                }

                if (_userService.EmailExists(user.Email))
                {
                    ModelState.AddModelError("", "Email already registered!");
                    return RedirectToAction("UserManager");
                }

                user.Password = _userService.HashPassword(user.Password);
                _userService.AddUser(user);

                return RedirectToAction("UserManager");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user");
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return RedirectToAction("UserManager");
            }
        }

        [HttpPost]
        public IActionResult UpdateUser([FromBody] UserUpdateRequest request = null)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { error = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(new { error = "Invalid data" });

                var existingUser = _userService.GetUserById(request.UserId);
                if (existingUser == null)
                    return NotFound(new { error = "User not found" });

                existingUser.FullName = request.FullName?.Trim();
                existingUser.Email = request.Email?.Trim();
                existingUser.PhoneNumber = request.PhoneNumber?.Trim();
                existingUser.Gender = request.Gender?.Trim();
                if (DateTime.TryParse(request.DateOfBirth, out var dateOfBirth))
                    existingUser.DateOfBirth = dateOfBirth;
                existingUser.Address = request.Address?.Trim();
                existingUser.RoleId = request.RoleId;

                _userService.UpdateUser(existingUser);
                return Json(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new { success = false, error = "An error occurred while updating user" });
            }
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                if (!_userService.UserExists(id))
                    return NotFound(new { error = "User not found" });

                _userService.DeleteUser(id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting user" });
            }
        }

        // ===== COURSE MANAGEMENT =====

        public IActionResult CourseManager()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCourses(string search = "", string dept = "", int page = 1, int pageSize = 10, string sort = "name_asc")
        {
            var (total, items) = _courseService.GetCoursesWithPagination(search, page, pageSize, sort);
            return Json(new { total, items });
        }

        [HttpGet]
        public IActionResult GetCourse(int id)
        {
            var course = _courseService.GetCourseById(id);
            if (course == null)
                return NotFound();

            return Json(new
            {
                id = course.CourseId,
                code = course.CourseName,
                name = course.CourseName,
                credits = course.Credits,
                departmentId = course.TeacherId,
                semester = "",
                notes = course.Description ?? ""
            });
        }

        [HttpPost]
        public IActionResult CreateCourse([FromBody] CreateCourseRequest request)
        {
            try
            {
                var (isValid, errorMessage) = _courseValidator.ValidateName(request?.Name);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                (isValid, errorMessage) = _courseValidator.ValidateCredits(request.Credits);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                var course = new Course
                {
                    CourseName = request.Name.Trim(),
                    Credits = (byte)request.Credits,
                    Description = request.Notes?.Trim(),
                    TeacherId = string.IsNullOrEmpty(request.DepartmentId) ? null : int.TryParse(request.DepartmentId, out var tid) ? tid : (int?)null
                };

                _courseService.AddCourse(course);
                return Ok(new { id = course.CourseId, message = "Course created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, new { error = "An error occurred while saving course" });
            }
        }

        [HttpPut]
        public IActionResult UpdateCourse(int id, [FromBody] CreateCourseRequest request)
        {
            try
            {
                var course = _courseService.GetCourseById(id);
                if (course == null)
                    return NotFound(new { error = "Course not found" });

                var (isValid, errorMessage) = _courseValidator.ValidateName(request?.Name);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                (isValid, errorMessage) = _courseValidator.ValidateCredits(request.Credits);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                course.CourseName = request.Name.Trim();
                course.Credits = (byte)request.Credits;
                course.Description = request.Notes?.Trim();
                course.TeacherId = string.IsNullOrEmpty(request.DepartmentId) ? null : int.TryParse(request.DepartmentId, out var tid) ? tid : (int?)null;

                _courseService.UpdateCourse(course);
                return Ok(new { message = "Course updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course {CourseId}", id);
                return StatusCode(500, new { error = "An error occurred while saving course" });
            }
        }

        [HttpDelete]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                if (!_courseService.CourseExists(id))
                    return NotFound(new { error = "Course not found" });

                _courseService.DeleteCourse(id);
                return Ok(new { message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting course" });
            }
        }

        [HttpGet]
        public IActionResult GetDepartments()
        {
            var teachers = _courseService.GetDepartments();
            return Json(teachers);
        }

        // ===== TIMETABLE MANAGEMENT =====

        public IActionResult TimeTable()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCoursesSimple()
        {
            var courses = _courseService.GetCoursesSimple();
            return Json(courses);
        }

        [HttpGet]
        public IActionResult GetRooms()
        {
            var rooms = _timetableService.GetRooms();
            return Json(rooms);
        }

        [HttpGet]
        public IActionResult GetTimetables(string course = "", string semester = "", string day = "", int page = 1, int pageSize = 10)
        {
            int? courseId = null;
            if (!string.IsNullOrWhiteSpace(course) && int.TryParse(course, out int parsedCourseId))
                courseId = parsedCourseId;

            var (total, items) = _timetableService.GetTimetables(courseId, semester, day, page, pageSize);
            return Json(new { total, items });
        }

        [HttpGet]
        public IActionResult GetTimetable(int id)
        {
            var timetable = _timetableService.GetTimetableById(id);
            if (timetable == null)
                return NotFound();

            return Json(timetable);
        }

        [HttpPost]
        public IActionResult CreateTimetable([FromBody] CreateTimetableRequest request)
        {
            try
            {
                var validationError = ValidateTimetableRequest(request, null);
                if (validationError != null)
                    return validationError;

                TimeSpan.TryParse(request.StartTime, out var startTime);
                TimeSpan.TryParse(request.EndTime, out var endTime);

                var timetable = new Timetable
                {
                    CourseId = request.CourseId,
                    DayOfWeek = request.DayOfWeek.Trim(),
                    StartTime = startTime,
                    EndTime = endTime,
                    RoomId = request.RoomId.HasValue && request.RoomId.Value > 0 ? request.RoomId : null,
                    Semester = request.Semester.Trim()
                };

                _timetableService.AddTimetable(timetable);
                return Ok(new { id = timetable.TimetableId, message = "Timetable created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timetable");
                return StatusCode(500, new { error = "An error occurred while saving timetable" });
            }
        }

        [HttpPut]
        public IActionResult UpdateTimetable(int id, [FromBody] CreateTimetableRequest request)
        {
            try
            {
                if (!_timetableService.TimetableExists(id))
                    return NotFound(new { error = "Timetable not found" });

                var validationError = ValidateTimetableRequest(request, id);
                if (validationError != null)
                    return validationError;

                TimeSpan.TryParse(request.StartTime, out var startTime);
                TimeSpan.TryParse(request.EndTime, out var endTime);

                var timetable = _context.Timetables.Find(id);

                timetable.CourseId = request.CourseId;
                timetable.DayOfWeek = request.DayOfWeek.Trim();
                timetable.StartTime = startTime;
                timetable.EndTime = endTime;
                timetable.RoomId = request.RoomId.HasValue && request.RoomId.Value > 0 ? request.RoomId : null;
                timetable.Semester = request.Semester.Trim();

                _timetableService.UpdateTimetable(timetable);
                return Ok(new { message = "Timetable updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating timetable {TimetableId}", id);
                return StatusCode(500, new { error = "An error occurred while saving timetable" });
            }
        }

        [HttpDelete]
        public IActionResult DeleteTimetable(int id)
        {
            try
            {
                if (!_timetableService.TimetableExists(id))
                    return NotFound(new { error = "Timetable not found" });

                _timetableService.DeleteTimetable(id);
                return Ok(new { message = "Timetable deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timetable {TimetableId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting timetable" });
            }
        }

        // ===== STUDENT ENROLLMENT =====

        public IActionResult StudentCourseManager()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetStudentsInCourse(int courseId, string semester = "", int page = 1, int pageSize = 10)
        {
            try
            {
                if (!_courseService.CourseExists(courseId))
                    return NotFound(new { error = "Course not found" });

                var (total, items) = _courseService.GetStudentsInCourse(courseId, semester, page, pageSize);
                var count = _courseService.GetStudentCountInCourse(courseId, semester);
                return Json(new { total, items, currentCount = count, maxCapacity = MAX_STUDENTS_PER_COURSE });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students in course {CourseId}", courseId);
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        [HttpGet]
        public IActionResult GetAvailableStudentsForCourse(int courseId, string semester = "", string search = "", int page = 1, int pageSize = 10)
        {
            try
            {
                if (!_courseService.CourseExists(courseId))
                    return NotFound(new { error = "Course not found" });

                var enrolledStudentIds = _context.StudentCourses
                    .Where(sc => sc.CourseId == courseId &&
                           (string.IsNullOrWhiteSpace(semester) || sc.Semester == semester))
                    .Select(sc => sc.UserId)
                    .ToList();

                var query = _context.Users
                    .Where(u => u.RoleId == STUDENT_ROLE_ID && !enrolledStudentIds.Contains(u.UserId));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchTerm = search.ToLower();
                    query = query.Where(u => u.FullName.ToLower().Contains(searchTerm) || u.Username.ToLower().Contains(searchTerm));
                }

                var total = query.Count();

                var items = query
                    .OrderBy(u => u.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new { id = u.UserId, name = u.FullName, username = u.Username, email = u.Email })
                    .ToList();

                return Json(new { total, items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available students for course {CourseId}", courseId);
                return StatusCode(500, new { error = "An error occurred" });
            }
        }

        [HttpPost]
        public IActionResult EnrollStudentInCourse([FromBody] EnrollStudentRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { error = "Invalid request" });

                if (!_courseService.CourseExists(request.CourseId))
                    return NotFound(new { error = "Course not found" });

                if (!_userService.UserExists(request.StudentId))
                    return NotFound(new { error = "Student not found" });

                if (!_courseService.CanEnrollStudent(request.CourseId, request.Semester ?? ""))
                    return BadRequest(new { error = $"Course is full. Maximum {MAX_STUDENTS_PER_COURSE} students allowed." });

                if (_courseService.IsStudentEnrolled(request.StudentId, request.CourseId, request.Semester ?? ""))
                    return BadRequest(new { error = "Student is already enrolled in this course." });

                _courseService.EnrollStudentInCourse(request.StudentId, request.CourseId, request.Semester ?? "");
                return Ok(new { message = "Student enrolled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling student {StudentId} in course {CourseId}", request?.StudentId, request?.CourseId);
                return StatusCode(500, new { error = "An error occurred while enrolling student" });
            }
        }

        [HttpDelete]
        public IActionResult RemoveStudentFromCourse([FromQuery] int courseId, [FromQuery] int studentId, [FromQuery] string semester = "")
        {
            try
            {
                if (!_courseService.CourseExists(courseId))
                    return NotFound(new { error = "Course not found" });

                _courseService.RemoveStudentFromCourse(studentId, courseId, semester);
                return Ok(new { message = "Student removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing student {StudentId} from course {CourseId}", studentId, courseId);
                return StatusCode(500, new { error = "An error occurred while removing student" });
            }
        }

        // ===== HELPER METHODS =====

        /// <summary>
        /// Validate all timetable request fields (reused by Create & Update)
        /// </summary>
        private IActionResult ValidateTimetableRequest(CreateTimetableRequest request, int? timetableId)
        {
            var (isValid, errorMessage) = _timetableValidator.ValidateCourseId(request?.CourseId ?? 0);
            if (!isValid) return BadRequest(new { error = errorMessage });

            (isValid, errorMessage) = _timetableValidator.ValidateDayOfWeek(request?.DayOfWeek);
            if (!isValid) return BadRequest(new { error = errorMessage });

            (isValid, errorMessage) = _timetableValidator.ValidateTime(request?.StartTime, request?.EndTime);
            if (!isValid) return BadRequest(new { error = errorMessage });

            (isValid, errorMessage) = _timetableValidator.ValidateSemester(request?.Semester);
            if (!isValid) return BadRequest(new { error = errorMessage });

            (isValid, errorMessage) = _timetableValidator.ValidateRoomId(request?.RoomId);
            if (!isValid) return BadRequest(new { error = errorMessage });

            TimeSpan.TryParse(request.StartTime, out var startTime);
            TimeSpan.TryParse(request.EndTime, out var endTime);

            (isValid, errorMessage) = _timetableValidator.ValidateConflict(request.RoomId, request.DayOfWeek, startTime, endTime, request.Semester, timetableId);
            if (!isValid) return BadRequest(new { error = errorMessage });

            return null;
        }
    }
}
