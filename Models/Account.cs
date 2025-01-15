using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Account
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }

    public ICollection<Post>? Posts { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
