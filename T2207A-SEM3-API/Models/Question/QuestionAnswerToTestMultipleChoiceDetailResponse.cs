using T2207A_SEM3_API.Models.Answer;

namespace T2207A_SEM3_API.Models.Question
{
    public class QuestionAnswerToTestMultipleChoiceDetailResponse
    {
        public int id { get; set; }

        public string title { get; set; }

        public List<AnswerContentResultResponse> Answers { get; set; }

        public string answerForStudent { get; set; }
    }
}
