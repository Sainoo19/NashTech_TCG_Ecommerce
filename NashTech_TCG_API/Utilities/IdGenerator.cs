using Microsoft.EntityFrameworkCore;
using NashTech_TCG_API.Data;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Utilities.Interfaces;

namespace NashTech_TCG_API.Utilities
{
    public class IdGenerator : IIdGenerator
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IdGenerator> _logger;

        public IdGenerator(AppDbContext context, ILogger<IdGenerator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateId(string prefix)
        {
            // Use a transaction with Serializable isolation level to handle concurrent access
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // Find existing counter or create new one
                var counter = await _context.SequenceCounters
                    .FirstOrDefaultAsync(c => c.Id == prefix);

                if (counter == null)
                {
                    _logger.LogInformation($"Creating new sequence counter for prefix: {prefix}");
                    counter = new SequenceCounter { Id = prefix, Sequence = 1 };
                    _context.SequenceCounters.Add(counter);
                }
                else
                {
                    counter.Sequence++;
                    _logger.LogDebug($"Incremented sequence counter for {prefix} to {counter.Sequence}");
                }

                await _context.SaveChangesAsync();

                // Calculate number of digits needed
                int sequenceNumber = counter.Sequence;
                int numberOfDigits = Math.Max(3, sequenceNumber.ToString().Length);
                string id = prefix + sequenceNumber.ToString().PadLeft(numberOfDigits, '0');

                await transaction.CommitAsync();
                _logger.LogInformation($"Generated new ID: {id}");
                return id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error generating ID for prefix {prefix}");
                throw new InvalidOperationException($"Failed to generate ID for {prefix}. Please try again.", ex);
            }
        }
    }
}