using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	public class AccountController:BaseApiController
	{
		private readonly DataContext context;
		private readonly ITokenService tokenService;
		private readonly IMapper mapper;

		public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
		{
			this.context = context;
			this.tokenService = tokenService;
			this.mapper = mapper;
		}

		[HttpPost]
		[Route("register")]
		public async Task<ActionResult<UserDto>>Register(RegisterDto registerDto)
		{
			if(await UserExists(registerDto.Username))
				return BadRequest("Username is exist");
				
			var user = mapper.Map<AppUser>(registerDto);

			using var hmac = new HMACSHA512();
			
			user.UserName=registerDto.Username.ToLower();
			user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
			user.PasswordSalt=hmac.Key;
			
			context.Users.Add(user);
			await context.SaveChangesAsync();
			
			return new UserDto{
				Username=user.UserName,
				Token=tokenService.CreateToken(user),
				KnownAs=user.KnownAs
			};
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
			AppUser user = await context.Users.Include(user => user.Photos).SingleOrDefaultAsync(usr => usr.UserName==loginDto.Username);

			if(user is null)
				return Unauthorized("invalid username");

			using var hmac = new HMACSHA512(user.PasswordSalt);
			var computeHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
			
			for (int i = 0; i < computeHash.Length; i++)
			{
				if(computeHash[i]!=user.PasswordHash[i])
					return Unauthorized("invalid password");
			}

			return new UserDto{
				Username = user.UserName,
				Token = tokenService.CreateToken(user),
				PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain)?.Url,
				KnownAs=user.KnownAs
			};
		}

		private async Task<bool> UserExists(string username){
			return await context.Users.AnyAsync(usr => usr.UserName==username.ToLower());
		}
	}
}