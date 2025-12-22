
using StackExchange.Redis;

namespace BLOOM_FILTER.Services
{
    public class UserBloomService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly ILogger<UserBloomService> _logger;
        private readonly string _filterName;
        private readonly double _errorRate;
        private readonly int _expectedItems;

        public UserBloomService(IConnectionMultiplexer redis, IConfiguration config, ILogger<UserBloomService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _db = _redis.GetDatabase();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var section = config.GetSection("Redis");
            _filterName = section.GetValue<string>("BloomFilterKey") ?? "users_bloom";
            _errorRate = section.GetValue<double?>("ErrorRate") ?? 0.01;
            _expectedItems = section.GetValue<int?>("ExpectedItems") ?? 2_000_000;
        }

        /// <summary>
        /// Ensures a scalable Bloom filter exists. Uses BF.SCRESERVE if available (scalable filter).
        /// If it fails (module not present or command unknown) logs and allows BF.ADD to implicitly create filter.
        /// </summary>
        public async Task EnsureFilterExistsAsync()
        {
            try
            {
                // Try scalable reserve: BF.SCRESERVE <key> <error_rate>
                var res = await _db.ExecuteAsync("BF.SCRESERVE", _filterName, _errorRate.ToString(System.Globalization.CultureInfo.InvariantCulture));
                _logger.LogInformation("Called BF.SCRESERVE for {Filter} result={Result}", _filterName, res);
            }
            catch (RedisServerException ex)
            {
                // If BF.SCRESERVE unknown, fall back to BF.RESERVE (fixed-size), or ignore (redis will create upon first BF.ADD)
                _logger.LogWarning(ex, "BF.SCRESERVE failed (maybe older module). Attempting BF.RESERVE fallback for {Filter}", _filterName);
                try
                {
                    // Compute optimal m (bits) and k? We'll fallback to storing approximate. We'll reserve with expected items.
                    // BF.RESERVE key error_rate capacity
                    var res2 = await _db.ExecuteAsync("BF.RESERVE", _filterName, _errorRate.ToString(System.Globalization.CultureInfo.InvariantCulture), _expectedItems);
                    _logger.LogInformation("Called BF.RESERVE fallback for {Filter} result={Result}", _filterName, res2);
                }
                catch (Exception inner)
                {
                    _logger.LogWarning(inner, "BF.RESERVE also failed. RedisBloom may not be present; BF.ADD will attempt implicit creation if supported.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error ensuring Bloom filter exists for {Filter}", _filterName);
            }
        }

        /// <summary>
        /// Adds an element to the Bloom filter.
        /// </summary>
        public async Task<bool> AddAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try
            {
                // BF.ADD returns 1 if the item was not previously present, 0 otherwise.
                var result = await _db.ExecuteAsync("BF.ADD", _filterName, value.ToLowerInvariant());
                return result.Type == ResultType.Integer && (long)result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to BF.ADD value to {Filter}", _filterName);
                throw;
            }
        }

        /// <summary>
        /// Check if element might exist.
        /// </summary>
        public async Task<bool> MightContainAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try
            {
                var result = await _db.ExecuteAsync("BF.EXISTS", _filterName, value.ToLowerInvariant());
                return result.Type == ResultType.Integer && (long)result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to BF.EXISTS for {Filter}", _filterName);
                throw;
            }
        }
    }


}
