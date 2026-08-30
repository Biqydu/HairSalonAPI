using ErrorOr;

namespace HairSalon.Api.Data.Entities;

public sealed class Service
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public TimeSpan Duration { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    private Service() { }
    
    public static ErrorOr<Service> Create(
        string name,
        string? description,
        decimal price,
        TimeSpan duration,
        bool isActive = true)
    {
        var validationResult = Validate(name, description, price, duration);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
        }

        return new Service
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Price = decimal.Round(price, 2),
            Duration = duration,
            IsActive = isActive
        };
    }

    public ErrorOr<Success> Update(string name, string? description, decimal price, TimeSpan duration)
    {
        var validationResult = Validate(name, description, price, duration);
        if (validationResult.IsError)
        {
            return validationResult.Errors;
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Price = decimal.Round(price, 2);
        Duration = duration;

        return Result.Success;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    
    private static ErrorOr<Success> Validate(string name, string? description, decimal price, TimeSpan duration)
    {
        var trimmedName = name.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName) || trimmedName.Length < 3 || trimmedName.Length > 50)
        {
            return Error.Validation(
                "Service.InvalidName", 
                "Service name is required and must be between 3 and 50 characters.");
        }
        
        var trimmedDescription = description?.Trim();
        if (!string.IsNullOrEmpty(trimmedDescription) && trimmedDescription.Length is < 5 or > 500)
        {
            return Error.Validation(
                "Service.InvalidDescription", 
                "Description must be between 5 and 500 characters when provided.");
        }
        
        if (price <= 0)
        {
            return Error.Validation(
                "Service.InvalidPrice", 
                "Service price must be greater than 0.");
        }
        
        var totalMinutes = (int)duration.TotalMinutes;

        if (totalMinutes is < 5 or > 360)
        {
            return Error.Validation(
                "Service.InvalidDuration", 
                "Duration must be between 5 and 360 minutes.");
        }

        if (totalMinutes % 5 != 0)
        {
            return Error.Validation(
                "Service.DurationNotMultipleOfFive", 
                "Duration must be a multiple of 5 minutes.");
        }

        return Result.Success;
    }
}