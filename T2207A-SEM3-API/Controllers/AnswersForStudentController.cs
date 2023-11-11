using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.AnswerForStudent;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Test;
using T2207A_SEM3_API.Service.Tests;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/answersForStudent")]
    [ApiController]
    public class AnswersForStudentController : Controller
    {
        private readonly ExamonimyContext _context;
        private readonly ITestService _testService;

        public AnswersForStudentController(ExamonimyContext context, ITestService testService)
        {
            _context = context;
            _testService = testService;
        }

        [HttpPost("submit-exam/{test_slug}")]
        [Authorize]
        public async Task<IActionResult> SubmitAnswer(List<CreateAnswerForStudent> answersForStudents, string test_slug)
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

                var user = _context.Students.Find(Convert.ToInt32(userId));
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

                var test = await _testService.TestExists(test_slug);
                if (test == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Test does not exist",
                        Data = ""
                    });
                }

                // đã làm bài
                StudentTest studentTest = await _context.StudentTests.Where(st => st.TestId == test.Id && st.StudentId == user.Id).FirstOrDefaultAsync();
                if (studentTest == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "The test has not found",
                        Data = ""
                    });
                }
                if (studentTest.Status != 0)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "The test has been taken before",
                        Data = ""
                    });
                }
                

                var finish_at = DateTime.Now;

                // danh sách câu hỏi
                // Lấy danh sách ID của các câu hỏi thuộc bài thi
                var questionIds = await _context.QuestionTests
                    .Where(qt => qt.TestId == test.Id)
                    .OrderBy(qt => qt.Orders)
                    .Select(qt => qt.QuestionId)
                    .ToListAsync();

                // Lấy danh sách câu hỏi dựa trên các ID câu hỏi


                List<Question> questions = await _context.Questions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToListAsync();

                // lưu câu trả lời của student
                List<AnswersForStudent> answersForStudents1 = new List<AnswersForStudent>();

                foreach (var answer in answersForStudents)
                {
                    AnswersForStudent answersForStudent = new AnswersForStudent
                    {
                        StudentId = user.Id,
                        Content = answer.content,
                        QuestionId = answer.question_id,
                        TestId = test.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };

                    _context.AnswersForStudents.Add(answersForStudent);
                    await _context.SaveChangesAsync();

                    answersForStudents1.Add(answersForStudent);

                }

                // kiểm tra kiểu câu hỏi
                if (questions[0].QuestionType == 0)// nếu là trắc nghiệm
                {
                    studentTest.Status = 2;
                    await _context.SaveChangesAsync();


                    foreach (var question in questions)
                    {
                        question.Answers = _context.Answers
                            .Where(a => a.QuestionId == question.Id && a.Status == 1)
                            .ToList();
                    }

                    //  bạn có thể lấy danh sách câu trả lời đúng 
                    List<Answer> answerCorrect = questions
                        .SelectMany(q => q.Answers)
                        .Where(a => a.Status == 1)
                        .ToList();


                    // tính điểm
                    var score = CalculateScore(questions, answerCorrect, answersForStudents1);


                    // kiểm tra thời gian và trừ điểm
                    if (!(finish_at >= test.StartDate && finish_at <= test.EndDate))
                    {
                        // finish_at không nằm trong khoảng startDate và andDate
                        TimeSpan timeDifference = (finish_at > test.EndDate) ? finish_at - test.EndDate : test.EndDate - finish_at;

                        if (timeDifference.TotalMinutes > 30)
                        {
                            // Khoảng thời gian lớn hơn 30 phút
                            score = 0;
                        }
                        else if (timeDifference.TotalMinutes > 15)
                        {
                            // Khoảng thời gian lớn hơn 15 phút, nhưng không quá 30 phút
                            score = score - 50;
                        }
                        else
                        {
                            // Khoảng thời gian không lớn hơn 15 phút
                            score = score - 25;
                        }
                    }

                    if (score < 0)
                    {
                        score = 0;
                    }
                    // kiểm tra đã có điểm chưa

                    var gradeCurrent = await _context.Grades.FirstOrDefaultAsync(g => g.TestId == test.Id && g.StudentId == user.Id);
                    if (gradeCurrent == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "The test is not found",
                            Data = ""
                        });
                    }

                    // lưu điểm

                    gradeCurrent.Score = score;
                    gradeCurrent.FinishedAt = finish_at;
                    gradeCurrent.UpdatedAt = DateTime.Now;

                    // kiểm tra đỗ hay chưa
                    if (score >= test.PastMarks)
                    {
                        gradeCurrent.Status = 1;
                    }
                    else
                    {
                        gradeCurrent.Status = 0;
                    }

                    _context.Update(gradeCurrent);
                    await _context.SaveChangesAsync();
                    return Ok(new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Submitted successfully",
                        Data = gradeCurrent
                    });

                }
                // nếu quesion_type là tự luận
                else
                {
                    // để biết status là đã nộp những chưa được chấm
                    studentTest.Status = 1;
                    await _context.SaveChangesAsync();

                    // kiểm tra đã có điểm chưa

                    var gradeCurrent = await _context.Grades.FirstOrDefaultAsync(g => g.TestId == test.Id && g.StudentId == user.Id);
                    if (gradeCurrent == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "The test is not found",
                            Data = ""
                        });
                    }

                    // lưu điểm

                    gradeCurrent.FinishedAt = finish_at;
                    gradeCurrent.UpdatedAt = DateTime.Now;

                    _context.Update(gradeCurrent);
                    await _context.SaveChangesAsync();

                    return Ok(new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Submitted successfully",
                        Data = ""
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                });
            }

        }

        [HttpPost("scoring-essay")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ScoringEssay(CreateScore model)
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

                // đã làm bài
                StudentTest studentTest = await _context.StudentTests.Where(st => st.TestId == model.test_id && st.StudentId == model.student_id).FirstOrDefaultAsync();
                if (studentTest == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "The test has not found",
                        Data = ""
                    });
                }
                if (studentTest.Status == 0)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "This test is not complete",
                        Data = ""
                    });
                }
                if(studentTest.Status == 2)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "The test has been graded",
                        Data = ""
                    });
                }

                // kiểm tra test có tồn tại hay không
                var test = await _context.Tests.FindAsync(model.test_id);
                if(test == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Test is not found",
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

                if (user.Id != test.CreatedBy)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "You are not allowed to grade this article",
                        Data = ""
                    });
                }

                // kiểm tra đã nộp hay chưa
                var grade = await _context.Grades.Where(g => g.StudentId == model.student_id && g.TestId == model.test_id).FirstOrDefaultAsync();
                if (grade == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "This test is not complete",
                        Data = ""
                    });
                }

                if (grade.FinishedAt == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "This test is not complete",
                        Data = ""
                    });
                }

                // Kiểm tra xem điểm có nằm trong phạm vi hợp lệ (vd: từ 0 đến 100)
                if (model.score < 0 || model.score > 100)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid score range",
                        Data = ""
                    });
                }

                var current_score = model.score;

                if (grade.Score == 0)
                {
                    if (!(grade.FinishedAt >= test.StartDate && grade.FinishedAt <= test.EndDate))
                    {
                        // finish_at không nằm trong khoảng startDate và andDate
                        TimeSpan timeDifference = (TimeSpan)((grade.FinishedAt > test.EndDate) ? grade.FinishedAt - test.EndDate : test.EndDate - grade.FinishedAt);

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

                    // kiểm tra đỗ hay chưa
                    if (current_score >= test.PastMarks)
                    {
                        grade.Status = 1;
                    }
                    else
                    {
                        grade.Status = 0;
                    }

                    // Cập nhật điểm và lưu vào cơ sở dữ liệu
                    grade.Score = current_score;
                    grade.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Cập nhật trạng thái thi xong của học sinh
                    studentTest.Status = 2;
                    await _context.SaveChangesAsync();
                    return Ok(new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Scoring completed successfully",
                        Data = ""
                    });
                }

                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "The test has been graded",
                    Data = ""
                });

            } catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        private double CalculateScore(List<Question> questions, List<Answer> correctAnswers, List<AnswersForStudent> studentAnswers)
        {
            double totalScore = 0;

            foreach (var question in questions)
            {
                // Tìm câu trả lời đúng cho câu hỏi hiện tại
                var correctAnswer = correctAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                if (correctAnswer != null)
                {
                    // Tìm câu trả lời của sinh viên cho câu hỏi hiện tại
                    var studentAnswer = studentAnswers.FirstOrDefault(sa => sa.QuestionId == question.Id);

                    if (studentAnswer != null && studentAnswer.Content == correctAnswer.Content)
                    {
                        // Nếu câu trả lời của sinh viên giống với đáp án đúng, cộng thêm điểm
                        totalScore += question.Score;
                    }
                }
            }

            return totalScore;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<AnswersForStudent> answersForStudents = await _context.AnswersForStudents.ToListAsync();

                List<AnswerForStudentDTO> data = new List<AnswerForStudentDTO>();
                foreach (AnswersForStudent a in answersForStudents)
                {
                    data.Add(new AnswerForStudentDTO
                    {
                        id = a.Id,
                        student_id = a.StudentId,
                        content = a.Content,
                        question_id = a.QuestionId,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt
                    });
                }
                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Successfully",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        /*[HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                AnswersForStudent a = await _context.AnswersForStudents.FirstOrDefaultAsync(x => x.Id == id);
                if (a != null)
                {
                    return Ok(new AnswerForStudentDTO
                    {
                        id = a.Id,
                        student_id = a.StudentId,
                        content = a.Content,
                        question_id = a.QuestionId,
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateAnswerForStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AnswersForStudent data = new AnswersForStudent
                    {
                        StudentId = model.student_id,
                        Content = model.content,
                        QuestionId = model.question_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.AnswersForStudents.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new AnswerForStudentDTO
                    {
                        id = data.Id,
                        student_id = data.StudentId,
                        content = data.Content,
                        question_id = data.QuestionId,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            var msgs = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);
            return BadRequest(string.Join(" | ", msgs));
        }

        [HttpPut]
        public async Task<IActionResult> Update(EditAnswerForStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AnswersForStudent existingAnswer = await _context.AnswersForStudents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);
                    if (existingAnswer != null)
                    {
                        AnswersForStudent answer = new AnswersForStudent
                        {
                            Id = model.id,
                            StudentId = model.student_id,
                            Content = model.content,
                            QuestionId = model.question_id,
                            CreatedAt = existingAnswer.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (answer != null)
                        {
                            _context.AnswersForStudents.Update(answer);
                            await _context.SaveChangesAsync();
                            return NoContent();
                        }
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
                AnswersForStudent answer = await _context.AnswersForStudents.FindAsync(id);
                if (answer == null)
                    return NotFound();
                _context.AnswersForStudents.Remove(answer);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }*/

        [HttpGet]
        [Route("get-by-questionId")]
        public async Task<IActionResult> GetbyQuestion(int questionId)
        {
            try
            {
                List<AnswersForStudent> answers = await _context.AnswersForStudents.Where(p => p.QuestionId == questionId).ToListAsync();
                if (answers != null)
                {
                    List<AnswerForStudentDTO> data = answers.Select(q => new AnswerForStudentDTO
                    {
                        id = q.Id,
                        student_id = q.StudentId,
                        content = q.Content,
                        question_id = q.QuestionId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Successfully",
                        Data = data
                    });
                }
                else
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "No answer found in this question.",
                        Data = ""
                    });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = e.Message,
                    Data = ""
                });
            }
        }

        /*[HttpGet]
        [Route("get-by-testIdAndStudentId")]
        public async Task<IActionResult> GetbyTestAndStudent(int testID, int studentID)
        {
            try
            {
                List<AnswersForStudent> answers = await _context.AnswersForStudents.Where(p => p.StudentId == studentID && p.Question.TestId == testID).ToListAsync();
                if (answers != null)
                {
                    List<AnswerForStudentDTO> data = answers.Select(q => new AnswerForStudentDTO
                    {
                        id = q.Id,
                        student_id = q.StudentId,
                        content = q.Content,
                        question_id = q.QuestionId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No answer found in this Test.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }*/


    }
}
