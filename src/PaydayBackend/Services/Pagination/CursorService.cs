using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace PaydayBackend.Services.Pagination;

public class CursorService(IDataProtectionProvider provider) : ICursorService
{
    private readonly IDataProtector _protector = provider.CreateProtector(nameof(CursorService));

    public string EncodeCursor(Cursor cursor)
    {
        var json = JsonSerializer.Serialize(cursor);
        return _protector.Protect(json);
    }

    public Cursor? DecodeCursor(string encodedCursor)
    {
        try
        {
            var json = _protector.Unprotect(encodedCursor);
            return JsonSerializer.Deserialize<Cursor>(json);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
