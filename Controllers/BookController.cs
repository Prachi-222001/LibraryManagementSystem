using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using LibraryManagement.Entities;
using LibraryManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagement.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class BookController : Controller
    {
        // Cosmos DB container
        private Container container;

        // Constructor initializes the container
        public BookController()
        {
            container = GetContainer();
        }

        // Initialize Cosmos DB client and retrieve the container
        private Container GetContainer()
        {
            string URI = "https://localhost:8081";
            string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            string DatabaseName = "LibraryDB";
            string ContainerName = "Book";
            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
            Database database = cosmosClient.GetDatabase(DatabaseName);
            Container container = database.GetContainer(ContainerName);
            return container;
        }

        // Adds a new book to the database
        [HttpPost]
        public async Task<BookModel> AddBook(BookModel bookModel)
        {
            BookEntity book = new BookEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = bookModel.UId,
                Title = bookModel.Title,
                Author = bookModel.Author,
                PublishedDate = bookModel.PublishedDate,
                ISBN = bookModel.ISBN,
                IsIssued = bookModel.IsIssued,
                DocumentType = "book",
                CreatedBy = "Prachi",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            await container.CreateItemAsync(book);
            return bookModel;
        }

        // Retrieves a book by unique identifier
        [HttpGet]
        public async Task<BookModel> GetBookByUId(string UId)
        {
            var book = container.GetItemLinqQueryable<BookEntity>(true)
                                .Where(q => q.UId == UId && q.Active == true && q.Archived == false)
                                .FirstOrDefault();

            if (book == null) return null;

            return new BookModel
            {
                UId = book.UId,
                Title = book.Title,
                Author = book.Author,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsIssued = book.IsIssued
            };
        }

        // Retrieves a book by title
        [HttpGet]
        public async Task<BookModel> GetBookByTitle(string title)
        {
            var book = container.GetItemLinqQueryable<BookEntity>(true)
                                .Where(q => q.Title == title && q.Active == true && q.Archived == false)
                                .FirstOrDefault();

            if (book == null) return null;

            return new BookModel
            {
                UId = book.UId,
                Title = book.Title,
                Author = book.Author,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsIssued = book.IsIssued
            };
        }

        // Retrieves all books
        [HttpGet]
        public async Task<List<BookModel>> GetAllBooks()
        {
            var books = container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(q => q.Active == true && q.Archived == false && q.DocumentType == "book")
                                 .ToList();

            List<BookModel> bookModels = books.Select(book => new BookModel
            {
                UId = book.UId,
                Title = book.Title,
                Author = book.Author,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsIssued = book.IsIssued
            }).ToList();

            return bookModels;
        }

        // Retrieves available (not issued) books
        [HttpGet]
        public async Task<List<BookModel>> GetAvailableBooks()
        {
            var books = container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(q => q.IsIssued == false && q.Active == true && q.Archived == false && q.DocumentType == "book")
                                 .ToList();

            List<BookModel> bookModels = books.Select(book => new BookModel
            {
                UId = book.UId,
                Title = book.Title,
                Author = book.Author,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsIssued = book.IsIssued
            }).ToList();

            return bookModels;
        }

        // Retrieves issued books
        [HttpGet]
        public async Task<List<BookModel>> GetIssuedBooks()
        {
            var books = container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(q => q.IsIssued == true && q.Active == true && q.Archived == false && q.DocumentType == "book")
                                 .ToList();

            List<BookModel> bookModels = books.Select(book => new BookModel
            {
                UId = book.UId,
                Title = book.Title,
                Author = book.Author,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsIssued = book.IsIssued
            }).ToList();

            return bookModels;
        }

        // Updates an existing book by archiving the old record and adding a new one
        [HttpPost]
        public async Task<BookModel> UpdateBook(BookModel bookModel)
        {
            var existingBook = container.GetItemLinqQueryable<BookEntity>(true)
                                        .Where(q => q.UId == bookModel.UId && q.Active == true && q.Archived == false)
                                        .FirstOrDefault();

            if (existingBook == null) return null;

            existingBook.Archived = true;
            existingBook.Active = false;
            await container.ReplaceItemAsync(existingBook, existingBook.Id);

            BookEntity updatedBook = new BookEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = bookModel.UId,
                Title = bookModel.Title,
                Author = bookModel.Author,
                PublishedDate = bookModel.PublishedDate,
                ISBN = bookModel.ISBN,
                IsIssued = bookModel.IsIssued,
                DocumentType = "book",
                CreatedBy = "Prachi",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = existingBook.Version + 1,
                Active = true,
                Archived = false
            };

            await container.CreateItemAsync(updatedBook);

            return bookModel;
        }
    }
}
