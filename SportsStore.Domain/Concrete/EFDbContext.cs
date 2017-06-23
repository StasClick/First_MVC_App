using SportsStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsStore.Domain.Concrete
{
	public class EFDbContext : DbContext
	{
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Database.Log = (query) => Write(query);
		}

		private void Write(string query)
		{
			Trace.WriteLine(query);
		}
	}
}
