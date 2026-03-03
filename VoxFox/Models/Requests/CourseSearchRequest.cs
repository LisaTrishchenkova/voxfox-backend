public class CourseSearchRequest
{
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? CategoryId { get; set; }
    public CoursesSortBy? SortBy { get; set; } = CoursesSortBy.Relevance;
}
