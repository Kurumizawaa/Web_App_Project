using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Post
{
    [Key]
    public int Id { get; set;}
    [Required]
    public string? Title { get; set;}
    [Required]
    public string? Description { get; set;}
    [Required]
    public string? Picture { get; set;}
    public List<string>? Tags { get; set;} 
    public int EnrolledCount { get; set;}
    [Required]
    public int AmountAccept { get; set;}
    [Required]
    public string? AcceptType { get; set;}
    [Required]
    public DateTime CreateDate { get; set;}
    public DateTime CloseDate { get; set;}

    public int CreaterId { get; set;}
    public Account? Creator { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
