using NServiceBus;
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;


namespace JasDev.Saga
{
    class Program
    {
        public static void Main()
        {

            Console.Title = "Saga Run";
            MainAsync().GetAwaiter().GetResult();

            Console.ReadKey();

            //  Console.Title = "Saga Run";
            //  var endpointConfiguration = new EndpointConfiguration("Samples.SimpleSaga");


            ////  endpointConfiguration.UseSerialization<JsonSerializer>();
            //  endpointConfiguration.EnableInstallers();
            //  endpointConfiguration.UsePersistence<LearningPersistence>();
            //  endpointConfiguration.UseTransport<LearningTransport>();
            //  endpointConfiguration.SendFailedMessagesTo("error");

            //  var endpointInstance = await Endpoint.Start(endpointConfiguration)
            //      .ConfigureAwait(false);


            //  Console.WriteLine("Press any key to exit");
            //  Console.ReadKey();
            //  await endpointInstance.Stop()
            //     .ConfigureAwait(false); 

        }

        public static async Task MainAsync()
        {
            Console.Title = "JasDev Sqs Saga";
            
            #region ConfigureEndpoint

            var endpointConfiguration = new EndpointConfiguration("JasdevSqSimple");
            var transport = endpointConfiguration.UseTransport<SqsTransport>();
            //transport.S3("bucketname", "my/key/prefix");

            var sqsConfig = new AmazonSQSConfig();
            sqsConfig.ServiceURL = "http://sqs.us-west-1.amazonaws.com/";

            sqsConfig.RegionEndpoint = RegionEndpoint.USWest1;
            var client = new AmazonSQSClient(sqsConfig);

           // transport.ClientFactory(() => client);

            #endregion

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);


            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
