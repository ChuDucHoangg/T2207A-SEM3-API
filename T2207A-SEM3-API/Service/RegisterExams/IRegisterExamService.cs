using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.RegisterExam;

namespace T2207A_SEM3_API.Service.RegisterExams
{
    public interface IRegisterExamService
    {
        Task<List<RegisterExamDTO>> GetAllRegisterExamAsync();
        Task<RegisterExamDTO> CreateRegisterExamAsync(int student_id, int exam_id);
        Task<bool> DeleteRegisterExamAsync(int id, int student_id);
        Task<bool> ApproveRegisterExamAsync(int id);
    }
}
