using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
using PaydayBackend.Models;
using PaydayBackend.Models.Abstractions;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Test.Services;

public abstract class RepositoryWriterTest<TRepo, TEntity> : IDisposable
    where TRepo : IRepositoryWriter<TEntity>
    where TEntity : Entity
{
    protected readonly ContractContext _context;
    protected readonly DbSet<TEntity> _dbset;
    protected readonly TRepo _repository;

    protected abstract TEntity CreateFakeEntity(int id);

    public RepositoryWriterTest(Func<ContractContext, TRepo> repositoryFactory)
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
    public async Task CreateAsync_EntityDoesNotExist_CreateNew()
    {
        // Act
        var fakeEntity = CreateFakeEntity(10);
        await _repository.CreateAsync(fakeEntity, TestContext.Current.CancellationToken);

        // Assert
        _dbset.Find(10).Should().NotBeNull().And.BeEquivalentTo(fakeEntity);
    }

    [Fact]
    public async Task CreateAsync_EntityExist_ThrowDuplicateEntityException()
    {
        // Arrange
        var fakeEntity = CreateFakeEntity(10);
        _dbset.Add(fakeEntity);
        _context.SaveChanges();

        // Act
        var act = async () =>
            await _repository.CreateAsync(fakeEntity, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task CreateOrReplaceAsync_EntityDoeNotExist_CreateNew()
    {
        // Act
        var fakeEntity = CreateFakeEntity(10);
        await _repository.CreateOrReplaceAsync(fakeEntity, TestContext.Current.CancellationToken);

        // Assert
        _dbset.Find(10).Should().NotBeNull().And.BeEquivalentTo(fakeEntity);
    }

    [Fact]
    public async Task CreateOrReplaceAsync_EntityExist_DoesNotThrowDuplicateEntityException()
    {
        // Arrange
        var fakeEntity = CreateFakeEntity(10);
        _dbset.Add(fakeEntity);
        _context.SaveChanges();

        // Act
        var act = async () =>
            await _repository.CreateOrReplaceAsync(
                fakeEntity,
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().NotThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task DeleteAsync_EntityDoesNotExist_ThrowEntityNotFoundException()
    {
        // Act
        var act = async () =>
            await _repository.DeleteAsync(10, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_EntityExist_RemovesEntity()
    {
        // Arrange
        _dbset.Add(CreateFakeEntity(10));
        _context.SaveChanges();

        // Act
        await _repository.DeleteAsync(10, TestContext.Current.CancellationToken);

        // Assert
        _dbset.Find(10).Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_EntityDoesNotExist_ThrowEntityNotFoundException()
    {
        // Act
        var act = async () =>
            await _repository.UpdateAsync(
                10,
                new { Name = "Hello" },
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
