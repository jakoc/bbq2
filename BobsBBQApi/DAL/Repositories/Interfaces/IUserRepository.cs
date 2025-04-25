using BobsBBQApi.BE;

namespace BobsBBQApi.DAL.Repositories.Interfaces;

public interface IUserRepository
{
    bool RegisterUser(User user);
    User GetUserByEmail(string email);
}