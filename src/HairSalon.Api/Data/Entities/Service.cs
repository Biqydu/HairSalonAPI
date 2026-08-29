namespace HairSalon.Api.Data.Entities;

public sealed class Service
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; }
    public required TimeSpan Duration { get; set; }
    public bool IsActive { get; set; } = true;
}