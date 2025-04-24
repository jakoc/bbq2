namespace BobsBBQApi.Helpers.Interfaces;

public interface IJwtToken
{
    String GenerateJwtToken(String email, string role);
}