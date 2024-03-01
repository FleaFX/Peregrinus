namespace Peregrinus.Model; 

/// <summary>
/// Holds information about a SQL user, used when provisioning logins.
/// </summary>
/// <param name="Name">The name of the login.</param>
/// <param name="Password">The password for the login.</param>
public record struct LoginInfo(string Name, string Password);