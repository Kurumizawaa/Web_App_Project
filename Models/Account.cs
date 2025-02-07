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
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }

    public virtual ICollection<Post>? Posts { get; set; }
    public virtual ICollection<Enrollment>? Enrollments { get; set; }
    public virtual ICollection<Post>? SelectedInPosts { get; set; }
    public virtual ICollection<Announcement>? Announcements { get; set; }
}
