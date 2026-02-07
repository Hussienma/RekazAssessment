using Microsoft.EntityFrameworkCore;
using SimpleDrive.Entities;

namespace SimpleDrive.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<FileMetadata> Files => Set<FileMetadata>();
}