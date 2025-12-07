using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using System.Security.Claims;

namespace SIMS_APDP.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // 1. Danh sách l?p + Ch?m ?i?m sinh viên trong t?ng l?p
        public async Task<IActionResult> Classes()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.StudentCourses)
                    .ThenInclude(sc => sc.User)
                .ToListAsync();

            // Load t?t c? ?i?m c?a các sinh viên trong các l?p này
            var courseIds = courses.Select(c => c.CourseId).ToList();
            var grades = await _context.GradesProfiles
                .Where(g => courseIds.Contains(g.CourseId))
                .ToListAsync();

            // ??a d? li?u ?i?m vào ViewBag ho?c ViewData
            ViewBag.Grades = grades;

            return View(courses);
        }
        // 2. L?ch gi?ng d?y
        public async Task<IActionResult> Timetable()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Room)
                .Where(t => t.Course!.TeacherId == teacherId)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            return View(timetables);
        }

        // 3. Xem ph?n h?i
        public async Task<IActionResult> Feedback()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Course)
                .Where(f => f.Course != null && f.Course.TeacherId == teacherId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(feedbacks);
        }

        // POST: C?p nh?t ?i?m (dùng trong Classes.cshtml)
        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int userId, int courseId, string semester, decimal? grade)
        {
            var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Ki?m tra teacher có d?y môn này không
            var hasPermission = await _context.Courses
                .AnyAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);
            if (!hasPermission) return Forbid();

            var gp = await _context.GradesProfiles
                .FirstOrDefaultAsync(g => g.UserId == userId && g.CourseId == courseId && g.Semester == semester);

            if (gp == null)
            {
                gp = new GradesProfile
                {
                    UserId = userId,
                    CourseId = courseId,
                    Semester = semester,
                    Grade = grade,
                    UpdatedBy = teacherId,
                    UpdatedAt = DateTime.Now
                };
                _context.GradesProfiles.Add(gp);
            }
            else
            {
                gp.Grade = grade;
                gp.UpdatedBy = teacherId;
                gp.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Classes");
        }
    }
}