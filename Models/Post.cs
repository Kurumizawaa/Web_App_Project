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
    public string Description { get; set;} = null!;
    public string? Picture { get; set;}
    public int EnrolledCount { get; set;} = 0;
    [Required]
    [Range(0, 99, ErrorMessage = "Accept amount must be within range 0 - 99")]
    public int AmountAccept { get; set;}
    [Required]
    public string AcceptType { get; set;} = null!;
    [Required]
    public DateTime CreateDate { get; set;}
    [Required]
    public DateTime? CloseDate { get; set;} = null;
    public PostStatus Status { get; set;} = PostStatus.Open;

    public int CreatorId { get; set;}
    public virtual Account? Creator { get; set; } = null!;
    public virtual ICollection<Tag> Tags { get; set;} = new List<Tag>();
    public virtual ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Account>? SelectedAccounts { get; set; } = new List<Account>();
    public virtual ICollection<Announcement>? Announcements { get; set; }
}

public enum PostStatus
{
    Open,
    Concluded,
    Selected,
    Archived
}
