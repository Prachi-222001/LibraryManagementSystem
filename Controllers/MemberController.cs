using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using LibraryManagement.Entities;
using LibraryManagement.Model;
using System.Linq;

namespace LibraryManagement.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class MemberController : Controller
    {
        //Cosmos DB container
        private Container container; 

        //Constructor initializes the container
        public MemberController()
        {
            container = GetContainer();
        }

        //Initialize Cosmos DB client and retrieves the container
        private Container GetContainer()
        {
            string URI = "https://localhost:8081";
            string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            string DatabaseName = "LibraryDB";
            string ContainerName = "Member";
            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
            Database database = cosmosClient.GetDatabase(DatabaseName);
            Container container = database.GetContainer(ContainerName);
            return container;
        }

        //Adds a new member to the database
        [HttpPost]
        public async Task<MemberModel> AddMember(MemberModel memberModel)
        {
            MemberEntity member = new MemberEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = memberModel.UId,
                Name = memberModel.Name,
                DateOfBirth = memberModel.DateOfBirth,
                Email = memberModel.Email,
                DocumentType = "member",
                CreatedBy = "Prachi",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            await container.CreateItemAsync(member);
            return memberModel;
        }

        //Retrieves a member by unique identifier
        [HttpGet]
        public async Task<MemberModel> GetMemberByUId(string UId)
        {
            var member = container.GetItemLinqQueryable<MemberEntity>(true)
                                   .Where(q => q.UId == UId && q.Active == true && q.Archived == false)
                                   .FirstOrDefault();

            if (member == null) return null;

            return new MemberModel
            {
                UId = member.UId,
                Name = member.Name,
                DateOfBirth = member.DateOfBirth,
                Email = member.Email
            };
        }

        //Retrieves all members
        [HttpGet]
        public async Task<List<MemberModel>> GetAllMembers()
        {
            var members = container.GetItemLinqQueryable<MemberEntity>(true)
                                    .Where(q => q.Active == true && q.Archived == false && q.DocumentType == "member")
                                    .ToList();

            List<MemberModel> memberModels = new List<MemberModel>();
            foreach (var member in members)
            {
                memberModels.Add(new MemberModel
                {
                    UId = member.UId,
                    Name = member.Name,
                    DateOfBirth = member.DateOfBirth,
                    Email = member.Email
                });
            }

            return memberModels;
        }

        // Updates an existing member by archiving the old record and adding a new one
        [HttpPost]
        public async Task<MemberModel> UpdateMember(MemberModel memberModel)
        {
            var existingMember = container.GetItemLinqQueryable<MemberEntity>(true)
                                           .Where(q => q.UId == memberModel.UId && q.Active == true && q.Archived == false)
                                           .FirstOrDefault();

            if (existingMember == null) return null;

            existingMember.Archived = true;
            existingMember.Active = false;
            await container.ReplaceItemAsync(existingMember, existingMember.Id);

            MemberEntity updatedMember = new MemberEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = memberModel.UId,
                Name = memberModel.Name,
                DateOfBirth = memberModel.DateOfBirth,
                Email = memberModel.Email,
                DocumentType = "member",
                CreatedBy = "Prachi",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = existingMember.Version + 1,
                Active = true,
                Archived = false
            };

            await container.CreateItemAsync(updatedMember);

            return memberModel;
        }
    }
}
