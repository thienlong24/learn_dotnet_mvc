using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Controllers;

public class MoviesController : Controller
{
    private readonly ILogger<MoviesController> _logger;
     private readonly MvcMovieContext _context;

    public MoviesController(MvcMovieContext context, ILogger<MoviesController> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var movies = await _context.Movies.ToListAsync();
            _logger.LogInformation("Fetched {Count} movies from the database", movies.Count);
            return View(movies);
        }
        finally { 
            _logger.LogInformation("Index action completed");
        }
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
}
