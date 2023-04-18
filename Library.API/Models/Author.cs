using System;

namespace Library.API.Models
{
    /// <summary>
    /// An Author with ID  , FirstName and LastName Fields 
    /// </summary>
    public class Author
    {        
        /// <summary>
        ///  ID of The Author !! 
        /// </summary>
        public Guid Id { get; set; } 
        /// <summary>
        ///  FirstName of the Author
        /// </summary>
     
        public string? FirstName { get; set; }
        /// <summary>
        ///  LastName of the Author 
        /// </summary>
      
        public string? LastName { get; set; }
    }
}
