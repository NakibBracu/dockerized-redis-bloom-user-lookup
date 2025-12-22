using System.Threading.Tasks;
using BLOOM_FILTER.Data;
using BLOOM_FILTER.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOOM_FILTER.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserBloomService bloomService;
        private readonly ApplicationDbContext db; // Add DB context
        private readonly UserBloomService _bloom;
        public UsersController(UserBloomService bloomService, ApplicationDbContext db, UserBloomService bloom)
        {
            this.bloomService = bloomService;
            this.db = db;
            this._bloom = bloom;
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckUser(string username)
        {
            // 1️⃣ Bloom Filter check (fast, in-memory)
            bool mightExist = await bloomService.MightContainAsync(username);

            // 2️⃣ Bloom says DEFINITELY NOT → skip DB
            if (!mightExist)
            {
                return Ok(new
                {
                    exists = false,
                    source = "BloomFilter",
                    message = "Definitely does not exist"
                });
            }

            // 3️⃣ Bloom says MIGHT exist → VERIFY WITH DB
            bool existsInDb = await db.Users
                .AnyAsync(u => u.Email == username);

            // 4️⃣ Final truth comes from DB
            return Ok(new
            {
                exists = existsInDb,
                source = existsInDb ? "Database" : "Bloom false-positive",
                message = existsInDb
                    ? "User exists (confirmed by DB)"
                    : "Bloom false positive – user not found"
            });
        }


        // New endpoint: Get first 100 users
        [HttpGet("first100")]
        public async Task<IActionResult> GetFirst100Users()
        {
            var users = await db.Users
                .Take(100)
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("seed-users")]
        public async Task<IActionResult> SeedUsers()
        {
            await FakeUserSeed.SeedUsersAsync(db, _bloom, 2_000_000);
            return Ok("Seeding completed");
        }
    }
}
