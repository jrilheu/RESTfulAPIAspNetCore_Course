using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            this._libraryRepository = libraryRepository;
        }

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

            var bookForAuthor = AutoMapper.Mapper.Map<BookDTO>(bookForAuthorsFromRepo);
            return Ok(bookForAuthor);
        }

        [HttpPost]
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

            if (_libraryRepository.Save())
            {
                throw new Exception($"failed deleting book {id} for author {authorId} on save");
            }

            return NoContent();
        }


        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDTO book)
        {
            if (book == null)
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
                var bookToAdd = AutoMapper.Mapper.Map<Entities.Book>(book);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"failed upserting a book {id} for author {authorId} on save");
                }

                var bookToReturn = AutoMapper.Mapper.Map<BookDTO>(bookToAdd);

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
        public IActionResult PatiallyUpdateBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDTO> patchDocument)
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
                var bookDTO = new BookForUpdateDTO();
                patchDocument.ApplyTo(bookDTO);
                var bookToAdd = AutoMapper.Mapper.Map<Entities.Book>(bookDTO);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"failed upserting a book {id} for author {authorId} on save");
                }

                var bookToReturn = AutoMapper.Mapper.Map<BookDTO>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", id = bookToReturn.Id, bookToReturn);
            }

            var bookToPatch = AutoMapper.Mapper.Map<BookForUpdateDTO>(bookForAuthorsFromRepo);
            patchDocument.ApplyTo(bookToPatch);
            //add validation 
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
