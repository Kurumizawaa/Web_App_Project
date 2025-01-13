using System.ComponentModel.DataAnnotations;

namespace web_app_project.Models;

public class Post
{
    [Key]
    public int Id { get; set;}
    [Required]
    public string Title { get; set;}
    [Required]
    public string Description { get; set;}
    [Required]
    public List<string> Tags { get; set;} 
    [Required]
    public int AmountAccept { get; set;}
    [Required]
    public string AcceptType { get; set;}
    [Required]
    public string CloseDate { get; set;}
}
