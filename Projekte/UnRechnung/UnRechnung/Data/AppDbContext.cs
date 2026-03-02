using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using UnRechnung.Models;

namespace UnRechnung.Data
{
  public class AppDbContext : DbContext
  {
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<NumberRange> NumberRanges { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      // Get the platform-appropriate AppData folder
      string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

      // Define a folder for the app inside AppData
      string myAppFolder = Path.Combine(appDataFolder, "UnRechnung");

      // Make sure the directory exists (create if needed)
      Directory.CreateDirectory(myAppFolder);

      // Define full path to the SQLite database file
      string dbPath = Path.Combine(myAppFolder, "unrechnung.db");

      // Configure SQLite with that path
      optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Customer>()
        .HasKey(c => c.CustomerNumber);

      // Invoice -> Customer (1:n)
      modelBuilder.Entity<Invoice>()
        .HasOne(i => i.Customer)
        .WithMany(c => c.Invoices)
        .HasForeignKey(i => i.CustomerId)
        .OnDelete(DeleteBehavior.Restrict);

      // InvoiceItem -> Invoice (1:n)
      modelBuilder.Entity<InvoiceItem>()
        .HasOne(ii => ii.Invoice)
        .WithMany(i => i.Items)
        .HasForeignKey(ii => ii.InvoiceId)
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<NumberRange>()
        .Property(nr => nr.Type)
        .HasConversion<string>();
    }
  }
}
