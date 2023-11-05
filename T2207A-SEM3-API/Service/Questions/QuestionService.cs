using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.Question;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Service.Questions
{
    public class QuestionService : IQuestionService
    {
        private readonly ExamonimyContext _context;

        public QuestionService(ExamonimyContext context)
        {
            _context = context;
        }

        public async Task<QuestionDTO> CreateQuestionEssay(CreateQuestionEssay model)
        {
            Question question = new Question
            {
                Title = model.title,
                Level = 3,
                QuestionType = 1,
                CourseId = model.course_id,
                Score = 100,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = null,
            };


            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return new QuestionDTO
            {
                id = question.Id,
                title = question.Title,
                level = question.Level,
                score = question.Score,
                createdAt = question.CreatedAt,
                updatedAt = question.UpdatedAt,
                deletedAt = question.DeletedAt
            };
        }

        public async Task<QuestionDTO> CreateQuestionMultipleChoice(CreateQuestionMultipleChoice model)
        {
            Question question = new Question
            {
                Title = model.title,
                Level = model.level,
                QuestionType = 0,
                CourseId = model.course_id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = null,
            };
            // Đặt điểm dựa trên mức độ
            SetQuestionScore(question);

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            foreach (var answerModel in model.answers)
            {
                var answer = new Answer
                {
                    Content = answerModel.content,
                    Status = answerModel.status,
                    QuestionId = question.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DeletedAt = null,
                };

                _context.Answers.Add(answer);
                await _context.SaveChangesAsync();
            }
            
            return new QuestionDTO
            {
                id = question.Id,
                title = question.Title,
                level = question.Level,
                score = question.Score,
                createdAt = question.CreatedAt,
                updatedAt = question.UpdatedAt,
                deletedAt = question.DeletedAt
            };
        }

        public async Task<List<QuestionAnswerResponse>> GetTestQuestionsAnswers(List<Question> questions)
        {

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

        // Phương thức để đặt điểm dựa trên mức độ
        private void SetQuestionScore(Question question)
        {
            switch (question.Level)
            {
                case 1:
                    question.Score = 3.85; // Điểm cho câu dễ
                    break;
                case 2:
                    question.Score = 6.41; // Điểm cho câu trung bình
                    break;
                case 3:
                    question.Score = 8.97; // Điểm cho câu khó
                    break;
                default:
                    question.Score = 0.0; // Điểm mặc định hoặc giá trị khác tùy bạn
                    break;
            }
        }
    }
}
