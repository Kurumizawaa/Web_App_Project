using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Post
{
    [Key]
    public int Id { get; set;}
    [Required]
    public string Title { get; set;} = null!;
    [Required]
    public string? Description { get; set;}
    public string? Picture { get; set;}
    [Required]
    public List<string> Tags { get; set;} = new List<string>();
    [Range(0, 99, ErrorMessage = "Accept amount must be within range 0 - 99")]
    public int EnrolledCount { get; set;} = 0;
    [Required]
    public int AmountAccept { get; set;}
    [Required]
    public string AcceptType { get; set;} = null!;
    [Required]
    public DateTime CreateDate { get; set;}
    [Required]
    public DateTime? CloseDate { get; set;} = null;

    public int CreaterId { get; set;}
    public Account? Creator { get; set; } = null!;
    public ICollection<Enrollment>? Enrollments { get; set; }
}
