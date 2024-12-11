using System.Security.Cryptography;

namespace Snek.Client.SnekLogic;

public static class FoodLogic
{
    public static byte[] ConputeHash(this SHA256 sha256, byte[] data)
    {
        var sha = SHA1.Create();
        return sha256.ComputeHash(sha.ComputeHash(data));
    }
}
