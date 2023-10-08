

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
	public class TokenService : ITokenService
	{
		private readonly SymmetricSecurityKey key;

		public TokenService(IConfiguration config)
		{
			this.key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
		}

		public string CreateToken(AppUser user)
		{
			List<Claim> claims = new List<Claim>{
				new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
			};

			SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
			
			SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor(){
				Subject=new ClaimsIdentity(claims),
				Expires=DateTime.Now.AddDays(7),
				SigningCredentials=credentials
			};

			var tokenHandler = new JwtSecurityTokenHandler();

			var token = tokenHandler.CreateToken(descriptor);

			return tokenHandler.WriteToken(token);
		}
	}
}