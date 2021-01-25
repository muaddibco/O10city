using System;

namespace O10.Client.Mobile.Base.ViewModels
{

    public class MainMasterDetailPageMasterMenuItem
    {
        public MainMasterDetailPageMasterMenuItem()
        {
            TargetType = typeof(MainMasterDetailPageMasterMenuItem);
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string NavigationName { get; set; }

        public Type TargetType { get; set; }
    }
}