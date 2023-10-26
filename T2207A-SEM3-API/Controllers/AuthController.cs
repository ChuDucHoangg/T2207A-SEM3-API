﻿using Microsoft.AspNetCore.Http;
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

        private string GenerateToken(Staff user)
        {
            var secretKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var signatureKey = new SigningCredentials(secretKey,
                                    SecurityAlgorithms.HmacSha256);
            var payload = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
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

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
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
                    Data = GenerateToken(user)
                });
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}