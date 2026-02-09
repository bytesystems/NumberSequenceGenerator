using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bytesystems.NumberSequenceGenerator.Entity;

/// <summary>
/// Persisted number sequence tracking entity. Stores the current counter, pattern, and last update
/// timestamp for reset detection. Each sequence is uniquely identified by its key + segment combination.
/// </summary>
public class NumberSequence
{
    public const int DefaultInit = 0;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// The sequence key (e.g. "customer", "invoice", "order").
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("sequence_key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Optional segment within the sequence (e.g. "OFFER", "42" for supplier ID).
    /// Null for non-segmented sequences.
    /// </summary>
    [MaxLength(255)]
    public string? Segment { get; set; }

    /// <summary>
    /// The pattern used to generate numbers. Supports tokens like {#|6|y}, {Y}, {m}, {d}.
    /// </summary>
    [MaxLength(255)]
    public string Pattern { get; set; } = "{#}";

    /// <summary>
    /// The current sequence counter value. The next generated number will be this + 1.
    /// </summary>
    [ConcurrencyCheck]
    public int CurrentNumber { get; set; } = DefaultInit;

    /// <summary>
    /// Timestamp of the last number generation. Used for reset context detection.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Increments and returns the next number in the sequence.
    /// If the current value is less than the initial value, starts from the initial value.
    /// </summary>
    public int GetNextNumber(int initialValue = 0)
    {
        var currentValue = Math.Max(CurrentNumber, initialValue);
        CurrentNumber = currentValue + 1;
        UpdatedAt = DateTime.UtcNow;
        return CurrentNumber;
    }
}
