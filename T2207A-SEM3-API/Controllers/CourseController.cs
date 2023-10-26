using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.Student;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : Controller
    {
        private readonly ExamonimyContext _context;

        public CourseController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet, Authorize(Roles ="Teacher")]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Course> courses = await _context.Courses.ToListAsync();

                List<CourseDTO> data = new List<CourseDTO>();
                foreach (Course cr in courses)
                {
                    data.Add(new CourseDTO
                    {
                        id = cr.Id,
                        name = cr.Name,
                        course_code = cr.CourseCode,
                        createdAt = cr.CreatedAt,
                        updateAt = cr.UpdatedAt,
                        deleteAt = cr.DeletedAt
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
        [Route("get-by-codeCourse")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                Course cr = await _context.Courses.FindAsync(id);
                if (cr != null)
                {
                    return Ok(new CourseDTO
                    {
                        id = cr.Id,
                        name = cr.Name,
                        course_code = cr.CourseCode,
                        createdAt = cr.CreatedAt,
                        updateAt = cr.UpdatedAt,
                        deleteAt = cr.DeletedAt
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
        public async Task<IActionResult> Create(CreateCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem student_code đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool codeExists = await _context.Courses.AnyAsync(c => c.CourseCode == model.course_code);

                    if (codeExists)
                    {
                        return BadRequest("Student code already exists");
                    }

                    Course data = new Course
                    {
                        Name = model.name,
                        CourseCode = model.course_code,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Courses.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new CourseDTO
                    {
                        id = data.Id,
                        course_code = data.CourseCode,
                        name = data.Name,
                        createdAt = data.CreatedAt,
                        updateAt = data.UpdatedAt,
                        deleteAt = data.DeletedAt,
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
        public async Task<IActionResult> Update(EditCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Course exexistingCourse = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);

                    // Kiểm tra xem student_code đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng mã)
                    bool codeExists = await _context.Courses.AnyAsync(c => c.CourseCode == model.course_code && c.Id != model.id);

                    if (codeExists)
                    {
                        return BadRequest("Student code already exists");
                    }

                    Course course = new Course
                    {
                        Id = model.id,
                        Name = model.name,
                        CourseCode = model.course_code,
                        CreatedAt = exexistingCourse.CreatedAt,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };

                    if (course != null)
                    {
                        _context.Courses.Update(course);
                        await _context.SaveChangesAsync();
                        return NoContent();
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Course course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound();
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

