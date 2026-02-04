using System.Collections;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Models.Entities;
using VoxFox.Models.Requests;
namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesTestController : ControllerBase
    {
        private static List<CourseTest> courses = new List<CourseTest>();

        [HttpPost("Course")]
        public IActionResult Create(
            [FromBody] CreateCourseTestRequest request
        )
        {
            if (string.IsNullOrEmpty(request.Title))
            {
                return BadRequest();
            }
            var course = new CourseTest
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Tags = request.Tags
            };
            courses.Add(course);
            return CreatedAtAction(
                nameof(GetCourseById),
                new { id = course.Id },
                course
            );
        }

        [HttpGet("Courses")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseTestResponse))]
        public IActionResult GetAllCourses()
        {
            if (courses == null)
            {
                return NoContent();
            }
            var response = courses.Select(course => new CourseTestResponse
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Tags = course.Tags
            }).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseTestResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCourseById(
            [FromRoute] Guid id
        )
        {
            var course = courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            var response = new CourseTestResponse
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Tags = course.Tags
            };
            return Ok(response);
        }
    }
}
