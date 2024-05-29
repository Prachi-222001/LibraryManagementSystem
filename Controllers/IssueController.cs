using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using LibraryManagement.Entities;
using LibraryManagement.Model;
using System.Linq;

namespace LibraryManagement.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class IssueController : Controller
    {
        //Cosmos DB container
        private Container container;

        //Constructor initializes the container
        public IssueController()
        {
            container = GetContainer();
        }

        //Initialize Cosmos DB client and retrieves the container
        private Container GetContainer()
        {
            string URI = "https://localhost:8081";
            string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            string DatabaseName = "LibraryDB";
            string ContainerName = "Issue";
            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
            Database database = cosmosClient.GetDatabase(DatabaseName);
            Container container = database.GetContainer(ContainerName);
            return container;
        }

        //Adds a new issue to the database
        [HttpPost]
        public async Task<IssueModel> AddIssue(IssueModel issueModel)
        {
            IssueEntity issue = new IssueEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = issueModel.UId,
                BookId = issueModel.BookId,
                MemberId = issueModel.MemberId,
                IssueDate = issueModel.IssueDate,
                ReturnDate = issueModel.ReturnDate,
                IsReturned = issueModel.IsReturned,
                DocumentType = "issue",
                CreatedBy = "Prachi",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            await container.CreateItemAsync(issue);
            return issueModel;
        }

        //Retrieves an issue by unique identifier
        [HttpGet]
        public async Task<IssueModel> GetIssueByUId(string UId)
        {
            var issue = container.GetItemLinqQueryable<IssueEntity>(true)
                                  .Where(q => q.UId == UId && q.Active == true && q.Archived == false)
                                  .FirstOrDefault();

            if (issue == null) return null;

            return new IssueModel
            {
                UId = issue.UId,
                BookId = issue.BookId,
                MemberId = issue.MemberId,
                IssueDate = issue.IssueDate,
                ReturnDate = issue.ReturnDate,
                IsReturned = issue.IsReturned
            };
        }

        // Updates an existing issue by archiving the old record and adding a new one
        [HttpPost]
        public async Task<IssueModel> UpdateIssue(IssueModel issueModel)
        {
            var existingIssue = container.GetItemLinqQueryable<IssueEntity>(true)
                                          .Where(q => q.UId == issueModel.UId && q.Active == true && q.Archived == false)
                                          .FirstOrDefault();

            if (existingIssue == null) return null;

            existingIssue.Archived = true;
            existingIssue.Active = false;
            await container.ReplaceItemAsync(existingIssue, existingIssue.Id);

            IssueEntity updatedIssue = new IssueEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = issueModel.UId,
                BookId = issueModel.BookId,
                MemberId = issueModel.MemberId,
                IssueDate = issueModel.IssueDate,
                ReturnDate = issueModel.ReturnDate,
                IsReturned = issueModel.IsReturned,
                DocumentType = "issue",
                CreatedBy = "Prachi",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = existingIssue.Version + 1,
                Active = true,
                Archived = false
            };

            await container.CreateItemAsync(updatedIssue);

            return issueModel;
        }
    }
}
