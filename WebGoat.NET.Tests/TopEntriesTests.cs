﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebGoatCore.Data;
using WebGoatCore.Models;

namespace WebGoat.NET.Tests
{
    [TestFixture]
    public class TopEntriesTests
    {
        Mock<NorthwindContext> _context;

        [SetUp]
        public void Setup()
        {
            // create test DB
            var initialBlogEntries = new List<BlogEntry> {
            new BlogEntry() { Author = "admin", Contents = "Test Content", Id = 1, PostedDate = DateTime.Now, Responses = Array.Empty<BlogResponse>(), Title = "Test Title" }
        }.AsQueryable();

            Func<BlogEntry, EntityEntry<BlogEntry>> mockEntityEntry = (BlogEntry data) =>
            {
                var internalEntityEntry = new InternalEntityEntry(
                    new Mock<IStateManager>().Object,
                    new RuntimeEntityType(nameof(BlogEntry), typeof(BlogEntry), false, null, null, null, ChangeTrackingStrategy.Snapshot, null, false),
                    data);

                var entityEntry = new EntityEntry<BlogEntry>(internalEntityEntry);
                return entityEntry;
            };

            var mockSet = CreateDbSetMock(initialBlogEntries);

            mockSet.Setup(m => m.Add(It.IsAny<BlogEntry>())).Returns(mockEntityEntry);

            _context = new Mock<NorthwindContext>();
            _context.SetupGet(c => c.BlogEntries).Returns(mockSet.Object);
        }

        [Test]
        public void GetTopEntriesTest()
        {
            var blogEntryRepo = new BlogEntryRepository(_context.Object);

            var entries = blogEntryRepo.GetTopBlogEntries();

            Assert.That(entries.Count, Is.EqualTo(1));
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> elements) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());

            return dbSetMock;
        }
    }
}
