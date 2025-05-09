using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

public class UserController : ControllerBase
{

    private readonly IUserLogic _userLogic;


    public UserController( IUserLogic userLogic)
    {
        _userLogic = userLogic;
    }

    [HttpPost("[action]")]
    public IActionResult LoginUser(string email, string password)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("Login user called from controller");
        activity?.SetTag("user.email", email);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Email and password are required.");
        }
        try
        {
            MonitorService.Log.Information("LoginUser in UserLogic called from controller");
            var token = _userLogic.LoginUser(email, password);
            if (token != null)
            {
                MonitorService.Log.Information("checking if password is correct");
                var userResponse = new
                {
                    userToken = token
                };
                MonitorService.Log.Information("User logged in successfully");
                return Ok(userResponse);
            }
            else
            {
                return BadRequest("Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error logging in user @{email}", email);
            return StatusCode(500, "Error logging in user");
        }
    }
    [HttpPost("[action]")]
    public IActionResult RegisterUser([FromBody] RegisterUserDto dto)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("Register user called from controller");
        activity?.SetTag("user.username", dto.UserName);
        activity?.SetTag("user.email", dto.Email);
        activity?.SetTag("user.phoneNumber", dto.PhoneNumber);
        activity?.SetTag("user.role", dto.UserRole);
        if (dto == null)
        {
            MonitorService.Log.Warning("Invalid user data received");
            return BadRequest("Invalid user data.");
        }
        if (!ModelState.IsValid)
        {
            MonitorService.Log.Warning("Model state is invalid");
            return BadRequest(ModelState);
        }

        string username = dto.UserName;
        string password = dto.Password;
        string email = dto.Email;
        int phoneNumber = dto.PhoneNumber;
        string role = dto.UserRole;
        
        try
        {
            MonitorService.Log.Information("Sending user data to UserLogic");
            var user = _userLogic.RegisterUser(username, password, email, phoneNumber, role);
            if (user != null)
            {
                MonitorService.Log.Information("User created successfully: {@email}", email);
                return Ok(user);
            }
            else
            {
                return BadRequest("Error registering user");
            }
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error registering user");
            return StatusCode(500, "Error registering user");
        }
    }
}