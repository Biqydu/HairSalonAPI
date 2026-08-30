using ErrorOr;

namespace HairSalon.Api.Data.Entities;

public sealed class BarberAvailability
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid BarberId { get; private set; }
    public AppUser Barber { get; private set; } = null!;
    
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    
    private BarberAvailability() { }
    
    public static ErrorOr<BarberAvailability> Create(
        Guid barberId, 
        DayOfWeek dayOfWeek, 
        TimeOnly startTime, 
        TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            return Error.Validation(
                "BarberAvailability.InvalidTimeRange", 
                "The work end time must be later than the start time.");
        }
        
        if (endTime - startTime < TimeSpan.FromHours(1))
        {
            return Error.Validation(
                "BarberAvailability.ShiftTooShort", 
                "The work schedule must cover a period of at least one hour.");
        }

        return new BarberAvailability
        {
            BarberId = barberId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime
        };
    }
    
    public ErrorOr<Success> UpdateHours(TimeOnly newStartTime, TimeOnly newEndTime)
    {
        if (newEndTime <= newStartTime)
        {
            return Error.Validation(
                "BarberAvailability.InvalidTimeRange", 
                "The work end time must be later than the start time.");
        }

        StartTime = newStartTime;
        EndTime = newEndTime;

        return Result.Success;
    }
}