// using System.ComponentModel.DataAnnotations;

// namespace VoxFox.Models.Entities
// {
//     public class Course1
//     {
//         public Guid Id { get; set; } = Guid.NewGuid();

//         [Required]
//         [MaxLength(200)]
//         public string Title { get; set; } = default!;

//         [Required]
//         [MaxLength(5000)]
//         public string Description { get; set; } = default!;

//         [MaxLength(500)]
//         public string? ShortDescription { get; set; } = default!;

//         [MaxLength(500)]
//         public string Category { get; set; } = default!;

//         [Required]
//         [MaxLength(100)]
//         public string Level { get; set; } = default!;

//         [MaxLength(1000)]
//         public string? ImageUrl { get; set; }


//         public int LessonsCount { get; set; }

//         [MaxLength(50)]
//         public string? Duration { get; set; }

//         [Required]
//         [MaxLength(20)]
//         public string Format { get; set; } = default!;

//         public bool HasCertificate { get; set; } = true;

//         public bool HasHomework { get; set; } = true;

//         public bool IsPaid { get; set; } = false;
//         public decimal Price { get; set; } = 0;
//         public decimal? DiscountedPrice { get; set; }

//         [MaxLength(500)]
//         public string? Tags { get; set; }

//         [Required]
//         [MaxLength(20)]
//         public string Status { get; set; } = "draft";

//         [Required]
//         public Guid AuthorId { get; set; }
//         public User? Author { get; set; }

//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//         public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
//         public bool IsActive { get; set; } = true;
//     }
// }
