using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Announcement
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Message cannot be empty!")]
    public string Message { get; set; } =null!;
    [Required]
    public AnnouncementType Type { get; set; }
    public DateTime AnnounceAt { get; set; } = DateTime.Now;

    public int? PostId { get; set; }
    public virtual Post? Post { get; set; }
    public virtual ICollection<Account>? Recipients { get; set; }
}

public enum AnnouncementType
{
    SelectionResult,
    Rejection,
    CreatorUpdate,
    General
}
