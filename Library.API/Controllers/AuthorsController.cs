using AutoMapper;
using Library.API.Models;
using Library.API.Services; 
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/v{api:apiVersion}/authors")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json", "application/xml")]
   // [ApiExplorerSettings(GroupName ="Authors")]

    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsController(
            IAuthorRepository authorsRepository,
            IMapper mapper)
        {
            _authorsRepository = authorsRepository 
                ?? throw new ArgumentNullException(nameof(authorsRepository));
            _mapper = mapper 
                ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }
        //There is an Option that we have to Activate in Order to Generate XML Documentation in project property !!
        /// <summary>
        ///     Get an Author by its ID 
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns>ActionResult of Author </returns>
        [HttpGet("{authorId}")]
        public async Task<ActionResult<Author>> GetAuthor(
            Guid authorId)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Author>(authorFromRepo));
        }

        [HttpPut("{authorId}")]
        public async Task<ActionResult<Author>> UpdateAuthor(
            Guid authorId,
            AuthorForUpdate authorForUpdate)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(authorForUpdate, authorFromRepo);

            // update & save
            _authorsRepository.UpdateAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            // return the author
            return Ok(_mapper.Map<Author>(authorFromRepo));
        }
        /// <summary>
        /// Partial Update for the Author Entity 
        /// </summary>
        /// <param name="authorId"></param>
        /// <param name="patchDocument">a Set of Opperations to Aply for the Author !! </param>
        /// <returns>An ActionResult of Type Author </returns>
        /// <remarks>
        ///     Sample Request ( this Request updates the Author's First Name) \
        ///     
        ///  PATCH /authors/id \
        ///  [ \
        ///     { "op" : "update" \
        ///       "path" : "/firstName"  \
        ///        "value" : " new firstName" \
        ///        }    \
        ///  ]  
        /// </remarks>

        [HttpPatch("{authorId}")]
        public async Task<ActionResult<Author>> UpdateAuthor(
            Guid authorId,
            JsonPatchDocument<AuthorForUpdate> patchDocument)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            // map to DTO to apply the patch to
            var author = _mapper.Map<AuthorForUpdate>(authorFromRepo);
            patchDocument.ApplyTo(author, ModelState);

            // if there are errors when applying the patch the patch doc 
            // was badly formed  These aren't caught via the ApiController
            // validation, so we must manually check the modelstate and
            // potentially return these errors.
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // map the applied changes on the DTO back into the entity
            _mapper.Map(author, authorFromRepo);

            // update & save
            _authorsRepository.UpdateAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            // return the author
            return Ok(_mapper.Map<Author>(authorFromRepo));
        }
    }
}
