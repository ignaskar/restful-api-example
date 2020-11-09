using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Dtos;
using CourseLibrary.API.Entities;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseRepo;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository courseRepo, IMapper mapper)
        {
            _courseRepo = courseRepo ?? throw new ArgumentNullException(nameof(courseRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        [HttpGet]
        [HttpHead]
        public ActionResult<IReadOnlyList<AuthorDto>> GetAuthors([FromQuery]AuthorsResourceParameters parameters)
        {
            var authors = _courseRepo.GetAuthors(parameters);

            return Ok(_mapper.Map<IReadOnlyList<AuthorDto>>(authors));
        }

        [HttpGet("{authorId}", Name = "GetAuthor")]
        
        public IActionResult GetAuthor(Guid authorId)
        {
            var author = _courseRepo.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<AuthorDto>(author));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            var authorEntity = _mapper.Map<Author>(author);
            _courseRepo.AddAuthor(authorEntity);
            _courseRepo.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new {authorId = authorToReturn.Id}, authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var author = _courseRepo.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }
            
            _courseRepo.DeleteAuthor(author);
            _courseRepo.Save();

            return NoContent();
        }
    }
}