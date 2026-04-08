using VoxFox.Enums;

namespace VoxFox.Models.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? FullDescription { get; set; }
        public string? CoverImageUrl { get; set; }
        public decimal Price { get; set; } = 0;
        public CourseLevel Level { get; set; }
        public bool CertificateEnabled { get; set; } = false;
        public int EnrollmentCount { get; set; } = 0;
        public decimal Rating { get; set; } = 0;
        public int DurationMinutes { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
        public CourseStatus Status { get; set; }
        public Guid? CategoryId { get; set; }

        public Guid? AuthorId { get; set; }
        public User? Author { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Tag>? Tags { get; set; } = null!;
        public ICollection<Section> Sections { get; set; } = null!;
        public Category? Category { get; set; } = null!;
    }
}
