﻿using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentUpdatedEvent : CqrsEvent
	{
		public Guid StudentId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string StudentType { get; set; }
	}

	public class StudentUpdatedEventHandler : IRequestAsyncHandler<StudentUpdatedEvent>
    {
        private readonly IRepository<StudentFlattened> _repository;

        public StudentUpdatedEventHandler(IRepository<StudentFlattened> repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(StudentUpdatedEvent request, CancellationToken cancellationToken)
        {
            var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);
			student.Update(request);
            await _repository.UpdateAsync(student, cancellationToken);
        }
    }
}
