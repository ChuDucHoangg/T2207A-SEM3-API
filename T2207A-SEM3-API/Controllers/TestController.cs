using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Test;

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
                        student_id = t.StudentId,
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
                        student_id = t.StudentId,
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateTest model)
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
                        StudentId = model.student_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks, 
                        TotalMarks = model.total_marks,
                        CreatedBy = model.created_by,
                        Status = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Tests.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new TestDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        exam_id = data.ExamId,
                        student_id = data.StudentId,
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
                            StudentId = model.student_id,
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
                        student_id = c.StudentId,
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
