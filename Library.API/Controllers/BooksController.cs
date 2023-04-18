using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{

    //Don't REPEAT YOURSELF !
    //ProducesResponseType are REPETITIVE  Unfortunately that is why we have to find a better way of adding them !! 
    [Route("api/v{version:apiVersion}/authors/{authorId}/books")]
    [ApiController]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //We injected the attributes in FILTERS  in Addcontrollers method ! 
    [Produces("application/json","application/xml")]
    //[ApiExplorerSettings(GroupName = "Books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository 
                ?? throw new ArgumentNullException(nameof(bookRepository));
            _authorRepository = authorRepository 
                ?? throw new ArgumentNullException(nameof(authorRepository));
            _mapper = mapper 
                ?? throw new ArgumentNullException(nameof(mapper));
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]   
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
            Guid authorId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId);
            return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
        }
        /// <summary>
        /// Get a Book By ID for a Specific Author 
        /// </summary>
        /// <param name="authorId">the ID  of the Author </param>
        /// <param name="bookId">The ID of the book</param>
        /// <returns>An ActionResult of Type Book</returns>
        /// <response code="200">Returns the Requested Book </response>
        /// <response code="404"> Requested Book was not found </response>

        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK , Type =typeof(Book))]
        [HttpGet("{bookId}")]
        //let's pretend we won't be using ActionResult<Book> which gives acces to the type book ! 
        // let's change the return type to IActionResult 
        public async Task<IActionResult> GetBook(
            Guid authorId,
            Guid bookId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Book>(bookFromRepo));
        }
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [HttpPost()]
        public async Task<ActionResult<Book>> CreateBook(
            Guid authorId,
            BookForCreation bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }
    }
}
