using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool codeExists = _context.Students.Any(c => c.StudentCode == model.student_code);

                    if (codeExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Code student already exists");
                    }

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
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
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
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool codeExists = _context.Students.Any(c => c.StudentCode == model.student_code);

                    if (codeExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Code student already exists");
                    }

                    Student exexistingStudent = _context.Students.AsNoTracking().FirstOrDefault(e => e.Id == model.id);

                    if (exexistingStudent != null)
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
                            Status = exexistingStudent.Status,
                            CreatedAt = exexistingStudent.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (student != null)
                        {
                            _context.Students.Update(student);
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

        [HttpGet]
        [Route("get-by-classId")]
        public IActionResult GetbyClass(int classId)
        {
            try
            {
                List<Student> students = _context.Students.Where(p => p.ClassId == classId).ToList();
                if (students != null)
                {
                    List<StudentDTO> data = students.Select(c => new StudentDTO
                    {
                        id = c.Id,
                        student_code = c.StudentCode,
                        fullname = c.Fullname,
                        avatar = c.Avatar,
                        birthday = c.Birthday,
                        email = c.Email,
                        phone = c.Phone,
                        gender = c.Gender,
                        address = c.Address,
                        class_id = c.ClassId,
                        password = c.Password,
                        status = c.Status,
                        createdAt = c.CreatedAt,
                        updateAt = c.UpdatedAt,
                        deleteAt = c.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No student found in this class.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
