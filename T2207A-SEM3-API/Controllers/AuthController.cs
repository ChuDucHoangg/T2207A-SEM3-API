using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.User;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly IConfiguration _config;

        public AuthController(ExamonimyContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
                var user = _context.Staffs.SingleOrDefault(p => p.Email == model.email && p.Password == model.password);
                if (user == null)
                {
                    return Ok(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid username/password"
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
                var user = _context.Students.Where(st => st.Email.Equals(model.email)).First();
                if (user == null)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Invalid username/password"
                    });
                }
                bool verified = BCrypt.Net.BCrypt.Verify(model.password, user.Password);
                if (!verified)
                {
                    throw new Exception("email or pass is not corect");
                }

                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    Message = "Authenticate success",
                    Data = GenerateTokenStudent(user)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("student/change-password")]
        public async Task<IActionResult> StudentChangePassword(ChangePasswordModel model)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized("Not Authorized");
            }

            try
            {

                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = _context.Students.Find(Convert.ToInt32(userId));
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
                            Data = null
                        });
                    }
                    else
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 400,
                            Message = "Incorrect current password",
                            Data = null
                        });
                    }
                }
                else
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 404,
                        Message = "Incorrect current password",
                        Data = null
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
