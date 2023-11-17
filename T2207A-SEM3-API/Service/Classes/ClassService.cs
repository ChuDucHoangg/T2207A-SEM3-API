using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Class;
using T2207A_SEM3_API.Models.Course;

namespace T2207A_SEM3_API.Service.Classes
{
    public class ClassService : IClassService
    {
        private readonly ExamonimyContext _context;

        public ClassService(ExamonimyContext context)
        {
            _context = context;
        }

        public async Task<ClassDTO> CreateClassAsync(CreateClass model)
        {
            // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
            bool nameExists = await _context.Classes.AnyAsync(c => c.Name == model.name);

            if (nameExists)
            {
                // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                throw new Exception("Class name already exists");
            }

            Class data = new Class
            {
                Name = model.name,
                Slug = model.name.ToLower().Replace(" ", "-"),
                Room = model.room,
                TeacherId = model.teacher_id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = null,
            };

            _context.Classes.Add(data);
            await _context.SaveChangesAsync();

            return new ClassDTO
            {
                id = data.Id,
                name = data.Name,
                slug = data.Slug,
                room = data.Room,
                teacher_id = data.TeacherId,
                createdAt = data.CreatedAt,
                updatedAt = data.UpdatedAt,
                deletedAt = data.DeletedAt
            };
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            Class classes = await _context.Classes.FindAsync(id);
            if (classes == null)
            {
                return false;
            }

            _context.Classes.Remove(classes);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ClassDTO>> GetAllClassAsync()
        {
            List<Class> classes = await _context.Classes.OrderByDescending(s => s.Id).ToListAsync();
            List<ClassDTO> data = new List<ClassDTO>();

            foreach (Class c in classes)
            {
                data.Add(new ClassDTO
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    room = c.Room,
                    teacher_id = c.TeacherId,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    deletedAt = c.DeletedAt
                });
            }

            return data;
        }

        public async Task<ClassDTO> GetClassBySlugAsync(string slug)
        {
            Class c = await _context.Classes.FirstOrDefaultAsync(x => x.Slug == slug);
            if (c != null)
            {
                return new ClassDTO
                {
                    id = c.Id,
                    name = c.Name,
                    slug = c.Slug,
                    room = c.Room,
                    teacher_id = c.TeacherId,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    deletedAt = c.DeletedAt

                };
            }
            return null;
        }

        public async Task<bool> UpdateClassAsync(EditClass model)
        {
            Class existingClass = await _context.Classes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);

            if (existingClass != null)
            {
                // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                bool nameExists = _context.Classes.Any(c => c.Name == model.name && c.Id != model.id);

                if (nameExists)
                {
                    // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                    return false;
                }

                Class classes = new Class
                {
                    Id = model.id,
                    Name = model.name,
                    Slug = model.name.ToLower().Replace(" ", "-"),
                    Room = model.room,
                    TeacherId = model.teacher_id,
                    CreatedAt = existingClass.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null,
                };

                if (classes != null)
                {
                    _context.Classes.Update(classes);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            else
            {
                return false; // Không tìm thấy lớp để cập nhật
            }
        }
    }
}
