using Shouldly;
using Microsoft.Extensions.Logging;
using Moq;
using VoxFox.Interfaces.Lesson;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
using VoxFox.Services.Course;
using Xunit;

namespace VoxFox.Tests.Services;

public class LessonServiceTests
{
	private readonly LessonService _sut;
	private readonly Mock<ILessonRepository> _repoMock = new();
	private readonly Mock<ILogger<LessonService>> _loggerMock = new();

	public LessonServiceTests()
	{
		_sut = new LessonService(_repoMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task CreateLessonAsync_WhenSectionIdIsEmpty_ReturnsBadRequest()
	{
		var dto = new CreateLessonDto
		{
			Title = "Урок 1",
			Content = "Content",
			Description = "Description"
		};

		var result = await _sut.CreateLessonAsync(Guid.Empty, dto);

		result.Success.ShouldBeFalse();
		result.StatusCode.ShouldBe(400);
		_repoMock.Verify(
			r => r.SectionExistsAsync(It.IsAny<Guid>()),
			Times.Never);
	}

	[Fact]
	public async Task CreateLessonAsync_WhenSectionNotFound_Returns404()
	{
		var sectionId = Guid.NewGuid();
		var dto = new CreateLessonDto
		{
			Title = "Урок 1",
			Content = "Content",
			Description = "Description"
		};

		_repoMock
			.Setup(r => r.SectionExistsAsync(sectionId))
			.ReturnsAsync(false);

		var result = await _sut.CreateLessonAsync(sectionId, dto);

		result.Success.ShouldBeFalse();
		result.StatusCode.ShouldBe(404);
		_repoMock.Verify(r => r.AddAsync(It.IsAny<Lesson>()), Times.Never);
	}

	[Fact]
	public async Task CreateLessonAsync_WhenValid_ReturnsCreatedLesson()
	{
		var sectionId = Guid.NewGuid();
		var dto = new CreateLessonDto
		{
			Title = "Урок 1",
			Content = "Content",
			Description = "Description"
		};

		var savedLesson = new Lesson
		{
			Id = Guid.NewGuid(),
			Title = dto.Title,
			Content = dto.Content,
			Description = dto.Description,
			SectionId = sectionId
		};

		_repoMock
			.Setup(r => r.SectionExistsAsync(sectionId))
			.ReturnsAsync(true);

		_repoMock
			.Setup(r => r.AddAsync(It.IsAny<Lesson>()))
			.ReturnsAsync(savedLesson);

		var result = await _sut.CreateLessonAsync(sectionId, dto);

		result.Success.ShouldBeTrue();
		result.StatusCode.ShouldBe(201);
		result.Data.ShouldNotBeNull();
		result.Data.Title.ShouldBe(dto.Title);
		result.Data.Id.ShouldBe(savedLesson.Id);
	}
}
