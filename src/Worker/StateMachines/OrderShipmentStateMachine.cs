﻿using Automatonymous;
using Contracts;
using Microsoft.Extensions.Logging;

namespace Worker.StateMachines;

public class OrderShipmentStateMachine : MassTransitStateMachine<OrderShipmentState>
{
    public OrderShipmentStateMachine(ILogger<OrderShipmentStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);
                
        Event(() => OrderSubmitted, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderShipped, x => x.CorrelateById(context => context.Message.OrderId));

        Schedule(() => MonitorTimeout, x => x.MonitorTimeoutTokenId, x =>
        {
            x.Delay = TimeSpan.FromSeconds(20);
            x.Received = config =>
            {
                config.ConfigureConsumeTopology = false;
                config.CorrelateById(context => context.Message.OrderId);
            };
        });

        Initially(
            When(OrderSubmitted)
                .Then(context => logger.LogInformation("Monitoring Order Shipment: {OrderId}", context.Instance.CorrelationId))
                .Schedule(MonitorTimeout, context => new MonitorOrderShipmentTimeout { OrderId = context.Instance.CorrelationId })
                .TransitionTo(WaitingForShipment)
            );

        During(Initial, WaitingForShipment,
            When(MonitorTimeout.Received)
                .Then(context => logger.LogInformation("Shipment Overdue: {OrderId}", context.Instance.CorrelationId))
                .TransitionTo(ShipmentOverdue),
            When(OrderShipped)
                .Then(context => logger.LogInformation("Shipment Completed: {OrderId}", context.Instance.CorrelationId))
                .Unschedule(MonitorTimeout)
                .TransitionTo(ShipmentComplete)
            );

        During(ShipmentOverdue,
                Ignore(MonitorTimeout.Received),
                When(OrderShipped)
                    .Then(context => logger.LogInformation("Shipment Completed (overdue): {OrderId}", context.Instance.CorrelationId))
                    .TransitionTo(ShipmentComplete)
            );

        During(ShipmentComplete,
                Ignore(MonitorTimeout.Received),
                When(OrderSubmitted)
                    .Then(context => logger.LogInformation("Order Shipment (already shipped): {OrderId}", context.Instance.CorrelationId))
            );
    }

    public Event<OrderSubmitted> OrderSubmitted { get; }
    public Event<OrderShipped> OrderShipped { get; }

    public Schedule<OrderShipmentState, MonitorOrderShipmentTimeout> MonitorTimeout { get; }

    public State WaitingForShipment { get; }
    public State ShipmentOverdue { get; }
    public State ShipmentComplete { get; }
}

