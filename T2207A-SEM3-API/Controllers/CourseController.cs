using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult Index()
        {
            List<Course> courses = _context.Courses.ToList();

            List<CourseDTO> data = new List<CourseDTO>();
            foreach (Course cr in courses)
            {
                data.Add(new CourseDTO
                {
                    id = cr.Id,
                    name = cr.Name,
                    course_code = cr.CourseCode,
                    class_id = cr.ClassId,
                    created_by = cr.CreatedBy,
                    createdAt = cr.CreatedAt,
                    updateAt = cr.UpdatedAt,
                    deleteAt = cr.DeletedAt
                });
            }
            return Ok(data);
        }

        [HttpGet]
        [Route("get-by-codeCourse")]
        public IActionResult Get(int id)
        {
            try
            {
                Course cr = _context.Courses.Find(id);
                if (cr != null)
                {
                    return Ok(new CourseDTO
                    {
                        id = cr.Id,
                        name = cr.Name,
                        course_code = cr.CourseCode,
                        class_id = cr.ClassId,
                        created_by = cr.CreatedBy,
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
        public IActionResult Create(CreateCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Course data = new Course
                    {
                        Name = model.name,
                        CourseCode = model.course_code,
                        ClassId = model.class_id,
                        CreatedBy = model.created_by,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Courses.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new CourseDTO
                    {
                        id = data.Id,
                        course_code = data.CourseCode,
                        name = data.Name,
                        class_id = data.ClassId,
                        created_by = data.CreatedBy,
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
        public IActionResult Update(EditCourse model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Course course = new Course
                    {
                        Id = model.id,
                        Name = model.name,
                        CourseCode = model.course_code,
                        ClassId = model.class_id,
                        CreatedBy = model.created_by,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };

                    if (course != null)
                    {
                        _context.Courses.Update(course);
                        _context.SaveChanges();
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
        public IActionResult Delete(int id)
        {
            try
            {
                Course course = _context.Courses.Find(id);
                if (course == null)
                    return NotFound();
                _context.Courses.Remove(course);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

