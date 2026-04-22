using Bloxstrap.AppData;
using Bloxstrap.RobloxInterfaces;
using System;
using System.Threading.Channels;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Bloxstrap.UI.ViewModels.Bootstrapper
{
    public class FluentDialogViewModel : BootstrapperDialogViewModel
    {
        //TODO: FIX THE VERSION TEXT!
        public BackgroundType WindowBackdropType { get; set; } = BackgroundType.Mica;
        public SolidColorBrush BackgroundColourBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        public string VersionText { get; set; }
        public string ChannelText
        {
            get => _channelText;
            set
            {
                _channelText = value;
                OnPropertyChanged(nameof(ChannelText));
            }
        }
        private string _channelText = string.Empty;
        public FluentDialogViewModel(IBootstrapperDialog dialog, bool aero, string version) : base(dialog)
        {
            const int alpha = 128;

            WindowBackdropType = aero ? BackgroundType.Aero : BackgroundType.Mica;

            bool isLight = App.Settings.Prop.Theme.GetFinal() == Enums.Theme.Light;

            if (aero)
            {
                BackgroundColourBrush = isLight ?
                    new SolidColorBrush(Color.FromArgb(alpha, 225, 225, 225)) :
                    new SolidColorBrush(Color.FromArgb(alpha, 30, 30, 30));
            }
            else
            {
                // Fallback solid background so the dialog is never see-through
                // when Mica isn't applied (e.g. Windows 10 or unsupported setups).
                BackgroundColourBrush = isLight ?
                    new SolidColorBrush(Color.FromRgb(243, 243, 243)) :
                    new SolidColorBrush(Color.FromRgb(32, 32, 32));
            }

            VersionText = $"{Strings.Common_Version}: {version}";
            ChannelText = $"{Strings.Common_Channel}: {Deployment.Channel}";

            Deployment.ChannelChanged += (_, newChannel) =>
            {
                ChannelText = $"{Strings.Common_Channel}: {newChannel}";
            };
        }
    }
}
