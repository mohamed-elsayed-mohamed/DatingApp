using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.Controllers
{
	public class BuggyController: BaseApiController
	{
		private readonly DataContext context;
		public BuggyController(DataContext context)
		{
			this.context = context;
		}
		
		[HttpGet("auth")]
		public ActionResult<string> GetSecret()
		{
			return "Secret text";
		}
		
		[HttpGet("not-found")]
		public ActionResult<AppUser> GetNotFound()
		{
			var thing = context.Users.Find(-1);
			if(thing == null)
				return NotFound();
				
			return thing;
		}
		
		[HttpGet("server-error")]
		public ActionResult<string> GetServerError()
		{
			var thing = context.Users.Find(-1);
			var thingToReturn = thing.ToString();
			
			return thingToReturn;
		}
		
		[HttpGet("bad-request")]
		public ActionResult<string> GetBadRequest()
		{
			return BadRequest("This was not a good request");
		}
	}
}