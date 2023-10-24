using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Exam;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/exam")]
    [ApiController]
    public class ExamController : Controller
    {
        private readonly ExamonimyContext _context;

        public ExamController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Exam> exams = await _context.Exams.ToListAsync();

                List<ExamDTO> data = new List<ExamDTO>();
                foreach (Exam e in exams)
                {
                    data.Add(new ExamDTO
                    {
                        id = e.Id,
                        name = e.Name,
                        slug = e.Slug,
                        courseClass_id = e.CourseClassId,
                        start_date = e.StartDate,
                        created_by = e.CreatedBy,
                        createdAt = e.CreatedAt,
                        updatedAt = e.UpdatedAt,
                        deletedAt = e.DeletedAt
                    });
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-slug")]
        public async Task<IActionResult> Get(string slug)
        {
            try
            {
                Exam c = await _context.Exams.FirstOrDefaultAsync(x => x.Slug == slug);
                if (c != null)
                {
                    return Ok(new ExamDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        courseClass_id = c.CourseClassId,
                        start_date = c.StartDate,
                        created_by = c.CreatedBy,
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
        public async Task<IActionResult> Create(CreateExam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool nameExists = await _context.Exams.AnyAsync(c => c.Name == model.name);

                    if (nameExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Exam name already exists");
                    }

                    Exam data = new Exam
                    {
                        Name = model.name,
                        Slug = model.name.ToLower().Replace(" ", "-"),
                        CourseClassId = model.courseClass_id,
                        StartDate = model.start_date,
                        CreatedBy = model.created_by,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Exams.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new ExamDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        courseClass_id = data.CourseClassId,
                        start_date = data.StartDate,
                        created_by = data.CreatedBy,
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
        public async Task<IActionResult> Update(EditExam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Exam existingExam = _context.Exams.AsNoTracking().FirstOrDefault(e => e.Id == model.id);

                    if (existingExam != null)
                    {
                        // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng tên)
                        bool nameExists = await _context.Exams.AnyAsync(c => c.Name == model.name && c.Id != model.id);

                        if (nameExists)
                        {
                            // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                            return BadRequest("Exam name already exists");
                        }

                        Exam exam = new Exam
                        {
                            Id = model.id,
                            Name = model.name,
                            Slug = model.name.ToLower().Replace(" ", "-"),
                            CourseClassId = model.courseClass_id,
                            StartDate = model.start_date,
                            CreatedBy = model.created_by,
                            CreatedAt = existingExam.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (exam != null)
                        {
                            _context.Exams.Update(exam);
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
                Exam exam = await _context.Exams.FindAsync(id);
                if (exam == null)
                    return NotFound();
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-courseId")]
        public async Task<IActionResult> GetbyCourse(int courseId)
        {
            try
            {
                List<Exam> exams = await _context.Exams.Where(p => p.CourseClassId == courseId).ToListAsync();
                if (exams != null)
                {
                    List<ExamDTO> data = exams.Select(c => new ExamDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        courseClass_id = c.CourseClassId,
                        start_date = c.StartDate,
                        created_by = c.CreatedBy,
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
