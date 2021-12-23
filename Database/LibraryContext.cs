using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Database
{
    public class Item
    {
        [Key]
        public int ObjectId { get; set; }

        public string Label { get; set; }
        public string Path { get; set; }
        public float Confidence { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }
        public ItemDetails Details { get; set; }
    }

    public class ItemDetails
    {
        [Key]
        public int ImageId { get; set; }
        public byte[] Image { get; set; }
    }

    public class LibraryContext: DbContext
    {
        static private readonly string _connectionString = @"Data Source=E:\Projects\C#Labs\402_kolotov\Database\database.db";
        public DbSet<Item> Items { get; set; }

        public LibraryContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite(_connectionString);

    }
}
