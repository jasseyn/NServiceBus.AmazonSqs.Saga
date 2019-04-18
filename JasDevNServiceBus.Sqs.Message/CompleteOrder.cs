using NServiceBus;


namespace JasDevNServiceBus.Sqs.Message
{
    public class CompleteOrder :
        IMessage
    {
        public string OrderId { get; set; }
    }
}