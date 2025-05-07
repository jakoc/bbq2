using BobsBBQApi.BLL.Interfaces;
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
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Email and password are required.");
        }

        try
        {
            var (user, token) = _userLogic.LoginUser(email, password);
            if (user != null && token != null)
            {
                var userResponse = new
                {
                    userId = user.UserId,
                    token = token
                };
                return Ok(userResponse);
            }
            else
            {
                return BadRequest("Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error logging in user");
        }
    }
    [HttpPost("[action]")]
    public IActionResult RegisterUser(string username, string password, string email, int phoneNumber, string role)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("Username, password, email and role are required.");
        }

        try
        {
            var user = _userLogic.RegisterUser(username, password, email, phoneNumber, role);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("Error registering user");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error registering user");
        }
    }
}