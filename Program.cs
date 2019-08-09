using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ef_core_json
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new BlogContext())
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();

                ctx.Blogs.Add(new Blog { Name = "WAT", Json = new List<Item> { new Item { ItemName = "Man" } } });
                ctx.SaveChanges();
            }

            using (var ctx = new BlogContext())
            {
                var x = ctx.Blogs.Where(b => b.Json == new List<Item> { new Item { ItemName = "Man" } }).ToList();
            }
        }
    }

    public class BlogContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=test");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>().ToTable("blog")
                .Property(b => b.Json).HasColumnType("jsonb")
                .IsRequired()
            // Without Conversion: System.InvalidOperationException: 'The property 'Blog.Json' is of type 'IList<Item>' which is not supported by current database provider. Either change the property CLR type or ignore the property using the '[NotMapped]' attribute or by using 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.'
            // With Conversion: PostgresException: 42804: column "Json" is of type jsonb but expression is of type text
            // Works in Fine in EF Core 2.2.6
            .HasConversion(
                v => JsonConvert.SerializeObject(v,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<Item>>(v,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));


            modelBuilder.Entity<Blog>().HasData(new Blog { Id = -1, Name = "WAT", Json = new List<Item> { new Item { ItemName = "Man" } } });
        }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "jsonb")]
        public IList<Item> Json { get; set; }
    }

    public class Item
    {
        public string ItemName { get; set; }
    }
}
