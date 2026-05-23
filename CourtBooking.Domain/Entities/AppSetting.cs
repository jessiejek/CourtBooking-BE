using System.ComponentModel.DataAnnotations;

namespace CourtBooking.Domain.Entities;

public class AppSetting
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
