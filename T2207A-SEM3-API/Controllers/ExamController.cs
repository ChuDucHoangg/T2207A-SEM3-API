using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Exam;
using T2207A_SEM3_API.Models.General;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/exam")]
    [ApiController]
    public class ExamController : Controller
    {
        private readonly ExamonimyContext _context;

        public ExamController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Exam> exams = await _context.Exams.OrderByDescending(s => s.Id).ToListAsync();

                List<ExamDTO> data = new List<ExamDTO>();
                foreach (Exam e in exams)
                {
                    data.Add(new ExamDTO
                    {
                        id = e.Id,
                        name = e.Name,
                        slug = e.Slug,
                        courseClass_id = e.CourseClassId,
                        start_date = e.StartDate,
                        created_by = e.CreatedBy,
                        createdAt = e.CreatedAt,
                        updatedAt = e.UpdatedAt,
                        deletedAt = e.DeletedAt
                    });
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-slug")]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Get(string slug)
        {
            try
            {
                Exam c = await _context.Exams.FirstOrDefaultAsync(x => x.Slug == slug);
                if (c != null)
                {
                    return Ok(new ExamDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        courseClass_id = c.CourseClassId,
                        start_date = c.StartDate,
                        created_by = c.CreatedBy,
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt,
                        deletedAt = c.DeletedAt

                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Create(CreateExam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    List<Exam> exams = await _context.Exams.Where(p => p.CourseClassId == model.courseClass_id).ToListAsync();
                    if (exams.Count >= 2)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "You have created too many exams",
                            Data = ""
                        });
                    }

                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Exams.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Exam name already exists",
                            Data = ""
                        });
                    }

                    Exam data = new Exam
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        CourseClassId = model.courseClass_id,
                        StartDate = model.start_date,
                        CreatedBy = model.created_by,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Exams.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new ExamDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        courseClass_id = data.CourseClassId,
                        start_date = data.StartDate,
                        created_by = data.CreatedBy,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            var msgs = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);
            return BadRequest(string.Join(" | ", msgs));
        }

        [HttpPut]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Update(EditExam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Exam existingExam = _context.Exams.AsNoTracking().FirstOrDefault(e => e.Id == model.id);

                    if (existingExam != null)
                    {
                        // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng tên)
                        bool nameExists = await _context.Exams.AnyAsync(c => c.Name == model.name && c.Id != model.id);

                        if (nameExists)
                        {
                            // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                            return BadRequest("Exam name already exists");
                        }

                        Exam exam = new Exam
                        {
                            Id = model.id,
                            Name = model.name,
                            Slug = model.name.ToLower().Replace(" ", "-"),
                            CourseClassId = model.courseClass_id,
                            StartDate = model.start_date,
                            CreatedBy = model.created_by,
                            CreatedAt = existingExam.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (exam != null)
                        {
                            _context.Exams.Update(exam);
                            _context.SaveChanges();
                            return NoContent();
                        }
                    }
                    else
                    {
                        return NotFound(); // Không tìm thấy lớp để cập nhật
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }

        [HttpDelete]
        [Authorize(Roles = "Super Admin, Staff")]

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Exam exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                    return NotFound();
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-courseId")]
        [Authorize]
        public async Task<IActionResult> GetbyCourse(int courseId)
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

                // kiểm tra classCourseID
                var classCourse = await _context.ClassCourses.FirstOrDefaultAsync(c => c.Id == courseId);
                if (classCourse == null)
                {
                    return NotFound();
                }
                if (classCourse.ClassId != user.ClassId)
                {
                    return NotFound();
                }



                List<Exam> exams = await _context.Exams.Include(p => p.CourseClass).ThenInclude(p => p.Course).Where(p => p.CourseClassId == classCourse.Id).ToListAsync();
                if (exams != null)
                {
                    List<ExamResponse> data = exams.Select(c => new ExamResponse
                    {
                        id = c.Id,
                        name = c.Name,
                        courseName = c.CourseClass.Course.Name,
                        slug = c.Slug,
                        courseClass_id = c.CourseClassId,
                        start_date = c.StartDate,
                        created_by = c.CreatedBy,
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt,
                        deletedAt = c.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No products found in this category.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
