using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace web_app_project.Models;

public class Tag
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
