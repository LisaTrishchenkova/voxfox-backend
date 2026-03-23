using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Entities
{
    public class Lesson
    {
        public Guid Id { get; init; }
        public string Title { get; set; } = null!;
        [MaxLength(2000)]
        public string Description { get; set; } = null!;
        //TODO: Посмотреть позже (Саше)
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string? Content { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Guid SectionId { get; set; }

        public Section Section { get; set; } = null!;
    }
}