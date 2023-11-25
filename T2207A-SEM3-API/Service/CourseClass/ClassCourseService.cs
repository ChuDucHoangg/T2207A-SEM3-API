﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> DeleteClassAsync(int id)
        {
            ClassCourse classes = await _context.ClassCourses.Include(c => c.Exams).FirstOrDefaultAsync(c => c.Id == id);
            if (classes == null || classes.Exams.Count() >=1)
            {
                return false;
            }

            _context.ClassCourses.Remove(classes);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ClassCourseDTO>> GetClassCourseListAsync()
        {
            List<ClassCourse> classCourses = await _context.ClassCourses.Include(c => c.Class).Include(c => c.Course).Include(c => c.CreatedByNavigation).OrderByDescending(s => s.Id).ToListAsync();
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

        public async Task<List<ClassCourseResponse>> GetCourseByClassIdAsync(int id)
        {

            List<ClassCourse> classCourses = await _context.ClassCourses.Include(c => c.Class).ThenInclude(c => c.Students).Include(c => c.Course).Where(c => c.ClassId == id).ToListAsync();

            List<ClassCourseResponse> data = new List<ClassCourseResponse>();

            foreach (ClassCourse cr in classCourses)
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(st => st.Id == cr.Class.TeacherId);
                if (staff == null)
                {
                    throw new Exception("Not Found");
                }
                data.Add(new ClassCourseResponse
                {
                    classCourseId = cr.Id,
                    className = cr.Class.Name,
                    courseName = cr.Course.Name,
                    createByName = staff.Fullname,
                    numberOfStudents = cr.Class.Students.Count,
                    startDate = cr.StartDate,
                    endDate = cr.EndDate,
                });
            }

            return data;
        }

        public async Task<bool> UpdateClassAsync(EditClassCourse model)
        {
            ClassCourse existingClass = await _context.ClassCourses.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.Id);

            if (existingClass != null)
            {
                ClassCourse classes = new ClassCourse
                {
                    Id = model.Id,
                    ClassId = existingClass.ClassId,
                    CourseId = existingClass.CourseId,
                    Status = existingClass.Status,
                    StartDate = model.start_date,
                    EndDate = model.end_date,
                    CreatedBy = existingClass.CreatedBy,
                    CreatedAt = existingClass.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null,
                };

                if (classes != null)
                {
                    _context.ClassCourses.Update(classes);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            else
            {
                return false; // Không tìm thấy lớp để cập nhật
            }
        }
    }
}
