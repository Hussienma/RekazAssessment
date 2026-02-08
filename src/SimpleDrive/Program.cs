using System.Data.SqlTypes;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimpleDrive.Adapters;
using SimpleDrive.DAOs;
using SimpleDrive.Data;
using SimpleDrive.Interfaces;
using SimpleDrive.Services;
using SimpleDrive.Storage.S3.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("MetadataDatabase")));
builder.Services.AddDbContext<FileDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("FileDatabase")));

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Configuration for the desired storage option
switch ((builder.Configuration.GetValue<string>("StorageSettings:StorageOption") ?? "LOCAL").ToUpperInvariant())
{
    case "S3":
        builder.Services.AddScoped<IStorageService, S3StorageService>();
        break;
    case "DATABASE":
        builder.Services.AddScoped<IFileRecordDao, FileRecordDao>();
        builder.Services.AddScoped<IStorageService, DatabaseStorageService>();
        break;
    case "LOCAL":
        builder.Services.AddScoped<IStorageService, LocalStorageService>();
        builder.Services.AddSingleton<IFileSystem, PhysicalFileSystem>();
        break;
    default:
        throw new ArgumentException("Invalid storage option");
}

builder.Services.AddScoped<IFileMetadataDao, FileMetadataDao>();
builder.Services.AddSingleton<ISignatureProvider, SimpleDrive.Storage.S3.Utils.SignatureProvider>();
builder.Services.AddSingleton<IS3RequestProvider, S3RequestProvider>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
   options.TokenValidationParameters = new TokenValidationParameters
   {
       ValidateIssuer = true,
       ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
   };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
