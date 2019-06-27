 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Produces("application/json")]
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
            var authors = _libraryRepository.GetAuthors();
            return new JsonResult(authors);
        }
    }
}