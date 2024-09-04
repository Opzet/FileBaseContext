﻿using System.ComponentModel.DataAnnotations;
using FileBaseContext.Extensions;
using FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore;

namespace FileBaseContext.Tests.Csv;

[TestClass]
public class CsvSerializationTests
    : DbContextTestClassBase<CsvSerializationTests.CsvTestDbContext>
{
    [TestMethod]
    public void CanDifferentiateNullFromDefault_Primitives()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullables()
            {
                Id = 1,
                StringValue = null,
                IntValue = null
            });

            db.Add(new EntityHasNullables()
            {
                Id = 2,
                StringValue = string.Empty,
                IntValue = 0
            });

            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullables) + ".csv");
        using (var db = CreateDbContext())
        {
            var entityWithNulls = db.EntitiesHaveNullables.Single(o => o.Id == 1);
            var entityWithDefaults = db.EntitiesHaveNullables.Single(o => o.Id == 2);

            Assert.IsNull(entityWithNulls.StringValue);
            Assert.IsNull(entityWithNulls.IntValue);

            Assert.AreEqual(string.Empty, entityWithDefaults.StringValue);
            Assert.AreEqual(0, entityWithDefaults.IntValue);
        }
    }

    [TestMethod]
    public void CanSaveEscapedText()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullables()
            {
                Id = 1,
                StringValue = "This is a \"test\" of the emergency broadcast system.",
                IntValue = 0
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullables) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveNullables.Single();
            Assert.AreEqual("This is a \"test\" of the emergency broadcast system.", entity.StringValue);
        }
    }

    [TestMethod]
    public void CanSaveMultilineTextWithWindowsNewLine()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullables()
            {
                Id = 1,
                StringValue = "This is a test of the \r\nmultiline text.",
                IntValue = 0
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullables) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveNullables.Single();
            Assert.AreEqual("This is a test of the \r\nmultiline text.", entity.StringValue);
        }
    }

    [TestMethod]
    public void CanSaveMultilineTextStringColumnIsInMiddle()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullablesStringColumnInMiddle()
            {
                Id = 1,
                FStringValue = "This is a test of the \r\nmultiline text.",
                IntValue = 0
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullablesStringColumnInMiddle) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveNullablesAndStringColumnInMiddle.Single();
            Assert.AreEqual("This is a test of the \r\nmultiline text.", entity.FStringValue);
        }
    }

    [TestMethod]
    public void CanSaveEscapedMultilineTextStringColumnIsInMiddle()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullablesStringColumnInMiddle()
            {
                Id = 1,
                FStringValue = "This is a \"test\" of the \r\nmultiline text.",
                IntValue = 0
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullablesStringColumnInMiddle) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveNullablesAndStringColumnInMiddle.Single();
            Assert.AreEqual("This is a \"test\" of the \r\nmultiline text.", entity.FStringValue);
        }
    }

    [TestMethod]
    public void CanSaveMultilineTextWithLinuxNewLine()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullables()
            {
                Id = 1,
                StringValue = "This is a test of the \nmultiline text.",
                IntValue = 0
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullables) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveNullables.Single();
            Assert.AreEqual("This is a test of the \nmultiline text.", entity.StringValue);
        }
    }

    [TestMethod]
    public void CanSaveEscapedMultilineText()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasNullables()
            {
                Id = 1,
                StringValue = "This is a \"test\" of the emergency broadcast system.\nNew line.",
                IntValue = 0
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasNullables) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveNullables.Single();
            Assert.AreEqual("This is a \"test\" of the emergency broadcast system.\nNew line.", entity.StringValue);
        }
    }

    [TestMethod]
    public void CanSaveNullByteArrays()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasByteArray()
            {
                Id = 1,
                ByteArray = null
            });
            db.SaveChanges();
        }

        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveByteArrays.Single();
            Assert.IsNotNull(entity.ByteArray);
            Assert.AreEqual(0, entity.ByteArray.Length);
        }
    }

    [TestMethod]
    public void CanSaveEmptyByteArrays()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasByteArray()
            {
                Id = 1,
                ByteArray = new byte[] { }
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasByteArray) + ".csv");
        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveByteArrays.Single();
            Assert.IsNotNull(entity.ByteArray);
            Assert.AreEqual(0, entity.ByteArray.Length);
        }
    }

    [TestMethod]
    public void CanSaveByteArrays()
    {
        using (var db = CreateDbContext())
        {
            db.Add(new EntityHasByteArray()
            {
                Id = 1,
                ByteArray = new byte[] { 0x00, 0x01, 0x02, 0x03 }
            });
            db.SaveChanges();
        }

        string content = ReadDatabaseFileText(nameof(EntityHasByteArray) + ".csv");

        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveByteArrays.Single();
            Assert.IsNotNull(entity.ByteArray);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 }, entity.ByteArray);
        }
    }

    [TestMethod]
    public void CanDeserializeByteArraysFromCsv()
    {
        AddDatabaseJsonFile("EntityHasByteArray.csv",
                            "Id,ByteArray" + Environment.NewLine +
                            "1,AAECAw==" + Environment.NewLine);
        string content = ReadDatabaseFileText("EntityHasByteArray.csv");

        using (var db = CreateDbContext())
        {
            var entity = db.EntitiesHaveByteArrays.Single();
            Assert.IsNotNull(entity.ByteArray);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 }, entity.ByteArray);
        }
    }

    protected override void ConfigureDbContextOptions(DbContextOptionsBuilder<CsvTestDbContext> options)
    {
        options.UseFileBaseContextDatabase(nameof(CsvTestDbContext), null, services =>
        {
            services.AddMockFileSystem(FileSystem);
            services.AddCsvRowDataSerializer();
        });
    }

    protected override CsvTestDbContext CreateDbContext(DbContextOptions<CsvTestDbContext> options)
    {
        return new CsvTestDbContext(options);
    }

    public sealed class CsvTestDbContext(
            DbContextOptions<CsvTestDbContext> options)
        : DbContext(options)
    {
        public DbSet<EntityHasByteArray> EntitiesHaveByteArrays { get; set; } = null!;
        public DbSet<EntityHasNullables> EntitiesHaveNullables { get; set; } = null!;
        public DbSet<EntityHasNullablesStringColumnInMiddle> EntitiesHaveNullablesAndStringColumnInMiddle { get; set; } = null!;
    }

    public class EntityHasByteArray
    {
        [Key] public int Id { get; set; }

        public byte[]? ByteArray { get; set; }
    }

    public class EntityHasNullables
    {
        [Key] public int Id { get; set; }

        public string? StringValue { get; set; }

        public int? IntValue { get; set; }
    }

    public class EntityHasNullablesStringColumnInMiddle
    {
        [Key] public int Id { get; set; }

        public string? FStringValue { get; set; }

        public int? IntValue { get; set; }
    }
}