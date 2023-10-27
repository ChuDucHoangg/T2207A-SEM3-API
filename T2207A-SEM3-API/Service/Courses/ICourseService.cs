using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Models.Course;

namespace T2207A_SEM3_API.Service.Courses
{
    public interface ICourseService
    {
        Task<List<CourseDTO>> GetAllCoursesAsync();
        Task<CourseDTO> GetCourseByIdAsync(int id);
        Task<CourseDTO> CreateCourseAsync(CreateCourse model);
        Task<bool> UpdateCourseAsync(EditCourse model);
        Task<bool> DeleteCourseAsync(int id);
    }
}
