using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models;
using PaydayBackend.Models.Abstractions;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Test.Services;

public abstract class RepositoryReaderTest<TRepo, TEntity> : IDisposable
    where TRepo : IRepositoryReader<TEntity>
    where TEntity : Entity
{
    protected readonly ContractContext _context;
    protected readonly DbSet<TEntity> _dbset;
    protected readonly TRepo _repository;

    protected abstract TEntity CreateFakeEntity(int id);

    public RepositoryReaderTest(Func<ContractContext, TRepo> repositoryFactory)
    {
        var options = new DbContextOptionsBuilder<ContractContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContractContext(options);
        _dbset = _context.Set<TEntity>();

        _repository = repositoryFactory(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_NegativeId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = async () =>
            await _repository.GetByIdAsync(-10, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage(
                "Specified argument was out of the range of valid values. (Parameter 'Only positive `id` are allowed')"
            );
    }

    [Fact]
    public async Task GetByIdAsync_PayerExist_ReturnsPayer()
    {
        // Arrange
        var fakePayer = CreateFakeEntity(7);

        _dbset.Add(fakePayer);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByIdAsync(7, TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .NotBeNull()
            .And.Satisfy<Payer>(p =>
                p.Id.Should().Be(7, because: "that is the ID that we are looking for")
            );
    }

    [Fact]
    public async Task GetByIdAsync_RepoPopulatedButPayerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var fakePayer = CreateFakeEntity(8);

        _dbset.Add(fakePayer);
        _context.SaveChanges();

        // Act
        var result = await _repository.GetByIdAsync(7, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull(because: "the payer that exist ins't the one we are looking for");
    }

    [Fact]
    public async Task GetByIdAsync_PayerDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(10, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull(because: "the repository is empty");
    }

    [Fact]
    public async Task ExistsAsync_PayerDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(10, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_PayerExist_ReturnsTrue()
    {
        // Arrange
        var fakePayer = CreateFakeEntity(2);
        _dbset.Add(fakePayer);
        _context.SaveChanges();

        // Act
        var result = await _repository.ExistsAsync(2, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_RepoPopulatedButPayerDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var fakePayer = CreateFakeEntity(10);
        _dbset.Add(fakePayer);
        _context.SaveChanges();

        // Act
        var result = await _repository.ExistsAsync(2, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_NegativeId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = async () =>
            await _repository.ExistsAsync(-10, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage(
                "Specified argument was out of the range of valid values. (Parameter 'Only positive `id` are allowed')"
            );
    }

    [Fact]
    public async Task GetAllAsync_RepoNotPopulated_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync(cancel: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task GetAllAsync_RepoPopulated_ReturnsPayers(int n)
    {
        // Arrange
        for (int i = 1; i <= n; i++)
            _dbset.Add(CreateFakeEntity(i));

        _context.SaveChanges();

        // Act
        var result = await _repository.GetAllAsync(cancel: TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull().And.Contain(_dbset).And.HaveSameCount(_context.Payers);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task GetAllAsync_RepoPopulatedAndSizeFiltering_ReturnsPayersSubset(int size)
    {
        // Arrange
        for (int i = 1; i <= 100; i++)
            _dbset.Add(CreateFakeEntity(i));

        _context.SaveChanges();

        // Act
        var result = await _repository.GetAllAsync(
            size: size,
            cancel: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull().And.BeSubsetOf(_dbset).And.HaveCountLessThanOrEqualTo(size);
    }

    [Fact]
    public async Task GetAllAsync_NegativeSizeFiltering_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = async () =>
            await _repository.GetAllAsync(size: -7, cancel: TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage(
                "Specified argument was out of the range of valid values. (Parameter 'Only positive `size` are allowed')"
            );
    }

    [Fact]
    public async Task GetAllAsync_NegativeAfterId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = async () =>
            await _repository.GetAllAsync(
                afterId: -7,
                cancel: TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage(
                "Specified argument was out of the range of valid values. (Parameter 'Only positive `afterId` are allowed')"
            );
    }

    [Fact]
    public async Task GetAllAsync_NegativeBeforeId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = async () =>
            await _repository.GetAllAsync(
                beforeId: -7,
                cancel: TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage(
                "Specified argument was out of the range of valid values. (Parameter 'Only positive `beforeId` are allowed')"
            );
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(3, 10)]
    public async Task GetAllAsync_RepoPopulatedAndFilteredByBeforeIdAndAfterId_ReturnsPayersSubset(
        int afterId,
        int beforeId
    )
    {
        // Arrange
        for (int i = 1; i <= 100; i++)
            _dbset.Add(CreateFakeEntity(i));

        _context.SaveChanges();

        // Act
        var result = await _repository.GetAllAsync(
            afterId,
            beforeId,
            cancel: TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .NotBeNull()
            .And.BeSubsetOf(_dbset)
            .And.ContainInOrder(
                _dbset.Where(p => p.Id > afterId && p.Id < beforeId).OrderBy(p => p.Id)
            );
    }

    [Fact]
    public async Task GetAllAsync_SameAfterIdAndBeforId_ReturnsEmptyList()
    {
        // Arrange
        for (int i = 1; i <= 100; i++)
            _dbset.Add(CreateFakeEntity(i));

        _context.SaveChanges();

        // Act
        var result = await _repository.GetAllAsync(
            10,
            10,
            cancel: TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }
}
