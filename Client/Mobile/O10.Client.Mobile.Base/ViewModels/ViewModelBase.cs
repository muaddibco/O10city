using Prism.Mvvm;
using Prism.Navigation;

namespace O10.Client.Mobile.Base.ViewModels
{
    public abstract class ViewModelBase : BindableBase, INavigationAware, IDestructible
    {
        public INavigationService NavigationService { get; set; }

        private string _title;
        private bool _isError;
        private string _errorMessage;
        private bool _isLoading;
        private string _actionDescription;

        protected ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string ActionDescription
        {
            get => _actionDescription;
            set
            {
                SetProperty(ref _actionDescription, value);
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
            }
        }

        public bool IsError
        {
            get => _isError;
            set
            {
                SetProperty(ref _isError, value);
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
            }
        }

        public bool Navigated { get; private set; }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
            Navigated = true;
        }

        public virtual void OnNavigatingTo(INavigationParameters parameters)
        {

        }

        public virtual void Destroy()
        {

        }
    }
}
