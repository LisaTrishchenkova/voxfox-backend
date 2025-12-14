using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Requests
{
    public class CreateCourseRequest()
    {
        [Required]
        [MaxLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = default!;
      
        [Required]
        [MaxLength(5000)]
        public string Description { get; set; } = default!;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = default!;

        [Required]
        [MaxLength(50)]
        public string Level { get; set; } = default!;

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Range(0, 1000)]
        public int LessonsCount { get; set; } = 0;

        [MaxLength(50)]
        public string? Duration { get; set; }

        [Required]
        [MaxLength(20)]
        public string Format { get; set; } = default!;

        public bool HasCertificate { get; set; } = true;

        public bool HasHomework { get; set; } = true;

        public bool IsPaid { get; set; } = false;

        [Range(0, 1000000)]
        public decimal Price { get; set; } = 0;

        [Range(0, 1000000)]
        public decimal? DiscountedPrice { get; set; }

        public List<string>? Tags { get; set; }

    }

}
