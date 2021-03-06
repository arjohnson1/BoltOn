﻿using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public abstract class BaseCqrsEntity : BaseEntity<Guid>
	{
		public HashSet<CqrsEvent> EventsToBeProcessed { get; set; } = new HashSet<CqrsEvent>();

		public HashSet<CqrsEvent> ProcessedEvents { get; set; } = new HashSet<CqrsEvent>();

		protected bool RaiseEvent<TEvent>(TEvent @event)
			where TEvent : CqrsEvent
		{
			if (EventsToBeProcessed.Any(c => c.Id == @event.Id))
				return false;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.SourceId = Id;
			@event.SourceTypeName = GetType().AssemblyQualifiedName;
			// events with CreatedDate == null are filtered in the repository. this is used 
			// to differentiate events that were added in the current request and the existing events
			@event.CreatedDate = null;
			EventsToBeProcessed.Add(@event);
			return true;
		}

		protected bool ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)
			where TEvent : CqrsEvent
		{
			if (ProcessedEvents.Any(c => c.Id == @event.Id))
				return false;

			action(@event);
			@event.DestinationTypeName = GetType().AssemblyQualifiedName;
			// events with ProcessedDate == null are filtered in the repository. this is used 
			// to differentiate events that were added in the current request and the existing events
			@event.ProcessedDate = null;
			ProcessedEvents.Add(@event);
			return true;
		}
	}
}
