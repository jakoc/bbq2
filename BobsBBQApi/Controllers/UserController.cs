using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

[Route("api/[controller]")] 
public class UserController : ControllerBase
{

    private readonly IUserLogic _userLogic;


    public UserController( IUserLogic userLogic)
    {
        _userLogic = userLogic;
    }

    [HttpPost("LogInUser")]
    public IActionResult LoginUser([FromBody] LogInDto dto)
    {
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

        string email = dto.Email;
        string password = dto.Password;

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
                MonitorService.Log.Information("User logged in successfully");
                return Ok(new { token = token });
            }
            else
            {
                return BadRequest("Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error logging in user {@Email}", email);
            return StatusCode(500, "Error logging in user");
        }
    }
    [HttpPost("RegisterUser")]
    public IActionResult RegisterUser([FromBody] RegisterUserDto dto)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("Register user called from controller");
        
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
        activity?.SetTag("user.username", username);
        activity?.SetTag("user.email", email);
        activity?.SetTag("user.phoneNumber", phoneNumber);
        activity?.SetTag("user.role", role);
        try
        {
            MonitorService.Log.Information("Sending user data to UserLogic");
            var user = _userLogic.RegisterUser(username, password, email, phoneNumber, role);
            if (user != null)
            {
                MonitorService.Log.Information("User created successfully: {@Email}", email);
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