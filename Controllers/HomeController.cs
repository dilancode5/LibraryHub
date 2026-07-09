using LibraryHub.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibraryHub.Models;
using Microsoft.EntityFrameworkCore;
using LibraryHub.Filters;

namespace LibraryHub.Controllers;

[AdminSessionFilter]
public class HomeController : Controller
{
    private readonly LibraryContext _context;

    public HomeController(LibraryContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.BookCount = _context.Books.Count();
        ViewBag.AuthorCount = _context.Authors.Count();
        ViewBag.CategoryCount = _context.Categories.Count();
        ViewBag.MemberCount = _context.Members.Count();

        ViewBag.AvailableBookCount = _context.Books.Count(b => b.IsAvailable);
        ViewBag.ActiveBorrowCount = _context.Borrows.Count(b => !b.IsReturned);
        ViewBag.ReturnedBorrowCount = _context.Borrows.Count(b => b.IsReturned);
        
        var lastBorrows = _context.Borrows
            .Include(b => b.Book)
            .Include(b => b.Member)
            .OrderByDescending(b => b.BorrowDate)
            .Take(5)
            .ToList();

        ViewBag.LastBorrows = lastBorrows;

        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
