using System;
using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Tag
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
}
