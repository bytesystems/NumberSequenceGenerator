using Bytesystems.NumberSequenceGenerator.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bytesystems.NumberSequenceGenerator.Configuration;

/// <summary>
/// EF Core entity configuration for the <see cref="NumberSequence"/> entity.
/// Apply this in your DbContext's OnModelCreating to configure the table.
/// </summary>
public class NumberSequenceEntityConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    private readonly string _tableName;

    /// <summary>
    /// Creates a new configuration with the specified table name.
    /// </summary>
    /// <param name="tableName">The database table name. Defaults to "number_sequences".</param>
    public NumberSequenceEntityConfiguration(string tableName = "number_sequences")
    {
        _tableName = tableName;
    }

    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable(_tableName);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Key)
            .HasColumnName("sequence_key")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Segment)
            .HasMaxLength(255);

        builder.Property(e => e.Pattern)
            .HasMaxLength(255)
            .HasDefaultValue("{#}");

        builder.Property(e => e.CurrentNumber)
            .HasDefaultValue(0)
            .IsConcurrencyToken();

        builder.Property(e => e.UpdatedAt);

        builder.HasIndex(e => new { e.Key, e.Segment })
            .IsUnique()
            .HasDatabaseName("IX_NumberSequence_Key_Segment");
    }
}
