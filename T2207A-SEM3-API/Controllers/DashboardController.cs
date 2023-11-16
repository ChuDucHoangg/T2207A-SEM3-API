using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                TotalStudents = await _context.Staffs.CountAsync(u => u.Role == "Staff"),
                // Add more stats as needed
            };

            return Ok(userStats);
        }

        [HttpGet("test-stats")]
        public async Task<IActionResult> GetExamStats()
        {
            var examStats = new
            {
                TotalExams = await _context.Tests.CountAsync(),
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
    }

}
