using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Service.Students
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

        public async Task<StudentDTO> StudentExists(string student_code)
        {

            Student st = await _context.Students.AsNoTracking().FirstOrDefaultAsync(e => e.StudentCode == student_code);
            if (st != null)
            {
                var studentDTO = new StudentDTO
                {
                    id = st.Id,
                    student_code = st.StudentCode,
                    fullname = st.Fullname,
                    avatar = st.Avatar,
                    birthday = st.Birthday,
                    email = st.Email,
                    phone = st.Phone,
                    gender = st.Gender,
                    address = st.Address,
                    class_id = st.ClassId,
                    status = st.Status,
                    createdAt = st.CreatedAt,
                    updateAt = st.UpdatedAt,
                    deleteAt = st.DeletedAt

                };
                return studentDTO;
            }
            else
            {
                return null;
            }
        }
    }
}
