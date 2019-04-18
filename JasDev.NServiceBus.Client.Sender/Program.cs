using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amazon;
using Amazon.Internal;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using JasDevNServiceBus.Sqs.Message;
using NServiceBus;
using NServiceBus.Logging;

namespace JasDevNserviceBus.Sqs.Sender
{
    class Program
    {
        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();

            Console.ReadKey();
        }

        static ILog log = LogManager.GetLogger<Program>();
        public static async Task MainAsync()
        {
            Console.Title = "JasDev Sqs Sender";


            #region ConfigureEndpoint

            var endpointConfiguration = new EndpointConfiguration("JasdevSqSimple");
            var transport = endpointConfiguration.UseTransport<SqsTransport>();
           //transport.S3("bucketname", "my/key/prefix");
           
            var sqsConfig = new AmazonSQSConfig();
            sqsConfig.ServiceURL = "http://sqs.us-west-1.amazonaws.com/";

            sqsConfig.RegionEndpoint = RegionEndpoint.USWest1;
            var client = new AmazonSQSClient(sqsConfig);

            transport.ClientFactory(() => client);

            var routing = transport.Routing();
            routing.RouteToEndpoint(
                messageType: typeof(StartOrder),
                destination: "JasdevSqSimple");

            #endregion
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            #region sendonly
            //TODO: uncomment to view a message in transit
            endpointConfiguration.SendOnly();
            #endregion

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            while (true)
            {
                log.Info("Press 'P' to place an order, or 'Q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.P:
                        // Instantiate the command
                        var command = new StartOrder
                        {
                            OrderId = Guid.NewGuid().ToString()
                        };

                        // Send the command to the local endpoint
                        log.Info($"Sending Start Oder command, OrderId = {command.OrderId}");
                        await endpointInstance.Send(command)
                            .ConfigureAwait(false);

                        break;

                    case ConsoleKey.Q:
                        await endpointInstance.Stop()
                            .ConfigureAwait(false);
                        return;

                    default:
                        log.Info("Unknown input. Please try again.");
                        break;
                }
            }

            


        }
    }
}
