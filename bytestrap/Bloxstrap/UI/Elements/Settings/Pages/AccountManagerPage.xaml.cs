using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class AccountManagerPage
    {
        public AccountManagerPage()
        {
            DataContext = new AccountManagerViewModel();
            InitializeComponent();
        }
    }
}
