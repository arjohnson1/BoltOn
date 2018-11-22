﻿using System.Linq;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var serviceCollection = context.ServiceCollection;
			serviceCollection.AddTransient<IMediator, Mediator>();

			var mediatorOptions = context.GetOptions<MediatorOptions>();
			//mediatorOptions.Middlewares.ForEach(m => serviceCollection.AddTransient(typeof(IMediatorMiddleware), m));

			context.ServiceCollection.Configure<UnitOfWorkOptions>(u =>
			{
				u.DefaultCommandIsolationLevel = mediatorOptions.UnitOfWorkOptions.DefaultCommandIsolationLevel;
				u.DefaultIsolationLevel = mediatorOptions.UnitOfWorkOptions.DefaultIsolationLevel;
				u.DefaultQueryIsolationLevel = mediatorOptions.UnitOfWorkOptions.DefaultQueryIsolationLevel;
				u.DefaultTransactionTimeout = mediatorOptions.UnitOfWorkOptions.DefaultTransactionTimeout;
			});

			RegisterHandlers(context);
		}

		private void RegisterHandlers(RegistrationTaskContext context)
		{
			var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
			var handlers = (from a in context.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								requestHandlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				context.ServiceCollection.AddTransient(handler.Interface, handler.Implementation);
		}
	}
}
