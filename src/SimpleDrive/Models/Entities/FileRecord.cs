using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleDrive.Entities;


public class FileRecord
{
    [Key]
    [Required]
    public string Id { get; set; }

    [Required]
    [ForeignKey("FileMetadata")]
    public string MetadataId {get; set;}
    
    public FileMetadata metadata {get;set;}
}