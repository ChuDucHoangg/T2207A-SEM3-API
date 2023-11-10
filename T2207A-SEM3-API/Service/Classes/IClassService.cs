using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Models.Class;
using T2207A_SEM3_API.Models.Course;

namespace T2207A_SEM3_API.Service.Classes
{
    public interface IClassService
    {
        Task<List<ClassDTO>> GetAllClassAsync();
        Task<ClassDTO> GetClassBySlugAsync(string slug);
        Task<ClassDTO> CreateClassAsync(CreateClass model);
        Task<bool> UpdateClassAsync(EditClass model);
        Task<bool> DeleteClassAsync(int id);
    }
}
