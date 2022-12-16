namespace NET6_WebApi_JWT_Role_Based_Auth.Helpers;

using Microsoft.EntityFrameworkCore;
using NET6_WebApi_JWT_Role_Based_Auth.Entities;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }

    private readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // in memory database used for simplicity, change to a real db for production applications
        options.UseInMemoryDatabase("TestDb");
    }
}