﻿using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Models.Class;
using T2207A_SEM3_API.Models.ClassCourse;
using T2207A_SEM3_API.Models.Course;

namespace T2207A_SEM3_API.Service.CourseClass
{
    public interface IClassCourseService
    {
        Task<List<ClassCourseDTO>> GetClassCourseListAsync();

        Task<ClassCourseDTO> CreateClassCourseAsync(CreateClassCourse model);

        Task<List<ClassCourseResponse>> GetCourseByClassIdAsync(int id);

        Task<bool> UpdateClassAsync(EditClassCourse model);
        Task<bool> DeleteClassAsync(int id);
    }
}
