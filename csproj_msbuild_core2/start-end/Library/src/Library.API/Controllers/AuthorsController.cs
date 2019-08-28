using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/Authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            IEnumerable<AuthorDto> authors;

            var authorsFromRepo = _libraryRepository.GetAuthors();
            authors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);
        }


        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorsFromRepo = _libraryRepository.GetAuthor(id);

            if (authorsFromRepo == null)
                return NotFound();

            var author = AutoMapper.Mapper.Map<AuthorDto>(authorsFromRepo);
            return Ok(author);
        }
    }
}