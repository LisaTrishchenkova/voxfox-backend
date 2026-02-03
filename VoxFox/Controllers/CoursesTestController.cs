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
        private static int _Id = 1;

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
                Id = _Id++,
                Title = request.Title,
                Description = request.Description,
                Tags = request.Tags
            };
            courses.Add(course);
            return NoContent(); //CreatedAtAction();
        }

        [HttpGet("Courses")]
        public IActionResult GetAllCourses()
        {
            return Ok(courses);
        }
    }
}
