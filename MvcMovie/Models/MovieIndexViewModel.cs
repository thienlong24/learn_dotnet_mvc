namespace MvcMovie.Models
{
    public class MovieIndexViewModel
    {
        public IEnumerable<Movie> Movies { get; set; } = [];
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}