using VoxFox.Enums;

namespace VoxFox.Models.Requests;

public class CourseSearchRequest
{
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? CategoryId { get; set; }
    public CoursesSortBy? SortBy { get; set; } = CoursesSortBy.Relevance;

    public CourseLevel? Level { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFree { get; set; }
    public CourseStatus? Status { get; set; }
    public bool IncludeDeleted { get; set; }
}
