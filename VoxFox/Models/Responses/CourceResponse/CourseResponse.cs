namespace VoxFox.Models.Responses
{
    public class CourseResponse
    {
  public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? ShortDescription { get; set; }
        public string Category { get; set; } = default!;
        public string Level { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public int LessonsCount { get; set; }
        public string? Duration { get; set; }
        public string Format { get; set; } = default!;
        public bool HasCertificate { get; set; }
        public bool HasHomework { get; set; }
        public bool IsPaid { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public List<string>? Tags { get; set; }
        public string Status { get; set; } = default!;
        public Guid AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
