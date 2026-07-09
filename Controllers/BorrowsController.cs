using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryHub.Data;
using LibraryHub.Models;
using LibraryHub.Filters;

namespace LibraryHub.Controllers
{
    [AdminSessionFilter]
    public class BorrowsController : Controller
    {
        private readonly LibraryContext _context;

        public BorrowsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Borrows
        public async Task<IActionResult> Index(string searchString)
        {
            var borrows = _context.Borrows
            .Include(b => b.Book)
            .Include(b => b.Member)
            .AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.ToLower();
                borrows = borrows.Where(b =>b.Book!.Title.ToLower().Contains(searchString) ||
                b.Member!.FullName.ToLower().Contains(searchString));
            }
            
            return View(await borrows.ToListAsync());
        }
        

        // GET: Borrows/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrow == null)
            {
                return NotFound();
            }

            return View(borrow);
        }

        // GET: Borrows/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName");
            return View(new Borrow { BorrowDate = DateTime.Today });
        }

        // POST: Borrows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BookId,MemberId,BorrowDate,ReturnDate")] Borrow borrow)
        {
            var book = await _context.Books.FindAsync(borrow.BookId);

            if (book == null)
            {
                ModelState.AddModelError(nameof(Borrow.BookId), "Seçilen kitap bulunamadı.");
            }
            else if (!book.IsAvailable)
            {
                ModelState.AddModelError(nameof(Borrow.BookId), "Bu kitap şu anda ödünçte.");
            }

            if (ModelState.IsValid)
            {
                book!.IsAvailable = false;
                borrow.IsReturned = false;
                borrow.ReturnDate = null;

                _context.Add(borrow);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kitap başarıyla ödünç verildi.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable || b.Id == borrow.BookId), "Id", "Title", borrow.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", borrow.MemberId);
            return View(borrow);
        }

        // GET: Borrows/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrow = await _context.Borrows.FindAsync(id);
            if (borrow == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", borrow.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", borrow.MemberId);
            return View(borrow);
        }

        // POST: Borrows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BookId,MemberId,BorrowDate,ReturnDate")] Borrow borrow)
        {
            if (id != borrow.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kitabı bul
                    var book = await _context.Books.FindAsync(borrow.BookId);

                    if (book != null && borrow.ReturnDate != null)
                    {
                        // İade edildiyse kitap tekrar müsait olsun
                        book.IsAvailable = true;
                        borrow.IsReturned = true;
                    }

                    _context.Update(borrow);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowExists(borrow.Id))
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
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", borrow.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", borrow.MemberId);
            return View(borrow);
        }

        // GET: Borrows/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (borrow == null)
            {
                return NotFound();
            }

            return View(borrow);
        }

        // POST: Borrows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrow = await _context.Borrows.FindAsync(id);
            if (borrow != null)
            {
                _context.Borrows.Remove(borrow);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.Borrows.FindAsync(id);

            if (borrow == null)
            {
                return NotFound();
            }

            borrow.ReturnDate = DateTime.Now;
            borrow.IsReturned = true;

            var book = await _context.Books.FindAsync(borrow.BookId);

            if (book != null)
            {
                book.IsAvailable = true;
            }
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kitap iade alındı.";
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowExists(int id)
        {
            return _context.Borrows.Any(e => e.Id == id);
        }
    }
}
