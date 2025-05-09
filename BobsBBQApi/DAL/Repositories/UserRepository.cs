using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Services;

namespace BobsBBQApi.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BobsBBQContext _context;

    public UserRepository(BobsBBQContext context)
    {
        _context = context;
    }

    public bool RegisterUser(User user)
    {
        try
        {
            MonitorService.Log.Information("Registering user with email {@UserEmail}", user.Email);

            _context.Users.Add(user);
            var result = _context.SaveChanges() > 0;

            if (result)
            {
                MonitorService.Log.Information("User with email {@UserEmail} successfully registered", user.Email);
            }
            else
            {
                MonitorService.Log.Warning("User registration failed for email {@UserEmail}", user.Email);
            }

            return result;
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error occurred while registering user with email {@UserEmail}", user.Email);
            throw;
        }
    }
    public User GetUserByEmail(string email)
    {
        MonitorService.Log.Information("Fetching user with email {@UserEmail}", email);

        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user != null)
        {
            MonitorService.Log.Information("User with email {@UserEmail} found", email);
        }
        else
        {
            MonitorService.Log.Warning("User with email {@UserEmail} not found", email);
        }

        return user;
    }

}