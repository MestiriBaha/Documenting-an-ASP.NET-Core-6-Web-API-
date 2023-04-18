using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/v{version:apiVersion}/authors/{authorId}")]
    [ApiVersion("2.0")]

    [ApiController]
    public class AuthorsV2Controller : ControllerBase
    {

        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsV2Controller(
            IAuthorRepository authorsRepository,
            IMapper mapper)
        {
            _authorsRepository = authorsRepository
                ?? throw new ArgumentNullException(nameof(authorsRepository));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }
        /// <summary>
        /// Fetching All the Authors from the API 
        /// </summary>
        /// <returns>IEnumerable of Author Author Type being mapped to an AuthorProfile</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }

    }
}
