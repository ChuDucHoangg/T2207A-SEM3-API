using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Helper.Email;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Staff;
using T2207A_SEM3_API.Models.Student;
using T2207A_SEM3_API.Models.User;
using T2207A_SEM3_API.Service.Email;
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
        private readonly IEmailService _emailService;

        public AuthController(ExamonimyContext context, IConfiguration config, IImgService imgService, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _imgService = imgService;
            _emailService = emailService;
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
                new Claim(ClaimTypes.Role,user.Role),
                new Claim("Thumbnail", user.Avatar)
            };
            var token = new JwtSecurityToken(
                    _config["JWT:Issuer"],
                    _config["JWT:Audience"],
                    payload,
                    expires: DateTime.Now.AddHours(2),
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
                    expires: DateTime.Now.AddHours(2),
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

        [HttpPost]
        [Route("student/forgot-password")]
        public async Task<IActionResult> StudentForgotPassword(ForgotPasswordModel model)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.email);
                if (student == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Student not found"
                    });
                }

                var resetToken = GenerateResetToken();
                student.ResetToken = resetToken;
                student.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Thời gian hết hiệu lực của token: 1 giờ
                await _context.SaveChangesAsync();

                var resetLink = "https://localhost:7218/api/Auth/student/reset-password/"+resetToken;

                /*Mailrequest mailrequest = new Mailrequest();
                mailrequest.ToEmail = "trungtvt.dev@gmail.com";
                mailrequest.Subject = "Password Reset";
                mailrequest.Body = $"Click the link to reset your password: {resetLink}";

                await _emailService.SendEmailAsync(mailrequest);*/


                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    Message = "Password reset email sent successfully"
                });
            } catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("student/reset-password/{token}")]
        public async Task<IActionResult> StudentResetPassword(string token, ResetPasswordModel model)
        {
            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
                if (student == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Student not found"
                    });
                }

                // Kiểm tra tính hợp lệ của mã reset
                if (model == null || string.IsNullOrEmpty(token) || student.ResetToken != token || student.Email != model.Email || student.ResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid or expired reset token"
                    });
                }

                // Cập nhật mật khẩu
                var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                var hassNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);

                student.Password = hassNewPassword; // Hash mật khẩu trước khi lưu
                student.ResetToken = null;
                student.ResetTokenExpiry = null;
                await _context.SaveChangesAsync();

                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    Message = "Password reset successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("staff/forgot-password")]
        public async Task<IActionResult> StaffForgotPassword(ForgotPasswordModel model)
        {
            try
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == model.email);
                if (staff == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Staff not found"
                    });
                }

                var resetToken = GenerateResetToken();
                staff.ResetToken = resetToken;
                staff.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Thời gian hết hiệu lực của token: 1 giờ
                await _context.SaveChangesAsync();

                var resetLink = "https://localhost:7218/api/Auth/staff/reset-password/" + resetToken;

                /*Mailrequest mailrequest = new Mailrequest();
                mailrequest.ToEmail = "trungtvt.dev@gmail.com";
                mailrequest.Subject = "Password Reset";
                mailrequest.Body = $"Click the link to reset your password: {resetLink}";

                await _emailService.SendEmailAsync(mailrequest);*/


                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    Message = "Password reset email sent successfully"
                });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("Staff/reset-password/{token}")]
        public async Task<IActionResult> StaffResetPassword(string token, ResetPasswordModel model)
        {
            try
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == model.Email);
                if (staff == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Staff not found"
                    });
                }

                // Kiểm tra tính hợp lệ của mã reset
                if (model == null || string.IsNullOrEmpty(token) || staff.ResetToken != token || staff.Email != model.Email || staff.ResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid or expired reset token"
                    });
                }

                // Cập nhật mật khẩu
                var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                var hassNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);

                staff.Password = hassNewPassword; // Hash mật khẩu trước khi lưu
                staff.ResetToken = null;
                staff.ResetTokenExpiry = null;
                await _context.SaveChangesAsync();

                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    Message = "Password reset successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
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
