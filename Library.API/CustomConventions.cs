using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Library.API
{
#nullable disable

    public static class CustomConventions
    {
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //INSERT will be just a Prefix not a full name of the method !! using the ApiExplorerr we will be configuring this 
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]

        public static void Insert (Object model)
        {

        }

    }
#nullable restore
}
