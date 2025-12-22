using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace BLOOM_FILTER.Services
{
    public class BloomFilter
    {
        private readonly BitArray bitArray;
        private readonly int size;
        private readonly int hashCount;

        public BloomFilter(int size, int hashCount)
        {
            this.size = size;
            this.hashCount = hashCount;
            bitArray = new BitArray(size);
        }

        public void Add(string item)
        {
            foreach (var position in GetHashPositions(item))
            {
                bitArray[position] = true;
            }
        }

        public bool MightContain(string item)
        {
            foreach (var position in GetHashPositions(item))
            {
                if (!bitArray[position])
                    return false;   // Definitely NOT exist
            }
            return true;            // Might exist
        }

        private int[] GetHashPositions(string input)
        {
            int[] positions = new int[hashCount];
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);

            // Use two 32-bit hashes from SHA256 for double hashing
            int hash1 = BitConverter.ToInt32(hash, 0);
            int hash2 = BitConverter.ToInt32(hash, 4);

            for (int i = 0; i < hashCount; i++)
            {
                long combinedHash = (hash1 + (long)i * hash2) & 0x7FFFFFFF;
                positions[i] = (int)(combinedHash % size);
            }

            return positions;
        }

    }
}
