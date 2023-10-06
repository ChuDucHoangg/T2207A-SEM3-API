using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace T2207A_SEM3_API.Entities;

public partial class ExamonimyContext : DbContext
{
    public ExamonimyContext()
    {
    }

    public ExamonimyContext(DbContextOptions<ExamonimyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<AnswersForStudent> AnswersForStudents { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamAgain> ExamAgains { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RegisterExam> RegisterExams { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=Examonimy;Integrated Security=True;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answers__3213E83F0EDCC73E");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Answers__questio__42E1EEFE");
        });

        modelBuilder.Entity<AnswersForStudent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AnswersF__3213E83F2A5B84A6");

            entity.ToTable("AnswersForStudent");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Question).WithMany(p => p.AnswersForStudents)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswersFo__quest__45BE5BA9");

            entity.HasOne(d => d.Student).WithMany(p => p.AnswersForStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswersFo__stude__46B27FE2");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Classes__3213E83FA7782CE1");

            entity.HasIndex(e => e.Name, "UQ__Classes__72E12F1BF106E6DC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Room)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("room");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("slug");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Classes__teacher__2A164134");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3213E83FA0953FFF");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("course_code");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Class).WithMany(p => p.Courses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__class_i__31B762FC");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__created__32AB8735");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exams__3213E83F08C37B94");

            entity.HasIndex(e => e.Name, "UQ__Exams__72E12F1B1208B312").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("slug");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Course).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Exams__course_id__367C1819");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Exams__created_b__37703C52");
        });

        modelBuilder.Entity<ExamAgain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExamAgai__3213E83F2D88F9EA");

            entity.ToTable("ExamAgain");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.RetestDate)
                .HasColumnType("datetime")
                .HasColumnName("retest_date");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Student).WithMany(p => p.ExamAgains)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamAgain__stude__51300E55");

            entity.HasOne(d => d.Test).WithMany(p => p.ExamAgains)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamAgain__test___5224328E");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Grades__3213E83F0C043F19");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.TimeTaken).HasColumnName("time_taken");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grades__student___498EEC8D");

            entity.HasOne(d => d.Test).WithMany(p => p.Grades)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grades__test_id__4A8310C6");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83FFCE2A490");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.Title)
                .HasColumnType("text")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Test).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK__Questions__test___40058253");
        });

        modelBuilder.Entity<RegisterExam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Register__3213E83F4939BDE7");

            entity.ToTable("RegisterExam");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Exam).WithMany(p => p.RegisterExams)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RegisterE__exam___4E53A1AA");

            entity.HasOne(d => d.Student).WithMany(p => p.RegisterExams)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RegisterE__stude__4D5F7D71");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Staffs__3213E83F38C13719");

            entity.HasIndex(e => e.StaffCode, "UQ__Staffs__097F3286C9F31D0A").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Staffs__AB6E6164E107F22D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.Birthday)
                .HasColumnType("date")
                .HasColumnName("birthday");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.StaffCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("staff_code");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Students__3213E83F040667BE");

            entity.HasIndex(e => e.StudentCode, "UQ__Students__6DF33C45FD641D6B").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Students__AB6E61641EAABF76").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar");
            entity.Property(e => e.Birthday)
                .HasColumnType("date")
                .HasColumnName("birthday");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("student_code");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__class___2EDAF651");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tests__3213E83F27C393C4");

            entity.HasIndex(e => e.Name, "UQ__Tests__72E12F1B9F8B7F86").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PastMarks).HasColumnName("past_marks");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("slug");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TotalMarks).HasColumnName("total_marks");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Tests)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tests__created_b__3D2915A8");

            entity.HasOne(d => d.Exam).WithMany(p => p.Tests)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tests__exam_id__3B40CD36");

            entity.HasOne(d => d.Student).WithMany(p => p.Tests)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tests__student_i__3C34F16F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
