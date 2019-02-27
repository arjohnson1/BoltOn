﻿using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Mediator.Pipeline
{
	internal class RequestHandlerDecorator<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IRequestHandler<TRequest, TResponse> _requestHandler;

		public RequestHandlerDecorator(IRequestHandler<TRequest, TResponse> requestHandler)
		{
			_requestHandler = requestHandler;
		}

		public TResponse Handle(IRequest<TResponse> request)
		{
			return _requestHandler.Handle((TRequest)request);
		}
	}

	internal class RequestAsyncHandlerDecorator<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IRequestAsyncHandler<TRequest, TResponse> _requestHandler;

		public RequestAsyncHandlerDecorator(IRequestAsyncHandler<TRequest, TResponse> requestHandler)
		{
			_requestHandler = requestHandler;
		}

		public async Task<TResponse> HandleAsync(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await _requestHandler.HandleAsync((TRequest)request, cancellationToken);
		}
	}

}
