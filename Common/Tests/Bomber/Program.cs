using NBomber.Contracts;
using NBomber.CSharp;
using Flurl;
using Flurl.Http;
using System;

namespace Bomber
{
    class Program
    {
        private static string _nodeApiUrl = "http://localhost:5001/api";
        private static long _gatewayNameIndex = 0;

        static void Main(string[] args)
        {
            var addGateway = Step.Create("Add Gateway", async c =>
            {
                var res = await _nodeApiUrl
                    .AppendPathSegments("service", "gateways")
                    .PostJsonAsync(new
                    {
                        uri = $"http://host{_gatewayNameIndex}/",
                        alias = $"GW{_gatewayNameIndex}"
                    })
                    .ReceiveString();
                return Response.Ok();
            });

            var scenario = ScenarioBuilder
                .CreateScenario("Gateways load", addGateway)
                .WithInit(async c => 
                {
                    IFlurlResponse response;
                    bool succeeded = false;
                    do
                    {
                        try
                        {
                            response = await _nodeApiUrl.AppendPathSegments("service", "gateways").GetAsync(c.CancellationToken);
                            succeeded = response.ResponseMessage.IsSuccessStatusCode;
                        }
                        catch (Exception)
                        {
                            succeeded = false;
                        }
                    } while (!succeeded);
                })
                .WithLoadSimulations(
                    Simulation.InjectPerSec(10, TimeSpan.FromSeconds(10))
                );

            NBomberRunner
                .RegisterScenarios(scenario)
                
                .Run();
        }
    }
}
