﻿using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Other;

namespace BoltOn.UoW
{
	public interface IUnitOfWork : IDisposable
	{
		void Start();
		void Commit();
	}

	[ExcludeFromRegistration]
	public sealed class UnitOfWork : IUnitOfWork
	{
		private TransactionScope _transactionScope;
		private readonly IBoltOnLogger<UnitOfWork> _logger;
		private readonly UnitOfWorkOptions _unitOfWorkOptions;

		internal UnitOfWork(IBoltOnLoggerFactory loggerFactory, UnitOfWorkOptions unitOfWorkOptions)
		{
			_logger = loggerFactory.Create<UnitOfWork>();
			_unitOfWorkOptions = unitOfWorkOptions;
		}

		public void Start()
		{
			_logger.Debug($"Starting UoW...");
			_transactionScope = new TransactionScope(_unitOfWorkOptions.TransactionScopeOption, new TransactionOptions
			{
				IsolationLevel = _unitOfWorkOptions.IsolationLevel,
				Timeout = _unitOfWorkOptions.TransactionTimeout
			}, TransactionScopeAsyncFlowOption.Enabled);
			_logger.Debug("Started UoW");
		}

		public void Commit()
		{
			_logger.Debug("Committing UoW...");
			_transactionScope.Complete();
			_logger.Debug("Committed UoW");
		}

		public void Dispose()
		{
			_logger.Debug("Disposing UoW...");
			_transactionScope.Dispose();
			_logger.Debug("Disposed UoW");
		}
	}
}
