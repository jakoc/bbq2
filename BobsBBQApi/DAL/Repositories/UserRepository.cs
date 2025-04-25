using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;

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
        _context.Users.Add(user);
        return _context.SaveChanges() > 0;
    }
    public User GetUserByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

}