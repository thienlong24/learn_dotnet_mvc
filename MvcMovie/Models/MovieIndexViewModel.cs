namespace MvcMovie.Models
{
    public class MovieIndexViewModel
    {
        public IEnumerable<Movie> Movies { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int PageSize { get; set; }
    }
}