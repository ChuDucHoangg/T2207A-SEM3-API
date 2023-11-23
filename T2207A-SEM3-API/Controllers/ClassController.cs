using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Class;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Service.Classes;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/classes")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly IClassService _classService;

        public ClassController(ExamonimyContext context, IClassService classService)
        {
            _context = context;
            _classService = classService;
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin, Staff")]


        public async Task<IActionResult> Index()
        {
            try
            {
                List<ClassDTO> classes = await _classService.GetAllClassAsync();
                return Ok(classes);
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

        [HttpGet]
        [Route("get-by-slug")]
        public async Task<IActionResult> Get(string slug)
        {
            try
            {
                ClassDTO classes = await _classService.GetClassBySlugAsync(slug);
                if (classes != null)
                {
                    return Ok(classes);
                }
                else
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not Found",
                        Data = ""
                    };

                    return NotFound(response);
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

        [HttpPost]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Create(CreateClass model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ClassDTO createdClasss = await _classService.CreateClassAsync(model);

                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 201, // Sử dụng 201 Created
                        Message = "Class created successfully",
                        Data = createdClasss
                    };

                    return Created($"get-by-id?id={createdClasss.id}", response);
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
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Update(EditClass model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool updated = await _classService.UpdateClassAsync(model);

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

                bool deleted = await _classService.DeleteClassAsync(id);

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
                        StatusCode = 404,
                        Message = "Class not found",
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

        [HttpGet]
        [Route("get-by-teacherId")]
        [Authorize(Roles = "Super Admin, Staff")]
        public async Task<IActionResult> GetbyClass(int teacherId)
        {
            try
            {
                List<Class> classes = await _context.Classes.Where(p => p.TeacherId == teacherId).ToListAsync();
                if (classes != null)
                {
                    List<ClassDTO> data = classes.Select(q => new ClassDTO
                    {
                        id = q.Id,
                        name = q.Name,
                        slug = q.Slug,
                        room = q.Room,
                        teacher_id = q.TeacherId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No course found in this class.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-teacher")]
        [Authorize(Roles = "Super Admin, Staff, Teacher")]
        public async Task<IActionResult> GetbyTeacherId()
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

                var user = await _context.Staffs
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
                List<Class> classes = await _context.Classes.Where(p => p.TeacherId == user.Id).OrderByDescending(s => s.Id).ToListAsync();
                if (classes != null)
                {
                    List<ClassDTO> data = classes.Select(q => new ClassDTO
                    {
                        id = q.Id,
                        name = q.Name,
                        slug = q.Slug,
                        room = q.Room,
                        teacher_id = q.TeacherId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No classes found in this Page.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(400, e.Message);
            }
        }
    }
}
