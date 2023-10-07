using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.AnswerForStudent;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/answersForStudent")]
    [ApiController]
    public class AnswersForStudentController : Controller
    {
        private readonly ExamonimyContext _context;

        public AnswersForStudentController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<AnswersForStudent> answersForStudents = _context.AnswersForStudents.ToList();

            List<AnswerForStudentDTO> data = new List<AnswerForStudentDTO>();
            foreach (AnswersForStudent a in answersForStudents)
            {
                data.Add(new AnswerForStudentDTO
                {
                    id = a.Id,
                    student_id = a.StudentId,
                    content = a.Content,
                    question_id = a.QuestionId,
                    createdAt = a.CreatedAt,
                    updatedAt = a.UpdatedAt,
                    deletedAt = a.DeletedAt
                });
            }
            return Ok(data);
        }

        [HttpGet]
        [Route("get-by-id")]
        public IActionResult Get(int id)
        {
            try
            {
                AnswersForStudent a = _context.AnswersForStudents.FirstOrDefault(x => x.Id == id);
                if (a != null)
                {
                    return Ok(new AnswerForStudentDTO
                    {
                        id = a.Id,
                        student_id=a.StudentId,
                        content = a.Content,
                        question_id = a.QuestionId,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt

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
        public IActionResult Create(CreateAnswerForStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AnswersForStudent data = new AnswersForStudent
                    {
                        StudentId = model.student_id,
                        Content = model.content,
                        QuestionId = model.question_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.AnswersForStudents.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new AnswerForStudentDTO
                    {
                        id = data.Id,
                        student_id = data.StudentId,
                        content = data.Content,
                        question_id = data.QuestionId,
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
        public IActionResult Update(EditAnswerForStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AnswersForStudent answer = new AnswersForStudent
                    {
                        Id = model.id,
                        StudentId = model.student_id,
                        Content = model.content,
                        QuestionId = model.question_id,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
                    };

                    if (answer != null)
                    {
                        _context.AnswersForStudents.Update(answer);
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
                AnswersForStudent answer = _context.AnswersForStudents.Find(id);
                if (answer == null)
                    return NotFound();
                _context.AnswersForStudents.Remove(answer);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-questionId")]
        public IActionResult GetbyCategory(int questionId)
        {
            try
            {
                List<AnswersForStudent> answers = _context.AnswersForStudents.Where(p => p.QuestionId == questionId).ToList();
                if (answers != null)
                {
                    List<AnswerForStudentDTO> data = answers.Select(q => new AnswerForStudentDTO
                    {
                        id = q.Id,
                        student_id = q.StudentId,
                        content = q.Content,
                        question_id = q.QuestionId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
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
        [Route("get-by-questionIdAndStudentId")]
        public IActionResult GetbyTestAndStudent(int testID, int studentID)
        {
            try
            {
                List<AnswersForStudent> answers = _context.AnswersForStudents.Where(p => p.StudentId == studentID && p.Question.TestId == testID).ToList();
                if (answers != null)
                {
                    List<AnswerForStudentDTO> data = answers.Select(q => new AnswerForStudentDTO
                    {
                        id = q.Id,
                        student_id = q.StudentId,
                        content = q.Content,
                        question_id = q.QuestionId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
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
