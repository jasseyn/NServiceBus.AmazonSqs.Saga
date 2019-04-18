using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JasDevNServiceBus.Sqs.Message;
using NServiceBus;
using NServiceBus.Logging;

#region thesaga
public class OrderSaga :
    Saga<OrderSagaData>,
    IAmStartedByMessages<StartOrder>,
    IHandleMessages<ProcessOrder>,
    IHandleMessages<CompleteOrder>,
    IHandleTimeouts<CancelOrder>
{
    static ILog log = LogManager.GetLogger<OrderSaga>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderSagaData> mapper)
    {
        mapper.ConfigureMapping<StartOrder>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
        mapper.ConfigureMapping<ProcessOrder>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
        mapper.ConfigureMapping<CompleteOrder>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
    }

    public async Task Handle(StartOrder message, IMessageHandlerContext context)
    {
        // Correlation property Data.OrderId is automatically assigned with the value from message.OrderId;
        log.Info($"StartOrder received with OrderId {message.OrderId}");

      //  log.Info($@"Sending a CompleteOrder that will be delayed by 10 seconds
       //         Stop the endpoint now to see the saga data in:
     //               {LearningLocationHelper.GetSagaLocation<OrderSaga>(Data.Id)}");
        var proceeOrder = new ProcessOrder
        {
            OrderId = Data.OrderId
        };
        var sendOptions = new SendOptions();
        sendOptions.DelayDeliveryWith(TimeSpan.FromSeconds(5));
        sendOptions.RouteToThisEndpoint();
     
        await context.Send(proceeOrder, sendOptions)
            .ConfigureAwait(false);

        //var timeout = DateTime.UtcNow.AddSeconds(20);
        //log.Info($@"Requesting a CancelOrder that will be executed in 30 seconds.
        //            Stop the endpoint now to see the timeout data in the delayed directory
        //                    {LearningLocationHelper.TransportDelayedDirectory(timeout)}");

        //await RequestTimeout<CancelOrder>(context, timeout)
        //    .ConfigureAwait(false);
    }

    public async Task Handle(CompleteOrder message, IMessageHandlerContext context)
    {
   
        //Order completed

        if (this.Data.Retries < 3)
        {
            //external service
          //ExternalService.IsCompleted(this.Data.OrderId);
           //radom for testing
            bool isCompleted = new Random().Next(100) % 2 == 0;
            if (isCompleted)
            {
                log.Info($"Last step CompleteOrder received with OrderId {message.OrderId}");
                base.MarkAsComplete();
            }
            else
            {
                Thread.Sleep(2000);
                this.Data.Retries++;
                var completOrder = new CompleteOrder
                {
                    OrderId = Data.OrderId
                };
                var sendOptions = new SendOptions();
                sendOptions.DelayDeliveryWith(TimeSpan.FromSeconds(5));
                sendOptions.RouteToThisEndpoint();
               
                await context.Send(completOrder, sendOptions)
                  .ConfigureAwait(false);
            }
        }
        else
        {
            log.Info($"Tried 3 time fail to complete OrderId {message.OrderId}");
            // base.MarkAsComplete();
            //  return Task.CompletedTask;
            var timeout = DateTime.UtcNow.AddSeconds(1);
            await RequestTimeout<CancelOrder>(context, timeout)
            .ConfigureAwait(false);
        }

        //   MarkAsComplete();
        // return Task.CompletedTask;
    }

    public Task Timeout(CancelOrder state, IMessageHandlerContext context)
    {
        log.Info($"CompleteOrder not received soon enough OrderId {Data.OrderId}. Calling MarkAsComplete");
        MarkAsComplete();
        return Task.CompletedTask;
    }

    public async Task Handle(ProcessOrder message, IMessageHandlerContext context)
    {
        log.Info($"Process with OrderId {message.OrderId}");
        //process order external service here...
        var completOrder = new CompleteOrder
        {
            OrderId = Data.OrderId
        };
        var sendOptions = new SendOptions();
        sendOptions.DelayDeliveryWith(TimeSpan.FromSeconds(5));
        sendOptions.RouteToThisEndpoint();
        await context.Send(completOrder, sendOptions)
            .ConfigureAwait(false);
    }
}



public static class LearningLocationHelper
{
    public static string TransportDirectory;
    public static string SagaDirectory;

    static LearningLocationHelper()
    {
        var location = Assembly.GetExecutingAssembly().Location;
        var runningDirectory = Directory.GetParent(location).FullName;
        SagaDirectory = Path.Combine(runningDirectory, ".sagas");
        TransportDirectory = Path.GetFullPath(Path.Combine(runningDirectory, @"..\..\..\"));
    }

    public static string TransportDelayedDirectory(DateTime dateTime)
    {
        return Path.Combine(TransportDirectory, ".delayed", dateTime.ToString("yyyyMMddHHmmss"));
    }

    public static string GetSagaLocation<T>(Guid sagaId)
        where T : Saga
    {
        return Path.Combine(SagaDirectory, typeof(T).Name, $"{sagaId}.json");
    }
}
#endregion