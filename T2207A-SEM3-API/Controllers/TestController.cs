using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            List<Test> tests = _context.Tests.ToList();

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
        }

        [HttpGet]
        [Route("get-by-slug")]
        public IActionResult Get(string slug)
        {
            try
            {
                Test t = _context.Tests.FirstOrDefault(x => x.Slug == slug);
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
        public IActionResult Create(CreateTest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Test data = new Test
                    {
                        Name = model.name,
                        Slug = model.slug,
                        ExamId = model.exam_id,
                        StudentId = model.student_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks, 
                        TotalMarks = model.total_marks,
                        CreatedBy = model.created_by,
                        Status = model.status,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Tests.Add(data);
                    _context.SaveChanges();
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
        public IActionResult Update(EditTest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Test test = new Test
                    {
                        Id = model.id,
                        Name = model.name,
                        Slug = model.slug,
                        ExamId = model.exam_id,
                        StudentId = model.student_id,
                        StartDate = model.startDate,
                        EndDate = model.endDate,
                        PastMarks = model.past_marks,
                        TotalMarks = model.total_marks,
                        CreatedBy = model.created_by,
                        Status = model.status,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
                    };

                    if (test != null)
                    {
                        _context.Tests.Update(test);
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
                Test test = _context.Tests.Find(id);
                if (test == null)
                    return NotFound();
                _context.Tests.Remove(test);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-examId")]
        public IActionResult GetbyCategory(int examId)
        {
            try
            {
                List<Test> tests = _context.Tests.Where(p => p.ExamId == examId).ToList();
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
