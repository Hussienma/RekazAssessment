using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleDrive.Entities;


public class FileRecord
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    public string Data {get; set;}
    
}