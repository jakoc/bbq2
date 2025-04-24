using BobsBBQApi.BE;

namespace BobsBBQApi.DAL.Repositories.Interfaces;

public interface IUserRepository
{
    void RegisterUser(User user);
    User GetUserByEmail(string email);
}