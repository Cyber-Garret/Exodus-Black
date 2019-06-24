using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Web.Models;

namespace Web
{
	public class WebContext : IdentityDbContext<NeiraUser>
	{
		public WebContext(DbContextOptions<WebContext> options)
			: base(options) => Database.EnsureCreated();
	}
}
