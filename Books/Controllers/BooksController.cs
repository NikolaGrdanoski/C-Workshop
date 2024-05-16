using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Books.Models;
using Books.Data;
using Books.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Books.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Books.Controllers
{
    public class BooksController : Controller
    {
        private readonly BooksContext _context;
        private readonly UserManager<BooksUser> _userManager;


        public BooksController(BooksContext context, UserManager<BooksUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchFunction)
        {
            IQueryable<Book> books = _context.Book
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .ThenInclude(bg => bg.Genre)
                .Include(b => b.Reviews);
            IQueryable<string> titleQuery = _context.Book
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .Distinct();

            if (!string.IsNullOrEmpty(searchFunction))
            {
                books = books.Where(b =>
                b.Title.Contains(searchFunction) ||
                b.Author.FirstName.Contains(searchFunction) ||
                b.Author.LastName.Contains(searchFunction) ||
                b.Genres.Any(b => b.Genre.GenreName.Contains(searchFunction)
                ));
            }

            foreach (var book in books)
            {
                var distinctGenres = book.Genres
                    .Where(bg => bg.Genre != null)
                    .GroupBy(bg => bg.GenreId)
                    .Select(group => group.First())
                    .ToList();

                book.Genres = distinctGenres;
            }

            var bookViewModel = new BookFilterViewModel
            {
                CurrentBooks = await books.ToListAsync(),
                FilterOptions = new SelectList(await titleQuery.ToListAsync())
            };

            return View(bookViewModel);
        }

        // GET: Books/Details/5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .ThenInclude(b => b.Genre)
                .Distinct()
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            string purchased = "No";
            var usr = await _userManager.GetUserAsync(HttpContext.User);
            if (usr != null)
            {
                var isPurchased = await _context.UserBooks.Where(b => b.BookId == id && b.AppUser == usr.Email).FirstOrDefaultAsync();
                if (isPurchased != null)
                {
                    purchased = "Yes";
                }
                ViewBag.Email = usr.Email;
            }
            else
            {
                ViewBag.Email = null;
            }
            // Get the reviews of this book
            var all_review = await _context.Review.Where(b => b.BookId == id).ToListAsync();

            var bookDetailsVM = new BookDetails
            {
                Book = book,
                Reviews = all_review,
                Purchased = purchased
            };

            return View(bookDetailsVM);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = new MultiSelectList(await _context.Genre.ToListAsync(), "Id", "GenreName");
            ViewBag.AuthorId = new SelectList(await _context.Author.ToListAsync(), "Id", "FullName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,YearPublished,NumPages,Description,Publisher,FrontPage,DownloadUrl,AuthorId")] Book book, int[] selectedGenres)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();

                if (selectedGenres != null)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        _context.Add(new BookGenre { BookId = book.Id, GenreId = genreId });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.AuthorId = new SelectList(await _context.Author.ToListAsync(), "Id", "FullName", book.AuthorId);
            ViewBag.Genres = new MultiSelectList(await _context.Genre.ToListAsync(), "Id", "GenreName", selectedGenres);

            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var selectedGenreIds = book.Genres.Select(bg => bg.GenreId).ToList();

            ViewBag.AuthorId = new SelectList(await _context.Author.ToListAsync(), "Id", "FullName", book.AuthorId);
            ViewBag.Genres = new SelectList(await _context.Genre.ToListAsync(), "Id", "GenreName", book.Genres.Select(bg => bg.GenreId));
            ViewBag.SelectedGenres = selectedGenreIds;
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,YearPublished,NumPages,Description,Publisher,FrontPage,DownloadUrl,AuthorId")] Book book, int[] selectedGenres)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);

                    var existingGenres = _context.BookGenre.Where(bg => bg.BookId == id);
                    _context.BookGenre.RemoveRange(existingGenres);

                    if (selectedGenres != null)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            _context.Add(new BookGenre { BookId = book.Id, GenreId = genreId });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Genres = new MultiSelectList(await _context.Genre.ToListAsync(), "Id", "GenreName", selectedGenres);
            ViewBag.AuthorId = new SelectList(_context.Author, "Id", "FullName", book.AuthorId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyBooks(string searchString)
        {
            var usr = await _userManager.GetUserAsync(HttpContext.User);
            IQueryable<UserBooks> books = _context.UserBooks.AsQueryable().Where(s => s.AppUser == usr.Email);
            IQueryable<string> genreQuery = _context.Genre.Distinct().Select(g => g.GenreName).Distinct();
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Book.Title.Contains(searchString));
            }

            books = books.Include(b => b.Book).ThenInclude(b => b.Author);

            var bookGenreVM = new BookGenreViewModel
            {
                Books = await books.Select(s => s.Book).Distinct().ToListAsync(),
                Reviews = await _context.Review.ToListAsync()
            };

            return View(bookGenreVM);
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Buy(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }


            var usr = await _userManager.GetUserAsync(HttpContext.User);
            if (usr == null)
            {
                return NotFound();
            }

            var ownAlready = await _context.UserBooks.Where(s => s.AppUser == usr.Email && s.BookId == id).FirstOrDefaultAsync();
            if (ownAlready != null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.UserBooks.Add(new UserBooks { AppUser = usr.Email, BookId = id });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var usr = await _userManager.GetUserAsync(HttpContext.User);
            if (id == null)
            {
                return NotFound(ModelState);
            }

            var book = _context.UserBooks.Where(s => s.BookId == id && s.AppUser == usr.Email).FirstOrDefault();
            if (book == null)
            {
                return RedirectToAction(nameof(Index));
            }

            _context.UserBooks.Remove(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(MyBooks));
        }
    }
}
