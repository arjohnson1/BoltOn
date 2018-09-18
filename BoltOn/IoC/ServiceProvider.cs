﻿using System;
using System.Collections.Generic;

namespace BoltOn.IoC
{
	public interface IServiceProvider
	{
		IEnumerable<TService> GetAllInstances<TService>() where TService : class;
		TService GetInstance<TService>() where TService : class;
		object GetInstance(Type type);
	}

    public class ServiceProvider : IServiceProvider
    {
        private readonly IBoltOnContainer _boltOnContainer;

        public ServiceProvider(IBoltOnContainer boltOnContainer)
        {
            _boltOnContainer = boltOnContainer;
        }

        public IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return _boltOnContainer.GetAllInstances<TService>();
        }

        public TService GetInstance<TService>() where TService : class
        {
            return _boltOnContainer.GetInstance<TService>();
        }

        public object GetInstance(Type type)
        {
            return _boltOnContainer.GetInstance(type);
        }
    }
}