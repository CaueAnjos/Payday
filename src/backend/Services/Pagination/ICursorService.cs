namespace PaydayBackend.Services.Pagination;

public enum CursorDirection
{
    Forward,
    Backward,
}

public sealed record Cursor(int Id, CursorDirection Direction = CursorDirection.Forward);

public interface ICursorService
{
    public string EncodeCursor(Cursor cursor);
    public Cursor? DecodeCursor(string encodedCursor);
}
