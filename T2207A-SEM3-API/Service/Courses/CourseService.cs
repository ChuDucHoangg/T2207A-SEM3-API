using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Course;

namespace T2207A_SEM3_API.Service.Courses
{
    public class CourseService : ICourseService
    {
        private readonly ExamonimyContext _context;

        public CourseService(ExamonimyContext context)
        {
            _context = context;
        }
        public async Task<List<CourseDTO>> GetAllCoursesAsync()
        {
            List<Course> courses = await _context.Courses.OrderByDescending(s => s.Id).ToListAsync();
            List<CourseDTO> data = new List<CourseDTO>();

            foreach (Course cr in courses)
            {
                data.Add(new CourseDTO
                {
                    id = cr.Id,
                    name = cr.Name,
                    course_code = cr.CourseCode,
                    createdAt = cr.CreatedAt,
                    updateAt = cr.UpdatedAt,
                    deleteAt = cr.DeletedAt
                });
            }

            return data;
        }

        public async Task<CourseDTO> GetCourseByIdAsync(int id)
        {
            Course cr = await _context.Courses.FindAsync(id);
            if (cr != null)
            {
                return new CourseDTO
                {
                    id = cr.Id,
                    name = cr.Name,
                    course_code = cr.CourseCode,
                    createdAt = cr.CreatedAt,
                    updateAt = cr.UpdatedAt,
                    deleteAt = cr.DeletedAt
                };
            }

            return null;
        }

        public async Task<CourseDTO> CreateCourseAsync(CreateCourse model)
        {
            bool codeExists = await _context.Courses.AnyAsync(c => c.CourseCode == model.course_code);
            if (codeExists)
            {
                throw new Exception("Course code already exists");
            }

            Course data = new Course
            {
                Name = model.name,
                CourseCode = model.course_code,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = DateTime.Now,
            };

            _context.Courses.Add(data);
            await _context.SaveChangesAsync();

            return new CourseDTO
            {
                id = data.Id,
                course_code = data.CourseCode,
                name = data.Name,
                createdAt = data.CreatedAt,
                updateAt = data.UpdatedAt,
                deleteAt = data.DeletedAt,
            };
        }

        public async Task<bool> UpdateCourseAsync(EditCourse model)
        {
            Course existingCourse = await _context.Courses.FirstOrDefaultAsync(e => e.Id == model.id);
            if (existingCourse == null)
            {
                return false;
            }

            bool codeExists = await _context.Courses.AnyAsync(c => c.CourseCode == model.course_code && c.Id != model.id);
            if (codeExists)
            {
                throw new Exception("Course code already exists");
            }   

            existingCourse.Name = model.name;
            existingCourse.CourseCode = model.course_code;
            existingCourse.UpdatedAt = DateTime.Now;
            existingCourse.DeletedAt = null;

            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            Course course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
