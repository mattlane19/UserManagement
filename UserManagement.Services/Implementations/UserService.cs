using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILookupNormalizer _lookupNormalizer;
    public UserService(IDataContext dataAccess, IPasswordHasher<User> passwordHasher, ILookupNormalizer lookupNormalizer)
    {
        _dataAccess = dataAccess;
        _passwordHasher = passwordHasher;
        _lookupNormalizer = lookupNormalizer;
    }

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public IEnumerable<User> FilterByActive(bool isActive)
    {
       return _dataAccess.GetAll<User>().Where(user => user.IsActive == isActive);
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public User? GetUserById(long id)
    {
        var user = _dataAccess.GetUserById<User>(id).SingleOrDefault();
        return user ?? null;
    }

    public void Add(User user)
    {
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        user.PasswordHash = _passwordHasher.HashPassword(user, "Password_123");
        user.EmailConfirmed = true;
        user.UserName = user.Email;
        user.NormalizedEmail = _lookupNormalizer.NormalizeEmail(user.Email);
        user.NormalizedUserName = _lookupNormalizer.NormalizeName(user.Email);

        _dataAccess.Create(user);
    }

    public void Update(User user)
    {
        _dataAccess.Update(user);
    }

    public void Delete(User user)
    {
        _dataAccess.Delete(user);
    }

}
