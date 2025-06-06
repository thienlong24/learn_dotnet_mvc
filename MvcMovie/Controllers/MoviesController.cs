using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using OfficeOpenXml;
using System.Text.Json;
using MvcMovie.Helper;

namespace MvcMovie.Controllers;

public class MoviesController : Controller
{
    private readonly ILogger<MoviesController> _logger;
    private readonly MvcMovieContext _context;
    private readonly int[] _allowedPageSizes = [5, 10, 20, 50];
    private const int DefaultPageSize = 10;


    public MoviesController(MvcMovieContext context, ILogger<MoviesController> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(
        string sortOrder, 
        string currentFilter, 
        string searchString, 
        int? pageNumber,
        int? pageSize)
    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["TitleSortParam"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
        ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
        ViewData["GenreSortParam"] = sortOrder == "genre" ? "genre_desc" : "genre";
        ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";

        if (string.IsNullOrWhiteSpace(searchString))
        {
            searchString = currentFilter;
        }
        else
        {
            pageNumber = 1;
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentPageSize"] = pageSize ?? DefaultPageSize;
        ViewData["AllowedPageSizes"] = _allowedPageSizes;

        var movies = from m in _context.Movies
                    select m;

        if (!String.IsNullOrEmpty(searchString))
        {
            movies = movies.Where(s => s.Title.Contains(searchString));
        }

        movies = sortOrder switch
        {
            "title_desc" => movies.OrderByDescending(m => m.Title),
            "date" => movies.OrderBy(m => m.ReleaseDate),
            "date_desc" => movies.OrderByDescending(m => m.ReleaseDate),
            "genre" => movies.OrderBy(m => m.Genre),
            "genre_desc" => movies.OrderByDescending(m => m.Genre),
            "price" => movies.OrderBy(m => m.Price),
            "price_desc" => movies.OrderByDescending(m => m.Price),
            _ => movies.OrderBy(m => m.Title),
        };

        int actualPageSize = pageSize.HasValue && _allowedPageSizes.Contains(pageSize.Value) 
            ? pageSize.Value 
            : DefaultPageSize;

        var paginatedMovies = await PaginatedList<Movie>.CreateAsync(movies, pageNumber ?? 1, actualPageSize);

        var viewModel = new MovieIndexViewModel
        {
            Movies = paginatedMovies,
            PageIndex = paginatedMovies.PageIndex,
            TotalPages = paginatedMovies.TotalPages,
            HasPreviousPage = paginatedMovies.HasPreviousPage,
            HasNextPage = paginatedMovies.HasNextPage,
            PageSize = actualPageSize
        };

        return View(viewModel);
    }
    
    // GET: Movies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Movies/Create
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Movie movie, string[] Genre)
    {
        if (ModelState.IsValid)
        {
            movie.Genre = string.Join(", ", Genre); // Combine selected genres into a single string
            _context.Add(movie);
            await _context.SaveChangesAsync();
            return Json(new { id = movie.Id, title = movie.Title });
        }
        return BadRequest(new { error = "Invalid data" });
    }
    
    // GET: Movies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: Movies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie, string[] Genre)
    {
        if (id != movie.Id)
        {
            return NotFound(new { error = "Movie ID mismatch" });
        }

        if (ModelState.IsValid)
        {
            try
            {
                movie.Genre = string.Join(", ", Genre); // Combine selected genres into a single string
                _context.Update(movie);
                await _context.SaveChangesAsync();
                return Json(new { id = movie.Id, title = movie.Title }); // Return JSON response on success
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
                {
                    return NotFound(new { error = "Movie not found" });
                }
                else
                {
                    throw;
                }
            }
        }
        return BadRequest(new { error = "Invalid data. Please check your input." }); // Return error response
    }

    // GET: Movies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: Movies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // DELETE: Movies/Delete/5
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound(new { error = "Movie not found" });
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Movie deleted successfully" });
    }

    private bool MovieExists(int id)
    {
        return _context.Movies.Any(e => e.Id == id);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Add these actions to your MoviesController
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to import.";
            return RedirectToAction(nameof(Import));
        }

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Please select an Excel file (.xlsx)";
            return RedirectToAction(nameof(Import));
        }

        var result = new ImportResult();

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            var movies = new List<Movie>();

            // Skip the header row, start from row 2
            for (var row = 2; row <= rowCount; row++)
            {
                try
                {
                    var title = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    
                    // Skip empty rows
                    if (string.IsNullOrEmpty(title))
                    {
                        continue;
                    }

                    var releaseDateStr = worksheet.Cells[row, 2].Value?.ToString();
                    var genre = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var priceStr = worksheet.Cells[row, 4].Value?.ToString();

                    if (!DateTime.TryParse(releaseDateStr, out var releaseDate))
                    {
                        throw new Exception("Invalid release date format");
                    }

                    if (!decimal.TryParse(priceStr, out var price))
                    {
                        throw new Exception("Invalid price format");
                    }

                    var movie = new Movie
                    {
                        Title = title,
                        ReleaseDate = releaseDate,
                        Genre = genre ?? "Unknown",
                        Price = decimal.ToDouble(price)
                    };

                    // Add basic validation
                    if (string.IsNullOrEmpty(movie.Title))
                    {
                        throw new Exception("Title is required");
                    }

                    if (movie.Price < 0)
                    {
                        throw new Exception("Price cannot be negative");
                    }

                    movies.Add(movie);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    var title = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "Unknown";
                    result.Failures.Add((row, title, ex.Message));
                }
            }

            // Save successful imports
            if (movies.Any())
            {
                await _context.Movies.AddRangeAsync(movies);
                await _context.SaveChangesAsync();
            }

            // Store import results in TempData
            TempData["ImportSuccess"] = result.SuccessCount;
            TempData["ImportFailures"] = JsonSerializer.Serialize(result.Failures);

            return RedirectToAction(nameof(Import));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error processing file: {ex.Message}";
            return RedirectToAction(nameof(Import));
        }
    }
}