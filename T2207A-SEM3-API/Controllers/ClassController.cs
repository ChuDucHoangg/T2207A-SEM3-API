using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Class;
using T2207A_SEM3_API.Models.Student;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/classes")]
    [ApiController]
    public class ClassController : Controller
    {
        private readonly ExamonimyContext _context;

        public ClassController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Class> classes = _context.Classes.ToList();

            List<ClassDTO> data = new List<ClassDTO>();
            foreach (Class c in classes)
            {
                data.Add(new ClassDTO
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    room = c.Room,
                    teacher_id = c.TeacherId,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    deletedAt = c.DeletedAt
                });
            }
            return Ok(data);
        }

        [HttpGet]
        [Route("get-by-slug")]
        public IActionResult Get(string slug)
        {
            try
            {
                Class c = _context.Classes.FirstOrDefault(x => x.Slug == slug);
                if (c != null)
                {
                    return Ok(new ClassDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        room = c.Room,
                        teacher_id = c.TeacherId,
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
        public IActionResult Create(CreateClass model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Class data = new Class
                    {
                        Name = model.name,
                        Slug = model.slug,
                        Room = model.room,
                        TeacherId = model.teacher_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Classes.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new ClassDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        room = data.Room,
                        teacher_id = data.TeacherId,
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
        public IActionResult Update(EditClass model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Class classes = new Class
                    {
                        Id = model.id,
                        Name = model.name,
                        Slug = model.slug,
                        Room = model.room,
                        TeacherId = model.teacher_id,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
                    };

                    if (classes != null)
                    {
                        _context.Classes.Update(classes);
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
                Class classes = _context.Classes.Find(id);
                if (classes == null)
                    return NotFound();
                _context.Classes.Remove(classes);
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
