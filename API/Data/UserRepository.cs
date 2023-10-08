using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
	public class UserRepository : IUserRepository
	{
		private readonly DataContext context;
		private readonly IMapper mapper;

		public UserRepository(DataContext context, IMapper mapper)
		{
			this.context = context;
			this.mapper = mapper;
		}

		public async Task<MemberDto> GetMemberAsync(string username)
		{
			return await context.Users.Where(usr => usr.UserName == username)
			.ProjectTo<MemberDto>(mapper.ConfigurationProvider).SingleOrDefaultAsync();
		}

		public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
		{
			var query = context.Users.AsQueryable();
			query = query.Where(user => user.UserName != userParams.CurrentUsername);
			query = query.Where(user => user.Gender == userParams.Gender);
			
			var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));

			var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
			
			query = query.Where(user => user.DateOfBirth >= minDob && user.DateOfBirth <= maxDob);
			
			query = userParams.OrderBy switch
			{
				"created" => query.OrderByDescending(u => u.Created),
				_ => query.OrderByDescending(u => u.LastActive)
			};
			
			var iQuery = query.AsNoTracking().ProjectTo<MemberDto>(mapper.ConfigurationProvider);
			
			return await PagedList<MemberDto>.CreateAsync(iQuery, userParams.PageNumber, userParams.PageSize);
		}

		public async Task<AppUser> GetUserByIdAsync(int id)
		{
			return await context.Users.FindAsync(id);
		}

		public async Task<AppUser> GetUserByUsernameAsync(string username)
		{
			return await context.Users.Include(usr => usr.Photos).SingleOrDefaultAsync(usr => usr.UserName == username);
		}

		public async Task<IEnumerable<AppUser>> GetUsersAsync()
		{
			return await context.Users.Include(usr => usr.Photos).ToListAsync();
		}

		public async Task<bool> SaveAllAsync()
		{
			return await context.SaveChangesAsync() != 0;
		}

		public void Update(AppUser user)
		{
			context.Entry(user).State = EntityState.Modified;
		}
	}
}