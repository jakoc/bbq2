using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers.Interfaces;

namespace BobsBBQApi.BLL;

public class UserLogic: IUserLogic
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordEncrypter _passwordEncrypter;
    private readonly IJwtToken _jwtToken;
    private readonly IEmail _email;

    public UserLogic(IUserRepository userRepository, IPasswordEncrypter passwordEncrypter, IJwtToken jwtToken, IEmail email)
    {
        _passwordEncrypter = passwordEncrypter;
        _userRepository = userRepository;
        _jwtToken = jwtToken;
        _email = email;

    }
    public string LoginUser(string email, string password)
    {

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.");
        }

        var user = _userRepository.GetUserByEmail(email);
        if (user == null)
        {
            throw new ArgumentException("Invalid email or password.");
        }

        var isValidPassword = _passwordEncrypter.EncryptPasswordWithUsersSalt(password, user.UserSalt);
        
        if (isValidPassword != user.UserHash)
        {
            throw new ArgumentException("Invalid email or password.");
        }

        var token = _jwtToken.GenerateJwtToken(user.UserId, user.Email, user.UserRole);

        return token;

    }
    public User RegisterUser(string username, string password, string email, int phoneNumber, string role)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.");
        }
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be null or empty.");
        }

        var existingUser = _userRepository.GetUserByEmail(email);
        if (existingUser != null)
        {
            throw new ArgumentException("Email is already taken.");
        }
        
        var (hash, salt) = _passwordEncrypter.EncryptPassword(password);
        
        var user = new User
        {
            UserId = Guid.NewGuid(),
            UserHash = hash,
            UserSalt = salt,
            UserName = username,
            Email = email,
            PhoneNumber = phoneNumber,
            UserRole = role,
        };
        
        var success = _userRepository.RegisterUser(user);
        if(!success)
        {
            throw new ArgumentException("Error registering user");
        }
        _email.SendSuccessfullAccountCreationEmail( user.Email, user.UserName);

        return user;
    }
}