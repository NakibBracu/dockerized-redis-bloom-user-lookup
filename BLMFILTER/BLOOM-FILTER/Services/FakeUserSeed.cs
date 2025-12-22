using BLOOM_FILTER.Data;
using BLOOM_FILTER.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLOOM_FILTER.Services
{
    public class FakeUserSeed
    {
        public static async Task SeedUsersAsync(
            IApplicationDbContext db,
            UserBloomService bloomService,
            long total = 2_000_000)
        {
            const int batchSize = 2_000;

            long inserted = await db.Users.LongCountAsync();
            if (inserted >= total)
            {
                Console.WriteLine("Seeding already completed.");
                return;
            }

            db.ChangeTracker.AutoDetectChangesEnabled = false;

            while (inserted < total)
            {
                var batch = new List<User>(batchSize);

                for (int i = 0; i < batchSize && inserted < total; i++)
                {
                    string email = $"user{inserted}@example.com";

                    batch.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        Email = email
                    });

                    // Bloom filter update stays here
                    await bloomService.AddAsync(email);

                    inserted++;
                }

                await db.Users.AddRangeAsync(batch);
                await db.SaveChangesAsync();

                // must clear this
                db.ChangeTracker.Clear();

                Console.WriteLine($"Inserted: {inserted:N0} users...");
            }

            db.ChangeTracker.AutoDetectChangesEnabled = true;
            Console.WriteLine("Seeding complete!");
        }
    }



}
