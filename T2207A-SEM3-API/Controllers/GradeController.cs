using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.ClassCourse;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Grade;
using T2207A_SEM3_API.Models.Staff;
using T2207A_SEM3_API.Models.Test;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/grade")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly ExamonimyContext _context;

        public GradeController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Grade> grades = await _context.Grades.ToListAsync();

                List<GradeDTO> data = new List<GradeDTO>();
                foreach (Grade g in grades)
                {
                    data.Add(new GradeDTO
                    {
                        id = g.Id,
                        student_id = g.StudentId,
                        test_id = g.TestId,
                        score = g.Score,
                        status = g.Status,
                        finishedAt = g.FinishedAt,
                        createdAt = g.CreatedAt,
                        updatedAt = g.UpdatedAt,
                        deletedAt = g.DeletedAt
                    });
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                Grade a = await _context.Grades.FirstOrDefaultAsync(x => x.Id == id);
                if (a != null)
                {
                    return Ok(new GradeDTO
                    {
                        id = a.Id,
                        student_id = a.StudentId,
                        test_id = a.TestId,
                        score = a.Score,
                        status = a.Status,
                        finishedAt = a.FinishedAt,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt

                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }
        [HttpGet]
        [Route("get-by-course/{studentId}")]
        public async Task<IActionResult> GetGradeByCourse(int studentId)
        {
            /*var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return NotFound(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "Not Authorized",
                    Data = ""
                });
            }*/

            try
            {
                /*var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Students
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }*/
                var student = _context.Students.FirstOrDefault(s => s.Id == studentId);

                if (student == null)
                {
                    return NotFound();
                }

                var classCourses = _context.ClassCourses.Include(p => p.Class).Include(p => p.Course)
                    .Where(cc => cc.Class.Students.Any(s => s.Id == studentId))
                    .ToList();

                var classCourseGrades = new List<ClassCourseGradeResponse>();

                foreach (var classCourse in classCourses)
                {
                    var grades = _context.Grades
                        .Include(g => g.Test)
                        .Where(g => g.StudentId == studentId)
                        .ToList();

                    double finalGrades = 0;
                    bool isPass = true;

                    foreach (var grade in grades)
                    {
                        if (grade.Status == 1)
                        {
                            finalGrades = (double)grade.Score;
                            isPass = true;
                        }
                        else
                        {
                            // Nếu học sinh thi lại, lấy điểm thi lại
                            finalGrades = 0;
                            isPass = false;
                        }
                    }

                    var classCourseGradeDTO = new ClassCourseGradeResponse
                    {
                        ClassCourseId = classCourse.Id,
                        ClassName = classCourse.Class.Name,
                        CourseName = classCourse.Course.Name,
                        FinalScore = finalGrades,
                        IsPass = isPass
                    };

                    classCourseGrades.Add(classCourseGradeDTO);
                }

                return Ok(classCourseGrades);

            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpGet]
        [Route("get-by-test")]
        [Authorize]
        public async Task<IActionResult> GetGradeByTest()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = ""
                });
            }

            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Students
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return Unauthorized(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }

                var studentTests = _context.StudentTests
                    .Include(st => st.Test)
                    .Where(st => st.StudentId == user.Id)
                    .ToList();

                var testGradeResponses = new List<TestGradeResponse>();

                foreach (var studentTest in studentTests)
                {
                    var grade = await _context.Grades.FirstOrDefaultAsync(g => g.TestId == studentTest.TestId && g.StudentId == user.Id);
                    if (grade.FinishedAt == null)
                    {
                        var testGradeResponse = new TestGradeResponse();
                        testGradeResponse.studentTestId = studentTest.TestId;
                        testGradeResponse.TestName = studentTest.Test.Name;
                        testGradeResponse.score = null;
                        testGradeResponse.IsPass = false;
                        testGradeResponse.finishAt = grade.FinishedAt;

                        testGradeResponses.Add(testGradeResponse);
                    }
                    else
                    {
                        var testGradeResponse = new TestGradeResponse();
                        testGradeResponse.studentTestId = studentTest.TestId;
                        testGradeResponse.TestName = studentTest.Test.Name;
                        testGradeResponse.score = grade.Score == null ? null : (double)grade.Score;
                        testGradeResponse.IsPass = grade.Status == 1 ? true : false;
                        testGradeResponse.finishAt = grade.FinishedAt;

                        testGradeResponses.Add(testGradeResponse);
                    }
                }

                return Ok(testGradeResponses);

            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Super Admin, Teacher")]
        public async Task<IActionResult> Update(EditGrade model)
        {
            if (ModelState.IsValid)
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (!identity.IsAuthenticated)
                {
                    return Unauthorized(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                try
                {

                    var userClaims = identity.Claims;
                    var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    var user = _context.Staffs.Find(Convert.ToInt32(userId));
                    if (user == null)
                    {
                        return Unauthorized(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 401,
                            Message = "Not Authorized",
                            Data = ""
                        });
                    }



                    Grade existingGrade = await _context.Grades.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);
                    if (existingGrade != null)
                    {
                        if (existingGrade.Score == null)
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "This test has not been graded yet",
                                Data = ""
                            });
                        }
                        else
                        {
                            var current_score = model.score;
                            var test = await _context.Tests.FirstOrDefaultAsync(t => t.Id == existingGrade.TestId);

                            if (test == null)
                            {
                                return NotFound(new GeneralServiceResponse
                                {
                                    Success = false,
                                    StatusCode = 404,
                                    Message = "Not Found",
                                    Data = ""
                                });
                            }

                            if (test.Status == 1)
                            {
                                return BadRequest(new GeneralServiceResponse
                                {
                                    Success = false,
                                    StatusCode = 400,
                                    Message = "The exam has been locked",
                                    Data = ""
                                });
                            }

                            if (test.TypeTest == 0)
                            {
                                return BadRequest(new GeneralServiceResponse
                                {
                                    Success = false,
                                    StatusCode = 400,
                                    Message = "Only essay test scores can be corrected",
                                    Data = ""
                                });
                            }

                            // phải đúng giáo viên dạy lớp đó thì mới chấm được 

                            var student = await _context.Students.FirstAsync(st => st.Id == existingGrade.StudentId);

                            if (student == null)
                            {
                                return NotFound(new GeneralServiceResponse
                                {
                                    Success = false,
                                    StatusCode = 404,
                                    Message = "Not Found",
                                    Data = ""
                                });
                            }

                            var classes = await _context.Classes.FirstAsync(cl => cl.Id == student.ClassId);

                            if (user.Id != classes.TeacherId)
                            {
                                return BadRequest(new GeneralServiceResponse
                                {
                                    Success = false,
                                    StatusCode = 400,
                                    Message = "You are not allowed to grade this article",
                                    Data = ""
                                });
                            }

                            if (user.Role != "Super Admin" && user.Id != classes.TeacherId)
                            {
                                return BadRequest(new GeneralServiceResponse
                                {
                                    Success = false,
                                    StatusCode = 400,
                                    Message = "You are not allowed to grade this article",
                                    Data = ""
                                });
                            }

                            if (!(existingGrade.FinishedAt >= test.StartDate && existingGrade.FinishedAt <= test.EndDate))
                            {
                                // finish_at không nằm trong khoảng startDate và andDate
                                TimeSpan timeDifference = (TimeSpan)((existingGrade.FinishedAt > test.EndDate) ? existingGrade.FinishedAt - test.EndDate : test.EndDate - existingGrade.FinishedAt);

                                if (timeDifference.TotalMinutes > 30)
                                {
                                    // Khoảng thời gian lớn hơn 30 phút
                                    current_score = 0;
                                }
                                else if (timeDifference.TotalMinutes > 15)
                                {
                                    // Khoảng thời gian lớn hơn 15 phút, nhưng không quá 30 phút
                                    current_score = current_score - 50;
                                }
                                else
                                {
                                    // Khoảng thời gian không lớn hơn 15 phút
                                    current_score = current_score - 25;
                                }
                            }
                            if (current_score < 0)
                            {
                                current_score = 0;
                            }

                            existingGrade.Score = current_score;
                            existingGrade.UpdatedAt = DateTime.Now;
                            // kiểm tra đỗ hay chưa
                            if (current_score >= test.PastMarks)
                            {
                                existingGrade.Status = 1;
                            }
                            else
                            {
                                existingGrade.Status = 0;
                            }
                        }

                        _context.Grades.Update(existingGrade);
                        await _context.SaveChangesAsync();
                        return Ok(new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = "Update score successfully",
                            Data = ""
                        });

                    }
                    else
                    {
                        return NotFound(); // Không tìm thấy lớp để cập nhật
                    }


                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Grade grade = await _context.Grades.FindAsync(id);
                if (grade == null)
                    return NotFound();
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("classcourse-scores/{classCourseId}/{student_id}")]
        public async Task<ActionResult<object>> GetClassCourseScores(int classCourseId, int student_id)
        {
            try
            {
                // Lấy thông tin về ClassCourse, kèm theo danh sách các Exam và Tests liên quan
                var classCourseInfo = await _context.ClassCourses
                    .Include(cc => cc.Exams)
                        .ThenInclude(e => e.Tests)
                        .Include(e => e.Class).Include(e => e.Course)
                    .FirstOrDefaultAsync(cc => cc.Id == classCourseId);

                if (classCourseInfo == null)
                {
                    return NotFound("ClassCourse not found");
                }

                // Tạo một đối tượng để chứa thông tin về điểm thi của ClassCourse
                var classCourseScores = new
                {
                    ClassCourseId = classCourseInfo.Id,
                    ClassName = classCourseInfo.Class.Name,
                    CourseName = classCourseInfo.Course.Name,
                    ExamScores = new List<object>() // Danh sách điểm của các bài kiểm tra
                };

                if (classCourseInfo.Class != null && classCourseInfo.Course != null)
                {
                    // Duyệt qua từng kỳ thi trong ClassCourse
                    foreach (var exam in classCourseInfo.Exams)
                    {
                        // Lấy điểm của bài thi essay (nếu có)
                        var TestScore = await _context.Grades
                            .Where(g => g.Test.ExamId == exam.Id && g.StudentId == student_id && g.Test.RetakeTestId == null && g.IsRetake == false) // Status = 1: Bài thi chính
                            .Select(g => new
                            {
                                Score = g.Score
                            })
                            .FirstOrDefaultAsync();

                        

                        // Lấy điểm của bài thi lại (nếu có) multipchoice
                        var retakeTestScore = await _context.Grades
                            .Where(g => g.Test.ExamId == exam.Id && g.StudentId == student_id && g.IsRetake == true) // Status = 2: Bài thi lại
                            .Select(g => new
                            {
                                Score = g.Score
                            })
                            .FirstOrDefaultAsync();

                        // Thêm thông tin về điểm vào danh sách
                        classCourseScores.ExamScores.Add(new
                        {
                            ExamName = exam.Name,
                            Score = TestScore,
                            RetakeScore = retakeTestScore
                        });
                    }

                    return Ok(classCourseScores);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
