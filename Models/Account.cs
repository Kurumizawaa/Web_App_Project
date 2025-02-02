using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Account
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Username cannot be empty!")]
    public string? Username { get; set; }
    public string? Password { get; set; }
    [Compare("Password", ErrorMessage = "Passwords do not match!")]
    public string? ConfirmPassword { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }

    public ICollection<Post>? Posts { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
