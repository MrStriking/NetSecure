using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
	public class UserDbContext : IdentityDbContext<IdentityUser>
	{
		public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().ToTable("Users");
			modelBuilder.Entity<User>().HasKey(u => new { u.Username, u.Email });

			modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
			modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

			string usersJson = System.IO.File.ReadAllText("users.json");
			List<User> users = System.Text.Json.JsonSerializer.Deserialize<List<User>>(usersJson);
			foreach (User user in users)
				modelBuilder.Entity<User>().HasData(user);
		}
	}
}
