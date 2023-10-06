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
                    avatar = st.Avatar,
                    birthday = st.Birthday,
                    email = st.Email,
                    phone = st.Phone,
                    gender = st.Gender,
                    address = st.Address,
                    class_id = st.ClassId,
                    password = st.Password,
                    status = st.Status,
                    createdAt = st.CreatedAt,
                    updateAt = st.UpdatedAt,
                    deleteAt = st.DeletedAt
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
                        avatar = st.Avatar,
                        birthday = st.Birthday,
                        email = st.Email,
                        phone = st.Phone,
                        gender = st.Gender,
                        address = st.Address,
                        class_id = st.ClassId,
                        password = st.Password,
                        status = st.Status,
                        createdAt = st.CreatedAt,
                        updateAt = st.UpdatedAt,
                        deleteAt = st.DeletedAt

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
                        Avatar = model.avatar,
                        Birthday = model.birthday,
                        Email = model.email,
                        Phone = model.phone,
                        Gender = model.gender,
                        Address = model.address,
                        ClassId = model.class_id,
                        Password = model.password,
                        Status = model.status,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Students.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new StudentDTO
                    {
                        id = data.Id,
                        student_code = data.StudentCode,
                        fullname = data.Fullname,
                        avatar = data.Avatar,
                        birthday = data.Birthday,
                        email = data.Email,
                        phone = data.Phone,
                        gender = data.Gender,
                        address = data.Address,
                        class_id = data.ClassId,
                        password = data.Password,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updateAt = data.UpdatedAt,
                        deleteAt = data.DeletedAt,
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
                        Avatar = model.avatar,
                        Birthday = model.birthday,
                        Email = model.email,
                        Phone = model.phone,
                        Gender = model.gender,
                        Address = model.address,
                        ClassId = model.class_id,
                        Password = model.password,
                        Status = model.status,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
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
