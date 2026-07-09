using Microsoft.EntityFrameworkCore;
using LibraryHub.Models;

namespace LibraryHub.Data;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Borrow> Borrows => Set<Borrow>();
    public DbSet<Admin> Admins => Set<Admin>();
}