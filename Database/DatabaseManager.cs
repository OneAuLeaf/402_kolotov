using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database
{
    public class Response
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class DatabaseManager
    {
        LibraryContext db;

        public DatabaseManager()
        {
            db = new LibraryContext();
        }

        public async Task AddAsync(Item item)
        {
            if (CheckExist(item))
                return;

            await db.Items.AddAsync(item);
            await db.SaveChangesAsync();
        }

        public void Clear()
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        public IEnumerable<Response> GetClasses()
        {
            return db.Items.GroupBy(
                x => x.Label
            ).Select(
                g => new Response{ Label = g.Key, Count = g.Count() }
            );
        }

        public IEnumerable<Item> GetClassObjects(string label)
        {
            return db.Items.Where(x => x.Label == label).Include(x => x.Details);
        }

        private bool CheckExist(Item item)
        {
            return db.Items.Where(
                x => (item.X == x.X) &&
                (item.Y == x.Y) &&
                (item.W == x.W) &&
                (item.H == x.H)
            ).Include(x => x.Details).Where(
                x => x.Details.Image.SequenceEqual(item.Details.Image)
            ).Any();
        }

    }
}
