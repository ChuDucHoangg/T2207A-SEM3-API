using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.RegisterExam;

namespace T2207A_SEM3_API.Service.RegisterExams
{
    public class RegisterExamService : IRegisterExamService
    {
        private readonly ExamonimyContext _context;

        public RegisterExamService(ExamonimyContext context)
        {
            _context = context;
        }

        public async Task<List<RegisterExamDTO>> GetAllRegisterExamAsync()
        {
            List<RegisterExam> registerExam = await _context.RegisterExams.OrderByDescending(s => s.Id).ToListAsync();
            List<RegisterExamDTO> data = new List<RegisterExamDTO>();

            foreach (RegisterExam re in registerExam)
            {
                data.Add(new RegisterExamDTO
                {
                    id = re.Id,
                    student_id = re.StudentId,
                    exam_id = re.ExamId,
                    status = re.Status,
                    createdAt = re.CreatedAt,
                    updatedAt = re.UpdatedAt,
                    deletedAt = re.DeletedAt
                });
            }

            return data;
        }

        public async Task<RegisterExamDTO> CreateRegisterExamAsync(int student_id, int exam_id)
        {
            bool registerExists = await _context.RegisterExams.AnyAsync(c => c.StudentId == student_id && c.ExamId == exam_id);
            if (registerExists)
            {
                throw new Exception("You have previously registered for the retake exam");
            }

            RegisterExam data = new RegisterExam
            {
                StudentId = student_id,
                ExamId = exam_id,
                Status = 0,// chưa được duyệt
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = null,
            };

            _context.RegisterExams.Add(data);
            await _context.SaveChangesAsync();

            return new RegisterExamDTO
            {
                id = data.Id,
                student_id = data.StudentId,
                exam_id = data.ExamId,
                status = data.Status,
                createdAt = data.CreatedAt,
                updatedAt = data.UpdatedAt,
                deletedAt = data.DeletedAt,
            };
        }

        public async Task<bool> DeleteRegisterExamAsync(int id, int student_id)
        {
            RegisterExam registerExam = await _context.RegisterExams.Where(r => r.Status == 0 && r.StudentId == student_id && r.Id == id).FirstOrDefaultAsync();
            if (registerExam == null)
            {
                return false;
            }

            _context.RegisterExams.Remove(registerExam);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApproveRegisterExamAsync(int id)
        { 
            RegisterExam existingRegister = await _context.RegisterExams.FirstOrDefaultAsync(e => e.Id == id);
            if (existingRegister == null)
            {
                return false;
            }

            existingRegister.Status = 1;
            _context.RegisterExams.Update(existingRegister);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<RegisterExamDTO>> GetRegisterByExamIdAsync(int id)
        {
            List<RegisterExam> registerExam = await _context.RegisterExams.Where(s => s.ExamId == id).OrderByDescending(s => s.Id).ToListAsync();
            List<RegisterExamDTO> data = new List<RegisterExamDTO>();

            foreach (RegisterExam re in registerExam)
            {
                data.Add(new RegisterExamDTO
                {
                    id = re.Id,
                    student_id = re.StudentId,
                    exam_id = re.ExamId,
                    status = re.Status,
                    createdAt = re.CreatedAt,
                    updatedAt = re.UpdatedAt,
                    deletedAt = re.DeletedAt
                });
            }

            return data;
        }
    }
}
