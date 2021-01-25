using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.ViewModels.Inherence;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class InherenceProtectionPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly IPageDialogService _pageDialogService;
        private readonly ILogger _logger;
        private List<InherenceVerifierViewModel> _inherenceVerifiers;
        private UserRootAttribute _rootAttribute;

        public InherenceProtectionPageViewModel(IExecutionContext executionContext,
                                          IDataAccessService dataAccessService,
                                          ILoggerService loggerService,
                                          IVerifierInteractionsManager verifierInteractionsManager,
                                          INavigationService navigationService,
                                          IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _verifierInteractionsManager = verifierInteractionsManager;
            _pageDialogService = pageDialogService;
            _logger = loggerService.GetLogger(nameof(InherenceProtectionPageViewModel));
        }

        public List<InherenceVerifierViewModel> InherenceVerifiers
        {
            get => _inherenceVerifiers;
            set
            {
                SetProperty(ref _inherenceVerifiers, value);
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            IsLoading = true;
            long rootAttributeId = parameters.GetValue<long>("rootAttributeId");
            _rootAttribute = _dataAccessService.GetUserRootAttribute(rootAttributeId);

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                var userRegistrations = _dataAccessService.GetUserRegistrations(_executionContext.AccountId);
                IEnumerable<InherenceServiceInfo> inherenceServiceInfos = _verifierInteractionsManager.GetInherenceServices();

                InherenceVerifiers = inherenceServiceInfos
                    .Where(i => _verifierInteractionsManager.GetInstance(i.Name) != null)
                    .Select(i =>
                    {
                        _executionContext.RelationsBindingService.GetBoundedCommitment(_rootAttribute.AssetId, i.Target.HexStringToByteArray(), out byte[] blindingFactor, out byte[] commitment);
                        return new InherenceVerifierViewModel(_verifierInteractionsManager, NavigationService)
                        {
                            Name = i.Name,
                            Alias = i.Alias,
                            Description = i.Description,
                            RootAttributeId = _rootAttribute.UserAttributeId,
                            IsRegistered = userRegistrations.Any(r => r.Commitment == commitment.ToHexString())
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize {nameof(InherenceProtectionPageViewModel)}", ex);
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_INHERENCE_PROTECTION_ALERT_TITLE, AppResources.CAP_INHERENCE_PROTECTION_ALERT_MSG, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
