﻿using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [Route("api/Authors")]
    public class AuthorsController : Controller
    {
        readonly ILibraryRepository _libraryRepository;
        private readonly IUrlHelper _urlHelper;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            this._urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors([FromQuery] Helpers.AuthorsResourceParameters parameters)
        {
            IEnumerable<AuthorDto> authors;
            var authorsFromRepo = _libraryRepository.GetAuthors(parameters);

            string previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(parameters, Helpers.ResourceUriType.PreviousPage) : null;

            string nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(parameters, Helpers.ResourceUriType.NextPage) : null;

            var paginationMetaData = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetaData));

            authors = AutoMapper.Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);
        }

        string CreateAuthorsResourceUri(
             Helpers.AuthorsResourceParameters authorsResourceParameters,
             Helpers.ResourceUriType type)
        {
            switch (type)
            {
                case Helpers.ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                      new
                      {
                          genre = authorsResourceParameters.Genre,
                          searchQuery = authorsResourceParameters.SearchQuery,
                          pageNumber = authorsResourceParameters.PageNumber - 1,
                          pageSize = authorsResourceParameters.PageSize
                      });
                case Helpers.ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                      new
                      {
                          genre = authorsResourceParameters.Genre,
                          searchQuery = authorsResourceParameters.SearchQuery,
                          pageNumber = authorsResourceParameters.PageNumber + 1,
                          pageSize = authorsResourceParameters.PageSize
                      });

                default:
                    return _urlHelper.Link("GetAuthors",
                    new
                    {
                        genre = authorsResourceParameters.Genre,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize
                    });
            }
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorsFromRepo = _libraryRepository.GetAuthor(id);

            if (authorsFromRepo == null)
                return NotFound();

            var author = AutoMapper.Mapper.Map<AuthorDto>(authorsFromRepo);
            return Ok(author);
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = AutoMapper.Mapper.Map<Entities.Author>(author);
            _libraryRepository.AddAuthor(authorEntity);
            if (!_libraryRepository.Save())
            {
                throw new Exception("dallo la creacion del autor al guardar");
            }

            var authorToReturn = AutoMapper.Mapper.Map<Models.AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlokAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(authorFromRepo);
            if (_libraryRepository.Save())
            {
                throw new Exception($"failed on delete author {id}");
            }

            return NoContent();
        }

    }
}