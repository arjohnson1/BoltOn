﻿using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb
{
	public class RegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			if (IntegrationTestHelper.IsCosmosDbServer)
			{
				var cosmosDbOptions = new TestSchoolCosmosDbOptions
				{
					Uri = "",
					AuthorizationKey = "",
					DatabaseName = ""
				};
				context.Container.AddCosmosDb<TestSchoolCosmosDbOptions>(options =>
				{
					options.Uri = cosmosDbOptions.Uri;
					options.AuthorizationKey = cosmosDbOptions.AuthorizationKey;
					options.DatabaseName = cosmosDbOptions.DatabaseName;
				});

				using (var client = new DocumentClient(new Uri(cosmosDbOptions.Uri), cosmosDbOptions.AuthorizationKey))
				{
					client.CreateDatabaseIfNotExistsAsync(new Database { Id = cosmosDbOptions.DatabaseName }).GetAwaiter().GetResult();

					var documentCollection = new DocumentCollection { Id = nameof(StudentFlattened).Pluralize() };
					documentCollection.PartitionKey.Paths.Add("/studentTypeId");
					client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(cosmosDbOptions.DatabaseName),
						documentCollection).GetAwaiter().GetResult();
				}
			}

			context.Container.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, TestSchoolCosmosDbOptions>>();
		}
	}
}
