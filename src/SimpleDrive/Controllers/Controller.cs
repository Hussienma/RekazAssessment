using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimpleDrive.DTOs;
using SimpleDrive.Interfaces;

namespace SimpleDrive.Controllers;

[ApiController]
[Route("v1")]
public class Controller : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly IConfiguration _config;
    public Controller(IStorageService storageService, IConfiguration config)
    {
        _storageService = storageService;
        _config = config;
    }

    // For simplicity's sake, we doing auth business logic in the same controller as the blob operations
    // Were are not unit testing this one 
    [HttpPost("auth")]
    public async Task<IActionResult> Authenticate(string name)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("Jwt:Key")!));
        var creditials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        Claim[] calims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(issuer: _config.GetValue<string>("Jwt:Issuer"), 
                        audience: _config.GetValue<string>("Jwt:Audience"),
                        claims: calims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: creditials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(tokenString);
    }

    [Authorize]
    [HttpPost("blobs")]
    public async Task<IActionResult> UploadFileAsync(FileUploadRequest request)
    {
        var response = await _storageService.UploadFileAsync(request);

        if(!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Value);
    }

    [Authorize]
    [HttpGet("blobs/{id}")]
    public async Task<IActionResult> GetFileById(string id)
    {
        var response = await _storageService.GetFileById(id);

        if(!response.Success)
        return NotFound(response.Message);

        return Ok(response.Value);
    }
}