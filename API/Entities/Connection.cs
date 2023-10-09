using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
	public class Connection
	{
		public string ConnectionId { get; set; }
		public string Username { get; set; }
		
		public Connection()
		{
			
		}
		
		public Connection(string connectionId, string Username)
		{
			this.ConnectionId = connectionId;
			this.Username = Username;
		}
	}
}