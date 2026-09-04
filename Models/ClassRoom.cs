using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class ClassRoom
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Shift { get; set; }

    public List<ApplicationUser> Students { get; set; } = [];
}
