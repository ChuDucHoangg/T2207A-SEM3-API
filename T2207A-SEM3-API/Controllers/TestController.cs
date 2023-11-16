using Azure.Core;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Helper.Email;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Question;
using T2207A_SEM3_API.Models.Student;
using T2207A_SEM3_API.Models.Test;
using T2207A_SEM3_API.Service.ClassCourses;
using T2207A_SEM3_API.Service.Email;
using static System.Net.Mime.MediaTypeNames;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly ExamonimyContext _context;
        private readonly IEmailService _emailService;

        public TestController(ExamonimyContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Test> tests = await _context.Tests.ToListAsync();

                List<TestDTO> data = new List<TestDTO>();
                foreach (Test t in tests)
                {
                    data.Add(new TestDTO
                    {
                        id = t.Id,
                        name = t.Name,
                        slug = t.Slug,
                        exam_id = t.ExamId,
                        startDate = t.StartDate,
                        endDate = t.EndDate,
                        past_marks = t.PastMarks,
                        total_marks = t.TotalMarks,
                        type_test = t.TypeTest,
                        created_by = t.CreatedBy,
                        status = t.Status,
                        createdAt = t.CreatedAt,
                        updatedAt = t.UpdatedAt,
                        deletedAt = t.DeletedAt
                    });
                }
                return Ok(data);
            } catch (Exception e)
            {
                return BadRequest($"An error occurred: {e.Message}");
            }
            
        }

        [HttpGet]
        [Route("get-by-slug")]
        public async Task<IActionResult> Get(string slug)
        {
            try
            {
                Test t = await _context.Tests.FirstOrDefaultAsync(x => x.Slug == slug);
                if (t != null)
                {
                    return Ok(new TestDTO
                    {
                        id = t.Id,
                        name = t.Name,
                        slug = t.Slug,
                        exam_id = t.ExamId,
                        startDate = t.StartDate,
                        endDate = t.EndDate,
                        past_marks = t.PastMarks,
                        total_marks = t.TotalMarks,
                        created_by = t.CreatedBy,
                        status = t.Status,
                        createdAt = t.CreatedAt,
                        updatedAt = t.UpdatedAt,
                        deletedAt = t.DeletedAt

                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost("multiple-choice-by-excel-file")]
        public async Task<IActionResult> CreateMultipleChoiceTestByExcelFile([FromForm]CreateMultipleChoiceTestByExcelFile model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 0);
                    if (testExists != null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test already exists",
                            Data = ""
                        });
                    }

                    // kiểm tra số câu hỏi 


                    // Kiểm tra số câu hỏi

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var file = model.excelFile;

                    if (file != null || file.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(file.FileName).ToLower(); // Lấy phần mở rộng của tên tệp và chuyển thành chữ thường

                        // Kiểm tra xem tệp có đúng định dạng Excel (ví dụ: .xlsx) hay không
                        if (fileExtension == ".xlsx" || fileExtension == ".xlsx")
                        {
                            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "excels");

                            if (!Directory.Exists(uploadDirectory))
                            {
                                Directory.CreateDirectory(uploadDirectory);
                            }

                            var filePath = Path.Combine(uploadDirectory, file.FileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                            {
                                using (var reader = ExcelReaderFactory.CreateReader(stream))
                                {
                                    int totalQuestions = 0;
                                    int easyCount = 0;
                                    int mediumCount = 0;
                                    int hardCount = 0;
                                    do
                                    {
                                        bool isHeaderSkipped = false;

                                        while (reader.Read())
                                        {
                                            if (!isHeaderSkipped)
                                            {
                                                isHeaderSkipped = true;
                                                continue;
                                            }

                                            int level = Convert.ToInt32(reader.GetValue(6).ToString());  // Assumed column 7 contains the difficulty level
                                                                                                         // Kiểm tra mức độ và tăng đếm cho từng loại
                                            switch (level)
                                            {
                                                case 1:
                                                    easyCount++;
                                                    break;
                                                case 2:
                                                    mediumCount++;
                                                    break;
                                                case 3:
                                                    hardCount++;
                                                    break;
                                                default:  // Xử lý nếu mức độ không hợp lệ
                                                    break;
                                            }  // Tăng tổng số câu hỏi
                                            totalQuestions++;
                                        }
                                    } while (reader.NextResult());
                                    reader.Close();

                                    if (totalQuestions == 16 && easyCount == 6 && mediumCount == 5 && hardCount == 5)
                                    {
                                        Test data = new Test
                                        {
                                            Name = model.name,
                                            Slug = model.name.ToLower().Replace(" ", "-"),
                                            ExamId = model.exam_id,
                                            StartDate = model.startDate,
                                            EndDate = model.endDate,
                                            PastMarks = model.past_marks,
                                            TotalMarks = model.total_marks,
                                            NumberOfQuestionsInExam = 16,
                                            RetakeTestId = null,
                                            TypeTest = 0,
                                            CreatedBy = model.created_by,
                                            Status = 0,
                                            CreatedAt = DateTime.Now,
                                            UpdatedAt = DateTime.Now,
                                            DeletedAt = null,
                                        };
                                        _context.Tests.Add(data);
                                        await _context.SaveChangesAsync();  // tạo danh sách thi
                                        foreach (var studentId in model.studentIds)
                                        {
                                            var studentTest = new StudentTest
                                            {
                                                TestId = data.Id,
                                                StudentId = studentId,
                                                Status = 0,
                                                CreatedAt = DateTime.Now,
                                                UpdatedAt = DateTime.Now,
                                                DeletedAt = null,
                                            };
                                            _context.StudentTests.Add(studentTest);
                                            await _context.SaveChangesAsync();

                                            // danh sách điểm
                                            var grade = new Grade
                                            {
                                                StudentId = studentId,
                                                TestId = data.Id,
                                                Status = 0,
                                                Score = null,
                                                FinishedAt = null,
                                                IsRetake = false,
                                                CreatedAt = DateTime.Now,
                                                UpdatedAt = DateTime.Now,
                                                DeletedAt = null,
                                            };

                                            _context.Grades.Add(grade);
                                            await _context.SaveChangesAsync();
                                        }
                                        int order = 1;
                                        int testId = data.Id;
                                        var courseClassId = _context.Tests.Where(t => t.Id == testId)
                                                                .Select(t => t.Exam.CourseClassId)
                                                                .SingleOrDefault();
                                        var courseId = _context.ClassCourses.Where(cc => cc.Id == courseClassId)
                                                           .Select(cc => cc.CourseId)
                                                           .SingleOrDefault();
                                        using (var newStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                                        {
                                            using (var newReader = ExcelReaderFactory.CreateReader(newStream))
                                            {
                                                do
                                                {
                                                    bool isHeaderSkipped1 = false;

                                                    while (newReader.Read())
                                                    {
                                                        if (!isHeaderSkipped1)
                                                        {
                                                            isHeaderSkipped1 = true;
                                                            continue;
                                                        }

                                                        string questionText = newReader.GetValue(1).ToString();
                                                        int level = Convert.ToInt32(newReader.GetValue(6).ToString());
                                                        var question = new Question
                                                        {
                                                            Title = questionText,
                                                            CourseId = courseId,
                                                            Level = level,
                                                            QuestionType = 0,
                                                            CreatedAt = DateTime.UtcNow,
                                                            UpdatedAt = DateTime.UtcNow,
                                                            DeletedAt = null,
                                                        };  // Thiết lập điểm (score) dựa trên mức độ (level)
                                                        if (level == 1)
                                                        {
                                                            question.Score = 3.85;  // Điểm cho câu dễ
                                                        }
                                                        else if (level == 2)
                                                        {
                                                            question.Score = 6.41;  // Điểm cho câu trung bình
                                                        }
                                                        else if (level == 3)
                                                        {
                                                            question.Score = 8.97;  // Điểm cho câu khó
                                                        }
                                                        else
                                                        {  // Xử lý khi mức độ không xác định, có thể đặt điểm mặc định
                                                           // hoặc thông báo lỗi.
                                                            question.Score = 0.0;  // Điểm mặc định hoặc giá trị khác tùy bạn
                                                        }
                                                        _context.Questions.Add(question);
                                                        await _context.SaveChangesAsync();
                                                        var question_test = new QuestionTest
                                                        {
                                                            TestId = data.Id,
                                                            QuestionId = question.Id,
                                                            Orders = order  // Gán thứ tự cho câu hỏi
                                                        };
                                                        _context.QuestionTests.Add(question_test);
                                                        await _context.SaveChangesAsync();
                                                        string answerCorrect =
                                                            newReader.GetValue(7).ToString();  // Tạo câu trả lời
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            string answerText = newReader.GetValue(2 + i).ToString();
                                                            var answer = new Answer
                                                            {
                                                                Content = answerText,
                                                                QuestionId = question.Id,
                                                                CreatedAt = DateTime.UtcNow,
                                                                UpdatedAt = DateTime.UtcNow,
                                                                DeletedAt = null,
                                                            };  // Đặt câu trả lời đúng
                                                            if (answer.Content.Trim() == answerCorrect.Trim())
                                                            {
                                                                answer.Status = 1;
                                                            }
                                                            else
                                                            {
                                                                answer.Status = 0;
                                                            }
                                                            _context.Answers.Add(answer);
                                                            await _context.SaveChangesAsync();
                                                        }
                                                        order++;

                                                    }
                                                } while (newReader.NextResult());
                                            }
                                        }

                                        return Created($"get-by-id?id={data.Id}", new TestDTO
                                        {
                                            id = data.Id,
                                            name = data.Name,
                                            slug = data.Slug,
                                            exam_id = data.ExamId,
                                            startDate = data.StartDate,
                                            endDate = data.EndDate,
                                            past_marks = data.PastMarks,
                                            total_marks = data.TotalMarks,
                                            type_test = data.TypeTest,
                                            created_by = data.CreatedBy,
                                            status = data.Status,
                                            createdAt = data.CreatedAt,
                                            updatedAt = data.UpdatedAt,
                                            deletedAt = data.DeletedAt
                                        });
                                    }
                                    else
                                    {
                                        return BadRequest(new GeneralServiceResponse
                                        {
                                            Success = false,
                                            StatusCode = 400,
                                            Message = "The number of questions is redundant or missing",
                                            Data = ""
                                        });
                                    }
                                }
                            }

                        }
                        else
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "bad format",
                                Data = ""
                            });
                        }

                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPost("multiple-choice-by-excel-file/retake")]
        public async Task<IActionResult> CreateMultipleChoiceRetakeTestByExcelFile([FromForm] CreateMultipleChoiceRetakeTestByExcelFile model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 0);
                    if (testExists == null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "There is no official test yet",
                            Data = ""
                        });
                    }

                    // lấy ra danh sách học sinh đăng kí thi lại
                    var studentIds = await _context.RegisterExams
                        .Where(re => re.ExamId == model.exam_id && re.Status == 1)// lấy học sinh đã được duyệt
                        .Select(re => re.StudentId)
                        .ToListAsync();

                    if (studentIds == null || !studentIds.Any())
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "No students registered to retake the exam",
                            Data = ""
                        });
                    }


                    // Kiểm tra số câu hỏi

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var file = model.excelFile;

                    if (file != null || file.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(file.FileName).ToLower(); // Lấy phần mở rộng của tên tệp và chuyển thành chữ thường

                        // Kiểm tra xem tệp có đúng định dạng Excel (ví dụ: .xlsx) hay không
                        if (fileExtension == ".xlsx" || fileExtension == ".xlsx")
                        {
                            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "excels");

                            if (!Directory.Exists(uploadDirectory))
                            {
                                Directory.CreateDirectory(uploadDirectory);
                            }

                            var filePath = Path.Combine(uploadDirectory, file.FileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                            {
                                using (var reader = ExcelReaderFactory.CreateReader(stream))
                                {
                                    int totalQuestions = 0;
                                    int easyCount = 0;
                                    int mediumCount = 0;
                                    int hardCount = 0;
                                    do
                                    {
                                        bool isHeaderSkipped = false;

                                        while (reader.Read())
                                        {
                                            if (!isHeaderSkipped)
                                            {
                                                isHeaderSkipped = true;
                                                continue;
                                            }

                                            int level = Convert.ToInt32(reader.GetValue(6).ToString());  // Assumed column 7 contains the difficulty level
                                                                                                         // Kiểm tra mức độ và tăng đếm cho từng loại
                                            switch (level)
                                            {
                                                case 1:
                                                    easyCount++;
                                                    break;
                                                case 2:
                                                    mediumCount++;
                                                    break;
                                                case 3:
                                                    hardCount++;
                                                    break;
                                                default:  // Xử lý nếu mức độ không hợp lệ
                                                    break;
                                            }  // Tăng tổng số câu hỏi
                                            totalQuestions++;
                                        }
                                    } while (reader.NextResult());
                                    reader.Close();

                                    if (totalQuestions == 16 && easyCount == 6 && mediumCount == 5 && hardCount == 5)
                                    {
                                        Test data = new Test
                                        {
                                            Name = model.name,
                                            Slug = model.name.ToLower().Replace(" ", "-"),
                                            ExamId = model.exam_id,
                                            StartDate = model.startDate,
                                            EndDate = model.endDate,
                                            PastMarks = model.past_marks,
                                            TotalMarks = model.total_marks,
                                            NumberOfQuestionsInExam = 16,
                                            RetakeTestId = testExists.Id,
                                            TypeTest = 0,
                                            CreatedBy = model.created_by,
                                            Status = 0,
                                            CreatedAt = DateTime.Now,
                                            UpdatedAt = DateTime.Now,
                                            DeletedAt = null,
                                        };
                                        _context.Tests.Add(data);
                                        await _context.SaveChangesAsync();  // tạo danh sách thi
                                        foreach (var studentId in studentIds)
                                        {
                                            var studentTest = new StudentTest
                                            {
                                                TestId = data.Id,
                                                StudentId = studentId,
                                                Status = 0,
                                                CreatedAt = DateTime.Now,
                                                UpdatedAt = DateTime.Now,
                                                DeletedAt = null,
                                            };
                                            _context.StudentTests.Add(studentTest);
                                            await _context.SaveChangesAsync();

                                            // danh sách điểm
                                            var grade = new Grade
                                            {
                                                StudentId = studentId,
                                                TestId = data.Id,
                                                Status = 0,
                                                Score = null,
                                                FinishedAt = null,
                                                IsRetake = true,
                                                CreatedAt = DateTime.Now,
                                                UpdatedAt = DateTime.Now,
                                                DeletedAt = null,
                                            };

                                            _context.Grades.Add(grade);
                                            await _context.SaveChangesAsync();

                                            // chuyển đổi trạng thái đăng kí thi lại
                                            var registerExam = await _context.RegisterExams.FirstOrDefaultAsync(r => r.StudentId == studentId && r.ExamId == model.exam_id);

                                            registerExam.Status = 2;
                                            _context.RegisterExams.Update(registerExam);
                                            await _context.SaveChangesAsync();
                                        }
                                        int order = 1;
                                        int testId = data.Id;
                                        var courseClassId = _context.Tests.Where(t => t.Id == testId)
                                                                .Select(t => t.Exam.CourseClassId)
                                                                .SingleOrDefault();
                                        var courseId = _context.ClassCourses.Where(cc => cc.Id == courseClassId)
                                                           .Select(cc => cc.CourseId)
                                                           .SingleOrDefault();
                                        using (var newStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                                        {
                                            using (var newReader = ExcelReaderFactory.CreateReader(newStream))
                                            {
                                                do
                                                {
                                                    bool isHeaderSkipped1 = false;

                                                    while (newReader.Read())
                                                    {
                                                        if (!isHeaderSkipped1)
                                                        {
                                                            isHeaderSkipped1 = true;
                                                            continue;
                                                        }

                                                        string questionText = newReader.GetValue(1).ToString();
                                                        int level = Convert.ToInt32(newReader.GetValue(6).ToString());
                                                        var question = new Question
                                                        {
                                                            Title = questionText,
                                                            CourseId = courseId,
                                                            Level = level,
                                                            QuestionType = 0,
                                                            CreatedAt = DateTime.UtcNow,
                                                            UpdatedAt = DateTime.UtcNow,
                                                            DeletedAt = null,
                                                        };  // Thiết lập điểm (score) dựa trên mức độ (level)
                                                        if (level == 1)
                                                        {
                                                            question.Score = 3.85;  // Điểm cho câu dễ
                                                        }
                                                        else if (level == 2)
                                                        {
                                                            question.Score = 6.41;  // Điểm cho câu trung bình
                                                        }
                                                        else if (level == 3)
                                                        {
                                                            question.Score = 8.97;  // Điểm cho câu khó
                                                        }
                                                        else
                                                        {  // Xử lý khi mức độ không xác định, có thể đặt điểm mặc định
                                                           // hoặc thông báo lỗi.
                                                            question.Score = 0.0;  // Điểm mặc định hoặc giá trị khác tùy bạn
                                                        }
                                                        _context.Questions.Add(question);
                                                        await _context.SaveChangesAsync();
                                                        var question_test = new QuestionTest
                                                        {
                                                            TestId = data.Id,
                                                            QuestionId = question.Id,
                                                            Orders = order  // Gán thứ tự cho câu hỏi
                                                        };
                                                        _context.QuestionTests.Add(question_test);
                                                        await _context.SaveChangesAsync();
                                                        string answerCorrect =
                                                            newReader.GetValue(7).ToString();  // Tạo câu trả lời
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            string answerText = newReader.GetValue(2 + i).ToString();
                                                            var answer = new Answer
                                                            {
                                                                Content = answerText,
                                                                QuestionId = question.Id,
                                                                CreatedAt = DateTime.UtcNow,
                                                                UpdatedAt = DateTime.UtcNow,
                                                                DeletedAt = null,
                                                            };  // Đặt câu trả lời đúng
                                                            if (answer.Content.Trim() == answerCorrect.Trim())
                                                            {
                                                                answer.Status = 1;
                                                            }
                                                            else
                                                            {
                                                                answer.Status = 0;
                                                            }
                                                            _context.Answers.Add(answer);
                                                            await _context.SaveChangesAsync();
                                                        }
                                                        order++;

                                                    }
                                                } while (newReader.NextResult());
                                            }
                                        }

                                        return Created($"get-by-id?id={data.Id}", new TestDTO
                                        {
                                            id = data.Id,
                                            name = data.Name,
                                            slug = data.Slug,
                                            exam_id = data.ExamId,
                                            startDate = data.StartDate,
                                            endDate = data.EndDate,
                                            past_marks = data.PastMarks,
                                            total_marks = data.TotalMarks,
                                            type_test = data.TypeTest,
                                            created_by = data.CreatedBy,
                                            status = data.Status,
                                            createdAt = data.CreatedAt,
                                            updatedAt = data.UpdatedAt,
                                            deletedAt = data.DeletedAt
                                        });
                                    }
                                    else
                                    {
                                        return BadRequest(new GeneralServiceResponse
                                        {
                                            Success = false,
                                            StatusCode = 400,
                                            Message = "The number of questions is redundant or missing",
                                            Data = ""
                                        });
                                    }
                                }
                            }

                        }
                        else
                        {
                            return BadRequest(new GeneralServiceResponse
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "bad format",
                                Data = ""
                            });
                        }

                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPost("multiple-choice-by-hand")]
        public async Task<IActionResult> CreateMultipleChoiceTestByHand(CreateMultipleChoiceTestByHand model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 0);
                    if (testExists != null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test already exists",
                            Data = ""
                        });
                    }

                    // kiểm tra số câu hỏi 
                    var result = model.questions.Count();
                    if (result > 16 || result < 16)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of questions is redundant or missing",
                            Data = ""
                        });
                    }

                    // Kiểm tra số câu hỏi
                    int easyCount = model.questions.Count(q => q.level == 1);
                    int mediumCount = model.questions.Count(q => q.level == 2);
                    int hardCount = model.questions.Count(q => q.level == 3);

                    if (easyCount != 6 || mediumCount != 5 || hardCount != 5)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of questions is incorrect",
                            Data = ""
                        });
                    }


                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks, 
                        TotalMarks = model.total_marks,
                        NumberOfQuestionsInExam = 16,
                        TypeTest = 0,
                        RetakeTestId = null,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in model.studentIds)
                    {
                        // danh sách học sinh
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                    }

                    int order = 1;
                    int testId = data.Id;
                    var courseClassId = _context.Tests
                        .Where(t => t.Id == testId)
                        .Select(t => t.Exam.CourseClassId)
                        .SingleOrDefault();

                    var courseId = _context.ClassCourses
                        .Where(cc => cc.Id == courseClassId)
                        .Select(cc => cc.CourseId)
                        .SingleOrDefault();

                    // tạo câu hỏi và trả lời
                    foreach (var questionModel in model.questions)
                    {
                        var question = new Question
                        {
                            Title = questionModel.title,
                            Level = questionModel.level,
                            QuestionType = 0,
                            CourseId = courseId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            DeletedAt = null,
                        };
                        // Thiết lập điểm (score) dựa trên mức độ (level)
                        if (questionModel.level == 1)
                        {
                            question.Score = 3.85; // Điểm cho câu dễ
                        }
                        else if (questionModel.level == 2)
                        {
                            question.Score = 6.41; // Điểm cho câu trung bình
                        }
                        else if (questionModel.level == 3)
                        {
                            question.Score = 8.97; // Điểm cho câu khó
                        }
                        else
                        {
                            // Xử lý khi mức độ không xác định, có thể đặt điểm mặc định hoặc thông báo lỗi.
                            question.Score = 0.0; // Điểm mặc định hoặc giá trị khác tùy bạn
                        }

                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync();


                        var question_test = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = order // Gán thứ tự cho câu hỏi
                        };

                        _context.QuestionTests.Add(question_test);
                        await _context.SaveChangesAsync();

                        foreach (var answerModel in questionModel.answers)
                        {
                            var answer = new Answer
                            {
                                Content = answerModel.content,
                                Status = answerModel.status,
                                QuestionId = question.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                DeletedAt = null,
                            };

                            _context.Answers.Add(answer);
                            await _context.SaveChangesAsync();
                        }
                        // Tăng thứ tự cho câu hỏi tiếp theo
                        order++;
                    }

                    await _context.SaveChangesAsync();


                    /*var exam = await _context.Exams.FindAsync(model.exam_id);
                    
                    var classCourse = await _context.ClassCourses.FindAsync(exam.CourseClassId);

                    var course = await _context.Courses.FindAsync(classCourse.CourseId);

                    if (classCourse == null)
                    {
                        return NotFound("ClassCourse not found for the specified exam.");
                    }

                    List<StudentListOfEmail> students = new List<StudentListOfEmail>();

                    var studentlist = await _context.Students
                        .Where(s => model.studentIds.Contains(s.Id))
                        .ToListAsync();

                    foreach(var student in studentlist)
                    {
                        var st = new StudentListOfEmail
                        {
                            id = student.Id,
                            student_code = student.StudentCode,
                            fullname = student.Fullname,
                        };
                        students.Add(st);
                    }

                    Mailrequest mailrequest = new Mailrequest();
                    mailrequest.ToEmail = "trungtvt.dev@gmail.com";
                    mailrequest.Subject = "Welcome to Examonimy";
                    mailrequest.Body = EmailContentRegister.GetHtmlcontentTestMultipleChoice(course.Name, data.Name, data.StartDate, data.EndDate, data.PastMarks, students);

                    await _emailService.SendEmailAsync(mailrequest);*/


                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
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

        [HttpPost("multiple-choice-by-hand/retake")]
        public async Task<IActionResult> CreateMultipleChoiceRetakeTestByHand(CreateMultipleChoiceRetakeTestByHand model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 0);
                    if (testExists == null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "There is no official test yet",
                            Data = ""
                        });
                    }

                    // lấy ra danh sách học sinh đăng kí thi lại
                    var studentIds = await _context.RegisterExams
                        .Where(re => re.ExamId == model.exam_id && re.Status == 1)// lấy học sinh đã được duyệt
                        .Select(re => re.StudentId)
                        .ToListAsync();

                    if (studentIds == null || !studentIds.Any())
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "No students registered to retake the exam",
                            Data = ""
                        });
                    }

                    // kiểm tra số câu hỏi 
                    var result = model.questions.Count();
                    if (result > 16 || result < 16)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of questions is redundant or missing",
                            Data = ""
                        });
                    }

                    // Kiểm tra số câu hỏi
                    int easyCount = model.questions.Count(q => q.level == 1);
                    int mediumCount = model.questions.Count(q => q.level == 2);
                    int hardCount = model.questions.Count(q => q.level == 3);

                    if (easyCount != 6 || mediumCount != 5 || hardCount != 5)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of questions is incorrect",
                            Data = ""
                        });
                    }


                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        NumberOfQuestionsInExam = 16,
                        TypeTest = 0,
                        RetakeTestId = testExists.Id,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in studentIds)
                    {
                        // danh sách học sinh
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                        // chuyển đổi trạng thái đăng kí thi lại
                        var registerExam = await _context.RegisterExams.FirstOrDefaultAsync(r => r.StudentId == studentId && r.ExamId == model.exam_id);

                        registerExam.Status = 2;
                        _context.RegisterExams.Update(registerExam);
                        await _context.SaveChangesAsync();

                    }

                    int order = 1;
                    int testId = data.Id;
                    var courseClassId = _context.Tests
                        .Where(t => t.Id == testId)
                        .Select(t => t.Exam.CourseClassId)
                        .SingleOrDefault();

                    var courseId = _context.ClassCourses
                        .Where(cc => cc.Id == courseClassId)
                        .Select(cc => cc.CourseId)
                        .SingleOrDefault();

                    // tạo câu hỏi và trả lời
                    foreach (var questionModel in model.questions)
                    {
                        var question = new Question
                        {
                            Title = questionModel.title,
                            Level = questionModel.level,
                            QuestionType = 0,
                            CourseId = courseId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            DeletedAt = null,
                        };
                        // Thiết lập điểm (score) dựa trên mức độ (level)
                        if (questionModel.level == 1)
                        {
                            question.Score = 3.85; // Điểm cho câu dễ
                        }
                        else if (questionModel.level == 2)
                        {
                            question.Score = 6.41; // Điểm cho câu trung bình
                        }
                        else if (questionModel.level == 3)
                        {
                            question.Score = 8.97; // Điểm cho câu khó
                        }
                        else
                        {
                            // Xử lý khi mức độ không xác định, có thể đặt điểm mặc định hoặc thông báo lỗi.
                            question.Score = 0.0; // Điểm mặc định hoặc giá trị khác tùy bạn
                        }

                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync();


                        var question_test = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = order // Gán thứ tự cho câu hỏi
                        };

                        _context.QuestionTests.Add(question_test);
                        await _context.SaveChangesAsync();

                        foreach (var answerModel in questionModel.answers)
                        {
                            var answer = new Answer
                            {
                                Content = answerModel.content,
                                Status = answerModel.status,
                                QuestionId = question.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                DeletedAt = null,
                            };

                            _context.Answers.Add(answer);
                            await _context.SaveChangesAsync();
                        }
                        // Tăng thứ tự cho câu hỏi tiếp theo
                        order++;
                    }

                    await _context.SaveChangesAsync();


                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
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

        [HttpPost("multiple-choice-by-auto")]
        public async Task<IActionResult> CreateMultipleChoiceTestByAuto(CreateMultipleChoiceTestByAuto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null);
                    if (testExists != null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test already exists",
                            Data = ""
                        });
                    }

                    var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == model.exam_id);
                    if (exam == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }
                    var courseClass = await _context.ClassCourses.FirstOrDefaultAsync(e => e.Id == exam.CourseClassId);

                    if (courseClass == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }

                    // lấy danh sách câu hỏi cho đề thi
                    var selectedQuestions = new List<Question>();

                    // chọn câu hỏi eassy 
                    var randomEasyQuestions = _context.Questions
                        .Where(q => q.Level == 1 && q.QuestionType == 0 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(6) // Lấy 6 câu hỏi
                        .ToList();

                    if (randomEasyQuestions.Count() < 6)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of easy questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    // chọn câu hỏi medium 
                    var randomMediumQuestions = _context.Questions
                        .Where(q => q.Level == 2 && q.QuestionType == 0 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(5) // Lấy 5 câu hỏi
                        .ToList();

                    if (randomMediumQuestions.Count() < 5)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of easy questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    // chọn câu hỏi hard 
                    var randomHardQuestions = _context.Questions
                        .Where(q => q.Level == 3 && q.QuestionType == 0 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(5) // Lấy 5 câu hỏi
                        .ToList();

                    if (randomHardQuestions.Count() < 5)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of easy questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    selectedQuestions.AddRange(randomEasyQuestions);
                    selectedQuestions.AddRange(randomMediumQuestions);
                    selectedQuestions.AddRange(randomHardQuestions);


                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        TypeTest = 0,
                        NumberOfQuestionsInExam = 16,
                        RetakeTestId = null,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in model.studentIds)
                    {
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                    }
                    int order = 1;

                    foreach (var question in selectedQuestions)
                    {
                        var questionTest = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = order
                        };

                        _context.QuestionTests.Add(questionTest);
                        await _context.SaveChangesAsync();

                        order++;
                    }

                    var classCourse = await _context.ClassCourses.FindAsync(exam.CourseClassId);

                    var course = await _context.Courses.FindAsync(classCourse.CourseId);

                    if (classCourse == null)
                    {
                        return NotFound("ClassCourse not found for the specified exam.");
                    }

                    List<StudentListOfEmail> students = new List<StudentListOfEmail>();

                    var studentlist = await _context.Students
                        .Where(s => model.studentIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var student in studentlist)
                    {
                        var st = new StudentListOfEmail
                        {
                            id = student.Id,
                            student_code = student.StudentCode,
                            fullname = student.Fullname,
                        };
                        students.Add(st);
                    }

                    Mailrequest mailrequest = new Mailrequest();
                    mailrequest.ToEmail = "trungtvt.dev@gmail.com";
                    mailrequest.Subject = "Welcome to Examonimy";
                    mailrequest.Body = EmailContentRegister.GetHtmlcontentTestMultipleChoice(course.Name, data.Name, data.StartDate, data.EndDate, data.PastMarks, students);

                    await _emailService.SendEmailAsync(mailrequest);


                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
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

        [HttpPost("multiple-choice-by-auto/retake")]
        public async Task<IActionResult> CreateMultipleChoiceRetakeTestByAuto(CreateMultipleChoiceRetakeTestByAuto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    var testMain = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 0);
                    if (testMain == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "There is no official test yet",
                            Data = ""
                        });
                    }

                    // lấy ra danh sách học sinh đăng kí thi lại
                    var studentIds = await _context.RegisterExams
                        .Where(re => re.ExamId == model.exam_id && re.Status == 1)// lấy học sinh đã được duyệt
                        .Select(re => re.StudentId)
                        .ToListAsync();

                    if (studentIds == null || !studentIds.Any())
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "No students registered to retake the exam",
                            Data = ""
                        });
                    }

                    var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == model.exam_id);
                    if (exam == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }
                    var courseClass = await _context.ClassCourses.FirstOrDefaultAsync(e => e.Id == exam.CourseClassId);

                    if (courseClass == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }

                    // lấy danh sách câu hỏi cho đề thi
                    var selectedQuestions = new List<Question>();

                    // chọn câu hỏi eassy 
                    var randomEasyQuestions = _context.Questions
                        .Where(q => q.Level == 1 && q.QuestionType == 0 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(6) // Lấy 6 câu hỏi
                        .ToList();

                    if (randomEasyQuestions.Count() < 6)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of easy questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    // chọn câu hỏi medium 
                    var randomMediumQuestions = _context.Questions
                        .Where(q => q.Level == 2 && q.QuestionType == 0 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(5) // Lấy 5 câu hỏi
                        .ToList();

                    if (randomMediumQuestions.Count() < 5)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of easy questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    // chọn câu hỏi hard 
                    var randomHardQuestions = _context.Questions
                        .Where(q => q.Level == 3 && q.QuestionType == 0 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(5) // Lấy 5 câu hỏi
                        .ToList();

                    if (randomHardQuestions.Count() < 5)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of easy questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    selectedQuestions.AddRange(randomEasyQuestions);
                    selectedQuestions.AddRange(randomMediumQuestions);
                    selectedQuestions.AddRange(randomHardQuestions);


                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        TypeTest = 0,
                        NumberOfQuestionsInExam = 16,
                        RetakeTestId = testMain.Id,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    

                    // tạo danh sách thi
                    foreach (var studentId in studentIds)
                    {
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                        // chuyển đổi trạng thái đăng kí thi lại
                        var registerExam = await _context.RegisterExams.FirstOrDefaultAsync(r => r.StudentId == studentId && r.ExamId == model.exam_id);

                        registerExam.Status = 2;
                        _context.RegisterExams.Update(registerExam);
                        await _context.SaveChangesAsync();

                    }
                    int order = 1;

                    foreach (var question in selectedQuestions)
                    {
                        var questionTest = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = order
                        };

                        _context.QuestionTests.Add(questionTest);
                        await _context.SaveChangesAsync();

                        order++;
                    }


                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }
            var msgs = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);
            return BadRequest(string.Join(" | ", msgs));
        }

        [HttpPost("essay-by-hand")]
        public async Task<IActionResult> CreateEssayTestByHand(CreateEssayTestByHand model)
        { 
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }
                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 1);
                    if (testExists != null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test already exists",
                            Data = ""
                        });
                    }

                    // kiểm tra số câu hỏi 
                    var result = model.questions.Count();
                    if (result > 1 || result < 1)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of questions is redundant or missing",
                            Data = ""
                        });
                    }

                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        TypeTest = 1,
                        NumberOfQuestionsInExam = 1,
                        RetakeTestId = null,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in model.studentIds)
                    {
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();
                    }

                    int testId = data.Id;
                    var courseClassId = _context.Tests
                        .Where(t => t.Id == testId)
                        .Select(t => t.Exam.CourseClassId)
                        .SingleOrDefault();

                    var courseId = _context.ClassCourses
                        .Where(cc => cc.Id == courseClassId)
                        .Select(cc => cc.CourseId)
                        .SingleOrDefault();

                    foreach (var questionModel in model.questions)
                    {
                        var question = new Question
                        {
                            Title = questionModel.title,
                            Level = 3,
                            QuestionType = 1,
                            CourseId = courseId,
                            Score = 100,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            DeletedAt = null,
                        };

                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync();

                        var question_test = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = 1 // Gán thứ tự cho câu hỏi
                        };

                        
                        _context.QuestionTests.Add(question_test);
                        await _context.SaveChangesAsync();
                    }

                    var exam = await _context.Exams.FindAsync(model.exam_id);

                    var classCourse = await _context.ClassCourses.FindAsync(exam.CourseClassId);

                    var course = await _context.Courses.FindAsync(classCourse.CourseId);

                    if (classCourse == null)
                    {
                        return NotFound("ClassCourse not found for the specified exam.");
                    }

                    List<StudentListOfEmail> students = new List<StudentListOfEmail>();

                    var studentlist = await _context.Students
                        .Where(s => model.studentIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var student in studentlist)
                    {
                        var st = new StudentListOfEmail
                        {
                            id = student.Id,
                            student_code = student.StudentCode,
                            fullname = student.Fullname,
                        };
                        students.Add(st);
                    }

                    Mailrequest mailrequest = new Mailrequest();
                    mailrequest.ToEmail = "trungtvt.dev@gmail.com";
                    mailrequest.Subject = "Welcome to Examonimy";
                    mailrequest.Body = EmailContentRegister.GetHtmlcontentTestMultipleChoice(course.Name, data.Name, data.StartDate, data.EndDate, data.PastMarks, students);

                    await _emailService.SendEmailAsync(mailrequest);

                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }

                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }

            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPost("essay-by-hand/retake")]
        public async Task<IActionResult> CreateEssayRetakeTestByHand(CreateEssayRetakeTestByHand model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test name already exists",
                            Data = ""
                        });
                    }

                    // kiểm tra số câu hỏi 
                    var result = model.questions.Count();
                    if (result > 1 || result < 1)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of questions is redundant or missing",
                            Data = ""
                        });
                    }

                    var testMain = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 1);
                    if (testMain == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "There is no official test yet",
                            Data = ""
                        });
                    }

                    // lấy ra danh sách học sinh đăng kí thi lại
                    var studentIds = await _context.RegisterExams
                        .Where(re => re.ExamId == model.exam_id && re.Status == 1)// lấy học sinh đã được duyệt
                        .Select(re => re.StudentId)
                        .ToListAsync();

                    if (studentIds == null || !studentIds.Any())
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "No students registered to retake the exam",
                            Data = ""
                        });
                    }

                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        TypeTest = 1,
                        NumberOfQuestionsInExam = 1,
                        RetakeTestId = testMain.Id,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in studentIds)
                    {
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                        // chuyển đổi trạng thái đăng kí thi lại
                        var registerExam = await _context.RegisterExams.FirstOrDefaultAsync(r => r.StudentId == studentId && r.ExamId == model.exam_id);

                        registerExam.Status = 2;
                        _context.RegisterExams.Update(registerExam);
                        await _context.SaveChangesAsync();
                    }

                    int testId = data.Id;
                    var courseClassId = _context.Tests
                        .Where(t => t.Id == testId)
                        .Select(t => t.Exam.CourseClassId)
                        .SingleOrDefault();

                    var courseId = _context.ClassCourses
                        .Where(cc => cc.Id == courseClassId)
                        .Select(cc => cc.CourseId)
                        .SingleOrDefault();

                    foreach (var questionModel in model.questions)
                    {
                        var question = new Question
                        {
                            Title = questionModel.title,
                            Level = 3,
                            QuestionType = 1,
                            CourseId = courseId,
                            Score = 100,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            DeletedAt = null,
                        };

                        _context.Questions.Add(question);
                        await _context.SaveChangesAsync();

                        var question_test = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = 1 // Gán thứ tự cho câu hỏi
                        };


                        _context.QuestionTests.Add(question_test);
                        await _context.SaveChangesAsync();
                    }

                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }

                catch (Exception ex)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    });
                }
            }

            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPost("essay-by-auto")]
        public async Task<IActionResult> CreateEssayTestByAuto(CreateEssayTestByAuto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse { Success = false, StatusCode = 400, Message = "Class name already exists", Data = "" });
                    }

                    // chỉ có 1 bài test chính còn lại là test thi lại
                    var testExists = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 1);
                    if (testExists != null)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Test already exists",
                            Data = ""
                        });
                    }
                    //

                    var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == model.exam_id);
                    if (exam == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }
                    var courseClass = await _context.ClassCourses.FirstOrDefaultAsync(e => e.Id == exam.CourseClassId);

                    if (courseClass == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }

                    // lấy danh sách câu hỏi cho đề thi
                    var selectedQuestions = new List<Question>();


                    // chọn câu hỏi hard 
                    var randomHardQuestions = _context.Questions
                        .Where(q => q.Level == 3 && q.QuestionType == 1 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(1) // Lấy 1 câu hỏi
                        .ToList();

                    if (randomHardQuestions.Count() != 1)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of hard questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    selectedQuestions.AddRange(randomHardQuestions);


                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        TypeTest = 1,
                        NumberOfQuestionsInExam = 1,
                        RetakeTestId = null,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in model.studentIds)
                    {
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                    }

                    foreach (var question in selectedQuestions)
                    {
                        var questionTest = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = 1
                        };

                        _context.QuestionTests.Add(questionTest);
                        await _context.SaveChangesAsync();

                    }


                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }
                catch (Exception ex)
                {
                    new GeneralServiceResponse { Success = false, StatusCode = 400, Message = ex.Message, Data = "" };
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPost("essay-by-auto/retake")]
        public async Task<IActionResult> CreateEssayRetakeTestByAuto(CreateEssayRetakeTestByAuto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest(new GeneralServiceResponse { Success = false, StatusCode = 400, Message = "Class name already exists", Data = "" });
                    }

                    var testMain = await _context.Tests.FirstOrDefaultAsync(t => t.ExamId == model.exam_id && t.RetakeTestId == null && t.TypeTest == 1);
                    if (testMain == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "There is no official test yet",
                            Data = ""
                        });
                    }

                    // lấy ra danh sách học sinh đăng kí thi lại
                    var studentIds = await _context.RegisterExams
                        .Where(re => re.ExamId == model.exam_id && re.Status == 1)// lấy học sinh đã được duyệt
                        .Select(re => re.StudentId)
                        .ToListAsync();

                    if (studentIds == null || !studentIds.Any())
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "No students registered to retake the exam",
                            Data = ""
                        });
                    }

                    var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == model.exam_id);
                    if (exam == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }
                    var courseClass = await _context.ClassCourses.FirstOrDefaultAsync(e => e.Id == exam.CourseClassId);

                    if (courseClass == null)
                    {
                        return NotFound(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Not Found",
                            Data = ""
                        });
                    }

                    // lấy danh sách câu hỏi cho đề thi
                    var selectedQuestions = new List<Question>();


                    // chọn câu hỏi hard 
                    var randomHardQuestions = _context.Questions
                        .Where(q => q.Level == 3 && q.QuestionType == 1 && q.CourseId == courseClass.CourseId)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(1) // Lấy 1 câu hỏi
                        .ToList();

                    if (randomHardQuestions.Count() != 1)
                    {
                        return BadRequest(new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "The number of hard questions is not enough, the exam cannot be created",
                            Data = ""
                        });
                    }

                    selectedQuestions.AddRange(randomHardQuestions);


                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        ExamId = model.exam_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        TypeTest = 1,
                        NumberOfQuestionsInExam = 1,
                        RetakeTestId = testMain.Id,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();

                    // tạo danh sách thi
                    foreach (var studentId in studentIds)
                    {
                        var studentTest = new StudentTest
                        {
                            TestId = data.Id,
                            StudentId = studentId,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.StudentTests.Add(studentTest);
                        await _context.SaveChangesAsync();

                        // danh sách điểm
                        var grade = new Grade
                        {
                            StudentId = studentId,
                            TestId = data.Id,
                            Status = 0,
                            Score = null,
                            FinishedAt = null,
                            IsRetake = true,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        _context.Grades.Add(grade);
                        await _context.SaveChangesAsync();

                        // chuyển đổi trạng thái đăng kí thi lại
                        var registerExam = await _context.RegisterExams.FirstOrDefaultAsync(r => r.StudentId == studentId && r.ExamId == model.exam_id);

                        registerExam.Status = 2;
                        _context.RegisterExams.Update(registerExam);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var question in selectedQuestions)
                    {
                        var questionTest = new QuestionTest
                        {
                            TestId = data.Id,
                            QuestionId = question.Id,
                            Orders = 1
                        };

                        _context.QuestionTests.Add(questionTest);
                        await _context.SaveChangesAsync();

                    }


                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        startDate = data.StartDate,
                        endDate = data.EndDate,
                        past_marks = data.PastMarks,
                        total_marks = data.TotalMarks,
                        type_test = data.TypeTest,
                        created_by = data.CreatedBy,
                        status = data.Status,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }
                catch (Exception ex)
                {
                    new GeneralServiceResponse { Success = false, StatusCode = 400, Message = ex.Message, Data = "" };
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Update(EditTest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cùng name)
                    bool nameExists = await _context.Tests.AnyAsync(c => c.Name == model.name && c.Id != model.id);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Class name already exists");
                    }

                    Test existingTest = _context.Tests.AsNoTracking().FirstOrDefault(e => e.Id == model.id);
                    if (existingTest != null)
                    {
                        Test test = new Test
                        {
                            Id = model.id,
                            Name = model.name,
                            Slug = model.name.ToLower().Replace(" ", "-"),
                            ExamId = existingTest.ExamId,
                            StartDate = model.startDate,
                            EndDate = model.endDate,
                            PastMarks = model.past_marks,
                            TotalMarks = existingTest.TotalMarks,
                            CreatedBy = existingTest.CreatedBy,
                            Status = existingTest.Status,
                            CreatedAt = existingTest.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (test != null)
                        {
                            _context.Tests.Update(test);
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Test test = await _context.Tests.FindAsync(id);
                if (test == null)
                    return NotFound();
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-examId")]
        public async Task<IActionResult> GetbyCategory(int examId)
        {
            try
            {
                List<Test> tests = await _context.Tests.Where(p => p.ExamId == examId).ToListAsync();
                if (tests != null)
                {
                    List<TestDTO> data = tests.Select(c => new TestDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        exam_id = c.ExamId,
                        startDate = c.StartDate,
                        endDate = c.EndDate,
                        past_marks = c.PastMarks,
                        total_marks = c.TotalMarks,
                        created_by = c.CreatedBy,
                        status = c.Status,
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

        [HttpGet]
        [Route("get-by-exam/{slug_exam}")]
        [Authorize]
        public async Task<IActionResult> GetTestByStudentId(string slug_exam)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized("Not Authorized");
            }
            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = _context.Students.Find(Convert.ToInt32(userId));
                if (user == null)
                {
                    return Unauthorized("Not Authorized");
                }

                var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Slug == slug_exam);
                if (exam == null)
                {
                    return NotFound("No tests found");
                }

                // Lấy danh sách ID của các test
                var testIds = await _context.StudentTests
                    .Where(qt => qt.StudentId == user.Id)
                    .Select(qt => qt.TestId)
                    .ToListAsync();

                // Lấy danh sách câu hỏi dựa trên các ID test
                var tests = new List<Test>();
                foreach (var item in testIds)
                {
                    var test = await _context.Tests
                        .Where(q => q.Id == item && q.ExamId == exam.Id)
                        .FirstOrDefaultAsync();

                    if (test != null)
                    {
                        tests.Add(test);
                    }
                }

                if (tests.Count() != 0)
                {
                    List<TestDTO> data = tests.Select(c => new TestDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        exam_id = c.ExamId,
                        startDate = c.StartDate,
                        endDate = c.EndDate,
                        past_marks = c.PastMarks,
                        total_marks = c.TotalMarks,
                        type_test = c.TypeTest,
                        created_by = c.CreatedBy,
                        status = c.Status,
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt,
                        deletedAt = c.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No tests found");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPut]
        [Route("lock-test/{test_slug}")]
        [Authorize("Super Admin, Staff")]
        public async Task<IActionResult> LockTest(string test_slug)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = ""
                });
            }

            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Staffs
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                var testExist = await _context.Tests.FirstOrDefaultAsync(t => t.Slug == test_slug);
                if (testExist == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not Found",
                        Data = ""
                    });
                }

                // Kiểm tra thời gian bài thi
                DateTime currentTime = DateTime.Now;
                if (currentTime < testExist.EndDate)
                {
                    return BadRequest(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "The test has ended or has not started yet",
                        Data = ""
                    });
                }

                testExist.Status = 1;
                _context.Tests.Update(testExist);
                await _context.SaveChangesAsync();

                // lấy điểm của học sinh chưa nộp bài 
                List<Grade> grades = await _context.Grades.Where(g => g.TestId == testExist.Id && g.FinishedAt == null).ToListAsync();
                if (grades.Count > 0)
                {
                    foreach (var item in grades)
                    {
                        item.FinishedAt = DateTime.Now;
                        item.Score = 0;
                    }
                    _context.Grades.UpdateRange(grades);
                    await _context.SaveChangesAsync();
                }

                // lấy danh sách thi học sinh
                List<StudentTest> sttudentTests = await _context.StudentTests.Where(st => st.TestId == testExist.Id && st.Status == 0).ToListAsync();
                if (sttudentTests.Count > 0)
                {
                    foreach (var item in sttudentTests)
                    {
                        item.Status = 2;
                    }
                    _context.StudentTests.UpdateRange(sttudentTests);
                    await _context.SaveChangesAsync();
                }


                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Exam locked",
                    Data = ""
                });
            }
            catch (Exception e)
            {
                return NotFound(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = e.Message,
                    Data = ""
                });
            }
        }

        [HttpPut]
        [Route("unlock-test/{test_slug}")]
        [Authorize("Super Admin, Staff")]
        public async Task<IActionResult> UnLockTest(string test_slug)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (!identity.IsAuthenticated)
            {
                return Unauthorized(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 401,
                    Message = "Not Authorized",
                    Data = ""
                });
            }

            try
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _context.Staffs
                    .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(userId));

                if (user == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 401,
                        Message = "Not Authorized",
                        Data = ""
                    });
                }

                var testExist = await _context.Tests.FirstOrDefaultAsync(t => t.Slug == test_slug);
                if (testExist == null)
                {
                    return NotFound(new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "Not Found",
                        Data = ""
                    });
                }

                testExist.Status = 0;
                _context.Tests.Update(testExist);
                await _context.SaveChangesAsync();

                return Ok(new GeneralServiceResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Exam locked",
                    Data = ""
                });
            }
            catch (Exception e)
            {
                return NotFound(new GeneralServiceResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = e.Message,
                    Data = ""
                });
            }
        }
    }
}
