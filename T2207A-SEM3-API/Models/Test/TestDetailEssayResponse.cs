using T2207A_SEM3_API.Models.Question;

namespace T2207A_SEM3_API.Models.Test
{
    public class TestDetailEssayResponse
    {
        public string name { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public DateTime finished_at { get; set; }

        public int NumberOfQuestionsInExam { get; set; }

        public double past_marks { get; set; }

        public double total_marks { get; set; }

        public double? score { get; set; }

        public int status { get; set; }

        public List<QuestionAnswerToTestEssayDetailResponse> questions { get; set; }
    }
}
