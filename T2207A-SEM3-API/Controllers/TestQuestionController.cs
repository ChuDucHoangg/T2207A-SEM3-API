using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Question;
using T2207A_SEM3_API.Models.Test;
using T2207A_SEM3_API.Service.ClassCourses;
using T2207A_SEM3_API.Service.CourseClass;
using T2207A_SEM3_API.Service.Students;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestQuestionController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly ITestQuestionService _testQuestionService;
        private readonly IStudentService _studentService;

        public TestQuestionController(ExamonimyContext context, ITestQuestionService testQuestionService, IStudentService studentService)
        {
            _context = context;
            _testQuestionService = testQuestionService;
            _studentService = studentService;
        }

        [HttpGet("multiple-choice/take-test/{test_slug}/details")]
        [Authorize]
        public async Task<IActionResult> GetTestQuestionsForMultipleChoiceTestDetail(string test_slug)
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

                // kiểm tra bài thi có tồn tại hay không    
                var test = await _testQuestionService.TestExists(test_slug);
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

                if(test.TypeTest == 1)
                {
                   
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Test does not exist",
                        Data = ""
                    });
                }

                // kiểm tra đã làm bài
                var studentTest = await _testQuestionService.IsTestNotTaken(test.Id, user.Id);
                if (studentTest == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Test does not exist",
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

                // Kiểm tra thời gian bài thi
                DateTime currentTime = DateTime.Now;
                if (currentTime < test.StartDate || currentTime > test.EndDate)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "The test has ended or has not started yet",
                        Data = ""
                    });
                }

                var quequestionAnswerResponses = await _testQuestionService.GetTestQuestionsForTestDetail(test.Id);

                var takeTest = new TakeTestMultipleChoiceResponse
                {
                    name = test.Name,
                    startDate = test.StartDate,
                    endDate = test.EndDate,
                    NumberOfQuestionsInExam = test.NumberOfQuestionsInExam,
                    status = test.Status,
                    questions = quequestionAnswerResponses
                };

                return Ok(takeTest);


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

        [HttpGet("essay/take-test/{test_slug}/details")]
        [Authorize]
        public async Task<IActionResult> GetTestQuestionsForEssayTestDetail(string test_slug)
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

                // kiểm tra bài thi có tồn tại hay không    
                var test = await _testQuestionService.TestExists(test_slug);
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

                if (test.TypeTest == 0)
                {
                    return NotFound(new GeneralServiceResponse { Success = false, StatusCode = 404, Message = "Test does not exist", Data = "" });
                }

                // kiểm tra đã làm bài
                var studentTest = await _testQuestionService.IsTestNotTaken(test.Id, user.Id);
                if (studentTest == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Test does not exist",
                        Data = ""
                    });
                }
                

                // Kiểm tra thời gian bài thi
                DateTime currentTime = DateTime.Now;
                if (currentTime < test.StartDate)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "The test has ended or has not started yet",
                        Data = ""
                    });
                }

                // Lấy danh sách ID của các câu hỏi thuộc bài thi
                var questionIds = await _context.QuestionTests
                    .Where(qt => qt.TestId == test.Id)
                    .OrderBy(qt => qt.Orders)
                    .Select(qt => new { qt.QuestionId, qt.Orders })
                    .ToListAsync();

                // Lấy danh sách câu hỏi dựa trên các ID câu hỏi
                var questions = new List<Question>();
                foreach (var item in questionIds)
                {
                    var question = await _context.Questions
                        .Where(q => q.Id == item.QuestionId)
                        .FirstOrDefaultAsync();

                    if (question != null)
                    {
                        questions.Add(question);
                    }
                }

                var questionAnswerResponses = new List<QuestionAnswerToTestEssayDetailResponse>();

                foreach (var question in questions)
                {
                    var answersForStudent = await _context.AnswersForStudents.FirstOrDefaultAsync(a => a.QuestionId == question.Id && a.StudentId == user.Id);
                    if (answersForStudent == null)
                    {
                        var questionAnswerResponse = new QuestionAnswerToTestEssayDetailResponse
                        {
                            id = question.Id,
                            title = question.Title,
                            answerForStudent = "",
                        };

                        questionAnswerResponses.Add(questionAnswerResponse);
                    }
                    else
                    {
                        var questionAnswerResponse = new QuestionAnswerToTestEssayDetailResponse
                        {
                            id = question.Id,
                            title = question.Title,
                            answerForStudent = answersForStudent.Content
                        };

                        questionAnswerResponses.Add(questionAnswerResponse);
                    }

                   

                }
                var testDetail = new TakeTestEssayResponse();

                testDetail.name = test.Name;
                testDetail.startDate = test.StartDate;
                testDetail.endDate = test.EndDate;
                testDetail.NumberOfQuestionsInExam = test.NumberOfQuestionsInExam;
                testDetail.questions = questionAnswerResponses;

                

                // Lấy ra câu trả lời của student
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.TestId == test.Id && g.StudentId == user.Id);
                if (grade == null)
                {
                    testDetail.finished_at = null;
                    testDetail.status = 0;
                }
                else
                {
                    testDetail.finished_at = grade.FinishedAt;
                    if (testDetail.finished_at > test.EndDate)
                    {
                        testDetail.status = 0;
                    }
                    else
                    {
                        testDetail.status = 1;
                    }
                }

                return Ok(testDetail);

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

        [HttpGet("result-test/{test_slug}/details/{student_code}")]
        public async Task<IActionResult> GetTestQuestionsAndAnswerStudent(string test_slug, string student_code)
        {
            try
            {
                // kiểm tra bài thi có tồn tại hay không    
                var test = await _testQuestionService.TestExists(test_slug);
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
                // kiểm tra học sinh có tồn tại hay không
                var student = await _studentService.StudentExists(student_code);
                if (student == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Student does not exist",
                        Data = ""
                    });
                }
                // kiểm tra đã làm bài
                var studentTest = await _testQuestionService.IsTestNotTaken(test.Id, student.id);
                if (studentTest == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Test does not exist",
                        Data = ""
                    });
                }
                if (studentTest.Status == 0)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "This test has not been done yet",
                        Data = ""
                    });
                }

                // Lấy danh sách ID của các câu hỏi thuộc bài thi
                var questionIds = await _context.QuestionTests
                    .Where(qt => qt.TestId == test.Id)
                    .OrderBy(qt => qt.Orders)
                    .Select(qt => new { qt.QuestionId, qt.Orders })
                    .ToListAsync();

                // Lấy danh sách câu hỏi dựa trên các ID câu hỏi
                var questions = new List<Question>();
                foreach (var item in questionIds)
                {
                    var question = await _context.Questions
                        .Where(q => q.Id == item.QuestionId)
                        .FirstOrDefaultAsync();

                    if (question != null)
                    {
                        questions.Add(question);
                    }
                }
                // trắc nghiệm
                if (questions[0].QuestionType == 0)
                {
                    // Chuyển đổi dữ liệu câu hỏi và đáp án thành định dạng phản hồi

                    var questionAnswerResponses = new List<QuestionAnswerToTestMultipleChoiceDetailResponse>();

                    foreach (var question in questions)
                    {
                        var answersForStudent = await _context.AnswersForStudents.Where(a => a.QuestionId == question.Id && a.StudentId == student.id).FirstOrDefaultAsync();
                        if (answersForStudent == null)
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "This test has not been done yet",
                                Data = ""
                            });
                        }

                        var answerContentResponses = question.Answers.Select(answer => new AnswerContentResultResponse
                        {
                            id = answer.Id,
                            content = answer.Content,
                            status = answer.Status

                        }).ToList();

                        var questionAnswerResponse = new QuestionAnswerToTestMultipleChoiceDetailResponse
                        {
                            id = question.Id,
                            title = question.Title,
                            Answers = answerContentResponses,
                            answerForStudent = answersForStudent.Content
                        };

                        questionAnswerResponses.Add(questionAnswerResponse);

                    }
                    // Lấy ra câu trả lời của student
                    var grade = await _context.Grades.FirstOrDefaultAsync(g => g.TestId == test.Id && g.StudentId == student.id);
                    if (grade == null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "This test has not been done yet",
                            Data = ""
                        });
                    }

                    var testDetail = new TestDetailMultipleChoiceResponse
                    {
                        name = test.Name,
                        startDate = test.StartDate,
                        endDate = test.EndDate,
                        finished_at = grade.FinishedAt,
                        NumberOfQuestionsInExam = test.NumberOfQuestionsInExam,
                        past_marks = test.PastMarks,
                        total_marks = test.TotalMarks,
                        status = grade.Status,
                        score = grade.Score,
                        questions = questionAnswerResponses,

                    };
                    return Ok(testDetail);
                }
                // tự luận
                else
                {
                    // Chuyển đổi dữ liệu câu hỏi và đáp án thành định dạng phản hồi

                    var questionAnswerResponses = new List<QuestionAnswerToTestEssayDetailResponse>();

                    foreach (var question in questions)
                    {
                        var answersForStudent = await _context.AnswersForStudents.Where(a => a.QuestionId == question.Id && a.StudentId == student.id).FirstOrDefaultAsync();
                        if (answersForStudent == null)
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "This test has not been done yet",
                                Data = ""
                            });
                        }

                        var questionAnswerResponse = new QuestionAnswerToTestEssayDetailResponse
                        {
                            id = question.Id,
                            title = question.Title,
                            answerForStudent = answersForStudent.Content
                        };

                        questionAnswerResponses.Add(questionAnswerResponse);

                    }
                    // Lấy ra câu trả lời của student
                    var grade = await _context.Grades.Where(g => g.TestId == test.Id && g.StudentId == student.id).FirstOrDefaultAsync();
                    if (grade == null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "This test has not been done yet",
                            Data = ""
                        });
                    }

                    var testDetail = new TestDetailEssayResponse
                    {
                        name = test.Name,
                        startDate = test.StartDate,
                        endDate = test.EndDate,
                        finished_at = grade.FinishedAt,
                        NumberOfQuestionsInExam = test.NumberOfQuestionsInExam,
                        past_marks = test.PastMarks,
                        total_marks = test.TotalMarks,
                        status = grade.Status,
                        score = grade.Score,
                        questions = questionAnswerResponses,

                    };
                    return Ok(testDetail);
                }
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

        [HttpGet("{test_slug}")]
        public async Task<IActionResult> GetTestQuestion(string test_slug)
        {
            try
            {
                // kiểm tra bài thi có tồn tại hay không    
                var test = await _testQuestionService.TestExists(test_slug);
                if (test == null)
                {
                    return BadRequest("Test does not exist");
                }

                var quequestionAnswerResponses = await _testQuestionService.GetTestQuestionsForTestDetail(test.Id);

                return Ok(quequestionAnswerResponses);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
