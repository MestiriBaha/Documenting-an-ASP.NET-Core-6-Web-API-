using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    /// <summary>
    /// An Author for Update with LastName and FirstName fields ! 
    /// </summary>
    public class AuthorForUpdate
    {
        /// <summary>
        ///  Author's FirstName property ! 
        /// </summary>
        [Required(ErrorMessage ="Author's FirstName is Required ")]
        [MaxLength(50)]
     
        public string? FirstName { get; set; }
        /// <summary>
        ///    Author's LastName property ! 
        /// </summary>
        [Required(ErrorMessage = "Author's Last name is Required ")]
        [MaxLength(50)]
       

        public string? LastName { get; set; }
    }
}
