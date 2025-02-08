using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class AnnouncementRecipient
{
    [Key]
    public int Id { get; set; }
    public int AnnouncementId { get; set; }
    public int AccountId { get; set; }
    public bool IsRead { get; set; } = false;

    public virtual Announcement? Announcement { get; set; }
    public virtual Account? Recipient { get; set; }
}
