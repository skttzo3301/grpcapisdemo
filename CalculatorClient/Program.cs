using Grpc.Core;
using GrpcCalculator;

const string target = "127.0.0.1:50051";

Channel channel = new Channel(target, ChannelCredentials.Insecure);

await channel.ConnectAsync().ContinueWith(
    task =>
    {
        if (task.Status == TaskStatus.RanToCompletion)
            Console.WriteLine("The client connected succesfully to " + target);
    });

var client = new CalculatorService.CalculatorServiceClient(channel);


//await Unary(client);
//await ServerStream(client);
//await ClientStream(client);
await Bidi(client);


Console.ReadKey();

static async Task ServerStream(CalculatorService.CalculatorServiceClient client)
{
    //120=2*2*2*3*5
    var input = new ManyDividersRequest()
    {
        Request = 120
    };

    var response = client.BreakIntoSections(input);

    while (await response.ResponseStream.MoveNext())
    {
        Console.WriteLine(response.ResponseStream.Current.Response);
    }
}

static async Task Unary(CalculatorService.CalculatorServiceClient client)
{
    var input = new SumInput()
    {
        FirstInt = 10,
        SecondInt = 3
    };

    var sumRequest = new SumRequest()
    {
        SumInput = input
    };

    var response = await client.SumTwoIntegersAsync(sumRequest);

    Console.WriteLine($"Requested {input.FirstInt} and {input.SecondInt}. Responded {response.Result}");
}

static async Task ClientStream(CalculatorService.CalculatorServiceClient client)
{
    var stream = client.CalculateAverage();

    foreach (int i in Enumerable.Range(1, 4))
    {
        var request = new LongAverageRequest() { RequestNumber = new RequestNumber() { Number = i } };
        await stream.RequestStream.WriteAsync(request);
        await Task.Delay(500);
    }

    await stream.RequestStream.CompleteAsync();

    var response = await stream.ResponseAsync;

    Console.WriteLine("Averaged:");
    Console.WriteLine(response.ResponseNumber);
}

static async Task Bidi(CalculatorService.CalculatorServiceClient client)
{
    var stream = client.GiveCurrentMaxNumber();

    int[] numbers = { 1, 5, 3, 6, 2, 20 };

    foreach (var number in numbers)
    {
        await Task.Delay(500);
        var request = new MaxNumberRequest() { RequestNumber = number };
        await stream.RequestStream.WriteAsync(request);

        Console.WriteLine("Client sent: " + number);
    }
    await stream.RequestStream.CompleteAsync();


    while (await stream.ResponseStream.MoveNext())
    {
        Console.WriteLine($"Received: " + stream.ResponseStream.Current.ResponseNumber);
    }
}