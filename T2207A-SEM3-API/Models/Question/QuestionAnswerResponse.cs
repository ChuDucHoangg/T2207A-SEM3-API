using T2207A_SEM3_API.Models.Answer;

namespace T2207A_SEM3_API.Models.Question
{
    public class QuestionAnswerResponse
    {
        public int id { get; set; }

        public string title { get; set; }

        public List<AnswerContentResponse> Answers { get; set; }

    }
}
