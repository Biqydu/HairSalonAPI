using HairSalon.Api.Data.Enums;

namespace HairSalon.Api.Data.Entities;

public sealed class Appointment
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }
    public required AppointmentStatus Status { get; set; }
    public required Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public required Guid ClientId { get; set; }
    public AppUser Client { get; set; } = null!;
    public required Guid BarberId { get; set; }
    public AppUser Barber { get; set; } = null!;
}