using System;
using System.Linq;
using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Data.CosmosDb
{
	[Collection("IntegrationTests")]
	public class CosmosDbRepositoryTests : IDisposable
	{
		private readonly IRepository<StudentFlattened> _sut;

		public CosmosDbRepositoryTests()
		{
			// this flag can be set to true for [few] tests. Running all the tests with this set to true might slow down.
			IntegrationTestHelper.IsCosmosDbServer = false;
			IntegrationTestHelper.IsSeedCosmosDbData = true;

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn(options =>
				{
					options
						.BoltOnCosmosDbModule();
				});
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();

			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			_sut = serviceProvider.GetService<IRepository<StudentFlattened>>();
		}

		[Fact]
		public async Task DeleteAsync_DeleteById_DeletesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.Parse("ff96d626-3911-4c78-b337-00d7ecd2eadd");
			var studentFlattened = new StudentFlattened { Id = id };

			// act
			await _sut.DeleteAsync(studentFlattened, new RequestOptions { PartitionKey = new PartitionKey(1) });

			// assert
			var queryResult = await _sut.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(1) });
			Assert.Null(queryResult);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");

			// act
			var result = await _sut.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(1) });

			// assert
			Assert.NotNull(result);
			Assert.Equal("john", result.FirstName);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = await _sut.GetAllAsync();

			// assert
			Assert.True(result.Count() > 1);
		}

		[Fact]
		public async Task FindByAsync_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = (await _sut.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "johnny")).FirstOrDefault();

			// assert
			Assert.Null(result);
		}

		[Fact]
		public async Task FindByAsync_WhenRecordExist_ReturnsRecord()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;

			// act
			var result = (await _sut.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "john")).FirstOrDefault();

			// assert
			Assert.NotNull(result);
		}

		[Fact]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
			var student = new StudentFlattened { Id = id, LastName = "smith jr", FirstName = "john", StudentTypeId = 1 };

			// act
			await _sut.UpdateAsync(student, new RequestOptions { PartitionKey = new PartitionKey(1) });

			// assert
			var result = (await _sut.FindByAsync(f => f.StudentTypeId == 1 && f.FirstName == "john")).FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal("smith jr", result.LastName);
		}

		[Fact]
		public async Task AddAsync_AddANewEntity_ReturnsAddedEntity()
		{
			// arrange
			if (!IntegrationTestHelper.IsCosmosDbServer)
				return;
			var id = Guid.NewGuid();
			var studentFlattened = new StudentFlattened { Id = id, StudentTypeId = 2, FirstName = "meghan", LastName = "doe" };

			// act
			_ = await _sut.AddAsync(studentFlattened);

			// assert
			var result = await _sut.GetByIdAsync(id, new RequestOptions { PartitionKey = new PartitionKey(2) });
			Assert.NotNull(result);
			Assert.Equal("meghan", result.FirstName);
		}

		public void Dispose()
		{
			if (IntegrationTestHelper.IsCosmosDbServer)
			{
				var id = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
				var student = new StudentFlattened { Id = id };
				_sut.DeleteAsync(student, new RequestOptions { PartitionKey = new PartitionKey(1) }).GetAwaiter().GetResult();
			}

			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}
