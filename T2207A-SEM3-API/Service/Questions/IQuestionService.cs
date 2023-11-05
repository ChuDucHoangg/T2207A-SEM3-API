using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Question;

namespace T2207A_SEM3_API.Service.Questions
{
    public interface IQuestionService
    {
        Task<QuestionDTO> CreateQuestionMultipleChoice(CreateQuestionMultipleChoice model);
        Task<QuestionDTO> CreateQuestionEssay(CreateQuestionEssay model);
        Task<List<QuestionAnswerResponse>> GetTestQuestionsAnswers(List<Question> questions);
    }
}
