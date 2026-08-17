namespace PaydayBackend.Services.Pagination;

public record Cursor(object Id);

public interface ICursorService
{
    public string EncodeCursor(Cursor cursor);
    public Cursor? DecodeCursor(string endodedCursor);
}
