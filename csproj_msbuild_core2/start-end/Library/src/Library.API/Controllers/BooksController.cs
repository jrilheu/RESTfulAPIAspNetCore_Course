using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    public class BooksController : Controller
    {
        ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            this._libraryRepository = libraryRepository;
        }

        [HttpGet("api/authors/{authorId}/books")]
        public IActionResult GetBooksForAuthors(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorsFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var books = AutoMapper.Mapper.Map<IEnumerable<BookDTO>>(booksForAuthorsFromRepo);

            return Ok(books);
        }

        [HttpGet("api/authors/{authorId}/books/{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorsFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorsFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = AutoMapper.Mapper.Map<BookDTO>(bookForAuthorsFromRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost("api/authors/{authorId}/books")]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDTO book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = AutoMapper.Mapper.Map<Entities.Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if(!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for {authorId} failed on save");
            }

            var bookToReturn = AutoMapper.Mapper.Map<Models.BookDTO>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { id = bookToReturn.Id }, bookToReturn);
        }


    }
}
