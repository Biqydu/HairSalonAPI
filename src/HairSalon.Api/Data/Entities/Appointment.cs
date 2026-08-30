using ErrorOr;
using HairSalon.Api.Data.Enums;

namespace HairSalon.Api.Data.Entities;

public sealed class Appointment
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;
    
    public decimal PriceAtBooking { get; private set; }
    public TimeSpan DurationAtBooking { get; private set; }

    public Guid ServiceId { get; private set; }
    public Service Service { get; private set; } = null!;

    public Guid ClientId { get; private set; }
    public AppUser Client { get; private set; } = null!;

    public Guid BarberId { get; private set; }
    public AppUser Barber { get; private set; } = null!;

    public string? CancellationReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    
    private Appointment() { }

    public static ErrorOr<Appointment> Create(
        Service service,
        Guid clientId,
        Guid barberId,
        DateTimeOffset startTime)
    {
        if (!service.IsActive)
        {
            return Error.Validation(
                "Appointment.InactiveService", 
                "Cannot schedule an appointment for an inactive service.");
        }

        if (startTime <= DateTimeOffset.UtcNow)
        {
            return Error.Validation(
                "Appointment.InvalidStartTime", 
                "Appointment start time must be in the future.");
        }
        
        if (clientId == Guid.Empty)
        {
            return Error.Validation(
                "Appointment.InvalidClient", 
                "A valid client ID must be provided.");
        }

        if (barberId == Guid.Empty)
        {
            return Error.Validation(
                "Appointment.InvalidBarber", 
                "A valid barber ID must be provided.");
        }

        if (clientId == barberId)
        {
            return Error.Validation(
                "Appointment.SameClientAndBarber", 
                "Client and barber cannot be the same user.");
        }

        var endTime = startTime.Add(service.Duration);

        return new Appointment
        {
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentStatus.Scheduled,
            PriceAtBooking = service.Price,
            DurationAtBooking = service.Duration,
            ServiceId = service.Id,
            ClientId = clientId,
            BarberId = barberId
        };
    }

    public ErrorOr<Success> Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            return Error.Conflict(
                "Appointment.CannotConfirm", 
                $"Only appointments in '{AppointmentStatus.Scheduled}' status can be confirmed.");
        }

        Status = AppointmentStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> Complete()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.Scheduled)
        {
            return Error.Conflict(
                "Appointment.CannotComplete", 
                "Only scheduled or confirmed appointments can be marked as completed.");
        }

        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> Cancel(string? reason = null)
    {
        if (Status == AppointmentStatus.Completed)
        {
            return Error.Conflict(
                "Appointment.CannotCancelCompleted", 
                "Completed appointments cannot be cancelled.");
        }

        if (Status == AppointmentStatus.Cancelled)
        {
            return Error.Conflict(
                "Appointment.AlreadyCancelled", 
                "Appointment is already cancelled.");
        }
        
        var trimmedReason = reason?.Trim();
        
        if (!string.IsNullOrEmpty(trimmedReason) && trimmedReason.Length is < 3 or > 200)
        {
            return Error.Validation(
                "Appointment.InvalidCancellationReason", 
                "Cancellation reason must be between 3 and 200 characters if provided.");
        }

        Status = AppointmentStatus.Cancelled;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : trimmedReason;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success;
    }

    public ErrorOr<Success> Reschedule(DateTimeOffset newStartTime)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            return Error.Conflict(
                "Appointment.CannotRescheduleFinished", 
                "Cannot reschedule a completed or cancelled appointment.");
        }

        if (newStartTime <= DateTimeOffset.UtcNow)
        {
            return Error.Validation(
                "Appointment.InvalidRescheduleTime", 
                "New start time must be in the future.");
        }

        StartTime = newStartTime;
        EndTime = newStartTime.Add(DurationAtBooking);
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success;
    }
}