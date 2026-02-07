using System.ComponentModel.DataAnnotations;

namespace SimpleDrive.Entities;


public class FileMetadata
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    public int Size {get; set;}

    [Required]
    public DateTime CreatedAt {get; set;} = DateTime.Now;
}