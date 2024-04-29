using System.IdentityModel.Tokens.Jwt;

namespace NeuLdapMgnt.WebApp.Services;

public class JwtService {
	public JwtSecurityToken DecodeToken(string token) {
		var handler = new JwtSecurityTokenHandler();
		return handler.ReadToken(token) as JwtSecurityToken
		       ?? throw new ArgumentException("Invalid JWT token.");
	}

	public DateTime GetExpireDate(JwtSecurityToken token) {
		return DateTimeOffset.FromUnixTimeSeconds(token.Payload.Expiration!.Value).DateTime;
	}

	public bool IsExpired(JwtSecurityToken token) {
		return DateTime.UtcNow > GetExpireDate(token);
	}

	public TimeSpan GetRemainingTime(JwtSecurityToken token) {
		return GetExpireDate(token) - DateTime.UtcNow;
	}
}
