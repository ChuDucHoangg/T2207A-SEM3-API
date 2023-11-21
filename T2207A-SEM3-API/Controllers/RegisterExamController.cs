using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.RegisterExam;
using T2207A_SEM3_API.Service.RegisterExams;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterExamController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly IRegisterExamService _registerExamService;

        public RegisterExamController(ExamonimyContext context, IRegisterExamService registerExamService)
        {
            _context = context;
            _registerExamService = registerExamService;
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> GetAll()
        {
            try
            {
                List<RegisterExamDTO> registerExams = await _registerExamService.GetAllRegisterExamAsync();
                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Success",
                    Data = registerExams
                });
            } catch (Exception ex)
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

        [HttpGet("{exam_id}")]
        public async Task<IActionResult> GetByExamId(int exam_id)
        {
            try
            {
                List<RegisterExamDTO> registerExams = await _registerExamService.GetRegisterByExamIdAsync(exam_id);
                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Success",
                    Data = registerExams
                });
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

        [HttpPost("register-exam")]
        [Authorize]
        public async Task<IActionResult> RegisterExam(RegisterExamRequest model)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity?.IsAuthenticated ?? false)
            {
                return Unauthorized("Not Authorized");
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Students.FindAsync(Convert.ToInt32(userId));
                if (user == null)
                {
                    return Unauthorized("Not Authorized");
                }

                // kiểm tra đã có testMain chưa, nếu có thì mới cho đi tiếp
                var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null);
                if (testExists == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Not Found",
                        Data = ""
                    });
                }

                // kiểm tra đã có điểm testMain chưa
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.TestId == testExists.Id && g.StudentId == user.Id);
                if (grade == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Not Found",
                        Data = ""
                    });
                }

                // kiểm tra phải làm testMain rồi thì mới được đăng kí thi lại
                if (grade.FinishedAt == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Not Found",
                        Data = ""
                    });
                }

                RegisterExamDTO registerExam = await _registerExamService.CreateRegisterExamAsync(user.Id, model.exam_id);

                var response = new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 201, // Sử dụng 201 Created
                    Message = "Successfully registered for the retake exam",
                    Data = registerExam
                };

                return Created($"get-by-id?id={registerExam.id}", response);

            } catch (Exception ex)
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

        [HttpPut("approve-register")]
        [Authorize(Roles = "Super Admin, Staff")]
        public async Task<IActionResult> ApproveRegisterExam(ApproveRegisterExamRequest model)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity?.IsAuthenticated ?? false)
            {
                return Unauthorized("Not Authorized");
            }
            try
            {

                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Staffs.FindAsync(Convert.ToInt32(userId));
                if (user == null)
                {
                    return Unauthorized("Not Authorized");
                }

                bool approved = await _registerExamService.ApproveRegisterExamAsync(model.id);

                if (approved)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 204, // Sử dụng 204 No Content
                        Message = "Approved successfully",
                        Data = ""
                    };

                    return Ok(response);
                }
                else
                {
                    var notFoundResponse = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "This request does not exist",
                        Data = ""
                    };

                    return NotFound(notFoundResponse);
                }


            } catch (Exception ex)
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

        [HttpDelete]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Delete(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity?.IsAuthenticated ?? false)
            {
                return Unauthorized("Not Authorized");
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Students.FindAsync(Convert.ToInt32(userId));
                if (user == null)
                {
                    return Unauthorized("Not Authorized");
                }
                bool deleted = await _registerExamService.DeleteRegisterExamAsync(id, user.Id);

                if (deleted)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 204, // Sử dụng 204 No Content
                        Message = "Course deleted successfully",
                        Data = ""
                    };

                    return Ok(response);
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

            } catch (Exception ex)
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
