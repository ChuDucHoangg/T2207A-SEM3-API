namespace T2207A_SEM3_API.Models.Test
{
    public class TestGradeResponse
    {
        public int studentTestId { get; set; }
        public string TestName { get; set; }
        public DateTime? finishAt { get; set; }
        public double? score { get; set; }
        public bool IsPass { get; set; }
    }
}
