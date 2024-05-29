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
    //API controller
    
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class BookController : Controller
    {
       //container
        private Container Container { get; }

        // Constructor to initialize container
        public BookController()
        {
            Container = GetContainer();
        }

        //method to get container
        private Container GetContainer()
        {
           //defines URI,primary key,Database,container name
            string URI = "https://localhost:8081";
            string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            string DatabaseName = "LibraryDB";
            string ContainerName = "Book";

            //retrieve the database and container
            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
            Database database = cosmosClient.GetDatabase(DatabaseName);
            Container container = database.GetContainer(ContainerName);

            return container;
        }

        // to add a new book
        [HttpPost]
        public async Task<BookModel> AddBook(BookModel bookModel)
        {
            // Entity from BookModel
            BookEntity book = new BookEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = bookModel.UId,
                Title = bookModel.Title,
                Author = bookModel.Author,
                PublishedDate = bookModel.PublishedDate,
                ISBN = bookModel.ISBN,//International Standard Book Number
                IsIssued = bookModel.IsIssued,
                DocumentType = "book",
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            // Add the new book to the container
            await Container.CreateItemAsync(book);
            return bookModel;
        }

        // get a book by its unique identifier (UId)
        [HttpGet]
        public async Task<BookModel> GetBookByUId(string UId)
        {
            // Query to find the book by UId
            var book = Container.GetItemLinqQueryable<BookEntity>(true)
                                .Where(q => q.UId == UId && q.Active == true && q.Archived == false)
                                .FirstOrDefault();

            if (book == null) return null;

            // Return 
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

        // to get a book by its title
        [HttpGet]
        public async Task<BookModel> GetBookByTitle(string title)
        {
            // Query the find the book by title
            var book = Container.GetItemLinqQueryable<BookEntity>(true)
                                .Where(q => q.Title == title && q.Active == true && q.Archived == false)
                                .FirstOrDefault();

            if (book == null) return null;

            // Return 
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

        //  to get all books
        [HttpGet]
        public async Task<List<BookModel>> GetAllBooks()
        {
            // Query to get all active and non-archived books
            var books = Container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(q => q.Active == true && q.Archived == false && q.DocumentType == "book")
                                 .ToList();

            //result to a list of BookModel
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

        //to get all available (not issued) books
        [HttpGet]
        public async Task<List<BookModel>> GetAvailableBooks()
        {
            // Query the container to get all available (not issued) books
            var books = Container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(q => q.IsIssued == false && q.Active == true && q.Archived == false && q.DocumentType == "book")
                                 .ToList();

            // Convert the result to a list of BookModel
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

        // to get all issued books
        [HttpGet]
        public async Task<List<BookModel>> GetIssuedBooks()
        {
            // Query to get all issued books
            var books = Container.GetItemLinqQueryable<BookEntity>(true)
                                 .Where(q => q.IsIssued == true && q.Active == true && q.Archived == false && q.DocumentType == "book")
                                 .ToList();

            // Convert the result to a list of BookModel
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

        // Action to update an existing book
        [HttpPost]
        public async Task<BookModel> UpdateBook(BookModel bookModel)
        {
            // Find the existing book by UId
            var existingBook = Container.GetItemLinqQueryable<BookEntity>(true)
                                        .Where(q => q.UId == bookModel.UId && q.Active == true && q.Archived == false)
                                        .FirstOrDefault();

            if (existingBook == null) return null;

            // Archive the existing book
            existingBook.Archived = true;
            existingBook.Active = false;
            await Container.ReplaceItemAsync(existingBook, existingBook.Id);

            // Create a new BookEntity with updated information
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
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "Admin",
                UpdatedOn = DateTime.Now,
                Version = existingBook.Version + 1,
                Active = true,
                Archived = false
            };

            // Add the updated book to the container
            await Container.CreateItemAsync(updatedBook);

            return bookModel;
        }
    }
}
