using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Dtos;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseRepo;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseRepo, IMapper mapper)
        {
            _courseRepo = courseRepo ?? throw new ArgumentNullException(nameof(courseRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IReadOnlyList<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseRepo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courses = _courseRepo.GetCourses(authorId);
            return Ok(_mapper.Map<IReadOnlyList<CourseDto>>(courses));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseRepo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var course = _courseRepo.GetCourse(authorId, courseId);
            if (course == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if (!_courseRepo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Course>(course);
            _courseRepo.AddCourse(authorId, courseEntity);
            _courseRepo.Save();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor", new {authorId = authorId, courseId = courseToReturn.Id}, courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto course)
        {
            if (!_courseRepo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthor = _courseRepo.GetCourse(authorId, courseId);

            if (courseForAuthor == null)
            {
                var courseToAdd = _mapper.Map<Course>(course);
                courseToAdd.Id = courseId;
                
                _courseRepo.AddCourse(authorId, courseToAdd);
                _courseRepo.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor", new {authorId = authorId, courseId = courseToReturn.Id},
                    courseToReturn);
            }
            
            // map the entity to a CourseForUpdateDto
            // Apply the updated field values to that Dto
            // Map the CourseForUpdateDto back to an entity
            _mapper.Map(course, courseForAuthor);
            
            _courseRepo.UpdateCourse(courseForAuthor);

            _courseRepo.Save();
            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseRepo.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthor = _courseRepo.GetCourse(authorId, courseId);
            
            if (courseForAuthor == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }
                
                var courseToAdd = _mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;
                
                _courseRepo.AddCourse(authorId, courseToAdd);
                _courseRepo.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor", new {authorId = authorId, courseId = courseToReturn.Id},
                    courseToReturn);
            }

            var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthor);
            // add validation
            patchDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }
            
            _mapper.Map(courseToPatch, courseForAuthor);
            
            _courseRepo.UpdateCourse(courseForAuthor);

            _courseRepo.Save();
            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseRepo.AuthorExists(authorId))
            {
                return NotFound();
            }
            
            var courseForAuthor = _courseRepo.GetCourse(authorId, courseId);
            
            if (courseForAuthor == null)
            {
                return NotFound();
            }
            
            _courseRepo.DeleteCourse(courseForAuthor);
            _courseRepo.Save();

            return NoContent();
        }

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult) options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}