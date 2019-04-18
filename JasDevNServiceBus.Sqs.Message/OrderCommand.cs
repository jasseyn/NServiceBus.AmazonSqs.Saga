using System;
using System.Collections.Generic;
using System.Text;
using NServiceBus;

namespace JasDevNServiceBus.Sqs.Message
{
     public  class OrderCommand: ICommand
    {
        public string  OrderId { get; set; }
        
    }
}
