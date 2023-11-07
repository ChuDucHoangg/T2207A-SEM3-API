using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;

namespace T2207A_SEM3_API.Service.Students
{
    public interface IStudentService
    {
        Task<string> GenerateStudentCode();
        Task<StudentDTO> StudentExists(string student_code);
    }
}
