using Microsoft.AspNetCore.Mvc;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using SIMS_APDP.Services;
using SIMS_APDP.Validators;
using Microsoft.EntityFrameworkCore;

namespace SIMS_APDP.Controllers
{
    /// <summary>
    /// ===== DESIGN PATTERNS ĐƯỢC SỬ DỤNG =====
    /// 
    /// 1. DEPENDENCY INJECTION (DI) PATTERN
    ///    - Lớp AdminController nhận các dependencies (IUserService, ICourseService, v.v.) qua constructor
    ///    - Lợi ích: Dễ test, loose coupling, dễ maintain
    ///    - Được cấu hình trong Program.cs
    /// 
    /// 2. SERVICE LAYER PATTERN
    ///    - Tất cả logic xử lý dữ liệu được đóng gói trong các Service (UserService, CourseService, TimetableService)
    ///    - Controller chỉ dùng để nhận request và gọi Service
    ///    - Giúp tách biệt logic từ controller, dễ tái sử dụng
    /// 
    /// 3. VALIDATOR PATTERN
    ///    - Sử dụng ICourseValidator và ITimetableValidator để validate dữ liệu
    ///    - Tách biệt logic validation ra khỏi business logic
    ///    - Dễ reuse và maintain
    /// 
    /// 4. REPOSITORY PATTERN (qua Entity Framework)
    ///    - ApplicationDbContext (_context) đóng vai trò repository
    ///    - Quản lý tất cả truy cập cơ sở dữ liệu
    /// 
    /// 5. DTO (Data Transfer Object) PATTERN
    ///    - Sử dụng CreateCourseRequest và CreateTimetableRequest
    ///    - Tách biệt dữ liệu client gửi lên từ Entity model
    ///    - Giúp validate và transform dữ liệu an toàn
    /// </summary>
    public class AdminController : Controller
    {
        // ===== DEPENDENCY INJECTION =====
        // Các private readonly fields được inject qua constructor
        // readonly đảm bảo không thể thay đổi reference sau khi khởi tạo
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ITimetableService _timetableService;
        private readonly ICourseValidator _courseValidator;
        private readonly ITimetableValidator _timetableValidator;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// CONSTRUCTOR INJECTION (Dependency Injection Pattern)
        /// Tất cả dependencies được nhận qua constructor
        /// Lợi ích:
        /// - Dễ kiểm tra và test (unit test)
        /// - Loose coupling - không phụ thuộc trực tiếp vào implementation
        /// - Dễ thay đổi implementation mà không cần sửa controller
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
            // Gọi service để lấy dữ liệu thay vì truy cập DB trực tiếp
            var users = _userService.GetAllUsers();
            return View(users);
        }

        // POST: /Admin/AddUser
        /// <summary>
        /// ===== BUSINESS LOGIC FLOW =====
        /// 1. Validate ModelState (dữ liệu từ client)
        /// 2. Validate dữ liệu nghiệp vụ (password match, unique username, email)
        /// 3. Hash password (bảo mật)
        /// 4. Gọi Service để lưu vào DB
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
                // Service kiểm tra dữ liệu độc lập từ database
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
                // Xử lý cả JSON request (từ fetch) và form POST request
                
                User user = null;

                // Nếu request là JSON body
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
                // 1. Lấy entity hiện có từ database
                // 2. Cập nhật chỉ những field cần thiết (không cập nhật mọi field)
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Gender = user.Gender;
                existingUser.DateOfBirth = user.DateOfBirth;
                existingUser.Address = user.Address;
                existingUser.RoleId = user.RoleId;

                // 3. Lưu cập nhật thông qua Service
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
                // Kiểm tra resource có tồn tại trước khi xóa
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
        /// - Controller chỉ nhận request và trả response
        /// - Logic filter, sort, pagination được xử lý trong Service
        /// - Dễ thay đổi logic mà không ảnh hưởng controller
        /// </summary>
        [HttpGet]
        public IActionResult GetCourses(string search = "", string dept = "", int page = 1, int pageSize = 10, string sort = "name_asc")
        {
            // ===== TUPLE PATTERN =====
            // Service trả về tuple (total, items) để cung cấp pagination info
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
            // Transform entity thành DTO trước khi trả về client
            // Lợi ích:
            // - Client chỉ thấy những field cần thiết
            // - Bảo vệ dữ liệu nhạy cảm
            // - Có thể thay đổi entity mà không ảnh hưởng API
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
        /// 1. Validate từng field bằng Validator
        /// 2. Return error ngay khi có vấn đề (fail-fast)
        /// 3. Tạo entity từ DTO
        /// 4. Lưu qua Service
        /// 5. Return response
        /// </summary>
        [HttpPost]
        public IActionResult CreateCourse([FromBody] CreateCourseRequest request)
        {
            try
            {
                // ===== VALIDATOR PATTERN =====
                // Tách validation logic thành Validator class
                // Controller chỉ gọi validate và kiểm tra kết quả
                var (isValid, errorMessage) = _courseValidator.ValidateName(request?.Name);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                (isValid, errorMessage) = _courseValidator.ValidateCredits(request.Credits);
                if (!isValid)
                    return BadRequest(new { error = errorMessage });

                // ===== DATA TRANSFORMATION (DTO to Entity) =====
                // Chuyển đổi DTO thành Entity model
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
                // Kiểm tra resource trước khi thao tác
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
        /// - Validate nhiều field liên quan đến nhau
        /// - Validator xử lý validate logic phức tạp
        /// - Controller chỉ gọi và check kết quả
        /// </summary>
        [HttpPost]
        public IActionResult CreateTimetable([FromBody] CreateTimetableRequest request)
        {
            try
            {
                // ===== CHAIN VALIDATION PATTERN =====
                // Validate lần lượt, return ngay khi có lỗi (fail-fast)
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

                (isValid, errorMessage) = _timetableValidator.ValidateConflict(request.RoomId, request.DayOfWeek, startTime, endTime, request.Semester, null);
                if (!isValid) return BadRequest(new { error = errorMessage });

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

                TimeSpan.TryParse(request.StartTime, out var startTime);
                TimeSpan.TryParse(request.EndTime, out var endTime);

                (isValid, errorMessage) = _timetableValidator.ValidateConflict(request.RoomId, request.DayOfWeek, startTime, endTime, request.Semester, id);
                if (!isValid) return BadRequest(new { error = errorMessage });

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
                    .Where(u => u.RoleId == 3 && !enrolledStudentIds.Contains(u.UserId));

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
                Console.WriteLine($"ENROLL ERROR: {ex}");
                if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException}");
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
        /// CreateCourseRequest là một DTO
        /// Lợi ích:
        /// - Tách biệt request model từ domain model (Course entity)
        /// - Có thể validate dữ liệu độc lập trước khi tạo entity
        /// - Bảo vệ entity khỏi mass assignment vulnerability
        /// - Dễ thay đổi API contract mà không thay đổi entity
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
        /// CreateTimetableRequest là một DTO
        /// Tương tự CreateCourseRequest, tách biệt request từ entity
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
        /// UserUpdateRequest là một DTO để nhận data từ AJAX request
        /// Được sử dụng khi client gửi JSON body thay vì form POST
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
