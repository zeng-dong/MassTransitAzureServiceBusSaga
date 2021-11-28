using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Definition;

namespace Worker.StateMachines;

public class OrderShipmentSagaDefinition : SagaDefinition<OrderShipmentState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderShipmentState> sagaConfigurator)
    {
        if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
        {
            sb.RequiresSession = true;
        }

        //base.ConfigureSaga(endpointConfigurator, sagaConfigurator);
    }
}

