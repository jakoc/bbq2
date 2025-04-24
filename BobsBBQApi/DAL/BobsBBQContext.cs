using BobsBBQApi.BE;
using Microsoft.EntityFrameworkCore;

namespace BobsBBQApi.DAL;

public class BobsBBQContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    
    public BobsBBQContext(DbContextOptions<BobsBBQContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
    }
}