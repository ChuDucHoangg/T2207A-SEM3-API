using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Student;
using T2207A_SEM3_API.Service.Courses;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : Controller
    {
        private readonly ExamonimyContext _context;

        private readonly ICourseService _courseService;

        public CourseController(ExamonimyContext context, ICourseService courseService)
        {
            _context = context;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<CourseDTO> courses = await _courseService.GetAllCoursesAsync();
                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Success",
                    Data = courses
                });
            }
            catch (Exception ex) 
            {
                var response = new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = ""
                };

                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("get-by-codeCourse")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                CourseDTO course = await _courseService.GetCourseByIdAsync(id);
                if (course != null)
                {
                    return Ok(new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Success",
                        Data = course
                    });
                }
            }
            catch (Exception ex)
            {
                var response = new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                };

                return BadRequest(response);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CourseDTO createdCourse = await _courseService.CreateCourseAsync(model);

                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 201, // Sử dụng 201 Created
                        Message = "Course created successfully",
                        Data = createdCourse
                    };

                    return Created($"get-by-id?id={createdCourse.id}", response);
                }
                catch (Exception ex)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    };

                    return BadRequest(response);
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Update(EditCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool updated = await _courseService.UpdateCourseAsync(model);

                    if (updated)
                    {
                        var response = new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 204, // Sử dụng 204 No Content
                            Message = "Course updated successfully",
                            Data = ""
                        };

                        return NoContent();
                    }
                    else
                    {
                        var notFoundResponse = new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Course not found",
                            Data = ""
                        };

                        return NotFound(notFoundResponse);
                    }
                }
                catch (Exception ex)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    };

                    return BadRequest(response);
                }
            }

            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await _courseService.DeleteCourseAsync(id);

                if (deleted)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 204, // Sử dụng 204 No Content
                        Message = "Course deleted successfully",
                        Data = ""
                    };

                    return NoContent();
                }
                else
                {
                    var notFoundResponse = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Course not found",
                        Data = ""
                    };

                    return NotFound(notFoundResponse);
                }
            }
            catch (Exception ex)
            {
                var response = new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                };

                return BadRequest(response);
            }
        }
    }
}

