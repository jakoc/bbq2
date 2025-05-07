namespace BobsBBQApi.Helpers.Interfaces;

public interface IJwtToken
{
    String GenerateJwtToken(Guid userId, string email, string role);
}