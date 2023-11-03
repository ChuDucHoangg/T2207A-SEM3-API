using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Service.Student
{
    public class StudentService : IStudentService
    {
        private readonly ExamonimyContext _context;

        public StudentService(ExamonimyContext context) 
        {
            _context = context;
        }

        public async Task<string> GenerateStudentCode()
        {
            // Lấy năm hiện tại dưới dạng chuỗi (ví dụ: "2022")
            string currentYear = DateTime.Now.ToString("yy");

            // Lấy tháng hiện tại dưới dạng chuỗi (ví dụ: "04" cho tháng 4)
            string currentMonth = DateTime.Now.ToString("MM");

            var codePrefix = $"TH{currentYear}{currentMonth:D2}";

            var lastStudent = await _context.Students
            .Where(s => s.StudentCode.StartsWith(codePrefix))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

            int newSequenceNumber;
            if (lastStudent != null)
            {
                var lastSequenceNumber = int.Parse(lastStudent.StudentCode.Substring(8));
                newSequenceNumber = lastSequenceNumber + 1;
            }
            else
            {
                newSequenceNumber = 1;
            }

            string studentCode = $"{codePrefix}{newSequenceNumber:D3}";

            return studentCode;
        }
    }
}
