using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
	public class UserParams
	{
		private const int MAX_PAGE_SIZE = 50;
		
		public int PageNumber { get; set; } = 1;
		
		private int pageSize = 10;
		public int PageSize
		{
			get { return pageSize; }
			set { pageSize = value > MAX_PAGE_SIZE ? pageSize : value; }
		}
		
		public string CurrentUsername { get; set; }
		public string Gender { get; set; }
		public int MinAge { get; set; } = 18;
		public int MaxAge { get; set; } = 100;
		public string OrderBy { get; set; } = "lastActive";
	}
}