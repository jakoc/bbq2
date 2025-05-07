using BobsBBQApi.BE;

namespace BobsBBQApi.BLL.Interfaces;

public interface IUserLogic
{
    User RegisterUser(string username, string password, string email, int phoneNumber, string role);
    string LoginUser(string email, string password);
}