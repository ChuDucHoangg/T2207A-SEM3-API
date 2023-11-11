using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Staff;
using T2207A_SEM3_API.Models.Student;
using T2207A_SEM3_API.Models.User;
using T2207A_SEM3_API.Service.UploadFiles;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly IConfiguration _config;
        private readonly IImgService _imgService;

        public AuthController(ExamonimyContext context, IConfiguration config, IImgService imgService)
        {
            _context = context;
            _config = config;
            _imgService = imgService;
        }

        private string GenerateTokenStaff(Staff user)
        {
            var secretKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var signatureKey = new SigningCredentials(secretKey,
                                    SecurityAlgorithms.HmacSha256);
            var payload = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim("Staff-Code", user.StaffCode),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.Fullname),
                new Claim(ClaimTypes.Role,user.Role)
            };
            var token = new JwtSecurityToken(
                    _config["JWT:Issuer"],
                    _config["JWT:Audience"],
                    payload,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: signatureKey
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateTokenStudent(Student user)
        {
            var secretKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var signatureKey = new SigningCredentials(secretKey,
                                    SecurityAlgorithms.HmacSha256);
            var payload = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim("Student-Code", user.StudentCode),
                new Claim("Class-Id", user.ClassId.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.Fullname),
            };
            var token = new JwtSecurityToken(
                    _config["JWT:Issuer"],
                    _config["JWT:Audience"],
                    payload,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: signatureKey
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Route("staff-login")]
        public async Task<IActionResult> LoginWithStaff(LoginModel model)
        {
            try
            {
                var user = await _context.Staffs.Where(st => st.Email.Equals(model.email)).FirstOrDefaultAsync();
                if (user == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid email/password"
                    });
                }
                bool verified = BCrypt.Net.BCrypt.Verify(model.password, user.Password);
                if (!verified)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid email/password"
                    });
                }

                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    Message = "Authenticate success",
                    Data = GenerateTokenStaff(user)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("student-login")]
        public async Task<IActionResult> LoginWithStudent(LoginModel model)
        {
            try
            {
                var user = await _context.Students.Where(st => st.Email.Equals(model.email)).FirstOrDefaultAsync();
                if (user == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid email/password"
                    });
                }
                bool verified = BCrypt.Net.BCrypt.Verify(model.password, user.Password);
                if (!verified)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid email/password"
                    });
                }

                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Authenticate success",
                    Data = GenerateTokenStudent(user)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("student/change-password")]
        [Authorize]
        public async Task<IActionResult> StudentChangePassword(ChangePasswordModel model)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = ""
                });
            }

            try
            {

                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Students.FindAsync(Convert.ToInt32(userId));
                if (user != null)
                {
                    bool verified = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password);

                    // Kiểm tra mật khẩu hiện tại
                    if (verified)
                    {
                        // hash password
                        var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                        var hassNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);
                        // Thực hiện thay đổi mật khẩu
                        user.Password = hassNewPassword;
                        _context.SaveChanges();
                        return Ok(new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = "Password changed successfully",
                            Data = ""
                        });
                    }
                    else
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Incorrect current password",
                            Data = ""
                        });
                    }
                }
                else
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("staff/change-password")]
        public async Task<IActionResult> StaffChangePassword(ChangePasswordModel model)
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

                var user = await _context.Staffs.FindAsync(Convert.ToInt32(userId));
                if (user != null)
                {
                    bool verified = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password);

                    // Kiểm tra mật khẩu hiện tại
                    if (verified)
                    {
                        // hash password
                        var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                        var hassNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);
                        // Thực hiện thay đổi mật khẩu
                        user.Password = hassNewPassword;
                        _context.SaveChanges();
                        return Ok(new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 200,
                            Message = "Password changed successfully",
                            Data = ""
                        });
                    }
                    else
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Incorrect current password",
                            Data = ""
                        });
                    }
                }
                else
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = ""
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpGet]
        [Route("student/profile")]
        [Authorize]
        public async Task<IActionResult> StudentProfile()
        {// get info form token
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

                return Ok(new StudentProfileRespone // đúng ra phải là UserProfileDTO
                {
                    id = user.Id,
                    student_code = user.StudentCode,
                    email = user.Email,
                    fullname = user.Fullname,
                    avatar = user.Avatar,
                    birthday = user.Birthday,
                    phone = user.Phone,
                    gender = user.Gender,
                    address = user.Address,
                    className = user.Class.Name,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpGet]
        [Route("staff/profile")]
        [Authorize]
        public async Task<IActionResult> StaffProfile()
        {// get info form token
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

                return Ok(new StaffProfileResponse // đúng ra phải là UserProfileDTO
                {
                    id = user.Id,
                    staff_code = user.StaffCode,
                    email = user.Email,
                    fullname = user.Fullname,
                    avatar = user.Avatar,
                    birthday = user.Birthday,
                    phone = user.Phone,
                    gender = user.Gender,
                    address = user.Address,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = ""
                });
            }
        }

        [HttpPut]
        [Route("student/update-profile")]
        public async Task<IActionResult> StudentUpdateProfile([FromForm]StudentUpdateProfileRequest model)
        {
            if (ModelState.IsValid)
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (!identity.IsAuthenticated)
                {
                    return Unauthorized(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                try
                {
                    var userClaims = identity.Claims;
                    var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    var user = await _context.Students
                        .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                    if (user == null)
                    {
                        return Unauthorized(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 401,
                            Message = "Not Authorized",
                            Data = ""
                        });
                    }

                    user.Birthday = model.birthday;
                    user.Phone = model.phone;
                    user.Gender = model.gender;
                    user.Address = model.address;
                    user.UpdatedAt = DateTime.Now;

                    if (model.avatar != null)
                    {
                        string imageUrl = await _imgService.UploadImageAsync(model.avatar);

                        if (imageUrl == null)
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 404,
                                Message = "Failed to upload avatar.",
                                Data = ""
                            });
                        }

                        user.Avatar = imageUrl;
                    }
                    else
                    {
                        user.Avatar = user.Avatar;
                    }

                    _context.Students.Update(user);
                    await _context.SaveChangesAsync();

                    return NoContent();

                }
                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }

            return BadRequest(new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 404,
                Message = "",
                Data = ""
            });

        }

        [HttpPut]
        [Route("staff/update-profile")]
        [Authorize]
        public async Task<IActionResult> StafUpdateProfile([FromForm] StaffUpdateProfileRequest model)
        {
            if (ModelState.IsValid)
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (!identity.IsAuthenticated)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not Authorized",
                        Data = ""
                    });
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


                    user.Birthday = model.birthday;
                    user.Phone = model.phone;
                    user.Gender = model.gender;
                    user.Address = model.address;
                    user.UpdatedAt = DateTime.Now;

                    if (model.avatar != null)
                    {
                        string imageUrl = await _imgService.UploadImageAsync(model.avatar);

                        if (imageUrl == null)
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 404,
                                Message = "Failed to upload avatar.",
                                Data = ""
                            });
                        }

                        user.Avatar = imageUrl;
                    }
                    else
                    {
                        user.Avatar = user.Avatar;
                    }

                    _context.Staffs.Update(user);
                    await _context.SaveChangesAsync();

                    return NoContent();

                }
                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }

            return BadRequest(new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 404,
                Message = "",
                Data = ""
            });

        }
    }
}
