# Bytesystems.NumberSequenceGenerator

Attribute-driven number sequence generator for Entity Framework Core. Automatically generates sequential, formatted numbers for entity properties on `SaveChanges`.

Supports configurable patterns with date tokens, zero-padding, auto-reset (yearly/monthly/daily), and segmentation. .NET port of the [ByteSystems Symfony NumberSequenceGeneratorBundle](https://github.com/bytesystems/NumberSequenceGeneratorBundle).

## Installation

```bash
dotnet add package Bytesystems.NumberSequenceGenerator
```

## Quick Start

### 1. Register services

```csharp
builder.Services.AddNumberSequenceGenerator();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
        .AddInterceptors(sp.GetRequiredService<NumberSequenceInterceptor>());
});
```

### 2. Configure the entity table

```csharp
// In your DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new NumberSequenceEntityConfiguration());
}
```

### 3. Annotate your entities

```csharp
public class Invoice
{
    public int Id { get; set; }

    [Sequence(Key = "invoice", Pattern = "IV{Y}-{#|6|y}")]
    public string? InvoiceNumber { get; set; }
}
```

### 4. Numbers are generated automatically

```csharp
var invoice = new Invoice();
context.Invoices.Add(invoice);
await context.SaveChangesAsync();

Console.WriteLine(invoice.InvoiceNumber); // "IV2026-000001"
```

## Pattern Syntax

Patterns use `{token|param1|param2}` syntax:

| Token | Description | Example Output |
|-------|-------------|---------------|
| `{#}` | Sequence number (plain) | `42` |
| `{#\|6}` | Sequence, zero-padded to 6 digits | `000042` |
| `{#\|6\|y}` | Sequence, 6 digits, resets yearly | `000001` (after Jan 1) |
| `{Y}` | Year, 4 digits | `2026` |
| `{y}` | Year, 2 digits | `26` |
| `{m}` | Month, 2 digits | `02` |
| `{M}` | Month, 3-letter abbreviation | `Feb` |
| `{d}` | Day, 2 digits | `09` |
| `{D}` | Day, 3-letter abbreviation | `Mon` |
| `{w}` | ISO week number | `06` |
| `{H}` | Hour, 24-hour format | `14` |

### Reset Contexts

The sequence token `{#|padding|context}` supports automatic counter reset:

| Context | Resets When |
|---------|-------------|
| `y` | Year changes |
| `m` | Month changes |
| `w` | ISO week changes |
| `d` | Day changes |
| `h` | Hour changes |

## Pattern Examples

```
"KD-{#|6}"              → KD-000142         (customer number, never resets)
"IV{Y}-{#|6|y}"         → IV2026-000023     (invoice, resets yearly)
"AG-{y}{m}-{#|4|m}"     → AG-2602-0015      (offer, resets monthly)
"JN-{y}{m}{d}-{#|5|d}"  → JN-260209-00003   (journal, resets daily)
"PO-{#|7}"              → PO-0001234        (order, never resets)
```

## Segmentation

Different patterns for different entity types using the same sequence key:

```csharp
// Define segments
[Segment(Value = "OFFER", Pattern = "AG-{y}{m}-{#|4|y}")]
public class OfferSegment;

[Segment(Value = "ORDER", Pattern = "KV-{y}{m}-{#|4|y}")]
public class OrderSegment;

// Use on entity
public class Document
{
    public string DocumentType { get; set; } = string.Empty;

    [Sequence(Key = "document", Segment = "{DocumentType}",
        Pattern = "DOC-{#|4}",
        Segments = new[] { typeof(OfferSegment), typeof(OrderSegment) })]
    public string? DocumentNumber { get; set; }
}
```

Each segment maintains its own independent counter.

## Custom Token Handlers

Extend the system with custom tokens:

```csharp
public class FiscalYearTokenHandler : ITokenHandler
{
    public bool Handles(Token token) => token.Identifier == "FY";
    public string GetValue(Token token, int sequenceValue)
    {
        var now = DateTime.UtcNow;
        var fiscalYear = now.Month >= 4 ? now.Year : now.Year - 1;
        return fiscalYear.ToString();
    }
    public bool RequestsReset(Token token) => false;
}

// Register
builder.Services.AddNumberSequenceGenerator(options =>
{
    options.AddTokenHandler<FiscalYearTokenHandler>();
});

// Use: "INV-{FY}-{#|5|y}" → "INV-2025-00042"
```

## How It Works

1. An EF Core `SaveChangesInterceptor` detects newly added entities
2. It scans for properties with `[Sequence]` attributes via reflection
3. The `NumberGenerator` service resolves the correct sequence from the database
4. Tokens are parsed, reset logic is evaluated, the counter is incremented
5. The formatted number is set on the property before the entity is saved
6. The sequence counter is persisted in the `number_sequences` table

## Configuration

### Table Name

```csharp
modelBuilder.ApplyConfiguration(new NumberSequenceEntityConfiguration("my_custom_table"));
```

### Concurrency

The `CurrentNumber` column uses EF Core's `[ConcurrencyCheck]` for optimistic concurrency control. If two requests try to generate a number simultaneously, one will get a `DbUpdateConcurrencyException` and should retry.

## License

[MIT](LICENSE)
