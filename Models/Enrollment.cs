using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Enrollment
{
    [Key]
    public int AccountId { get; set; }
    [Key]
    public int PostId { get; set; }

    public virtual Account? Account { get; set; }
    public virtual Post? Post { get; set; }

    public DateTime EnrolledAt { get; set; }
}