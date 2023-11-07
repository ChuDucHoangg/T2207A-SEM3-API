using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.Controllers;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.Question;
using static System.Net.Mime.MediaTypeNames;
using T2207A_SEM3_API.Models.Test;

namespace T2207A_SEM3_API.Service.ClassCourses
{
    public class TestQuestionService : ITestQuestionService
    {
        private readonly ExamonimyContext _context;

        public TestQuestionService(ExamonimyContext context)
        {
            _context = context;
        }
        public async Task<List<QuestionAnswerResponse>> GetTestQuestionsForTestDetail(int testId)
        {
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

            // Chuyển đổi dữ liệu câu hỏi và đáp án thành định dạng phản hồi
            var questionAnswerResponses = new List<QuestionAnswerResponse>();
            foreach (var question in questions)
            {
                var answerContentResponses = question.Answers.Select(answer => new AnswerContentResponse
                {
                    id = answer.Id,
                    content = answer.Content
                }).ToList();

                var questionAnswerResponse = new QuestionAnswerResponse
                {
                    id = question.Id,
                    title = question.Title,
                    Answers = answerContentResponses
                };

                questionAnswerResponses.Add(questionAnswerResponse);

            }
            return questionAnswerResponses;
        }

        public async Task<Test> TestExists(string test_slug)
        {
            return await _context.Tests.Include(t => t.QuestionTests).ThenInclude(t => t.Question).ThenInclude(q => q.Answers).SingleOrDefaultAsync(t => t.Slug == test_slug);
        }

        public async Task<StudentTest> IsTestNotTaken(int testId, int studentId)
        {
            return await _context.StudentTests.Where(st => st.TestId == testId && st.StudentId == studentId).FirstOrDefaultAsync();
        }

    }
}
