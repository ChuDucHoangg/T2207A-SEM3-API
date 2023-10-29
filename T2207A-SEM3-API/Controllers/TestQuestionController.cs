using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.Question;
using T2207A_SEM3_API.Models.Test;
using T2207A_SEM3_API.Service.ClassCourses;
using T2207A_SEM3_API.Service.CourseClass;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestQuestionController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly ITestQuestionService _testQuestionService;

        public TestQuestionController(ExamonimyContext context, ITestQuestionService testQuestionService)
        {
            _context = context;
            _testQuestionService = testQuestionService;
        }

        [HttpGet("take-test/{testId}/details/{studentId}")]
        public async Task<IActionResult> GetTestQuestionsForTestDetail(int testId, int studentId)
        {
            // kiểm tra bài thi có tồn tại hay không    
            var test = await _testQuestionService.TestExists(testId);
            if (test == null)
            {
                return BadRequest("Test does not exist");
            }

            // kiểm tra đã làm bài
            var studentTest = await _testQuestionService.IsTestNotTaken(testId, studentId);
            if (studentTest == null)
            {
                return BadRequest("Test does not exist");
            }
            if (studentTest.Status != 0)
            {
                return BadRequest("The test has been taken before");
            }

            // Kiểm tra thời gian bài thi
            DateTime currentTime = DateTime.Now;
            if (currentTime < test.StartDate || currentTime > test.EndDate)
            {
                return BadRequest("The test has ended or has not started yet");
            }

            var quequestionAnswerResponses = await _testQuestionService.GetTestQuestionsForTestDetail(testId);

            return Ok(quequestionAnswerResponses);
        }

        [HttpGet("result-test/{testId}/details/{studentId}")]
        public async Task<IActionResult> GetTestQuestionsAndAnswerStudent(int testId, int studentId)
        {
            // kiểm tra bài thi có tồn tại hay không    
            var test = await _testQuestionService.TestExists(testId);
            if (test == null)
            {
                return BadRequest("Test does not exist");
            }

            // kiểm tra đã làm bài
            var studentTest = await _testQuestionService.IsTestNotTaken(testId, studentId);
            if (studentTest == null)
            {
                return BadRequest("Test does not exist");
            }
            if (studentTest.Status != 0)
            {
                return BadRequest("The test has been taken before");
            }

            // Lấy danh sách ID của các câu hỏi thuộc bài thi
            var questionIds = await _context.QuestionTests
                .Where(qt => qt.TestId == testId)
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
                    var answersForStudent = await _context.AnswersForStudents.Where(a => a.QuestionId == question.Id && a.StudentId == studentId).FirstOrDefaultAsync();
                    if (answersForStudent == null)
                    {
                        return BadRequest("The test has not been done yet");
                    }

                    var answerContentResponses = question.Answers.Select(answer => new AnswerContentResponse
                    {
                        id = answer.Id,
                        content = answer.Content
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
                var grade = await _context.Grades.Where(g => g.TestId == testId && g.StudentId == studentId).FirstOrDefaultAsync();
                if (grade == null)
                {
                    return BadRequest("The test has not been done yet");
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
                    var answersForStudent = await _context.AnswersForStudents.Where(a => a.QuestionId == question.Id && a.StudentId == studentId).FirstOrDefaultAsync();
                    if (answersForStudent == null)
                    {
                        return BadRequest("The test has not been done yet");
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
                var grade = await _context.Grades.Where(g => g.TestId == testId && g.StudentId == studentId).FirstOrDefaultAsync();
                if (grade == null)
                {
                    return BadRequest("The test has not been done yet");
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
        }

        [HttpGet("{testId}")]
        public async Task<IActionResult> GetTestQuestion(int testId)
        {
            try
            {
                // kiểm tra bài thi có tồn tại hay không    
                var test = await _testQuestionService.TestExists(testId);
                if (test == null)
                {
                    return BadRequest("Test does not exist");
                }

                var quequestionAnswerResponses = await _testQuestionService.GetTestQuestionsForTestDetail(testId);

                return Ok(quequestionAnswerResponses);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
