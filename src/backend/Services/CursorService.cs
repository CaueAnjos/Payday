using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace PaydayBackend.Services;

public class CursorService(IDataProtectionProvider provider)
{
    private readonly IDataProtector protector = provider.CreateProtector(nameof(CursorService));

    public record Cursor(int Id)
    {
        public static implicit operator Cursor(int Id)
        {
            return new Cursor(Id);
        }
    };

    public string EncodeCursor(Cursor cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return protector.Protect(json);
    }

    public Cursor? DecodeCursor(string encodedCursor)
    {
        try
        {
            var json = protector.Unprotect(encodedCursor);
            return JsonSerializer.Deserialize<Cursor>(json);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
