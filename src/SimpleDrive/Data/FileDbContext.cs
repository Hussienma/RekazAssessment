using Microsoft.EntityFrameworkCore;
using SimpleDrive.Entities;

namespace SimpleDrive.Data;

public class FileDbContext : DbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options)
    {
        
    }
    public DbSet<FileRecord> Records => Set<FileRecord>();

}