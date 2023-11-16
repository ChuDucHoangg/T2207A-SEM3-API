using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.User;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        
        public MenuController(ExamonimyContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMenu()
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

                var user = await _context.Staffs.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

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

                if (user.Role.Contains("Super Admin"))
                {
                    var menu = new List<MenuItem>
                    {
                        new MenuItem { Title = "Dashboard", Url = "/", Icon = "<i class=\"feather-grid\"></i>" },
                        new MenuItem { Title = "Exam", Url = "/exam-list", Icon = "<i class=\"fas fa-clipboard-list\"></i>" },
                        new MenuItem { Title = "Test", Url = "/test-list", Icon = "<i class=\"feather-calendar\"></i>"},
                        new MenuItem { Title = "Class", Url = "/classes-list", Icon = "<i class=\"fas fa-building\"></i>"},
                        new MenuItem { Title = "Course", Url = "/course-list", Icon = "<i class=\"fas fa-book-reader\"></i>"},
                        new MenuItem { Title = "Student", Url = "/student-list", Icon = "<i class=\"fas fa-graduation-cap\"></i>"},
                        new MenuItem { Title = "Staff", Url = "/teacher-list", Icon = "<i class=\"fas fa-chalkboard-teacher\"></i>"},
                        new MenuItem { Title = "Profile", Url = "/profile", Icon = "<i class=\"fas fa-cog\"></i>"},
                        // Thêm các mục menu khác cho Admin
                    };
                    return Ok(menu);
                }
                else if (user.Role.Contains("Exam Administrator"))
                {
                    var menu = new List<MenuItem>
                    {
                        new MenuItem { Title = "Dashboard", Url = "/", Icon = "<i class=\"feather-grid\"></i>" },
                        new MenuItem { Title = "Exam", Url = "/exam-list", Icon = "<i class=\"fas fa-clipboard-list\"></i>" },
                        new MenuItem { Title = "Test", Url = "/test-list", Icon = "<i class=\"feather-calendar\"></i>"},
                        new MenuItem { Title = "Profile", Url = "/profile", Icon = "<i class=\"fas fa-cog\"></i>"},



                        // Thêm các mục menu khác cho User
                        //
                    };
                    return Ok(menu);
                }
                else if (user.Role.Contains("Teacher"))
                {
                    var menu = new List<MenuItem>
                    {
                        new MenuItem { Title = "Dashboard", Url = "/", Icon = "<i class=\"feather-grid\"></i>" },
                        new MenuItem { Title = "Test", Url = "/test-list", Icon = "<i class=\"feather-calendar\"></i>"},
                        new MenuItem { Title = "Class", Url = "/classes-list", Icon = "<i class=\"fas fa-building\"></i>"},
                        new MenuItem { Title = "Profile", Url = "/profile", Icon = "<i class=\"fas fa-cog\"></i>"},



                        // Thêm các mục menu khác cho User
                    };
                return Ok(menu);
            }
                else
                {
                    // Mặc định cho các trường hợp khác
                    var menu = new List<MenuItem>();
                    return Ok(menu);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                });
            }
        }
    }
}
