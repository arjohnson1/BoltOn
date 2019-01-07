using System;
using BoltOn.Data;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bootstrapping;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace BoltOn.Tests.Data
{
	public class EFRepositoryTests : IDisposable
	{
		private ITestRepository _sut;

		[Fact]
		public void GetById_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.GetById<Student>(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact]
		public void GetById_WhenRecordDoesNotExist_ReturnsNull()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.GetById<Student>(3);

			// assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetByIdAsync_WhenRecordExists_ReturnsRecord()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = await _sut.GetByIdAsync<Student>(1);

			// assert
			Assert.NotNull(result);
			Assert.Equal("a", result.FirstName);
		}

		[Fact]
		public void GetAll_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.GetAll<Student>().ToList();

			// assert
			Assert.Equal(2, result.Count);
		}

		[Fact]
		public async Task GetAllAsync_WhenRecordsExist_ReturnsAllTheRecords()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = await _sut.GetAllAsync<Student>();

			// assert
			Assert.Equal(2, result.Count());
		}

		[Fact]
		public void FindByWithoutIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.FindBy<Student>(f => f.Id == 2).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			// without includes this should be empty, but it's not, as InMemoryDb uses the same dbcontext and the entity is already loaded
			//Assert.Empty(result.Addresses);
		}

		[Fact]
		public void FindByWithIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = _sut.FindBy<Student>(f => f.Id == 2, f => f.Addresses).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			Assert.NotEmpty(result.Addresses);
		}

		[Fact]
		public async Task FindByAsyncWithIncludes_WhenRecordsExist_ReturnsRecordsThatMatchesTheCriteria()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);

			// act
			var result = (await _sut.FindByAsync<Student>(f => f.Id == 2, default(CancellationToken), f => f.Addresses)).FirstOrDefault();

			// assert
			Assert.NotNull(result);
			Assert.Equal("x", result.FirstName);
			Assert.NotEmpty(result.Addresses);
		}

		[Fact]
		public void Add_AddANewEntity_ReturnsAddedEntity()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);
			const int newStudentId = 3;
			var student = new Student
			{
				Id = newStudentId,
				FirstName = "c",
				LastName = "d"
			};

			// act
			var result = _sut.Add(student);
			var queryResult = _sut.GetById<Student>(newStudentId);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
			Assert.Equal(result.FirstName, queryResult.FirstName);
		}

		[Fact]
		public async Task AddAsync_AddANewEntity_ReturnsAddedEntity()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);
			const int newStudentId = 3;
			var student = new Student
			{
				Id = newStudentId,
				FirstName = "c",
				LastName = "d"
			};

			// act
			var result = await _sut.AddAsync(student);
			var queryResult = _sut.GetById<Student>(newStudentId);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
			Assert.Equal(result.FirstName, queryResult.FirstName);
		}

		[Fact]
		public void Update_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);
			var student = _sut.GetById<Student>(2);

			// act
			student.FirstName = "c";
			_sut.Update(student);
			var queryResult = _sut.GetById<Student>(2);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
		}

		[Fact]
		public async Task UpdateAsync_UpdateAnExistingEntity_UpdatesTheEntity()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			var serviceProvider = SetUpInMemoryDb(serviceCollection);
			var student = _sut.GetById<Student>(2);

			// act
			student.FirstName = "c";
		    await _sut.UpdateAsync(student);
			var queryResult = _sut.GetById<Student>(2);

			// assert
			Assert.NotNull(queryResult);
			Assert.Equal("c", queryResult.FirstName);
		}

		private ServiceProvider SetUpInMemoryDb(IServiceCollection serviceCollection)
		{
			serviceCollection.AddDbContext<SchoolDbContext>(options =>
			{
				options.UseInMemoryDatabase("InMemoryDbForTesting");
			});
			serviceCollection.BoltOn();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			var testDbContext = serviceProvider.GetService<SchoolDbContext>();
			testDbContext.Set<Student>().Add(new Student
			{
				Id = 1,
				FirstName = "a",
				LastName = "b"
			});
			var student = new Student
			{
				Id = 2,
				FirstName = "x",
				LastName = "y"
			};
			var address = new Address { Id = Guid.NewGuid(), Street = "Computer Science", Student = student };
			testDbContext.Set<Student>().Add(student);
			testDbContext.Set<Address>().Add(address);
			testDbContext.SaveChanges();
			serviceProvider.UseBoltOn();
			_sut = serviceProvider.GetService<ITestRepository>();
			return serviceProvider;
		}

		public void Dispose()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public interface ITestRepository : IRepository
	{

	}

	public class TestRepository : BaseEFRepository<SchoolDbContext>, ITestRepository
	{
		public TestRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}

	public class SchoolDbContext : BaseDbContext<SchoolDbContext>
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}
	}

	public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<Address> Addresses { get; set; } = new List<Address>();
	}

	public class Address : BaseEntity<Guid>
	{
		public string Street { get; set; }
		public string City { get; set; }
		public Student Student { get; set; }
	}

	public class StudentMapping : IEntityTypeConfiguration<Student>
	{
		public void Configure(EntityTypeBuilder<Student> builder)
		{
			builder
				.ToTable("Student")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("StudentId");
			builder
				.HasMany(p => p.Addresses)
				.WithOne(p => p.Student);
		}
	}

	public class AddressMapping : IEntityTypeConfiguration<Address>
	{
		public void Configure(EntityTypeBuilder<Address> builder)
		{
			builder
				.ToTable("Address")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("AddressId");
		}
	}
}
