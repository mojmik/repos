using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCblog.Models {
    public class LoginViewModel {
        [Required]
        [EmailAddress(ErrorMessage = "Neplatná emailová adresa")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Heslo")]
        public string Password { get; set; }

        [Display(Name = "Pamatuj si mě")]
        public bool RememberMe { get; set; }
    }
}
