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
        private readonly Container _container;

        public MemberController()
        {
            _container = GetContainer();
        }

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
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "",
                UpdatedOn = DateTime.Now,
                Version = 1,
                Active = true,
                Archived = false
            };

            await _container.CreateItemAsync(member);
            return memberModel;
        }

        [HttpGet]
        public async Task<MemberModel> GetMemberByUId(string UId)
        {
            var member = _container.GetItemLinqQueryable<MemberEntity>(true)
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

        [HttpGet]
        public async Task<List<MemberModel>> GetAllMembers()
        {
            var members = _container.GetItemLinqQueryable<MemberEntity>(true)
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

        [HttpPost]
        public async Task<MemberModel> UpdateMember(MemberModel memberModel)
        {
            var existingMember = _container.GetItemLinqQueryable<MemberEntity>(true)
                                           .Where(q => q.UId == memberModel.UId && q.Active == true && q.Archived == false)
                                           .FirstOrDefault();

            if (existingMember == null) return null;

            existingMember.Archived = true;
            existingMember.Active = false;
            await _container.ReplaceItemAsync(existingMember, existingMember.Id);

            MemberEntity updatedMember = new MemberEntity
            {
                Id = Guid.NewGuid().ToString(),
                UId = memberModel.UId,
                Name = memberModel.Name,
                DateOfBirth = memberModel.DateOfBirth,
                Email = memberModel.Email,
                DocumentType = "member",
                CreatedBy = "Admin",
                CreatedOn = DateTime.Now,
                UpdatedBy = "Admin",
                UpdatedOn = DateTime.Now,
                Version = existingMember.Version + 1,
                Active = true,
                Archived = false
            };

            await _container.CreateItemAsync(updatedMember);

            return memberModel;
        }
    }
}
