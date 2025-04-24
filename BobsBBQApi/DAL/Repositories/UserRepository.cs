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

    public void RegisterUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }
    public User GetUserByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

}