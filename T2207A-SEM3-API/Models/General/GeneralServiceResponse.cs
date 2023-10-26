namespace T2207A_SEM3_API.Models.General
{
    public class GeneralServiceResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
