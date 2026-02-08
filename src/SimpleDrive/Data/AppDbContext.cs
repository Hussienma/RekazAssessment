using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using SimpleDrive.Entities;

namespace SimpleDrive.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<FileMetadata> Files => Set<FileMetadata>();

    public DbSet<FileRecord> Records => Set<FileRecord>();
}