using NServiceBus;

public class ProcessOrder :
    IMessage
{
    public string OrderId { get; set; }
}