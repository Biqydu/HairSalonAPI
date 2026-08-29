namespace HairSalon.Api.Constants;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Barber = "Barber";
    public const string Client = "Client";
    
    public static readonly string[] All = [Admin, Barber, Client];
}