using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class ApplicationUser
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? RegistrationCode { get; set; }

    public UserRole Role { get; set; }
}
