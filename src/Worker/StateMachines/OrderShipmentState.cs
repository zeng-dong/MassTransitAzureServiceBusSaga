using Automatonymous;

namespace Worker.StateMachines;

public class OrderShipmentState :
        SagaStateMachineInstance
{
    public string CurrentState { get; set; }

    public Guid? MonitorTimeoutTokenId { get; set; }

    public Guid CorrelationId { get; set; }
}