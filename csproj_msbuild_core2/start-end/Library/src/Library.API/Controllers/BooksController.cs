using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        ILibraryRepository _libraryRepository;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger)
        {
            this._libraryRepository = libraryRepository;
            this._logger = logger;
        }

        public IActionResult GetBooksForAuthors(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorsFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var books = AutoMapper.Mapper.Map<IEnumerable<BookDto>>(booksForAuthorsFromRepo);

            return Ok(books);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
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

            var bookForAuthor = AutoMapper.Mapper.Map<BookDto>(bookForAuthorsFromRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),
                    "Description debe ser diferente de Title");
            }

            if (!ModelState.IsValid)
            {
                return new Helpers.UnprocessableEntityObjectResult(ModelState);
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

            var bookToReturn = AutoMapper.Mapper.Map<Models.BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { id = bookToReturn.Id }, bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
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

            _libraryRepository.DeleteBook(bookForAuthorsFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"failed deleting book {id} for author {authorId} on save");
            }

            _logger.LogInformation(100, $"Book {id} for athor {authorId} was deleted");

            return NoContent();
        }


        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "Description debe ser diferente de Title");
            }

            if (!ModelState.IsValid)
            {
                return new Helpers.UnprocessableEntityObjectResult(ModelState);
            }


            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorsFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorsFromRepo == null)
            {
                var bookToAdd = AutoMapper.Mapper.Map<Entities.Book>(book);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"failed upserting a book {id} for author {authorId} on save");
                }

                var bookToReturn = AutoMapper.Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", id = bookToReturn.Id, bookToReturn);

            }

            AutoMapper.Mapper.Map(book, bookForAuthorsFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorsFromRepo);


            if (!_libraryRepository.Save())
            {
                throw new Exception($"failed update a book {id} for author {authorId} on save");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorsFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorsFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDocument.ApplyTo(bookDto, ModelState);

                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto),
                        "Description debe ser diferente de Title");
                }

                TryValidateModel(bookDto);

                if (!ModelState.IsValid)
                {
                    return new Helpers.UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = AutoMapper.Mapper.Map<Entities.Book>(bookDto);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"failed upserting a book {id} for author {authorId} on save");
                }

                var bookToReturn = AutoMapper.Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", id = bookToReturn.Id, bookToReturn);
            }

            var bookToPatch = AutoMapper.Mapper.Map<BookForUpdateDto>(bookForAuthorsFromRepo);
            //patchDocument.ApplyTo(bookToPatch, ModelState);
            patchDocument.ApplyTo(bookToPatch);
            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto),
                    "Description debe ser diferente de Title");
            }

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new Helpers.UnprocessableEntityObjectResult(ModelState);
            }

            AutoMapper.Mapper.Map(bookToPatch, bookForAuthorsFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorsFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"failed patch a book {id} for author {authorId} on save");
            }

            return NoContent();

        }


    }
}
