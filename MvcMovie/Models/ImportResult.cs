namespace MvcMovie.Models;

public class ImportResult
{
    public int SuccessCount { get; set; }
    public List<(int RowNumber, string Title, string Error)> Failures { get; set; } = new();
}