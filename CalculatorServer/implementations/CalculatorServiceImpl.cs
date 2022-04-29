using Grpc.Core;
using GrpcCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrpcCalculator.CalculatorService;

namespace CalculatorServer.implementations
{
    public class CalculatorServiceImpl : CalculatorServiceBase
    {
        public override Task<SumResponse> SumTwoIntegers(SumRequest request, ServerCallContext context)
        {
            var result = request.SumInput.FirstInt + request.SumInput.SecondInt;
            var response = new SumResponse()
            {
                Result = result
            };

            return Task.FromResult(response);
        }

        public override async Task BreakIntoSections(ManyDividersRequest request, IServerStreamWriter<ManyDividersResponse> responseStream, ServerCallContext context)
        {
            var workingNumber = request.Request;

            var counter = 2;
            while (workingNumber > 1)
            {
                if (workingNumber%counter==0)
                {
                    workingNumber /= counter;
                    await responseStream.WriteAsync(new ManyDividersResponse() { Response = counter });
                    counter = 2;

                    await Task.Delay(300);
                    continue;
                }

                ++counter;
            }
            
        }

        public async override Task<LongAverageResponse> CalculateAverage(IAsyncStreamReader<LongAverageRequest> requestStream, ServerCallContext context)
        {
            int total = 0;
            double result = 0.0;
            
            while (await requestStream.MoveNext())
            {
                result += requestStream.Current.RequestNumber.Number;
                ++total;
            }

            return new LongAverageResponse() { ResponseNumber = result / total };
        }

        public async override Task GiveCurrentMaxNumber(IAsyncStreamReader<MaxNumberRequest> requestStream, IServerStreamWriter<MaxNumberResponse> responseStream, ServerCallContext context)
        {
            var currentMaxNumber = 0;
            while (await requestStream.MoveNext())
            {
                int requestNumber = requestStream.Current.RequestNumber;
                Console.WriteLine("Server received: " +requestNumber);

                if (requestNumber > currentMaxNumber)
                {
                    currentMaxNumber = requestNumber;
                    await responseStream.WriteAsync(new MaxNumberResponse() { ResponseNumber = currentMaxNumber });
                    Console.WriteLine("Server sends current max number: " +currentMaxNumber);
                }
            }
           
        }
    }
}
