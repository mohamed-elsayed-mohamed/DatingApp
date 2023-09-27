using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController: BaseApiController{
	private readonly IUserRepository userRepository;
	private readonly IMapper mapper;

	public UsersController(IUserRepository userRepository, IMapper mapper)
	{
		this.userRepository = userRepository;
		this.mapper = mapper;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers(){
		return Ok(await userRepository.GetMembersAsync());
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<MemberDto>> GetUser(int id){
		return Ok(mapper.Map<MemberDto>(await userRepository.GetUserByIdAsync(id)));
	}
	
	[HttpGet("{username}")]
	public async Task<ActionResult<MemberDto>> GetUser(string username){
		return Ok(await userRepository.GetMemberAsync(username));
	}
	
	[HttpPut]
	public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
	{
		var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		var user = await userRepository.GetUserByUsernameAsync(username);
		
		if(user == null) 
			return NotFound();
		
		mapper.Map(memberUpdateDto, user);
		
		if(await userRepository.SaveAllAsync())
			return NoContent();
			
		return BadRequest("Failed to update user!");
	}
}