using System.ComponentModel;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController: BaseApiController{
	private readonly IUnitOfWork unitOfWork;
	private readonly IMapper mapper;
	private readonly IPhotoService photoService;

	public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
	{
		this.unitOfWork = unitOfWork;
		this.mapper = mapper;
		this.photoService = photoService;
	}
	
	[HttpGet]
	public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams){
		var gender = await unitOfWork.UserRepository
		.GetUserGender(User.GetUsername());
		
		userParams.CurrentUsername = User.GetUsername();
		
		if(string.IsNullOrEmpty(userParams.Gender))
		{
			userParams.Gender = gender == "male"? "female": "male";
		}
		
		var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
		
		Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
		
		return Ok(users);
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<MemberDto>> GetUser(int id){
		return Ok(mapper.Map<MemberDto>(await unitOfWork.UserRepository.GetUserByIdAsync(id)));
	}

	[HttpGet("{username}")]
	public async Task<ActionResult<MemberDto>> GetUser(string username){
		return Ok(await unitOfWork.UserRepository.GetMemberAsync(username));
	}
	
	[HttpPut]
	public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
	{
		var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
		
		if(user == null) 
			return NotFound();
		
		mapper.Map(memberUpdateDto, user);
		
		if(await unitOfWork.Complete())
			return NoContent();
			
		return BadRequest("Failed to update user!");
	}
	
	[HttpPost("add-photo")]
	public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
	{
		var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
		
		if(user == null)
			return NotFound();
			
		var result = await photoService.AddPhotoAsync(file);
		
		if(result.Error != null)
			return BadRequest(result.Error.Message);
		
		var photo = new Photo
		{
			Url = result.SecureUri.AbsoluteUri,	
			PublicId = result.PublicId
		};
		
		photo.IsMain = user.Photos.Count == 0;
		user.Photos.Add(photo);
		
		if(await unitOfWork.Complete())
		{
			return CreatedAtAction(nameof(GetUser), new 
			{
				username = user.UserName
			}, mapper.Map<PhotoDto>(photo));		
		}
		
		return BadRequest("Problem adding photo");
	} 
	
	[HttpPut("set-main-photo/{photoId}")]
	public async Task<ActionResult> SetMainPhoto(int photoId)
	{
		var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User
		.GetUsername());
		
		if(user == null) return NotFound();
		
		var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
		
		if(photo.IsMain) return BadRequest("this is already your main photo");
		
		var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
		
		if(currentMain != null)
			currentMain.IsMain = false;
		
		photo.IsMain = true;
		
		if(await unitOfWork.Complete())
			return NoContent();
		
		return BadRequest("Problem setting main photo");
	}
	
	[HttpDelete("delete-photo/{photoId}")]
	public async Task<ActionResult>DeletePhoto(int photoId)
	{
		var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User
		.GetUsername());
		
		if(user == null) return NotFound();
		
		var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
		
		if(photo == null) return NotFound();
		
		if(photo.IsMain) return BadRequest("You cannot delete your main photo");
		
		
		if(photo.PublicId != null)
		{
			var res = await photoService.DeletePhotoAsync(photo.PublicId);
			if(res.Error != null)
				return BadRequest(res.Error.Message);
		}
		
		user.Photos.Remove(photo);
		
		if(await unitOfWork.Complete())
			return Ok();
			
		return BadRequest("Problem deleting photo");
	}
}