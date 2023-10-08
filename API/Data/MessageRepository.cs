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
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
	public class MessageRepository : IMessageRepository
	{
		private readonly DataContext context;
		private readonly IMapper mapper;

		public MessageRepository(DataContext context, IMapper mapper)
		{
			this.context = context;
			this.mapper = mapper;
		}
		
		public void AddMessage(Message message)
		{
			context.Messages.Add(message);
		}

		public void DeleteMessage(Message message)
		{
			context.Messages.Remove(message);
		}

		public async Task<Message> GetMessage(int id)
		{
			return await context.Messages.FindAsync(id);
		}

		public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
		{
			var query = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();
			
			query = messageParams.Container switch 
			{
				"Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
				"Outbox" => query.Where(u => u.SenderUserName == messageParams.Username && u.SenderDeleted == false),
				_ => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
			};
			
			var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
			
			return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
		}

		public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
		{
			var messages = await context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
			.Include(u => u.Recipient).ThenInclude(p => p.Photos)
			.Where(m => m.RecipientUsername == currentUserName &&
			m.RecipientDeleted == false && m.SenderUserName == recipientUserName || m.RecipientUsername == recipientUserName && m.SenderDeleted == false && m.SenderUserName == currentUserName)
			.OrderBy(m => m.MessageSent)
			.ToListAsync();
			
			var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList();
			
			if(unreadMessages.Any())
			{
				foreach(var message in unreadMessages)
					message.DateRead = DateTime.UtcNow;
					
				await context.SaveChangesAsync();
			}
			
			return mapper.Map<IEnumerable<MessageDto>>(messages);
		}

		public async Task<bool> SaveAllAsync()
		{
			return await context.SaveChangesAsync() > 0;
		}
	}
}