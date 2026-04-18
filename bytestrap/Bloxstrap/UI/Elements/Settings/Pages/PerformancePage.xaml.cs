using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class PerformancePage
    {
        private PerformanceViewModel _viewModel = null!;

        public PerformancePage()
        {
            SetupViewModel();
            InitializeComponent();
        }

        private void SetupViewModel()
        {
            _viewModel = new PerformanceViewModel();
            _viewModel.RequestPageReloadEvent += (_, _) => SetupViewModel();
            DataContext = _viewModel;
        }
    }
}
