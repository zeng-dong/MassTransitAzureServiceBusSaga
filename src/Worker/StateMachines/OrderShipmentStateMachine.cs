using Automatonymous;
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

       

    }

    public Event<OrderSubmitted> OrderSubmitted { get; }
    public Event<OrderShipped> OrderShipped { get; }

    public Schedule<OrderShipmentState, MonitorOrderShipmentTimeout> MonitorTimeout { get; }

    public State WaitingForShipment { get; }
    public State ShipmentOverdue { get; }
    public State ShipmentComplete { get; }
}

