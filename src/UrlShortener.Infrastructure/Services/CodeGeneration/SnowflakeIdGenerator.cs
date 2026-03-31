using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.IdGeneration;

namespace UrlShortener.Infrastructure.Services.CodeGeneration;

public class SnowflakeSettings
{
    public int MachineId { get; set; } = 1;

    /// <summary>
    /// The epoch used as the timestamp origin for ID generation.
    /// IMPORTANT: This value must remain fixed once IDs start being generated,
    /// as changing it would break decoding of existing IDs.
    /// Override via SnowflakeSettings:Epoch in configuration or environment variables.
    /// </summary>
    public DateTime Epoch { get; set; } = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}

public class SnowflakeIdGenerator : ISnowflakeIdGenerator
{
    private const int MachineIdBits = 10;
    private const int SequenceBits = 12;
    private const long MaxMachineId = (1L << MachineIdBits) - 1;
    private const long MaxSequence = (1L << SequenceBits) - 1;
    private const int TimestampShift = MachineIdBits + SequenceBits;
    private const int MachineIdShift = SequenceBits;

    private readonly long _machineId;
    private readonly long _epochMs;
    private long _lastTimestamp = -1;
    private long _sequence = 0;
    private readonly object _lock = new();

    public SnowflakeIdGenerator(IOptions<SnowflakeSettings> settings)
    {
        var s = settings.Value;
        if (s.MachineId < 0 || s.MachineId > MaxMachineId)
            throw new ArgumentOutOfRangeException(nameof(settings), $"MachineId must be between 0 and {MaxMachineId}.");
        _machineId = s.MachineId;
        var epoch = DateTime.SpecifyKind(s.Epoch, DateTimeKind.Utc);
        _epochMs = new DateTimeOffset(epoch).ToUnixTimeMilliseconds();
    }

    public long NextId()
    {
        lock (_lock)
        {
            var timestamp = GetCurrentTimestamp();

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException("Clock moved backwards. Refusing to generate ID.");

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                    timestamp = WaitNextMillisecond(_lastTimestamp);
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;
            return (timestamp << TimestampShift) | (_machineId << MachineIdShift) | _sequence;
        }
    }

    private long GetCurrentTimestamp() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _epochMs;

    private long WaitNextMillisecond(long lastTimestamp)
    {
        var timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp)
            timestamp = GetCurrentTimestamp();
        return timestamp;
    }
}
