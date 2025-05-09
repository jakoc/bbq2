using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using BobsBBQApi.Helpers.Interfaces;
using BobsBBQApi.Services;

namespace BobsBBQApi.Helpers;

public class PasswordEncrypter : IPasswordEncrypter
{
    private readonly int _keySize;
    private readonly int _iterations;
    private readonly HashAlgorithmName _algorithm;

    public PasswordEncrypter(int keySize = 64, int iterations = 10000)
    {
        _keySize = keySize;
        _iterations = iterations;
        _algorithm = HashAlgorithmName.SHA512;
    }

    public (string Hash, string Salt) EncryptPassword(string password)
    {
        var currentActivity = Activity.Current;
        MonitorService.Log.Information("Encrypting password");
        var salt = RandomNumberGenerator.GetBytes(_keySize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, _iterations, _algorithm, _keySize);
        MonitorService.Log.Information("returning hashed password");
        return (Convert.ToHexString(hash), Convert.ToHexString(salt));
    }
    
    public string EncryptPasswordWithUsersSalt(string password, string saltHex)
    { 
        
        MonitorService.Log.Information("Encrypting password with user's salt");
        var salt = Convert.FromHexString(saltHex); 
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, _iterations, _algorithm, _keySize);
        MonitorService.Log.Information("returning hashed password");
        return Convert.ToHexString(hash); 
    }
}