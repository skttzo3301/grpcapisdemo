using CalculatorServer.implementations;
using Grpc.Core;
using GrpcCalculator;

var port = 50051;
Server server = null;

try
{
    server = new Server()
    {
        Services = {
            CalculatorService.BindService(new CalculatorServiceImpl())
        },
        Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
    };

    server.Start();
    Console.WriteLine("Server started on port " + port);
    Console.WriteLine("Press any key to quit");
    Console.ReadKey();
}
catch (IOException e)
{
    Console.WriteLine("The server failed to start " + e.Message);
    throw;
}
finally
{
    if(server != null)
        server.ShutdownAsync().Wait(); 
}
