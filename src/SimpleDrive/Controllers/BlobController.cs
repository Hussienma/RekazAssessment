using Microsoft.AspNetCore.Mvc;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;

namespace SimpleDrive.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class BlobController : ControllerBase
{
    private readonly IStorageService _storageService;
    public BlobController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileAsync(FileUploadRequest request)
    {
        var response = await _storageService.UploadFileAsync(request.id, request.data);

        if(!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFileById(string id)
    {
        var response = await _storageService.GetFileById(id);

        if(!response.Success)
        return NotFound(response.Message);

        return Ok(response.Value);
    }
}