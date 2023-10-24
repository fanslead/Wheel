﻿using MediatR;

namespace Wheel.EventBus.Local
{
    public interface ILocalEventHandler<in TEventData> : IEventHandler, INotificationHandler<TEventData> where TEventData : INotification
    {
        Task Handle(TEventData eventData, CancellationToken cancellationToken = default);
    }
}