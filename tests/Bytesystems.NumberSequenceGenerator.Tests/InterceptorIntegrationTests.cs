using Bytesystems.NumberSequenceGenerator.Attributes;
using Bytesystems.NumberSequenceGenerator.Configuration;
using Bytesystems.NumberSequenceGenerator.Extensions;
using Bytesystems.NumberSequenceGenerator.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bytesystems.NumberSequenceGenerator.Tests;

// Test entities for integration tests
public class Invoice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [Sequence(Key = "invoice", Pattern = "IV{Y}-{#|6|y}")]
    public string? InvoiceNumber { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;

    [Sequence(Key = "customer", Pattern = "KD-{#|6}")]
    public string? CustomerNumber { get; set; }
}

public class Document
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;

    [Sequence(Key = "document", Segment = "{DocumentType}", Pattern = "DOC-{#|4}",
        Segments = [typeof(TestOfferSegment), typeof(TestOrderSegment)])]
    public string? DocumentNumber { get; set; }
}

[Segment(Value = "OFFER", Pattern = "AG-{y}{m}-{#|4|y}")]
public class TestOfferSegment;

[Segment(Value = "ORDER", Pattern = "KV-{y}{m}-{#|4|y}")]
public class TestOrderSegment;

// Test DbContext
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Entity.NumberSequence> NumberSequences => Set<Entity.NumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new NumberSequenceEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

public class InterceptorIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;

    public InterceptorIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddNumberSequenceGenerator();

        services.AddDbContext<TestDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .AddInterceptors(sp.GetRequiredService<NumberSequenceInterceptor>());
        });

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TestDbContext>();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SaveChanges_NewInvoice_GeneratesInvoiceNumber()
    {
        // Arrange
        var invoice = new Invoice { Name = "Test Invoice" };

        // Act
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Assert
        invoice.InvoiceNumber.Should().NotBeNullOrEmpty();
        invoice.InvoiceNumber.Should().StartWith($"IV{DateTime.UtcNow:yyyy}-");
        invoice.InvoiceNumber.Should().MatchRegex(@"^IV\d{4}-\d{6}$");
    }

    [Fact]
    public async Task SaveChanges_MultipleInvoices_GeneratesSequentialNumbers()
    {
        // Arrange & Act
        var invoice1 = new Invoice { Name = "Invoice 1" };
        _context.Invoices.Add(invoice1);
        await _context.SaveChangesAsync();

        var invoice2 = new Invoice { Name = "Invoice 2" };
        _context.Invoices.Add(invoice2);
        await _context.SaveChangesAsync();

        // Assert
        invoice1.InvoiceNumber.Should().EndWith("000001");
        invoice2.InvoiceNumber.Should().EndWith("000002");
    }

    [Fact]
    public async Task SaveChanges_NewCustomer_GeneratesCustomerNumber()
    {
        // Arrange
        var customer = new Customer { FirstName = "Max" };

        // Act
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Assert
        customer.CustomerNumber.Should().Be("KD-000001");
    }

    [Fact]
    public async Task SaveChanges_MultipleCustomers_SequentialNumbers()
    {
        // Arrange & Act
        for (int i = 1; i <= 3; i++)
        {
            var customer = new Customer { FirstName = $"Customer {i}" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            customer.CustomerNumber.Should().Be($"KD-{i:D6}");
        }
    }

    [Fact]
    public async Task SaveChanges_DifferentSequenceKeys_IndependentCounters()
    {
        // Arrange & Act
        var invoice = new Invoice { Name = "Invoice" };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var customer = new Customer { FirstName = "Customer" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Assert - each should be #1 in their own sequence
        invoice.InvoiceNumber.Should().EndWith("000001");
        customer.CustomerNumber.Should().Be("KD-000001");
    }

    [Fact]
    public async Task SaveChanges_SegmentedDocument_UsesSegmentPattern()
    {
        // Arrange
        var offer = new Document { DocumentType = "OFFER" };
        var order = new Document { DocumentType = "ORDER" };

        // Act
        _context.Documents.Add(offer);
        await _context.SaveChangesAsync();

        _context.Documents.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var now = DateTime.UtcNow;
        offer.DocumentNumber.Should().StartWith($"AG-{now:yy}{now:MM}-");
        offer.DocumentNumber.Should().EndWith("0001");

        order.DocumentNumber.Should().StartWith($"KV-{now:yy}{now:MM}-");
        order.DocumentNumber.Should().EndWith("0001"); // Independent counter per segment
    }

    [Fact]
    public async Task SaveChanges_UnknownSegment_CreatesOwnSequenceWithDefaultPattern()
    {
        // Arrange
        var doc = new Document { DocumentType = "INVOICE" };

        // Act
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        // Assert - uses default pattern but with its own independent counter
        doc.DocumentNumber.Should().Be("DOC-0001");
    }

    [Fact]
    public async Task SaveChanges_MultipleUnknownSegments_HaveIndependentCounters()
    {
        // Arrange - two different unknown segments
        var invoice = new Document { DocumentType = "INVOICE" };
        var receipt = new Document { DocumentType = "RECEIPT" };

        // Act
        _context.Documents.Add(invoice);
        await _context.SaveChangesAsync();

        _context.Documents.Add(receipt);
        await _context.SaveChangesAsync();

        // Assert - each unknown segment gets its own counter starting at 1
        invoice.DocumentNumber.Should().Be("DOC-0001");
        receipt.DocumentNumber.Should().Be("DOC-0001"); // Independent, not 0002!
    }

    [Fact]
    public async Task SaveChanges_BatchInsert_GeneratesSequentialNumbers()
    {
        // Arrange - add multiple entities in a single SaveChanges call
        var customers = Enumerable.Range(1, 50)
            .Select(i => new Customer { FirstName = $"Customer {i}" })
            .ToList();

        // Act - batch insert
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();

        // Assert - each should have a unique sequential number
        for (int i = 0; i < customers.Count; i++)
        {
            customers[i].CustomerNumber.Should().Be($"KD-{(i + 1):D6}",
                because: $"customer {i + 1} should get sequential number {i + 1}");
        }

        // Verify only ONE sequence record exists
        var sequenceCount = _context.NumberSequences.Count(s => s.Key == "customer");
        sequenceCount.Should().Be(1, because: "all customers should share one sequence record");
    }

    [Fact]
    public async Task SaveChanges_ExistingValue_DoesNotOverwrite()
    {
        // Arrange
        var invoice = new Invoice
        {
            Name = "Manual Invoice",
            InvoiceNumber = "MANUAL-001"
        };

        // Act
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Assert - should keep the manual value
        invoice.InvoiceNumber.Should().Be("MANUAL-001");
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
