using Microsoft.AspNetCore.Identity;

namespace TravelCore.Modules.Identity.Infrastructure.Security;

/// <summary>
/// Framework-approved one-way password hashing (ASP.NET Core Identity PasswordHasher / PBKDF2).
/// Not a custom crypto scheme. Does not choose auth ticket transport (R1 deferred).
/// </summary>
public interface IIdentityPasswordHasher
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}

public sealed class AspNetCoreIdentityPasswordHasher : IIdentityPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _hasher.HashPassword(new object(), password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(providedPassword);

        var result = _hasher.VerifyHashedPassword(new object(), hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
