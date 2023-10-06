using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Class;
using T2207A_SEM3_API.Models.Exam;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public IActionResult Index()
        {
            List<Exam> exams = _context.Exams.ToList();

            List<ExamDTO> data = new List<ExamDTO>();
            foreach (Exam e in exams)
            {
                data.Add(new ExamDTO
                {
                    id = e.Id,
                    name = e.Name,
                    slug = e.Slug,
                    course_id = e.CourseId,
                    start_date = e.StartDate,
                    created_by = e.CreatedBy,
                    createdAt = e.CreatedAt,
                    updatedAt = e.UpdatedAt,
                    deletedAt = e.DeletedAt
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
                Exam c = _context.Exams.FirstOrDefault(x => x.Slug == slug);
                if (c != null)
                {
                    return Ok(new ExamDTO
                    {
                        id = c.Id,
                        name = c.Name,
                        slug = c.Slug,
                        course_id = c.CourseId,
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
        public IActionResult Create(CreateExam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Exam data = new Exam
                    {
                        Name = model.name,
                        Slug = model.slug,
                        CourseId = model.course_id,
                        StartDate = model.start_date,
                        CreatedBy = model.created_by,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Exams.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new ExamDTO
                    {
                        id = data.Id,
                        name = data.Name,
                        slug = data.Slug,
                        course_id = data.CourseId,
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
        public IActionResult Update(EditExam model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Exam exam = new Exam
                    {
                        Id = model.id,
                        Name = model.name,
                        Slug = model.slug,
                        CourseId = model.course_id,
                        StartDate = model.start_date,
                        CreatedBy = model.created_by,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
                    };

                    if (exam != null)
                    {
                        _context.Exams.Update(exam);
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
                Exam exam = _context.Exams.Find(id);
                if (exam == null)
                    return NotFound();
                _context.Exams.Remove(exam);
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
