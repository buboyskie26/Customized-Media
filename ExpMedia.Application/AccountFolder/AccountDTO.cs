using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMedia.Application.AccountFolder
{
    public class AccountDTO
    {
        public class LoginVM
        {
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
        }
        public class RegisterVM
        {
            [Required]
            [EmailAddress]

            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
            public string RoleName { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public IFormFile ImageUrl { get; set; }
        }
    }
}
