using ErrorOr;

namespace HairSalon.Api.Data.Entities;

public sealed class BarberAvailability
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public Guid BarberId { get; private set; }
    public AppUser Barber { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    private BarberAvailability() { }

    public static ErrorOr<BarberAvailability> Create(
        Guid barberId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        var validationResult = Validate(barberId, startTime, endTime);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
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
        var validationResult = Validate(BarberId, newStartTime, newEndTime);
        if (validationResult.IsError)
            return validationResult.Errors;

        StartTime = newStartTime;
        EndTime = newEndTime;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success;
    }

    private static ErrorOr<Success> Validate(Guid barberId, TimeOnly startTime, TimeOnly endTime)
    {
        if (barberId == Guid.Empty)
        {
            return Error.Validation(
                "BarberAvailability.InvalidBarber", 
                "A valid barber ID must be provided.");
        }

        if (endTime <= startTime)
        {
            return Error.Validation(
                "BarberAvailability.InvalidTimeRange", 
                "End time must be later than start time.");
        }
        
        if (endTime - startTime < TimeSpan.FromHours(1))
        {
            return Error.Validation(
                "BarberAvailability.ShiftTooShort", 
                "Availability shift must be at least 1 hour long.");
        }

        return Result.Success;
    }
}