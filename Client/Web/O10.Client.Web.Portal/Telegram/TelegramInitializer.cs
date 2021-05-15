﻿using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;

using O10.Client.Web.Portal.Telegram.Bots;

namespace O10.Client.Web.Portal.Telegram
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class TelegramInitializer : InitializerBase
    {
        private readonly IO10BotService _o10BotService;

        public TelegramInitializer(IO10BotService o10BotService)
        {
            _o10BotService = o10BotService;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            await _o10BotService.Initialize(cancellationToken).ConfigureAwait(false);
            _o10BotService.Start();
        }
    }
}
