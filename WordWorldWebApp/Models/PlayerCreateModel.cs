using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Models
{
    public class PlayerCreateModel
    {
        [Required(ErrorMessage = "username_missing")]
        [MinLength(4, ErrorMessage = "username_too_short")]
        [MaxLength(10, ErrorMessage = "username_too_long")]
        [RegularExpression(@"\w+", ErrorMessage = "username_invalid_characters")]
        public string Username { get; set; } 

        public string Board { get; set; } 
    }
}
