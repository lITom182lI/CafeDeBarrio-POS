using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using CafeBarrio.Application.Common.Interfaces;

namespace CafeBarrio.Infrastructure.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int DegreeOfParallelism = 8;
    private const int Iterations          = 4;
    private const int MemorySizeKb        = 128 * 1024; // 128 MiB (OWASP 2026)
    private const int HashLength          = 32;
    private const int SaltLength          = 16;

    public string Hash(string plainPassword)
    {
        var salt = new byte[SaltLength];
        RandomNumberGenerator.Fill(salt);
        var hash = Compute(Encoding.UTF8.GetBytes(plainPassword), salt);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string plainPassword, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        var salt     = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual   = Compute(Encoding.UTF8.GetBytes(plainPassword), salt);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Compute(byte[] password, byte[] salt)
    {
        using var argon2 = new Argon2id(password)
        {
            Salt                = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations          = Iterations,
            MemorySize          = MemorySizeKb,
        };
        return argon2.GetBytes(HashLength);
    }
}
