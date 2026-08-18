namespace PaydayBackend.Services.Pagination;

public sealed record Cursor(int Id);

public interface ICursorService
{
    public string EncodeCursor(Cursor cursor);
    public Cursor? DecodeCursor(string endodedCursor);
}
