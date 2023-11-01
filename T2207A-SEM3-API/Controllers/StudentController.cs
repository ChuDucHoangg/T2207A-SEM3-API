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
using static System.Net.Mime.MediaTypeNames;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController : Controller
    {
        private readonly ExamonimyContext _context;
        private readonly IEmailService _emailService;

        public StudentController(ExamonimyContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                        password = st.Password,
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

        private async Task<string> GenerateStudentCode()
        {
            // Lấy năm hiện tại dưới dạng chuỗi (ví dụ: "2022")
            string currentYear = DateTime.Now.ToString("yy");

            // Lấy tháng hiện tại dưới dạng chuỗi (ví dụ: "04" cho tháng 4)
            string currentMonth = DateTime.Now.ToString("MM");

            var codePrefix = $"TH{currentYear}{currentMonth:D2}";

            var lastStudent = await _context.Students
            .Where(s => s.StudentCode.StartsWith(codePrefix))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

            int newSequenceNumber;
            if (lastStudent != null)
            {
                var lastSequenceNumber = int.Parse(lastStudent.StudentCode.Substring(8));
                newSequenceNumber = lastSequenceNumber + 1;
            }
            else
            {
                newSequenceNumber = 1;
            }

            string studentCode = $"{codePrefix}{newSequenceNumber:D3}";

            return studentCode;
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

                string imageUrl = await UploadImageAsync(model.avatar);
                // general password
                //var password = AutoGeneratorPassword.passwordGenerator(7, 2, 2, 2);
                
                // hash password
                var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                var hassPassword = BCrypt.Net.BCrypt.HashPassword(model.password, salt);

                if (imageUrl != null) 
                {
                    Student data = new Student
                    {
                        StudentCode = await GenerateStudentCode(),
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
                    mailrequest.Body = GetHtmlcontent(data.Fullname, data.Email, password);

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
                        password = data.Password,
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

        private string GetHtmlcontent(string name, string email, string password)
        {
            string Response = "<!doctype html><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head><title>Contactlab Marketing Cloud</title><!--[if !mso]><!-- --><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><!--<![endif]--><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><style type=\"text/css\">#outlook a{padding:0}body{margin:0;padding:0;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%}table,td{border-collapse:collapse;mso-table-lspace:0pt;mso-table-rspace:0pt}img{border:0;height:auto;line-height:100%;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic}p{display:block;margin:13px 0}</style><!--[if mso]> <xml> <o:OfficeDocumentSettings> <o:AllowPNG/> <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings> </xml> <![endif]--><!--[if lte mso 11]><style type=\"text/css\">.mj-outlook-group-fix{width:100% !important}</style><![endif]--><!--[if !mso]><!--><link href=\"https://fonts.googleapis.com/css?family=Montserrat:400,700\" rel=\"stylesheet\" type=\"text/css\"><style type=\"text/css\">@import url(https://fonts.googleapis.com/css?family=Montserrat:400,700);</style><!--<![endif]--><style type=\"text/css\">@media only screen and (min-width:400px){.mj-column-per-100{width:100% !important;max-width:100%}}</style><style type=\"text/css\">@media only screen and (max-width:400px){table.mj-full-width-mobile{width:100% !important}td.mj-full-width-mobile{width:auto !important}}</style></head><body style=\"background-color:#F7FCFF;\"><div style=\"display:none;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;\">Automatic email sent by the Contactlab Marketing Cloud platform. Please, don&#x27;t reply.</div><div style=\"background-color:#F7FCFF;\"><!--[if mso | IE]><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:460px;\" width=\"460\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"margin:0px auto;max-width:460px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"width:100%;\"><tbody><tr><td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" width=\"460px\" ><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:460px;\" width=\"460\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"background:#F7FCFF;background-color:#F7FCFF;margin:0px auto;max-width:460px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:#F7FCFF;background-color:#F7FCFF;width:100%;\"><tbody><tr><td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:25px;padding-left:10px;padding-right:10px;padding-top:0px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" style=\"vertical-align:top;width:440px;\" ><![endif]--><div class=\"mj-column-per-100 mj-outlook-group-fix\" style=\"font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:top;\" width=\"100%\"><tr><td align=\"center\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"border-collapse:collapse;border-spacing:0px;\" class=\"mj-full-width-mobile\"><tbody><tr><td style=\"width:300px;\" class=\"mj-full-width-mobile\"><a href=\"http://mc.contactlab.it\" target=\"_blank\"><img alt=\"Contactlab Marketing Cloud logo\" height=\"auto\" src=\"https://i.postimg.cc/zvdjMxkG/logo-mc-full-positive-593x60.png\" style=\"border:0;display:block;outline:none;text-decoration:none;height:auto;width:100%;font-size:13px;\" title=\"Contactlab Marketing Cloud\" width=\"300\"></a></td></tr></tbody></table></td></tr></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body-section-outlook\" style=\"width:460px;\" width=\"460\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div class=\"body-section\" style=\"-webkit-box-shadow: 0 1px 3px 0 rgba(0, 20, 32, 0.12); -moz-box-shadow: 0 1px 3px 0 rgba(0, 20, 32, 0.12); box-shadow: 0 1px 3px 0 rgba(0, 20, 32, 0.12); margin: 0px auto; max-width: 460px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"width:100%;\"><tbody><tr><td style=\"direction:ltr;font-size:0px;padding:0px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" width=\"460px\" ><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:460px;\" width=\"460\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"background:#FFFFFF;background-color:#FFFFFF;margin:0px auto;border-radius:8px;max-width:460px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:#FFFFFF;background-color:#FFFFFF;width:100%;border-radius:8px;\"><tbody><tr><td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:25px;padding-left:10px;padding-right:10px;padding-top:25px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" style=\"vertical-align:top;width:440px;\" ><![endif]--><div class=\"mj-column-per-100 mj-outlook-group-fix\" style=\"font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:top;\" width=\"100%\"><tr><td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:30px;font-weight:700;line-height:36px;text-align:left;color:#1D3344;\">Hi "+name+",</div></td></tr><tr><td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:400;line-height:21px;text-align:left;color:#001420;\">your Contactlab Marketing Cloud account has been created! 🎉</div></td></tr><tr><td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:400;line-height:21px;text-align:left;color:#001420;\">Find below your credentials.</div></td></tr><tr><td style=\"font-size:0px;padding:20px 0;padding-top:10px;padding-right:25px;padding-bottom:10px;padding-left:25px;word-break:break-word;\"><!--[if mso | IE]><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:440px;\" width=\"440\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"background:#F7FCFF;background-color:#F7FCFF;margin:0px auto;max-width:440px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:#F7FCFF;background-color:#F7FCFF;width:100%;\"><tbody><tr><td style=\"border-left:4px solid #0391EC;direction:ltr;font-size:0px;padding:20px 0;padding-bottom:10px;padding-left:25px;padding-right:25px;padding-top:10px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" style=\"vertical-align:top;width:386px;\" ><![endif]--><div class=\"mj-column-per-100 mj-outlook-group-fix\" style=\"font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:top;\" width=\"100%\"><tr><td align=\"left\" style=\"font-size:0px;padding:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:left;color:#5B768C;\">Email:</div></td></tr><tr><td align=\"left\" style=\"font-size:0px;padding:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:700;line-height:21px;text-align:left;color:#001420;\">"+email+"</div></td></tr></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr><tr><td style=\"font-size:0px;padding:20px 0;padding-top:10px;padding-right:25px;padding-bottom:10px;padding-left:25px;word-break:break-word;\"><!--[if mso | IE]><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:440px;\" width=\"440\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"background:#F7FCFF;background-color:#F7FCFF;margin:0px auto;max-width:440px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:#F7FCFF;background-color:#F7FCFF;width:100%;\"><tbody><tr><td style=\"border-left:4px solid #0391EC;direction:ltr;font-size:0px;padding:20px 0;padding-bottom:10px;padding-left:25px;padding-right:25px;padding-top:10px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" style=\"vertical-align:top;width:386px;\" ><![endif]--><div class=\"mj-column-per-100 mj-outlook-group-fix\" style=\"font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:top;\" width=\"100%\"><tr><td align=\"left\" style=\"font-size:0px;padding:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:left;color:#5B768C;\">Password:</div></td></tr><tr><td align=\"left\" style=\"font-size:0px;padding:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:700;line-height:21px;text-align:left;color:#001420;\">"+password+"</div></td></tr></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr><tr><td align=\"center\" vertical-align=\"middle\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"border-collapse:separate;width:200px;line-height:100%;\"><tr><td align=\"center\" bgcolor=\"#0391EC\" role=\"presentation\" style=\"border:none;border-radius:8px;cursor:auto;mso-padding-alt:10px 25px;background:#0391EC;\" valign=\"middle\"><a href=\"https://login.contactlab.it\" style=\"display:inline-block;width:150px;background:#0391EC;color:#FFFFFF;font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:400;line-height:21px;margin:0;text-decoration:none;text-transform:none;padding:10px 25px;mso-padding-alt:0px;border-radius:8px;\" target=\"_blank\">Sign In</a></td></tr></table></td></tr><tr><td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:400;line-height:21px;text-align:left;color:#001420;\">For security reasons, please change the temporary password and keep the new one.</div></td></tr><tr><td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:14px;font-weight:400;line-height:21px;text-align:left;color:#001420;\">Thank you, the Contactlab team.</div></td></tr></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:460px;\" width=\"460\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"margin:0px auto;max-width:460px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"width:100%;\"><tbody><tr><td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-top:8px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" width=\"460px\" ><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"\" style=\"width:460px;\" width=\"460\" ><tr><td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"><![endif]--><div style=\"background:#F7FCFF;background-color:#F7FCFF;margin:0px auto;max-width:460px;\"><table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:#F7FCFF;background-color:#F7FCFF;width:100%;\"><tbody><tr><td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:25px;padding-left:10px;padding-right:10px;padding-top:25px;text-align:center;\"><!--[if mso | IE]><table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr><td class=\"\" style=\"vertical-align:top;width:440px;\" ><![endif]--><div class=\"mj-column-per-100 mj-outlook-group-fix\" style=\"font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;\"><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:top;\" width=\"100%\"><tr><td align=\"center\" style=\"font-size:0px;padding:10px 25px;padding-top:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:center;color:#5B768C;\">The email is auto generated. Please, don&#x27;t reply.</div></td></tr><tr><td align=\"center\" style=\"font-size:0px;padding:10px 25px;padding-bottom:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:center;color:#5B768C;\">Any doubts or questions? <a href=\"https://support.contactlab.com/hc/en-us\" style=\"font-weight:400;color:#0391EC;text-decoration:none;font-size:12px;line-height:16px\">Contact us.</a></div></td></tr><tr><td align=\"center\" style=\"font-size:0px;padding:10px 25px;padding-top:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:center;color:#5B768C;\">Otherwise, consult <a href=\"https://explore.contactlab.com\" style=\"font-weight:400;color:#0391EC;text-decoration:none;font-size:12px;line-height:16px\">the platform documentation.</a></div></td></tr><tr><td align=\"center\" style=\"font-size:0px;padding:10px 25px;padding-bottom:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:center;color:#5B768C;\">Contactlab Marketing Cloud is a product of <a href=\"https://explore.contactlab.com\" style=\"font-weight:400;color:#0391EC;text-decoration:none;font-size:12px;line-height:16px\">Contactlab S.p.A.</a></div></td></tr><tr><td align=\"center\" style=\"font-size:0px;padding:10px 25px;padding-top:0px;word-break:break-word;\"><div style=\"font-family:Montserrat, Helvetica, Arial, sans-serif;font-size:12px;font-weight:400;line-height:16px;text-align:center;color:#5B768C;\">Via Natale Battaglia, 12 - 20127 Milan, Italy</div></td></tr></table></div><!--[if mso | IE]></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table></td></tr></table><![endif]--></td></tr></tbody></table></div><!--[if mso | IE]></td></tr></table><![endif]--></div></body></html>";
            return Response;
        }

        private async Task<string> UploadImageAsync(IFormFile avatar)
        {
            if (avatar != null && avatar.Length > 0)
            {
                // Lấy tên tệp tin
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";

                // Đường dẫn lưu trữ tệp tin
                string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                string filePath = Path.Combine(uploadDirectory, fileName);

                Directory.CreateDirectory(uploadDirectory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                return $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            }

            return null; // Trả về null nếu không có hình ảnh
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
                        
                        // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng mã)
                        bool codeExists = await _context.Students.AnyAsync(c => c.StudentCode == model.student_code && c.Id != model.id);

                        if (codeExists)
                        {
                            // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                            return BadRequest("Code student already exists");
                        }
                        // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng email)
                        bool emailExists = await _context.Students.AnyAsync(c => c.Email == model.email && c.Id != model.id);

                        if (emailExists)
                        {
                            return BadRequest("Student email already exists");
                        }

                        Student student = new Student
                        {
                            Id = model.id,
                            StudentCode = model.student_code,
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
                            string imageUrl = await UploadImageAsync(model.avatar);

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
