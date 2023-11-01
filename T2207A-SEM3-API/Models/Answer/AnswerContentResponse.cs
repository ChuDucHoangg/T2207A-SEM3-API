using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Answer
{
    public class AnswerContentResponse
    {
        public int id { get; set; }

        public string content { get; set; }

        public int status { get; set; }
    }
}
