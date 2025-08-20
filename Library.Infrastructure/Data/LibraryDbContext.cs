using Library.Domain;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Data;

public class LibraryDbContext : DbContext
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<User> Users => Set<User>();

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>().HasKey(b => b.Id);
        modelBuilder.Entity<Loan>().HasKey(l => l.Id);
        modelBuilder.Entity<User>().HasKey(u => u.Id);

        modelBuilder.Entity<Loan>()
            .HasOne<Book>()
            .WithMany()
            .HasForeignKey(l => l.BookId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
