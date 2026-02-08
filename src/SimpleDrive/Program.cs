using Microsoft.EntityFrameworkCore;
using SimpleDrive.DAOs;
using SimpleDrive.Data;
using SimpleDrive.Interfaces;
using SimpleDrive.Services;
using SimpleDrive.Storage.S3.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=SimpleDrive.db"));

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStorageService, S3StorageService>();
builder.Services.AddScoped<IFileDao, FileDao>();
builder.Services.AddSingleton<ISignatureProvider, SignatureProvider>();
builder.Services.AddSingleton<IS3RequestProvider, S3RequestProvider>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
