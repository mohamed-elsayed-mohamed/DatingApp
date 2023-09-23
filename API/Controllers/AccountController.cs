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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController:BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<UserDto>>Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
                return BadRequest("Username is exist");

            using var hmac = new HMACSHA512();

            AppUser user = new AppUser{
                UserName=registerDto.Username.ToLower(),
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt=hmac.Key
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            return new UserDto{
                Username=user.UserName,
                Token=tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            AppUser user = await context.Users.SingleOrDefaultAsync(usr => usr.UserName==loginDto.Username);

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
                Username=user.UserName,
                Token=tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username){
            return await context.Users.AnyAsync(usr => usr.UserName==username.ToLower());
        }
    }
}