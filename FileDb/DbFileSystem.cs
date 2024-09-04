﻿using Microsoft.EntityFrameworkCore;
using kDg.FileBaseContext.Extensions;

namespace FileDb
{
    public class DbFileSystem : DbContext
    {
        public const string DatabaseName = "my_local_db";


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseFileBaseContextDatabase(databaseName: "my_local_db"); //location: "c:\temp\fileDb");
        }


        public DbSet<ContentEntry> ContentEntries { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<GenericTest<int>> Generics { get; set; }
        public DbSet<Messurement> Messurements { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<SimpleEntity> SimpleEntities { get; set; }
        public DbSet<User> Users { get; set; }

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public static void InitDb(DbFileSystem db)
        {
            SimpleEntity? entity = db.SimpleEntities.Local.FirstOrDefault(x => x.Name.Contains("2"));
            if(entity != null)
                db.SimpleEntities.Remove(entity);
            else
                db.SimpleEntities.Add(new SimpleEntity() { Id = 2, Name = "Name2" });

            db.SaveChanges();

            if(!db.Messurements.Any())
            {
                var messurement = new Messurement()
                {
                    Id = 1,
                    EntryCount = 12,
                    CreatedOn = DateTime.Parse("01/01/2001"),
                    UpdatedOn = DateTime.Parse("02/02/2002"),
                    TimeRead = TimeSpan.Parse("0:10"),
                    TimeWrite = TimeSpan.Parse("0:15")
                };
                db.Messurements.Add(messurement);
            }

            var user = new User()
            {
                Name = "User11",
                Username = "Username222",
                Id = 2,
                Type = User.UserType.User,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.MinValue,
                Contents = new List<Content>()
            {
                new()
                {
                    Id = 55,
                    Text = "Content Text",
                    Entries = new List<ContentEntry>()
                    {
                        new()
                        {
                            Id = 777,
                            Text = "uyiuyuiyiuyui"
                        }
                    }
                }
            },
                Ignored = "false",
                Settings = new List<Setting>()
            {
                new()
                {
                    Id = 2,
                    CreatedOn = DateTime.Now,
                    UpdatedOn = DateTime.MinValue,
                    Key = "key",
                    Value = "setting value"
                }
            }
            };

            db.Users.Add(user);

            db.SaveChanges();
        }
    }
}
