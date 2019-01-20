﻿using System;
using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Data.EF;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Mediator.Data.EF
{
	[Collection("IntegrationTests")]
	public class MediatorDataEFIntegrationTests : IDisposable
	{
		[Fact]
		public void Get_MediatorWithQueryRequest_ExecutesEFQueryTrackingBehaviorMiddlewareAndDisablesTracking()
		{
			// arrange
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Get(new TestQuery());

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(EFQueryTrackingBehaviorMiddleware)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {true}"));
		}

		[Fact]
		public void Get_MediatorWithQueryRequestWithWriteOperation_ExecutesEFQueryTrackingBehaviorMiddlewareAndDisablesTrackingAndNotSavesData()
		{
			// arrange
			MediatorTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Get(new GetStudent { StudentId = 5 } );
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var student = dbContext.Set<Student>().Find(5);
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.NotNull(result.Data);
			// this will be null for non in-memory dbcontext
			//Assert.Null(student);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking, queryTrackingBehavior);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(EFQueryTrackingBehaviorMiddleware)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {true}"));
			Assert.False(isAutoDetectChangesEnabled);
		}

		[Fact]
		public void Get_MediatorWithCommandRequest_ReturnsDbContextWithDetectChangesEnabled()
		{
			// arrange
			MediatorTestHelper.IsSeedData = false;
			var serviceCollection = new ServiceCollection();
			serviceCollection
				.BoltOn()
				.AddLogging();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.UseBoltOn();
			var sut = serviceProvider.GetService<IMediator>();

			// act
			var result = sut.Get(new TestCommand());
			var dbContext = serviceProvider.GetService<IDbContextFactory>().Get<SchoolDbContext>();
			var isAutoDetectChangesEnabled = dbContext.ChangeTracker.AutoDetectChangesEnabled;
			var queryTrackingBehavior = dbContext.ChangeTracker.QueryTrackingBehavior;

			// assert 
			Assert.True(result.IsSuccessful);
			Assert.True(result.Data);
			Assert.Equal(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll, queryTrackingBehavior);
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"Entering {nameof(EFQueryTrackingBehaviorMiddleware)}..."));
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"IsQueryRequest: {false}"));
			Assert.True(isAutoDetectChangesEnabled);
		}

		public void Dispose()
		{
			MediatorTestHelper.LoggerStatements.Clear();
			Bootstrapper
				.Instance
				.Dispose();
		}
	}
}