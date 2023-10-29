﻿using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.Question;
using T2207A_SEM3_API.Models.Test;
using static System.Net.Mime.MediaTypeNames;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly ExamonimyContext _context;

        public TestController(ExamonimyContext context)
        {
            _context = context;
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
                        return BadRequest("Class name already exists");
                    }

                    // kiểm tra số câu hỏi 
                    var result = model.questions.Count();
                    if (result > 16 || result < 16)
                    {
                        return BadRequest("The number of questions is redundant or missing");
                    }

                    // Kiểm tra số câu hỏi
                    int easyCount = model.questions.Count(q => q.level == 1);
                    int mediumCount = model.questions.Count(q => q.level == 2);
                    int hardCount = model.questions.Count(q => q.level == 3);

                    if (easyCount != 6 || mediumCount != 5 || hardCount != 5)
                    {
                        return BadRequest(new { error = "The number of questions is incorrect" });
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
                        return BadRequest("Class name already exists");
                    }

                    // lấy danh sách câu hỏi cho đề thi
                    var selectedQuestions = new List<Question>();

                    // chọn câu hỏi eassy 
                    var randomEasyQuestions = _context.Questions
                        .Where(q => q.Level == 1 && q.QuestionType == 0)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(6) // Lấy 6 câu hỏi
                        .ToList();

                    if (randomEasyQuestions.Count() < 6)
                    {
                        return BadRequest("The number of easy questions is not enough, the exam cannot be created");
                    }

                    // chọn câu hỏi medium 
                    var randomMediumQuestions = _context.Questions
                        .Where(q => q.Level == 2 && q.QuestionType == 0)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(5) // Lấy 5 câu hỏi
                        .ToList();

                    if (randomMediumQuestions.Count() < 5)
                    {
                        return BadRequest("The number of medium questions is not enough, the exam cannot be created");
                    }

                    // chọn câu hỏi hard 
                    var randomHardQuestions = _context.Questions
                        .Where(q => q.Level == 3 && q.QuestionType == 0)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(5) // Lấy 5 câu hỏi
                        .ToList();

                    if (randomHardQuestions.Count() < 5)
                    {
                        return BadRequest("The number of hard questions is not enough, the exam cannot be created");
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
                        NumberOfQuestionsInExam = 16,
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
                        return BadRequest("Class name already exists");
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
                        NumberOfQuestionsInExam = 1,
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
                            Level = questionModel.level,
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

            var msgs = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(msgs);
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
                        return BadRequest("Class name already exists");
                    }

                    // lấy danh sách câu hỏi cho đề thi
                    var selectedQuestions = new List<Question>();


                    // chọn câu hỏi hard 
                    var randomHardQuestions = _context.Questions
                        .Where(q => q.Level == 3 && q.QuestionType == 1)
                        .OrderBy(x => Guid.NewGuid()) // Lấy ngẫu nhiên
                        .Take(1) // Lấy 1 câu hỏi
                        .ToList();

                    if (randomHardQuestions.Count() != 1)
                    {
                        return BadRequest("The number of hard questions is not enough, the exam cannot be created");
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
                        NumberOfQuestionsInExam = 1,
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
                            ExamId = model.exam_id,
                            StartDate = model.startDate,
                            EndDate = model.endDate,
                            PastMarks = model.past_marks,
                            TotalMarks = model.total_marks,
                            CreatedBy = model.created_by,
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
    }
}
