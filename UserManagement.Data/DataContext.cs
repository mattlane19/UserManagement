using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data;

public class DataContext : DbContext, IDataContext
{
    private static long _nextUserId = 12;

    public DataContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseInMemoryDatabase("UserManagement.Data.DataContext");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Log>()
        .HasOne(l => l.User)
        .WithMany(u => u.Logs)
        .HasForeignKey(l => l.UserId)
        .OnDelete(DeleteBehavior.Restrict);

        model.Entity<User>().HasData(new[]
        {
            new User { Id = 1, Forename = "Peter", Surname = "Loew", DateOfBirth = new DateTime(1985, 4, 12), Email = "ploew@example.com", IsActive = true },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", DateOfBirth = new DateTime(1990, 7, 23), Email = "bfgates@example.com", IsActive = true },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", DateOfBirth = new DateTime(1978, 1, 15), Email = "ctroy@example.com", IsActive = false },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", DateOfBirth = new DateTime(1983, 11, 30), Email = "mraines@example.com", IsActive = true },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", DateOfBirth = new DateTime(1992, 6, 5), Email = "sgodspeed@example.com", IsActive = true },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", DateOfBirth = new DateTime(1998, 3, 19), Email = "himcdunnough@example.com", IsActive = true },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", DateOfBirth = new DateTime(1975, 10, 21), Email = "cpoe@example.com", IsActive = false },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", DateOfBirth = new DateTime(1995, 9, 14), Email = "emalus@example.com", IsActive = false },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", DateOfBirth = new DateTime(1980, 5, 8), Email = "dmacready@example.com", IsActive = false },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", DateOfBirth = new DateTime(1987, 2, 27), Email = "jblaze@example.com", IsActive = true },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", DateOfBirth = new DateTime(1993, 12, 3), Email = "rfeld@example.com", IsActive = true },
        });

        model.Entity<Log>().HasData(new[]
        {
            new Log { Id = 1, UserId = 1, Action = "Add User", UserName = "Admin", Details = "User added - , Id: 1, Forename: Peter, Surname: Loew, DateOfBirth: 01-01-2000, Email: peter@example.com, IsActive: true", Timestamp = DateTime.UtcNow },
            new Log { Id = 2, UserId = 2, Action = "Edit User", UserName = "Admin", Details = "User updated - , Forename changed from 'Benjamin Franklin' to 'Ben', Email changed from 'ben@example.com' to 'bfgates@example.com'", Timestamp = DateTime.UtcNow },
            new Log { Id = 3, UserId = 3, Action = "Delete User", UserName = "Admin", Details = "User deleted - , Id: 3, Email: castro@example.com", Timestamp = DateTime.UtcNow },
            new Log { Id = 4, UserId = 4, Action = "Add User", UserName = "Admin", Details = "User added - , Id: 4, Forename: Memphis, Surname: Raines, DateOfBirth: 02-02-2001, Email: mraines@example.com, IsActive: true", Timestamp = DateTime.UtcNow },
            new Log { Id = 5, UserId = 5, Action = "Edit User", UserName = "Admin", Details = "User updated - , Surname changed from 'Stan' to 'Stanley', IsActive changed from 'true' to 'false'", Timestamp = DateTime.UtcNow },
            new Log { Id = 6, UserId = 6, Action = "Delete User", UserName = "Admin", Details = "User deleted - , Id: 6, Email: hi@example.com", Timestamp = DateTime.UtcNow },
            new Log { Id = 7, UserId = 7, Action = "Add User", UserName = "Admin", Details = "User added - , Id: 7, Forename: Cameron, Surname: Poe, DateOfBirth: 03-03-1998, Email: cameron@example.com, IsActive: true", Timestamp = DateTime.UtcNow },
            new Log { Id = 8, UserId = 8, Action = "Edit User", UserName = "Admin", Details = "User updated - , Email changed from 'edward@example.com' to 'edward.jones@example.com'", Timestamp = DateTime.UtcNow },
            new Log { Id = 9, UserId = 9, Action = "Delete User", UserName = "Admin", Details = "User deleted - , Id: 9, Email: dmacready@example.com", Timestamp = DateTime.UtcNow },
            new Log { Id = 10, UserId = 10, Action = "Add User", UserName = "Admin", Details = "User added - , Id: 10, Forename: Johnny, Surname: Blaze, DateOfBirth: 04-04-1995, Email: jblaze@example.com, IsActive: true", Timestamp = DateTime.UtcNow }
        });
    }

    public DbSet<User>? Users { get; set; }
    public DbSet<Log>? Logs { get; set; }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        => base.Set<TEntity>();
    public IQueryable<TEntity> GetById<TEntity>(long id) where TEntity : class
    {
        return base.Set<TEntity>().Where(e => EF.Property<long>(e, "Id") == id);
    }
    public IQueryable<TEntity> GetUserById<TEntity>(long id) where TEntity : class
    {
        return base.Set<TEntity>()
            .Include("Logs")
            .Where(e => EF.Property<long>(e, "Id") == id);
    }

    public void Create<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is User user)
        {
            user.Id = _nextUserId++;
        }
        base.Add(entity);
        SaveChanges();
    }

    public new void Update<TEntity>(TEntity entity) where TEntity : class
    {
        base.Update(entity);
        SaveChanges();
    }

    public void Delete<TEntity>(TEntity entity) where TEntity : class
    {
        base.Remove(entity);
        SaveChanges();
    }
}
