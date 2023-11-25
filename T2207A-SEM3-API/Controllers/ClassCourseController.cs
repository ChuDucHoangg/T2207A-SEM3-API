using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.ClassCourse;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Service.CourseClass;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassCourseController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly IClassCourseService  _classCourseService;

        public ClassCourseController(ExamonimyContext context, IClassCourseService classCourseService)
        {
            _context = context;
            _classCourseService = classCourseService;
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> GetClassCourseAll()
        {
            try
            {
                List<ClassCourseDTO> courses = await _classCourseService.GetClassCourseListAsync();
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
                    Data = null
                };

                return BadRequest(response);
            }
        }

        [HttpGet("by-classId")]
        [Authorize]
        public async Task<IActionResult> GetCourseByClassId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralServiceResponse { Success = false, StatusCode = 401, Message = "Not Authorized", Data = "" });
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Students
                    .Include(u => u.Class)
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }

                List<ClassCourseResponse> courses = await _classCourseService.GetCourseByClassIdAsync(user.ClassId);
                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Success",
                    Data = courses
                });

            } catch (Exception ex)
            {
                var response = new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                };

                return BadRequest(response);
            }
        }

        //
        [HttpPost]
        [Authorize(Roles = "Super Admin, Staff")]
        public async Task<IActionResult> Create(CreateClassCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _classCourseService.CreateClassCourseAsync(model);
                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 201, // Sử dụng 201 Created
                        Message = "Course created successfully",
                        Data = created
                    };

                    return Created($"get-by-id?id={created.id}", response);
                }
                catch (Exception ex)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = ex.Message,
                        Data = null
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
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Delete(int id)
        {
            bool hasStudents = await _context.Students.AnyAsync(s => s.ClassId == id);
            if (hasStudents)
            {
                return BadRequest("The class cannot be deleted because this class currently has students");
            }
            try
            {

                bool deleted = await _classCourseService.DeleteClassAsync(id);

                if (deleted)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 204, // Sử dụng 204 No Content
                        Message = "Class deleted successfully",
                        Data = ""
                    };

                    return NoContent();
                }
                else
                {
                    var notFoundResponse = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "can not delete",
                        Data = ""
                    };

                    return NotFound(notFoundResponse);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Update(EditClassCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool updated = await _classCourseService.UpdateClassAsync(model);

                    if (updated)
                    {
                        var response = new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 204, // Sử dụng 204 No Content
                            Message = "Class updated successfully",
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
                            Message = "Class not found",
                            Data = ""
                        };

                        return NotFound(notFoundResponse);
                    }
                }
                catch (Exception e)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = e.Message,
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
    }
}
