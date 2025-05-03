using System.ComponentModel.DataAnnotations;

namespace Kairos.Shared.Data;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide user name")]
    public string UserName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide password")]
    public string Password { get; set; }
}
