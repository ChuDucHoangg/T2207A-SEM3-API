﻿using T2207A_SEM3_API.Models.Question;

namespace T2207A_SEM3_API.Models.Test
{
    public class TakeTestEssayResponse
    {
        public string name { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public DateTime? finished_at { get; set; }

        public int NumberOfQuestionsInExam { get; set; }

        public int status { get; set; }

        public List<QuestionAnswerToTestEssayDetailResponse> questions { get; set; }

    }
}
