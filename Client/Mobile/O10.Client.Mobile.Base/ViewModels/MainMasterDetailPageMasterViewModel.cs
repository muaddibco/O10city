using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Client.Mobile.Base.Resx;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class MainMasterDetailPageMasterViewModel : ViewModelBase
    {
        private readonly IPageDialogService _pageDialogService;
#pragma warning disable CS0649 // Field 'MainMasterDetailPageMasterViewModel._selectedMenuItem' is never assigned to, and will always have its default value null
        private readonly MainMasterDetailPageMasterMenuItem _selectedMenuItem;
#pragma warning restore CS0649 // Field 'MainMasterDetailPageMasterViewModel._selectedMenuItem' is never assigned to, and will always have its default value null

        public MainMasterDetailPageMasterViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : base(navigationService)
        {
            MenuItems = new ObservableCollection<MainMasterDetailPageMasterMenuItem>(new[]
                {
                    new MainMasterDetailPageMasterMenuItem { Id = 0, Title = AppResources.PAGE_TITLE_SETTINS,  NavigationName = "Settings"},
                    new MainMasterDetailPageMasterMenuItem { Id = 0, Title = AppResources.PAGE_TITLE_DISCLOSE_SECRETS,  NavigationName = "DiscloseSecrets"}
                });
            _pageDialogService = pageDialogService;
        }

        public ObservableCollection<MainMasterDetailPageMasterMenuItem> MenuItems { get; }

        public DelegateCommand<string> SelectedMenuItemCommand => new DelegateCommand<string>(m => NavigationService.NavigateAsync(m));

        public MainMasterDetailPageMasterMenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                //INavigationResult navigationResult = NavigationService.NavigateAsync(_selectedMenuItem.NavigationName).Result;

                //if(!navigationResult.Success)
                //{
                //    _pageDialogService.DisplayAlertAsync(AppResources.CAP_NAV_MENU_ALERT_TITLE, navigationResult.Exception.Message, AppResources.BTN_OK);
                //}
                NavigationService.NavigateAsync(value.NavigationName);
            }
        }
    }
}
