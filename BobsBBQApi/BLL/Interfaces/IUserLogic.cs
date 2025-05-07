using BobsBBQApi.BE;

namespace BobsBBQApi.BLL.Interfaces;

public interface IUserLogic
{
    User RegisterUser(string username, string password, string email, int phoneNumber, string role);
    (User, string token) LoginUser(string email, string password);
}