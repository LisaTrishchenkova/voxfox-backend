using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Requests
{
    public class UpdateCourseRequest()
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(5000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string? Level { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Range(0, 1000)]
        public int? LessonsCount { get; set; }

        [MaxLength(50)]
        public string? Duration { get; set; }

        [MaxLength(20)]
        public string? Format { get; set; }

        public bool? HasCertificate { get; set; }

        public bool? HasHomework { get; set; }

        public bool? IsPaid { get; set; }

        [Range(0, 1000000)]
        public decimal? Price { get; set; }

        [Range(0, 1000000)]
        public decimal? DiscountedPrice { get; set; }

        public List<string>? Tags { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }

    }

}
