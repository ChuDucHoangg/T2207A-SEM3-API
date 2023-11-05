using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Helper.Email;
using T2207A_SEM3_API.Helper.Password;
using T2207A_SEM3_API.Models.Student;
using T2207A_SEM3_API.Service.Email;
using T2207A_SEM3_API.Service.Student;
using T2207A_SEM3_API.Service.UploadFiles;
using static System.Net.Mime.MediaTypeNames;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ExamonimyContext _context;
        private readonly IEmailService _emailService;
        private readonly IImgService _imgService;
        private readonly IStudentService _studentService;

        public StudentController(ExamonimyContext context, IEmailService emailService, IImgService imgService, IStudentService studentService)
        {
            _context = context;
            _emailService = emailService;
            _imgService = imgService;
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Student> students = await _context.Students.ToListAsync();

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
                        status = st.Status,
                        createdAt = st.CreatedAt,
                        updateAt = st.UpdatedAt,
                        deleteAt = st.DeletedAt
                    });
                }
                return Ok(data);
            } 
            catch (Exception e)
            {
                return BadRequest($"An error occurred: {e.Message}");
            }
            
        }

        [HttpGet]
        [Route("get-by-codeStudent")]
        public async Task<IActionResult> Get(string code_student)
        {
            try
            {
                Student st = await _context.Students.AsNoTracking().FirstOrDefaultAsync(e => e.StudentCode == code_student);
                if (st != null)
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
        public async Task<IActionResult> Create([FromForm] CreateStudent model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);
                return BadRequest(string.Join(" | ", errors));
            }

            try
            {
                
                // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa
                bool emailExists = await _context.Students.AnyAsync(c => c.Email == model.email);

                if (emailExists)
                {
                    return BadRequest("Student email already exists");
                }

                var imageUrl = await _imgService.UploadImageAsync(model.avatar);

                // general password
                //var password = AutoGeneratorPassword.passwordGenerator(7, 2, 2, 2);
                
                // hash password
                var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                var hassPassword = BCrypt.Net.BCrypt.HashPassword(model.password, salt);

                if (imageUrl != null) 
                {
                    Student data = new Student
                    {
                        StudentCode = await _studentService.GenerateStudentCode(),
                        Fullname = model.fullname,
                        Avatar = imageUrl,
                        Birthday = model.birthday,
                        Email = model.email,
                        Phone = model.phone,
                        Gender = model.gender,
                        Address = model.address,
                        ClassId = model.class_id,
                        Password = hassPassword,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };

                    _context.Students.Add(data);
                    await _context.SaveChangesAsync();

                    // start send mail

                    /*Mailrequest mailrequest = new Mailrequest();
                    mailrequest.ToEmail = "trungtvt.dev@gmail.com";
                    mailrequest.Subject = "Welcome to Examonimy";
                    mailrequest.Body = EmailContentRegister.GetHtmlcontent(data.Fullname, data.Email, password);

                    await _emailService.SendEmailAsync(mailrequest);*/

                    // end send mail


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
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updateAt = data.UpdatedAt,
                        deleteAt = data.DeletedAt,
                    });
                }
                else
                {
                    return BadRequest("Please provide an avatar.");
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] EditStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Student exexistingStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);

                    if (exexistingStudent != null)
                    {
                        
                        // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng email)
                        bool emailExists = await _context.Students.AnyAsync(c => c.Email == model.email && c.Id != model.id);

                        if (emailExists)
                        {
                            return BadRequest("Student email already exists");
                        }

                        Student student = new Student
                        {
                            Id = model.id,
                            StudentCode = exexistingStudent.StudentCode,
                            Fullname = model.fullname,
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

                        if (model.avatar != null)
                        {
                            string imageUrl = await _imgService.UploadImageAsync(model.avatar);

                            if (imageUrl == null)
                            {
                                return BadRequest("Failed to upload avatar.");
                            }

                            student.Avatar = imageUrl;
                        }
                        else
                        {
                            student.Avatar = exexistingStudent.Avatar;
                        }

                        _context.Students.Update(student);
                        await _context.SaveChangesAsync();

                        return NoContent();
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Student student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound();

                student.DeletedAt = DateTime.Now; // Đặt thời gian xóa mềm

                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost]
        [Route("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                Student student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound();

                student.DeletedAt = null; // Đặt thời gian xóa mềm thành null để khôi phục sinh viên

                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpDelete]
        [Route("permanently-delete/{id}")]
        public async Task<IActionResult> PermanentlyDelete(int id)
        {
            try
            {
                Student student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound();

                _context.Students.Remove(student); // Xóa bản ghi sinh viên hoàn toàn khỏi cơ sở dữ liệu

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        [Route("get-by-classId")]
        public async Task<IActionResult> GetbyClass(int classId)
        {
            try
            {
                List<Student> students = await _context.Students.Where(p => p.ClassId == classId).ToListAsync();
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

        [HttpGet]
        [Route("student-test/{test_slug}")]
        public async Task<IActionResult> GetStudentTest(string test_slug)
        {
            try
            {
                var test = await _context.Tests.FirstOrDefaultAsync(g => g.Slug.Equals(test_slug));
                if (test == null)
                {
                    return BadRequest("The test does not exist");
                }

                // Lấy danh sách ID của các câu hỏi thuộc bài thi
                var studentIds = await _context.StudentTests
                    .Where(qt => qt.TestId == test.Id)
                    .ToListAsync();

                // Lấy danh sách câu hỏi dựa trên các ID câu hỏi
                var students = new List<Student>();
                foreach (var item in studentIds)
                {
                    var student = await _context.Students
                        .Where(q => q.Id == item.StudentId)
                        .FirstOrDefaultAsync();

                    if (student != null)
                    {
                        students.Add(student);
                    }
                }
                if (students != null)
                {
                    List<StudentOfTestResponse> data = students.Select(c => new StudentOfTestResponse
                    {
                        id = c.Id,
                        student_code = c.StudentCode,
                        fullname = c.Fullname,
                        avatar = c.Avatar,
                        birthday = c.Birthday,
                        email = c.Email,
                        class_id = c.ClassId,
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
