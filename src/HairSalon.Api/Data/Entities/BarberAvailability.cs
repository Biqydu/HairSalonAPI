namespace HairSalon.Api.Data.Entities;

public sealed class BarberAvailability
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required Guid BarberId { get; set; }
    public AppUser Barber { get; set; } = null!;
    public required DayOfWeek DayOfWeek { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; } 
}