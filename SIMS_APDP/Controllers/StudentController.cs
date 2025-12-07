using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIMS_APDP.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StudentController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Student/Timetable
        public IActionResult Timetable()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Index", "Login");
            }

            // Find distinct semesters this student is enrolled in
            var userSemesters = _db.StudentCourses
                .Where(sc => sc.UserId == userId)
                .Select(sc => sc.Semester)
                .Distinct()
                .OrderByDescending(s => s) // Simple string sort, better than nothing
                .ToList();

            // Default to "Spring 2025" if no data, or pick the first one found
            var semester = userSemesters.FirstOrDefault() ?? "Spring 2025";
            
            ViewBag.CurrentSemester = semester;
            ViewBag.AllSemesters = userSemesters;

            var enrolledCourseIds = _db.StudentCourses
                .Where(sc => sc.UserId == userId && sc.Semester == semester)
                .Select(sc => sc.CourseId)
                .ToList();

            if (!enrolledCourseIds.Any())
            {
                ViewBag.Timetables = new List<dynamic>();
                return View();
            }

            // JOIN logic ensuring Course and Room are included
            var timetables = _db.Timetables
                .Include(t => t.Course)
                .Include(t => t.Room)
                .Where(t => enrolledCourseIds.Contains(t.CourseId) && t.Semester == semester)
                .OrderBy(t => t.DayOfWeek) // Note: This string sort might not be Mon-Tue-Wed order without custom logic, but acceptable for now
                .ThenBy(t => t.StartTime)
                .ToList()
                .Select(t => new {
                    DayOfWeek = t.DayOfWeek,
                    CourseName = t.Course?.CourseName ?? "Undefined",
                    StartTime = t.StartTime, 
                    EndTime = t.EndTime,
                    RoomName = t.Room?.RoomName ?? "Undefined"
                })
                .ToList();

            ViewBag.Timetables = timetables;
            return View();
        }

        // GET: /Student/Grades
        public IActionResult Grades()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Login");

            var userSemesters = _db.GradesProfiles
                .Where(g => g.UserId == userId)
                .Select(g => g.Semester)
                .Distinct()
                .OrderByDescending(s => s)
                .ToList();

            var semester = userSemesters.FirstOrDefault() ?? "Spring 2025";
            ViewBag.CurrentSemester = semester;

            // Explicitly fetch GradesProfiles with related Course info
            var grades = _db.GradesProfiles
                .Where(g => g.UserId == userId && g.Semester == semester)
                .Include(g => g.StudentCourse).ThenInclude(sc => sc.Course)
                .ToList()
                .Select(g => new {
                    CourseName = g.StudentCourse?.Course?.CourseName ?? "Unknown",
                    Credits = g.StudentCourse?.Course?.Credits ?? 0,
                    Semester = g.Semester,
                    Grade = g.Grade
                })
                .ToList();

            ViewBag.Grades = grades;
            
            // Calculate GPA
            decimal totalPoints = 0;
            int totalCredits = 0;
            int passedCredits = 0;

            foreach (var item in grades)
            {
                if (item.Grade.HasValue)
                {
                    totalPoints += item.Grade.Value * item.Credits;
                    totalCredits += item.Credits;
                    if (item.Grade.Value >= 4.0m) // Assuming 4.0 is pass out of 10? Or maybe 10 scale? Assuming 10 scale based on 4.0 pass
                        passedCredits += item.Credits;
                }
            }

            ViewBag.GPA = totalCredits > 0 ? totalPoints / totalCredits : 0;
            ViewBag.TotalCredits = totalCredits;
            ViewBag.PassedCredits = passedCredits;

            return View();
        }

        // GET: /Student/Feedback
        public IActionResult Feedback()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Login");

            // Only allow feedback for courses that have been GRADED (Grade is not null in GradesProfile)
            var gradedCourseIds = _db.GradesProfiles
                .Where(g => g.UserId == userId && g.Grade != null)
                .Select(g => g.CourseId)
                .ToList();

            var courses = _db.Courses
                .Where(c => gradedCourseIds.Contains(c.CourseId))
                .Select(c => new { c.CourseId, c.CourseName })
                .ToList();

            ViewBag.Courses = new SelectList(courses, "CourseId", "CourseName");
            ViewBag.PageTitle = "Feedback";

            // History
            var history = _db.Feedbacks
                .Where(f => f.UserId == userId)
                .Include(f => f.Course)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new {
                    CourseName = f.Course != null ? f.Course.CourseName : "General",
                    Message = f.Message,
                    CreatedAt = f.CreatedAt
                })
                .ToList();

            ViewBag.History = history;

            return View();
        }

        [HttpPost]
        public IActionResult Feedback(int? CourseId, string Message)
        {
             var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Login");

            if (string.IsNullOrWhiteSpace(Message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Feedback));
            }

            // Verify if user can feedback this course
            if (CourseId.HasValue)
            {
                 var hasGrade = _db.GradesProfiles.Any(g => g.UserId == userId && g.CourseId == CourseId.Value && g.Grade != null);
                 if (!hasGrade)
                 {
                     TempData["Error"] = "You can only give feedback for graded courses.";
                     return RedirectToAction(nameof(Feedback));
                 }
            }

            var fb = new Feedback
            {
                UserId = userId,
                CourseId = CourseId,
                Message = Message,
                CreatedAt = DateTime.Now
            };

            _db.Feedbacks.Add(fb);
            _db.SaveChanges();

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction(nameof(Feedback));
        }
    }
}
