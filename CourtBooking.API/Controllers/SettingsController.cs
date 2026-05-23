using CourtBooking.Application.Common.Interfaces;
using CourtBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SettingsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("scoring")]
    public async Task<IActionResult> GetScoringSetting()
    {
        var setting = await _unitOfWork.Repository<AppSetting>()
            .FirstOrDefaultAsync(s => s.Key == "ScoringRequiresBooking");

        return Ok(new
        {
            scoringRequiresBooking = setting?.Value?.ToLower() == "true"
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("scoring")]
    public async Task<IActionResult> UpdateScoringSetting([FromBody] UpdateScoringSettingRequest request)
    {
        var setting = await _unitOfWork.Repository<AppSetting>()
            .FirstOrDefaultAsync(s => s.Key == "ScoringRequiresBooking");

        if (setting == null)
        {
            setting = new AppSetting
            {
                Key = "ScoringRequiresBooking",
                Value = request.Value.ToString().ToLower(),
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Repository<AppSetting>().Add(setting);
        }
        else
        {
            setting.Value = request.Value.ToString().ToLower();
            setting.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<AppSetting>().Update(setting);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { scoringRequiresBooking = request.Value });
    }
}

public class UpdateScoringSettingRequest
{
    public bool Value { get; set; }
}
