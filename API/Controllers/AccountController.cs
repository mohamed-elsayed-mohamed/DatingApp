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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	public class AccountController:BaseApiController
	{
		private readonly UserManager<AppUser> userManager;
		private readonly ITokenService tokenService;
		private readonly IMapper mapper;

		public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
		{
			this.userManager = userManager;
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

			user.UserName=registerDto.Username.ToLower();
			
			var result = await userManager.CreateAsync(user, registerDto.Password);
			
			if(!result.Succeeded)
				return BadRequest(result.Errors);
				
			var roleResult = await userManager.AddToRoleAsync(user, "Member");
			
			if(!roleResult.Succeeded)
				return BadRequest(result.Errors);
			
			return new UserDto{
				Username = user.UserName,
				Token = await tokenService.CreateToken(user),
				KnownAs = user.KnownAs,
				Gender = user.Gender
			};
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
			AppUser user = await userManager.Users.Include(user => user.Photos).SingleOrDefaultAsync(usr => usr.UserName==loginDto.Username);

			if(user is null)
				return Unauthorized("invalid username");
				
			var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
			
			if(!result)
				return Unauthorized("Invalid Password");

			return new UserDto{
				Username = user.UserName,
				Token = await tokenService.CreateToken(user),
				PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain)?.Url,
				KnownAs = user.KnownAs,
				Gender = user.Gender
			};
		}

		private async Task<bool> UserExists(string username){
			return await userManager.Users.AnyAsync(usr => usr.UserName==username.ToLower());
		}
	}
}