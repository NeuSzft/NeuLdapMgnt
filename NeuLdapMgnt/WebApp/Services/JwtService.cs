using System.IdentityModel.Tokens.Jwt;

namespace NeuLdapMgnt.WebApp.Services;

/// <summary>
/// Service for decoding and managing JWT tokens.
/// </summary>
public class JwtService
{
	/// <summary>
	/// Decodes a JWT token.
	/// </summary>
	/// <param name="token">The JWT token to decode.</param>
	/// <returns>The decoded JWT token.</returns>
	public JwtSecurityToken DecodeToken(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		return handler.ReadToken(token) as JwtSecurityToken
		       ?? throw new ArgumentException("Invalid JWT token.");
	}

	/// <summary>
	/// Gets the expiration date of a JWT token.
	/// </summary>
	/// <param name="token">The JWT token.</param>
	/// <returns>The expiration date of the JWT token.</returns>
	private DateTime GetExpireDate(JwtSecurityToken token)
	{
		return DateTimeOffset.FromUnixTimeSeconds(token.Payload.Expiration!.Value).DateTime;
	}

	/// <summary>
	/// Checks if a JWT token is expired.
	/// </summary>
	/// <param name="token">The JWT token to check.</param>
	/// <returns>True if the JWT token is expired, otherwise false.</returns>
	public bool IsExpired(JwtSecurityToken token)
	{
		return DateTime.UtcNow > GetExpireDate(token);
	}

	/// <summary>
	/// Gets the remaining time until a JWT token expires.
	/// </summary>
	/// <param name="token">The JWT token.</param>
	/// <returns>The remaining time until the JWT token expires.</returns>
	public TimeSpan GetRemainingTime(JwtSecurityToken token)
	{
		return GetExpireDate(token) - DateTime.UtcNow;
	}
}
