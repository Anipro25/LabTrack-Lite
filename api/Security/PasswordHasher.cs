using System.Security.Cryptography;
using System.Text;

namespace LabTrack.Api.Security;

public static class PasswordHasher
{
    // Uses PBKDF2 with SHA256, 10k iterations, 32-byte key and 16-byte salt
    private const int Iterations = 10000;
    private const int KeySize = 32; // 256-bit
    private const int SaltSize = 16; // 128-bit

    public static string Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);
        // store as base64: salt:hash:iterations
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}:{Iterations}";
    }

    public static bool Verify(string password, string stored)
    {
        if (string.IsNullOrWhiteSpace(stored)) return false;
        var parts = stored.Split(':');
        if (parts.Length < 3) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var storedKey = Convert.FromBase64String(parts[1]);
        var iterations = int.TryParse(parts[2], out var it) ? it : Iterations;

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var key = pbkdf2.GetBytes(KeySize);
        return CryptographicOperations.FixedTimeEquals(key, storedKey);
    }
}
