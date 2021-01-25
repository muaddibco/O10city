using Microsoft.Extensions.DependencyInjection;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Logging;

namespace O10.Client.Mobile.Base.ExtensionMethods
{
    public static class NavigationServiceExtensions
    {
        public static void NavigateByAccountStatus(this INavigationService navigationService, IServiceProvider serviceProvider, ILogger logger = null)
        {
            IAccountsService accountsService = serviceProvider.GetService<IAccountsService>();
            List<AccountDescriptor> accounts = accountsService.GetAll();
            AccountDescriptor account = accounts.FirstOrDefault();

            if (account?.IsActive != true)
            {
                navigationService.NavigateAsync("/AccountCreation")
                    .ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully || !t.Result.Success)
                        {
                            logger?.Error("Failed to navigate to AccountCreation", t.Result.Exception);
                        }
                    }, TaskScheduler.Current);
            }
            else if (account.IsCompromised)
            {
                navigationService.NavigateAsync($"/AccountCompromised");
            }
            else
            {
                navigationService.NavigateToRoot(logger, $"accountId={accounts[0].AccountId}");
            }
        }

        public static void NavigateToRoot(this INavigationService navigationService, ILogger logger, string args = null)
        {
            if (string.IsNullOrEmpty(args))
            {
                navigationService.NavigateWithLogging("/Root/NavigationPage/MainPage", logger);
            }
            else
            {
                navigationService.NavigateWithLogging($"/Root/NavigationPage/MainPage?{args}", logger);
            }
        }

        public static void NavigateWithLogging(this INavigationService navigationService, string uri, ILogger logger)
        {
            logger.Info($"Navigating by URI {uri}");
            navigationService.NavigateAsync(uri)
                .ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        foreach (var ex in t.Exception.InnerExceptions)
                        {
                            logger?.Error($"Failed to navigate to RequiredAndroidPermissions", ex);
                        }
                    }
                });
        }
    }
}
