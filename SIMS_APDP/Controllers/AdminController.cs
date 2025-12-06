using Microsoft.AspNetCore.Mvc;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using SIMS_APDP.Services;
using SIMS_APDP.Validators;
using Microsoft.EntityFrameworkCore;

namespace SIMS_APDP.Controllers
{
    /// <summary>
    /// ===== DESIGN PATTERNS ???C S? D?NG =====
    /// 
    /// 1. DEPENDENCY INJECTION (DI) PATTERN
    ///    - L?p AdminController nh?n các dependencies (IUserService, ICourseService, v.v.) qua constructor
    ///    - L?i ích: D? test, loose coupling, d? maintain
    ///    - ???c c?u hình trong Program.cs
    /// 
    /// 2. SERVICE LAYER PATTERN
    ///    - T?t c? logic x? lý d? li?u ???c ?óng gói trong các Service (UserService, CourseService, TimetableService)
    ///    - Controller ch? dùng ?? nh?n request và g?i Service
    ///    - Giúp tách bi?t logic t? controller, d? tái s? d?ng
    /// 
    /// 3. VALIDATOR PATTERN
    ///    - S? d?ng ICourseValidator và ITimetableValidator ?? validate d? li?u
    ///    - Tách bi?t logic validation ra kh?i business logic
    ///    - D? reuse và maintain
    /// 
    /// 4. REPOSITORY PATTERN (qua Entity Framework)
    ///    - ApplicationDbContext (_context) ?óng vai trò repository
    ///    - Qu?n lý t?t c? truy c?p c? s? d? li?u
    /// 
    /// 5. DTO (Data Transfer Object) PATTERN
    ///    - S? d?ng CreateCourseRequest và CreateTimetableRequest
    ///    - Tách bi?t d? li?u client g?i lên t? Entity model
    ///    - Giúp validate và transform d? li?u an toàn
    /// </summary>
    public class AdminController : Controller
    {
        // ===== DEPENDENCY INJECTION =====
        // Các private readonly fields ???c inject qua constructor
        // readonly ??m b?o không th? thay ??i reference sau khi kh?i t?o
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ITimetableService _timetableService;
        private readonly ICourseValidator _courseValidator;
        private readonly ITimetableValidator _timetableValidator;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// CONSTRUCTOR INJECTION (Dependency Injection Pattern)
        /// T?t c? dependencies ???c nh?n qua constructor
        /// L?i ích:
        /// - D? ki?m tra và test (unit test)
        /// - Loose coupling - không ph? thu?c tr?c ti?p vào implementation
        /// - D? thay ??i implementation mà không c?n s?a controller
        /// </summary>
        public AdminController(
            IUserService userService,
            ICourseService courseService,
            ITimetableService timetableService,
            ICourseValidator courseValidator,
            ITimetableValidator timetableValidator,
            ApplicationDbContext context)
        {
            _userService = userService;
            _courseService = courseService;
            _timetableService = timetableService;
            _courseValidator = courseValidator;
            _timetableValidator = timetableValidator;
            _context = context;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/UserManager
        public IActionResult UserManager()
        {
            // ===== SERVICE LAYER PATTERN =====
            // G?i service ?? l?y d? li?u thay vì truy c?p DB tr?c ti?p
            var users = _userService.GetAllUsers();
            return View(users);
        }

        // POST: /Admin/AddUser
        /// <summary>
        /// ===== BUSINESS LOGIC FLOW =====
        /// 1. Validate ModelState (d? li?u t? client)
        /// 2. Validate d? li?u nghi?p v? (password match, unique username, email)
        /// 3. Hash password (b?o m?t)
        /// 4. G?i Service ?? l?u vào DB
        /// 5. Return response cho client
        /// </summary>
        [HttpPost]
        public IActionResult AddUser(User user, string confirmPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("UserManager");
                }

                if (user.Password != confirmPassword)
                {
                    ModelState.AddModelError("", "Password confirmation does not match!");
                    return RedirectToAction("UserManager");
                }

                // ===== SERVICE LAYER + BUSINESS LOGIC =====
                // Service ki?m tra d? li?u ??c l?p t? database
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
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return RedirectToAction("UserManager");
            }
        }

        // POST: /Admin/UpdateUser
        [HttpPost]
        public IActionResult UpdateUser([FromBody] UserUpdateRequest request = null)
        {
            try
            {
                // ===== FLEXIBLE REQUEST HANDLING PATTERN =====
                // X? lý c? JSON request (t? fetch) và form POST request
                
                User user = null;

                // N?u request là JSON body
                if (request != null)
                {
                    user = new User
                    {
                        UserId = request.UserId,
                        FullName = request.FullName,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        Gender = request.Gender,
                        DateOfBirth = DateTime.Parse(request.DateOfBirth),
                        Address = request.Address,
                        RoleId = request.RoleId
                    };
                }
                else
                {
                    // Fallback - không có request body
                    return BadRequest(new { error = "Invalid request data" });
                }

                if (!ModelState.IsValid && request == null)
                {
                    return RedirectToAction("UserManager");
                }

                var existingUser = _userService.GetUserById(user.UserId);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // ===== FETCHING & UPDATING PATTERN =====
                // 1. L?y entity hi?n có t? database
                // 2. C?p nh?t ch? nh?ng field c?n thi?t (không c?p nh?t m?i field)
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Gender = user.Gender;
                existingUser.DateOfBirth = user.DateOfBirth;
                existingUser.Address = user.Address;
                existingUser.RoleId = user.RoleId;

                // 3. L?u c?p nh?t thông qua Service
                _userService.UpdateUser(existingUser);
                
                // Return JSON response cho AJAX request
                return Json(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                // ===== NULL CHECK PATTERN =====
                // Ki?m tra resource có t?n t?i tr??c khi xóa
                if (!_userService.UserExists(id))
                {
                    return NotFound();
                }

                _userService.DeleteUser(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting", details = ex.Message });
            }
        }

        // GET: /Admin/CourseManager
        public IActionResult CourseManager()
        {
            return View();
        }

        // API: GET courses with search, filter, sort, and pagination
        /// <summary>
        /// ===== SEPARATION OF CONCERNS (SoC) PATTERN =====
        /// - Controller ch? nh?n request và tr? response
        /// - Logic filter, sort, pagination ???c x? lý trong Service
        /// - D? thay ??i logic mà không ?nh h??ng controller
        /// </summary>
        [HttpGet]
        public IActionResult GetCourses(string search = "", string dept = "", int page = 1, int pageSize = 10, string sort = "name_asc")
        {
            // ===== TUPLE PATTERN =====
            // Service tr? v? tuple (total, items) ?? cung c?p pagination info
            var (total, items) = _courseService.GetCoursesWithPagination(search, page, pageSize, sort);
            return Json(new { total, items });
        }

        // API: GET single course by id
        [HttpGet]
        public IActionResult GetCourse(int id)
        {
            var course = _courseService.GetCourseById(id);
            if (course == null)
                return NotFound();

            // ===== DTO (Data Transfer Object) PATTERN =====
            // Transform entity thành DTO tr??c khi tr? v? client
            // L?i ích:
            // - Client ch? th?y nh?ng field c?n thi?t
            // - B?o v? d? li?u nh?y c?m
            // - Có th? thay ??i entity mà không ?nh h??ng API
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

        // API: POST create new course
        /// <summary>
        /// ===== VALIDATION & ERROR HANDLING PATTERN =====
        /// 1. Validate t?ng field b?ng Validator
        /// 2. Return error ngay khi có v?n ?? (fail-fast)
        /// 3. T?o entity t? DTO
        /// 4. L?u qua Service
        /// 5. Return response
        /// </summary>
        [HttpPost]
        public IActionResult CreateCourse([FromBody] CreateCourseRequest request)
        {
            try
            {
                // ===== VALIDATOR PATTERN =====
                // Tách validation logic thành Validator class
                // Controller ch? g?i validate và ki?m tra k?t qu?
                var (isValid, errorMessage) = _courseValidator.ValidateName(request?.Name);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                (isValid, errorMessage) = _courseValidator.ValidateCredits(request.Credits);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                // ===== DATA TRANSFORMATION (DTO to Entity) =====
                // Chuy?n ??i DTO thành Entity model
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
                return StatusCode(500, new { error = "An error occurred while saving", details = ex.Message });
            }
        }

        // API: PUT update course
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
                return StatusCode(500, new { error = "An error occurred while saving", details = ex.Message });
            }
        }

        // API: DELETE course
        [HttpDelete]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                // ===== EXISTENCE CHECK PATTERN =====
                // Ki?m tra resource tr??c khi thao tác
                if (!_courseService.CourseExists(id))
                    return NotFound(new { error = "Course not found" });

                _courseService.DeleteCourse(id);
                return Ok(new { message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting", details = ex.Message });
            }
        }

        // API: GET all departments (teachers)
        [HttpGet]
        public IActionResult GetDepartments()
        {
            var teachers = _courseService.GetDepartments();
            return Json(teachers);
        }

        // GET: /Admin/TimeTable
        public IActionResult TimeTable()
        {
            return View();
        }

        // GET: /Admin/StudentCourseManager
        public IActionResult StudentCourseManager()
        {
            return View();
        }

        // API: GET simple courses list (for dropdown)
        [HttpGet]
        public IActionResult GetCoursesSimple()
        {
            var courses = _courseService.GetCoursesSimple();
            return Json(courses);
        }

        // API: GET rooms list
        [HttpGet]
        public IActionResult GetRooms()
        {
            var rooms = _timetableService.GetRooms();
            return Json(rooms);
        }

        // API: GET timetables with filter and pagination
        [HttpGet]
        public IActionResult GetTimetables(string course = "", string semester = "", string day = "", int page = 1, int pageSize = 10)
        {
            int? courseId = null;
            if (!string.IsNullOrWhiteSpace(course) && int.TryParse(course, out int parsedCourseId))
            {
                courseId = parsedCourseId;
            }

            var (total, items) = _timetableService.GetTimetables(courseId, semester, day, page, pageSize);
            return Json(new { total, items });
        }

        // API: GET single timetable
        [HttpGet]
        public IActionResult GetTimetable(int id)
        {
            var timetable = _timetableService.GetTimetableById(id);
            if (timetable == null)
                return NotFound();

            return Json(timetable);
        }

        // API: POST create timetable
        /// <summary>
        /// ===== MULTI-FIELD VALIDATION PATTERN =====
        /// - Validate nhi?u field liên quan ??n nhau
        /// - Validator x? lý validate logic ph?c t?p
        /// - Controller ch? g?i và check k?t qu?
        /// </summary>
        [HttpPost]
        public IActionResult CreateTimetable([FromBody] CreateTimetableRequest request)
        {
            try
            {
                // ===== CHAIN VALIDATION PATTERN =====
                // Validate l?n l??t, return ngay khi có l?i (fail-fast)
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
                return StatusCode(500, new { error = "An error occurred while saving", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // API: PUT update timetable
        [HttpPut]
        public IActionResult UpdateTimetable(int id, [FromBody] CreateTimetableRequest request)
        {
            try
            {
                if (!_timetableService.TimetableExists(id))
                    return NotFound(new { error = "Timetable not found" });

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

                var timetable = _context.Timetables.Find(id);
                TimeSpan.TryParse(request.StartTime, out var startTime);
                TimeSpan.TryParse(request.EndTime, out var endTime);

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
                return StatusCode(500, new { error = "An error occurred while saving", details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // API: DELETE timetable
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
                return StatusCode(500, new { error = "An error occurred while deleting", details = ex.Message });
            }
        }

        // API: GET students in a course
        [HttpGet]
        public IActionResult GetStudentsInCourse(int courseId, string semester = "", int page = 1, int pageSize = 10)
        {
            try
            {
                if (!_courseService.CourseExists(courseId))
                    return NotFound(new { error = "Course not found" });

                var (total, items) = _courseService.GetStudentsInCourse(courseId, semester, page, pageSize);
                var count = _courseService.GetStudentCountInCourse(courseId, semester);
                return Json(new { total, items, currentCount = count, maxCapacity = 30 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        // API: GET available students (not enrolled) for a course
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
                    .Include(u => u.Role)
                    .Where(u => u.Role != null && u.Role.RoleName == "Student" && !enrolledStudentIds.Contains(u.UserId));

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
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        // API: POST enroll student in course
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
                    return BadRequest(new { error = "Course is full. Maximum 30 students allowed." });

                if (_courseService.IsStudentEnrolled(request.StudentId, request.CourseId, request.Semester ?? ""))
                    return BadRequest(new { error = "Student is already enrolled in this course." });

                _courseService.EnrollStudentInCourse(request.StudentId, request.CourseId, request.Semester ?? "");
                return Ok(new { message = "Student enrolled successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        // API: DELETE student from course
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
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// ===== DATA TRANSFER OBJECT (DTO) PATTERN =====
        /// CreateCourseRequest là m?t DTO
        /// L?i ích:
        /// - Tách bi?t request model t? domain model (Course entity)
        /// - Có th? validate d? li?u ??c l?p tr??c khi t?o entity
        /// - B?o v? entity kh?i mass assignment vulnerability
        /// - D? thay ??i API contract mà không thay ??i entity
        /// </summary>
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

        /// <summary>
        /// ===== DATA TRANSFER OBJECT (DTO) PATTERN =====
        /// CreateTimetableRequest là m?t DTO
        /// T??ng t? CreateCourseRequest, tách bi?t request t? entity
        /// </summary>
        public class CreateTimetableRequest
        {
            public int CourseId { get; set; }
            public string DayOfWeek { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public int? RoomId { get; set; }
            public string Semester { get; set; }
        }

        /// <summary>
        /// ===== DATA TRANSFER OBJECT (DTO) PATTERN =====
        /// UserUpdateRequest là m?t DTO ?? nh?n data t? AJAX request
        /// ???c s? d?ng khi client g?i JSON body thay vì form POST
        /// </summary>
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
    }
}
