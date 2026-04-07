using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using VoxFox.Enums;
using VoxFox.Interfaces;
using VoxFox.Models.Documents;
using VoxFox.Models.DTOs;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

namespace VoxFox.Services;

public class CourseSearchService
{
	private const string IndexName = "voxfox-courses";
	private readonly ElasticsearchClient _client;
	private readonly ICourseRepository _courseRepository;
	private readonly ILogger<CourseSearchService> _logger;

	public CourseSearchService(ElasticsearchClient client, ICourseRepository courseRepository,
		ILogger<CourseSearchService> logger)
	{
		_client = client;
		_courseRepository = courseRepository;
		_logger = logger;
	}

	public async Task IndexCourseAsync(CourseDocument course)
	{
		await _client.IndexAsync(course, i => i
			.Index(IndexName)
			.Id(course.Id.ToString()));
	}

	public async Task DeleteCourseAsync(Guid id)
	{
		await _client.DeleteAsync<CourseDocument>(
			id.ToString(),
			d => d.Index(IndexName));
	}

	public async Task<PaginatedResponse<CourseDto>> SearchCoursesAsync(CourseSearchRequest request)
	{
		// Логируем запрос для отладки
		_logger.LogInformation(
			"Search request: SearchTerm='{SearchTerm}', Page={Page}, PageSize={PageSize}, CategoryId={CategoryId}",
			request.SearchTerm, request.Page, request.PageSize, request.CategoryId);

		var searchRequest = new SearchRequest<CourseDocument>(IndexName)
		{
			From = (request.Page - 1) * request.PageSize,
			Size = request.PageSize,
			Query = BuildQuery(request)
		};

		var response = await _client.SearchAsync<CourseDocument>(searchRequest);

		// Проверяем успешность запроса
		if (!response.IsValidResponse)
		{
			_logger.LogError("Elasticsearch search failed: {DebugInfo}", response.DebugInformation);
			return new PaginatedResponse<CourseDto>
			{
				Items = new List<CourseDto>(),
				TotalCount = 0,
				CurrentPage = request.Page,
				TotalPages = 0,
				PageSize = request.PageSize
			};
		}

		_logger.LogInformation("Found {Total} courses", response.Total);

		var totalPages = (int)Math.Ceiling(response.Total / (double)request.PageSize);

		return new PaginatedResponse<CourseDto>
		{
			Items = response.Documents.Select(d => new CourseDto
			{
				Id = d.Id,
				Title = d.Title,
				Description = d.Description,
				// Добавьте остальные поля
				CategoryId = d.CategoryId,
				// IsPublished = d.IsPublished,
				CreatedAt = d.CreatedAt,
				Price = d.Price
			}).ToList(),
			TotalCount = (int)response.Total,
			CurrentPage = request.Page,
			TotalPages = totalPages,
			PageSize = request.PageSize
		};
	}

	private Query BuildQuery(CourseSearchRequest request)
	{
		var boolQuery = new BoolQuery();

		// Добавляем поисковый запрос, только если он не пустой
		if (!string.IsNullOrWhiteSpace(request.SearchTerm))
			boolQuery.Must = new List<Query>
			{
				new MultiMatchQuery
				{
					Query = request.SearchTerm,
					Fields = new[] { "title^3", "description^2", "tags^1" },
					Type = TextQueryType.BestFields,
					// Fuzziness = new Fuzziness("Auto"), // Добавляем автокоррекцию опечаток
					Operator = Operator.Or // ИЛИ вместо И
				}
			};
		else
			// Если нет поискового запроса - возвращаем всё
			boolQuery.Must = new List<Query>
			{
				new MatchAllQuery()
			};

		// Добавляем фильтры
		var filters = new List<Query>();

		if (request.CategoryId.HasValue)
			filters.Add(new TermQuery
			{
				Field = "categoryId",
				Value = request.CategoryId.Value.ToString()
			});

		// Всегда фильтруем только опубликованные курсы
		// filters.Add(new TermQuery
		// {
		// 	Field = "isPublished",
		// 	Value = true
		// });

		if (filters.Any()) boolQuery.Filter = filters;

		return boolQuery;
	}

	public async Task ReindexAllAsync(CancellationToken ct = default)
	{
		_logger.LogInformation("Starting full reindex...");
		const int batchSize = 500;
		var skip = 0;

		while (true)
		{
			var courses = await _courseRepository.GetForReindexAsync(skip, batchSize, ct);

			if (!courses.Any()) break;

			var documents = courses.Select(c => new CourseDocument
			{
				Id = c.Id,
				Title = c.Title,
				Description = c.Description,
				Tags = c.Tags != null && c.Tags.Any()
					? string.Join(" ", c.Tags.Select(x => x.Name))
					: string.Empty,
				CategoryId = c.CategoryId,
				IsPublished = c.Status == CourseStatus.Published,
				CreatedAt = c.CreatedAt,
				Price = c.Price
			});

			await _client.BulkAsync(b => b
				.Index(IndexName)
				.IndexMany(documents), ct);

			skip += batchSize;
			_logger.LogInformation("Reindexed {Skip} courses", skip);

			if (courses.Count < batchSize) break;
		}

		_logger.LogInformation("Full reindex completed. Total: {Total} courses", skip);
	}

	public async Task EnsureIndexExistsAsync()
	{
		var existsResponse = await _client.Indices.ExistsAsync(IndexName);

		if (!existsResponse.Exists)
		{
			_logger.LogInformation("Creating index {IndexName} with mapping...", IndexName);

			await _client.Indices.CreateAsync(IndexName, c => c
				.Settings(s => s
					.Analysis(a => a
						.Analyzers(an => an
							.Custom("russian_analyzer", ca => ca
								.Tokenizer("standard")
								.Filter("lowercase", "stop")
							)
						)
					)
				)
				.Mappings(m => m
					.Properties<CourseDocument>(p => p
						.Text(t => t.Title, td => td.Analyzer("russian_analyzer"))
						.Text(t => t.Description, td => td.Analyzer("russian_analyzer"))
						.Text(t => t.Tags)
						.Keyword(t => t.CategoryId)
						.Boolean(t => t.IsPublished)
						.Date(t => t.CreatedAt)
						.LongNumber(t => t.Price)
					)
				)
			);

			_logger.LogInformation("Index {IndexName} created successfully", IndexName);
		}
		else
		{
			_logger.LogInformation("Index {IndexName} already exists", IndexName);
		}
	}
}
