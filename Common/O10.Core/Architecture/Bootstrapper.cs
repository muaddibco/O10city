using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using O10.Core.Architecture.Registration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Core.Architecture
{
	public class Bootstrapper
    {
        private IRegistrationManager _registrationManager;

        public Bootstrapper()
        {
        }

        public IServiceCollection Container { get; protected set; }

        public virtual void Run(IServiceCollection container, ILogger log, RunMode runMode)
        {
            log?.Info("Starting Bootstrap Run");
            try
            {
                ConfigureContainer(container, log, runMode);
            }
            finally
            {
                log?.Info("Bootstrap Run completed");
            }
        }

		public virtual async Task RunInitializers(IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger log)
        {
            log?.Info("Running initializers started");

			using var scope = serviceProvider.CreateScope();

			try
			{
				IEnumerable<IInitializer> initializers = scope.ServiceProvider.GetServices<IInitializer>();
				IOrderedEnumerable<IInitializer> initializersOrdered = initializers.OrderBy(i => (int)i.Priority);
				foreach (IInitializer item in initializersOrdered)
				{
					log?.Info($"Running initializer {item.GetType().FullName}");
					try
					{
						await item.Initialize(cancellationToken);
					}
					catch (Exception ex)
					{
						log?.Error($"Failed to initialize {item.GetType().FullName}", ex);
					}
				}
			}
			finally
			{
				log?.Info("Run initializers completed");
			}
		}

        protected virtual IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return 
                new string[] { "O10.Core.dll" }
                .Concat(Directory.EnumerateFiles(rootFolder, "O10.Core.*.dll").Select(f => new FileInfo(f).Name))
                .Concat(Directory.EnumerateFiles(rootFolder, "O10.Tracking.*.dll").Select(f => new FileInfo(f).Name));
        }

        #region Private Functions

        private AggregateCatalog GetRegistrationSettings(ILogger log)
        {
            log?.Info("Obtaining Registration Settings started");
            try
            {
                AggregateCatalog coreCatalog = new AggregateCatalog();
                string path = Assembly.GetCallingAssembly()?.Location;
                if(string.IsNullOrWhiteSpace(path))
                {
                    path = Assembly.GetExecutingAssembly()?.Location;
                }
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = this.GetType().Assembly.Location;
                }
                log?.Info($"path = {path}");
                string exeFolder = Path.GetDirectoryName(path);
                log?.Info($"exeFolder = {exeFolder}");
                string exeName = Path.GetFileName(path);
                log?.Info($"exeName = {exeName}");

                if (exeFolder != null)
                {
                    log?.Info("About to add DirectoryCatalog");
                    if (exeName != null)
                    {
                        coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, exeName));
                    }

                    log?.Info("DirectoryCatalog added");

                    log?.Info("About to enumerate Catalog Items");

					string[] enumeratedCatalogItems = EnumerateCatalogItems(exeFolder).ToArray();
                    
                    log?.Info($"catalog item for loading: {string.Join(",", enumeratedCatalogItems)}");

                    foreach (string catalogItemName in enumeratedCatalogItems)
                    {
                        log?.Info($"catalogItemName = {catalogItemName} adding");
                        coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, catalogItemName));
                    }
                }

                log?.Info("GetRegistrationSettings completed");
				
				return coreCatalog;
            }
            catch (Exception ex)
            {
                log?.Error("Obtaining Registration Settings failed", ex);
				return null;
            }
            finally
            {
                log?.Info("Obtaining Registration Settings completed");
            }
        }

        public virtual void ConfigureContainer(IServiceCollection container, ILogger log, RunMode runMode)
        {
            log?.Info("Container Configuration started");
            try
            {
                AggregateCatalog aggregateCatalog = GetRegistrationSettings(log);

				_registrationManager = new RegistrationManager(runMode, container);

                if (aggregateCatalog != null)
                {
                    _registrationManager.AutoRegisterUsingMefCatalog(aggregateCatalog);
                }
                else
                {
                    _registrationManager.AutoRegisterUsingReflection();
                }

                _registrationManager.CommitRegistrationsToContainer();
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.LoaderExceptions != null)
                {
                    foreach (Exception loaderException in ex.LoaderExceptions)
                    {
                        log?.Error(loaderException.Message, loaderException);
                    }
                }

                log?.Error("Container Configuration failed", ex);
                throw;
            }
            catch (Exception ex)
            {
                log?.Error("Container Configuration failed", ex);
                throw;
            }
            finally
            {
                log?.Info("Container Configuration completed");
            }
        }

        #endregion Private Functions
    }
}
