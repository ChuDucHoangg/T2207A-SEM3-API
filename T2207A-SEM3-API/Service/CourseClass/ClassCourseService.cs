using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.ClassCourse;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Service.CourseClass
{
    public class ClassCourseService : IClassCourseService
    {
        private readonly ExamonimyContext _context;

        public ClassCourseService(ExamonimyContext context)
        {
            _context = context;
        }
        public async Task<ClassCourseDTO> CreateClassCourseAsync(CreateClassCourse model)
        {
            bool codeExists = await _context.ClassCourses.AnyAsync(c => c.ClassId == model.class_id && c.CourseId == model.course_id);
            if (codeExists)
            {
                throw new Exception("Course code already exists");
            }
            ClassCourse classCourse = new ClassCourse
            {
                ClassId = model.class_id,
                CourseId = model.course_id,
                Status = 0,
                StartDate = model.start_date,
                EndDate = model.end_date,
                CreatedBy = model.created_by,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = null,
            };

            _context.ClassCourses.Add(classCourse);
            await _context.SaveChangesAsync();

            return new ClassCourseDTO
            {
                id = classCourse.Id,
                class_id = classCourse.ClassId,
                course_id = classCourse.CourseId,
                startDate = classCourse.StartDate,
                endDate = classCourse.EndDate,
                createdBy = classCourse.CreatedBy,
                createdAt = classCourse.CreatedAt,
                updatedAt = classCourse.UpdatedAt,
                deletedAt = classCourse.DeletedAt,
            };
        }

        public async Task<List<ClassCourseDTO>> GetClassCourseListAsync()
        {
            List<ClassCourse> classCourses = await _context.ClassCourses.Include(c => c.Class).Include(c => c.Course).Include(c => c.CreatedByNavigation).ToListAsync();
            List<ClassCourseDTO> data = new List<ClassCourseDTO>();

            foreach (ClassCourse cr in classCourses)
            {
                data.Add(new ClassCourseDTO
                {
                    id = cr.Id,
                    className = cr.Class.Name,
                    courseName = cr.Course.Name,
                    createByName = cr.CreatedByNavigation.Fullname,
                    class_id = cr.ClassId,
                    startDate = cr.StartDate,
                    endDate = cr.EndDate,
                    course_id = cr.CourseId,
                    createdBy = cr.CreatedBy,
                    createdAt = cr.CreatedAt,
                    updatedAt = cr.UpdatedAt,
                    deletedAt = cr.DeletedAt
                });
            }

            return data;
        }

        public async Task<List<CourseDTO>> GetCourseByClassIdAsync(int id)
        {
            var courseIds = await _context.ClassCourses
                .Where(c => c.ClassId == id)
                .Select(cc => cc.CourseId)
                .ToListAsync();

            var courses = await _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .ToListAsync();

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
    }
}
