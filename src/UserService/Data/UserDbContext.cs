
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;
public class UserDbContext : DbContext
{
	public UserDbContext(DbContextOptions<UserDbContext> options): base(options)
	{
			
	}

	public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
		//Ensure email is unique
		modelBuilder.Entity<User>()
			.HasIndex(e => e.Email)
			.IsUnique();
    }
}
