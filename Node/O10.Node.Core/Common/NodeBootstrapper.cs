using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Node.Core.Exceptions;
using System.Threading.Tasks;

namespace O10.Node.Core.Common
{
	public class NodeBootstrapper : Bootstrapper
    {
        private readonly string[] _catalogItems = new string[] { "O10.Crypto.dll", "Chaos.NaCl.dll", "O10.Network.dll", "O10.Node.Core.dll", "O10.Transactions.Core.dll", "O10.Node.DataLayer.dll" };

        public NodeBootstrapper()
        {
        }

		public override async Task RunInitializers(IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger log)
		{
			log.Info("Starting NodeBootstrap Run");

			try
			{
				await base.RunInitializers(serviceProvider, cancellationToken, log).ConfigureAwait(false);

				StartNode(serviceProvider, cancellationToken, log);
			}
			finally
			{
				log.Info("NodeBootstrap Run completed");
			}
		}

        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
			return base.EnumerateCatalogItems(rootFolder)
				.Concat(_catalogItems)
				.Concat(Directory.EnumerateFiles(rootFolder, "O10.Node.DataLayer.*.dll").Select(f => new FileInfo(f).Name));
        }

        #region Private Functions

        protected virtual void StartNode(IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger log)
        {
            log.Info("Starting Node");
            try
            {
                NodeMain nodeMain = ActivatorUtilities.CreateInstance<NodeMain>(serviceProvider);

                nodeMain.Initialize(cancellationToken);

                nodeMain.Start();
            }
			catch(Exception ex)
			{
				log.Error("Failure during Node Start", ex);
				throw;
			}
            finally
            {
                log.Info("Starting Node completed");
            }
        }

        private static byte[] GetSecretKey(string secretKeyExpression)
        {
            byte[] sk = new byte[32];

            bool isValid = true;

            if (secretKeyExpression.Length != 64)
            {
                isValid = false;
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    string byteValueExpression = $"{secretKeyExpression[i * 2]}{secretKeyExpression[i * 2 + 1]}";

                    if (byte.TryParse(byteValueExpression, NumberStyles.HexNumber, null, out byte byteValue))
                    {
                        sk[i] = byteValue;
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (!isValid)
            {
                throw new SecretKeyInvalidException();
            }

            return sk;
        }

        #endregion Private Functions
    }
}
