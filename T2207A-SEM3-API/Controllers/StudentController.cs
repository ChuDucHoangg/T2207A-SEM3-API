using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Student;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController : Controller
    {
        private readonly ExamonimyContext _context;

        public StudentController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Student> students = _context.Students.ToList();

            List<StudentDTO> data = new List<StudentDTO>();
            foreach (Student st in students)
            {
                data.Add(new StudentDTO
                {
                    id = st.Id,
                    student_code = st.StudentCode,
                    fullname = st.Fullname,
                    birthday = st.Birthday,
                    email = st.Email,
                    phone = st.Phone,
                    class_id = st.ClassId,
                    password = st.Password,
                    role = st.Role,
                    status = st.Status,
                    createdAt = st.CreatedAt,
                    updateAt = st.UpdateAt,
                    deleteAt = st.DeleteAt
                });
            }
            return Ok(data);
        }

        [HttpGet]
        [Route("get-by-codeStudent")]
        public IActionResult Get(int id)
        {
            try
            {
                Student st = _context.Students.Find(id);
                if(st != null)
                {
                    return Ok(new StudentDTO
                    {
                        id = st.Id,
                        student_code = st.StudentCode,
                        fullname = st.Fullname,
                        birthday = st.Birthday,
                        email = st.Email,
                        phone = st.Phone,
                        class_id = st.ClassId,
                        password = st.Password,
                        role = st.Role,
                        status = st.Status,
                        createdAt = st.CreatedAt,
                        updateAt = st.UpdateAt,
                        deleteAt = st.DeleteAt

                    });
                }
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Create(CreateStudent model)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    Student data = new Student {
                        StudentCode = model.student_code,
                        Fullname = model.fullname,
                        Birthday = model.birthday,
                        Email = model.email,
                        Phone = model.phone,
                        ClassId = model.class_id,
                        Password = model.password,
                        Role = model.role,
                        Status = model.status,
                        CreatedAt = DateTime.Now,
                        UpdateAt = DateTime.Now,
                        DeleteAt = DateTime.Now,
                    };
                    _context.Students.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new StudentDTO
                    {
                        id = data.Id,
                        student_code = data.StudentCode,
                        fullname = data.Fullname,
                        birthday = data.Birthday,
                        email = data.Email,
                        phone = data.Phone,
                        class_id = data.ClassId,
                        password = data.Password,
                        role = data.Role,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updateAt = data.UpdateAt,
                        deleteAt = data.DeleteAt,
                    });
                } catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            var msgs = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);
            return BadRequest(string.Join(" | ", msgs));
        }

        [HttpPut]
        public IActionResult Update(EditStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Student student = new Student
                    {
                        Id = model.id,
                        StudentCode = model.student_code,
                        Fullname = model.fullname,
                        Birthday = model.birthday,
                        Email = model.email,
                        Phone = model.phone,
                        ClassId = model.class_id,
                        Password = model.password,
                        Role = model.role,
                        Status = model.status,
                        CreatedAt = model.createdAt,
                        UpdateAt = model.updatedAt,
                        DeleteAt = model.deletedAt,
                    };

                    if (student != null)
                    {
                        _context.Students.Update(student);
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
                Student student = _context.Students.Find(id);
                if (student == null)
                    return NotFound();
                _context.Students.Remove(student);
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
