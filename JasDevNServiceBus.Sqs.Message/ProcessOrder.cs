using NServiceBus;


namespace JasDevNServiceBus.Sqs.Message
{
    public class ProcessOrder :
        IMessage
    {
        public string OrderId { get; set; }
    }
}