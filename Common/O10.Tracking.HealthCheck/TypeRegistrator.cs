using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using O10.Core.Architecture;
using O10.Core.Architecture.Registration;
using O10.Core.HealthChecks;
using System;

namespace O10.Core
{
    public class TypeRegistrator : TypeRegistratorBase
    {
        public override void PostRegister(IServiceCollection services, IRegistrationManager registrationManager)
        {
            base.PostRegister(services, registrationManager);

            var builder = services.AddHealthChecks();
            foreach (var t in registrationManager.GetImplementingTypes<IHealthCheckService>())
            {
                builder.Add(new HealthCheckRegistration(t.Name, s => s.GetService<IHealthChecksProvider>().GetInstance(t), HealthStatus.Unhealthy, Array.Empty<string>()));
            }
        }
    }
}
