using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        public DashboardController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet("user-stats")]
        public async Task<IActionResult> GetCountStudent()
        {
            /*int totalStudents = await _context.Students.CountAsync();

            return Ok(totalStudents);*/
            var userStats = new
            {
                TotalUsers = await _context.Students.CountAsync(),
                EnrolledStudents = await _context.Students.Where(s => s.Status == 0).CountAsync(),
                TotalTeachers = await _context.Staffs.CountAsync(u => u.Role == "Teacher"),
                TotalStaffs = await _context.Staffs.CountAsync(u => u.Role == "Staff"),
                // Add more stats as needed
            };

            return Ok(userStats);
        }

        [HttpGet("test-stats")]
        public async Task<IActionResult> GetExamStats()
        {
            var examStats = new
            {
                TotalTests = await _context.Tests.CountAsync(),
                ExamsInProgress = await _context.Tests.CountAsync(e => e.StartDate <= DateTime.Now && e.EndDate >= DateTime.Now),
                // Add more stats as needed
            };

            return Ok(examStats);
        }

        [HttpGet("registerExam-stats")]
        public async Task<IActionResult> GetRegisterExamStats()
        {
            try
            {
                var registerExamStats = new
                {
                    TotalRegistrations = await _context.RegisterExams.CountAsync(),
                    ApprovedRegistrations = await _context.RegisterExams.CountAsync(r => r.Status == 1),
                    PendingRegistrations = await _context.RegisterExams.CountAsync(r => r.Status == 0),
                    CompletedRegistration = await _context.RegisterExams.CountAsync(r => r.Status == 2)
                    // Add more stats as needed
                };

                return Ok(registerExamStats);
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("test-exam-stats")]
        public async Task<IActionResult> GetTestExamStats()
        {
            try
            {
                var examsByCourse = await _context.Exams
                    .Include(e => e.CourseClass)
                    .ThenInclude(e => e.Course)
                    .Include(e => e.Tests)
                    .ToListAsync();

                if (examsByCourse == null)
                {
                    return BadRequest("List of exams is null.");
                }
                // Tạo danh sách thống kê
                var examsStats = examsByCourse
                    .GroupBy(e => e.CourseClass.Course.Name)
                    .Select(group => new
                    {
                        CourseName = group.Key,
                        NumberOfExams = group.Sum(e => e.Tests.Count)
                    })
                    .ToList();

                return Ok(examsStats);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test-grade-stats")]
        public async Task<IActionResult> GetTestGradeStats()
        {
            try
            {
                var examScores = await _context.Grades
                    .Include(g => g.Test)
                    .ToListAsync();

                if (examScores == null)
                {
                    return BadRequest("List of exams is null.");
                }
                // Tạo danh sách thống kê
                var resultStats = examScores
                    .GroupBy(g => g.Test.Name)
                    .Select(group => new
                    {
                        TestName = group.Key,
                        AverageScore = group.Average(g => g.Score ?? 0)
                    })
                    .ToList();

                return Ok(resultStats);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("top-10-recent-tests")]
        public async Task<IActionResult> GetRecentTests()
        {
            var recentTests = await _context.Tests
                .OrderByDescending(t => t.StartDate) 
                .Take(10)
                .ToListAsync();

            List<TestDTO> data = new List<TestDTO>();
            foreach (Test t in recentTests)
            {
                data.Add(new TestDTO
                {
                    id = t.Id,
                    name = t.Name,
                    slug = t.Slug,
                    exam_id = t.ExamId,
                    startDate = t.StartDate,
                    endDate = t.EndDate,
                    past_marks = t.PastMarks,
                    total_marks = t.TotalMarks,
                    type_test = t.TypeTest,
                    RetakeTestId = t.RetakeTestId,
                    numberOfQuestion = t.NumberOfQuestionsInExam,
                    created_by = t.CreatedBy,
                    status = t.Status,
                    createdAt = t.CreatedAt,
                    updatedAt = t.UpdatedAt,
                    deletedAt = t.DeletedAt
                });
            }
            return Ok(data);
        }

        [HttpGet("top-10-high-average")]
        public async Task<ActionResult<IEnumerable<Test>>> GetTop10HighAverageTests()
        {
            var top10Tests = await _context.Tests
                .Where(t => t.Status == 1) // Chỉ lấy các bài thi đã được duyệt
                .OrderByDescending(t => t.Grades.Average(g => g.Score))
                .Take(10)
                .ToListAsync();

            List<TestDTO> data = new List<TestDTO>();
            foreach (Test t in top10Tests)
            {
                data.Add(new TestDTO
                {
                    id = t.Id,
                    name = t.Name,
                    slug = t.Slug,
                    exam_id = t.ExamId,
                    startDate = t.StartDate,
                    endDate = t.EndDate,
                    past_marks = t.PastMarks,
                    total_marks = t.TotalMarks,
                    type_test = t.TypeTest,
                    RetakeTestId = t.RetakeTestId,
                    numberOfQuestion = t.NumberOfQuestionsInExam,
                    created_by = t.CreatedBy,
                    status = t.Status,
                    createdAt = t.CreatedAt,
                    updatedAt = t.UpdatedAt,
                    deletedAt = t.DeletedAt
                });
            }
            return Ok(data);
        }
        [HttpGet("gender-distribution")]
        public async Task<ActionResult<IEnumerable<object>>> GetGenderDistribution()
        {
            var currentDate = DateTime.Now;
            var fiveYearsAgo = currentDate.AddYears(-5);

            var genderDistribution = await _context.Students
                .Where(s => s.CreatedAt >= fiveYearsAgo)
                .GroupBy(s => new { s.CreatedAt.Value.Year })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    GenderDistribution = group.GroupBy(g => g.Gender)
                                               .Select(genderGroup => new
                                               {
                                                   Gender = genderGroup.Key,
                                                   StudentCount = genderGroup.Count()
                                               })
                })
                .ToListAsync();

            return Ok(genderDistribution);
        }

        [HttpGet("latest-classes/{teacherId}")]
        public async Task<ActionResult<IEnumerable<ClassDTO>>> GetLatestClassesForTeacher(int teacherId)
        {
            var latestClasses = await _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .ToListAsync();

            List<ClassDTO> data = new List<ClassDTO>();

            foreach (Class c in latestClasses)
            {
                data.Add(new ClassDTO
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    room = c.Room,
                    teacher_id = c.TeacherId,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    deletedAt = c.DeletedAt
                });
            }

            return data;
        }

        [HttpGet("average-scores/{courseId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAverageScoresByCourse(int courseId)
        {
            // Lấy ngày 5 năm trước từ ngày hiện tại
            var fiveYearsAgo = DateTime.Now.AddYears(-5);

            // Truy vấn để lấy điểm trung bình theo năm và môn học cụ thể
            var averageScores = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Test)
                    .ThenInclude(t => t.Exam)
                        .ThenInclude(e => e.CourseClass)
                .Where(g => g.FinishedAt >= fiveYearsAgo && g.Test.Exam.CourseClass.CourseId == courseId &&
                    g.Test.Status == 1)
                .GroupBy(g => new { g.FinishedAt.Value.Year })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    AverageScore = group.Average(g => g.Score)
                })
                .ToListAsync();

            return Ok(averageScores);
        }

        [HttpGet("recent-tests-for-teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecentTestsForTeacher(int teacherId)
        {
            // Lấy ngày 2 tháng trước từ ngày hiện tại
            var oneMonthAgo = DateTime.Now.AddMonths(-2);

            // Truy vấn để lấy 10 bài test tự luận gần nhất của tất cả các lớp mà giáo viên đang dạy
            var recentEssayTests = await _context.Tests
                .Include(t => t.Exam)
                    .ThenInclude(e => e.CourseClass)
                        .ThenInclude(cc => cc.Class)
                .Where(t => t.CreatedBy == teacherId &&
                            t.TypeTest == 1 &&
                            t.CreatedAt >= oneMonthAgo)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .Select(t => new
                {
                    TestId = t.Id,
                    TestName = t.Name,
                    ClassId = t.Exam.CourseClass.Class.Id,
                    ClassName = t.Exam.CourseClass.Class.Name,
                    CreatedAt = t.CreatedAt,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .ToListAsync();

            return Ok(recentEssayTests);
        }
    }
}
