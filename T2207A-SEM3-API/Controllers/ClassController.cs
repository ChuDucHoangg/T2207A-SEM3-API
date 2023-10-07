using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Class;

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
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = _context.Classes.Any(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Class name already exists");
                    }

                    Class data = new Class
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        Room = model.room,
                        TeacherId = model.teacher_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
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
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = _context.Classes.Any(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Class name already exists");
                    }

                    Class existingClass = _context.Classes.AsNoTracking().FirstOrDefault(e => e.Id == model.id);
                    if (existingClass != null)
                    {
                        Class classes = new Class
                        {
                            Id = model.id,
                            Name = model.name,
                            Slug = model.name.ToLower().Replace(" ", "-"),
                            Room = model.room,
                            TeacherId = model.teacher_id,
                            CreatedAt = existingClass.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (classes != null)
                        {
                            _context.Classes.Update(classes);
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
