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
        private readonly Container _container;

        public IssueController()
        {
            _container = GetContainer();
        }

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
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            await _container.CreateItemAsync(issue);
            return issueModel;
        }

        [HttpGet]
        public async Task<IssueModel> GetIssueByUId(string UId)
        {
            var issue = _container.GetItemLinqQueryable<IssueEntity>(true)
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

        [HttpPost]
        public async Task<IssueModel> UpdateIssue(IssueModel issueModel)
        {
            var existingIssue = _container.GetItemLinqQueryable<IssueEntity>(true)
                                          .Where(q => q.UId == issueModel.UId && q.Active == true && q.Archived == false)
                                          .FirstOrDefault();

            if (existingIssue == null) return null;

            existingIssue.Archived = true;
            existingIssue.Active = false;
            await _container.ReplaceItemAsync(existingIssue, existingIssue.Id);

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
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "Admin",
                UpdatedOn = DateTime.Now,
                Version = existingIssue.Version + 1,
                Active = true,
                Archived = false
            };

            await _container.CreateItemAsync(updatedIssue);

            return issueModel;
        }
    }
}
