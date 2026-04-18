using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class AccountManagerViewModel : NotifyPropertyChangedViewModel
    {
        public ObservableCollection<SavedAccount> Accounts { get; set; }

        private SavedAccount? _selectedAccount;
        public SavedAccount? SelectedAccount
        {
            get => _selectedAccount;
            set { _selectedAccount = value; OnPropertyChanged(nameof(SelectedAccount)); OnPropertyChanged(nameof(HasSelection)); }
        }

        public bool HasSelection => SelectedAccount != null;

        public ICommand AddCurrentAccountCommand => new AsyncRelayCommand(AddCurrentAccount);
        public ICommand RemoveAccountCommand => new RelayCommand(RemoveAccount);
        public ICommand SwitchAccountCommand => new AsyncRelayCommand(SwitchAccount);
        public ICommand RefreshAccountCommand => new AsyncRelayCommand(RefreshAccount);
        public ICommand EditNicknameCommand => new RelayCommand(EditNickname);
        public ICommand ClearAllAccountsCommand => new RelayCommand(ClearAllAccounts);

        public int AccountCount => Accounts.Count;

        public AccountManagerViewModel()
        {
            Accounts = new ObservableCollection<SavedAccount>(App.Settings.Prop.SavedAccounts);
        }

        private void Save()
        {
            App.Settings.Prop.SavedAccounts = new List<SavedAccount>(Accounts);
            OnPropertyChanged(nameof(AccountCount));
        }

        private async Task AddCurrentAccount()
        {
            const string LOG_IDENT = "AccountManagerViewModel::AddCurrentAccount";

            try
            {
                // Load the current Roblox cookie
                await App.Cookies.LoadCookies();

                if (!App.Cookies.Loaded)
                {
                    MessageBox.Show(
                        "Could not load Roblox cookies. Make sure Roblox is installed and you are logged in, and that cookie access is enabled in Behaviour settings.",
                        "Bytestrap",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Get authenticated user info
                var user = await App.Cookies.GetAuthenticated();
                if (user == null || user.Id == 0)
                {
                    MessageBox.Show(
                        "Cookie is invalid or expired. Please log into Roblox and try again.",
                        "Bytestrap",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Check if already saved
                if (Accounts.Any(a => a.UserId == user.Id))
                {
                    MessageBox.Show(
                        $"Account '{user.Username}' is already saved.",
                        "Bytestrap",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // Read and encrypt the cookie for storage
                string cookiePath = GetCookiesPath();
                if (!File.Exists(cookiePath))
                {
                    MessageBox.Show("Cookie file not found.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string rawContent = File.ReadAllText(cookiePath);
                byte[] rawBytes = Encoding.UTF8.GetBytes(rawContent);
                byte[] encrypted = ProtectedData.Protect(rawBytes, null, DataProtectionScope.CurrentUser);
                string encryptedB64 = Convert.ToBase64String(encrypted);

                var account = new SavedAccount
                {
                    UserId = user.Id,
                    Username = user.Username,
                    DisplayName = user.Displayname,
                    EncryptedCookie = encryptedB64,
                    LastUsed = DateTime.Now
                };

                Accounts.Add(account);
                Save();

                App.Logger.WriteLine(LOG_IDENT, $"Added account: {user.Username} ({user.Id})");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to add account");
                App.Logger.WriteException(LOG_IDENT, ex);
                MessageBox.Show($"Failed to add account: {ex.Message}", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveAccount()
        {
            if (SelectedAccount == null)
                return;

            var result = MessageBox.Show(
                $"Remove account '{SelectedAccount.Username}'?",
                "Bytestrap",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            Accounts.Remove(SelectedAccount);
            SelectedAccount = null;
            Save();
        }

        private async Task SwitchAccount()
        {
            const string LOG_IDENT = "AccountManagerViewModel::SwitchAccount";

            if (SelectedAccount == null)
                return;

            try
            {
                // Decrypt the stored cookie file content
                byte[] encrypted = Convert.FromBase64String(SelectedAccount.EncryptedCookie);
                byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                string cookieContent = Encoding.UTF8.GetString(decrypted);

                // Write it back to Roblox's cookie file
                string cookiePath = GetCookiesPath();
                string? dir = Path.GetDirectoryName(cookiePath);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(cookiePath, cookieContent);

                SelectedAccount.LastUsed = DateTime.Now;
                Save();

                // Validate
                await App.Cookies.LoadCookies();
                var user = await App.Cookies.GetAuthenticated();

                if (user != null && user.Id != 0)
                {
                    MessageBox.Show(
                        $"Switched to account: {user.Username} ({user.Displayname})",
                        "Bytestrap",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Account cookie may be expired. Please log in and re-add the account.",
                        "Bytestrap",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                App.Logger.WriteLine(LOG_IDENT, $"Switched to account: {SelectedAccount.Username}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to switch account");
                App.Logger.WriteException(LOG_IDENT, ex);
                MessageBox.Show($"Failed to switch account: {ex.Message}", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetCookiesPath()
        {
            return Path.Combine(Paths.Roblox, "LocalStorage",
                RobloxInterfaces.Deployment.IsDefaultRobloxDomain
                    ? "RobloxCookies.dat"
                    : $"{RobloxInterfaces.Deployment.RobloxDomain}_RobloxCookies.dat");
        }

        private async Task RefreshAccount()
        {
            const string LOG_IDENT = "AccountManagerViewModel::RefreshAccount";

            if (SelectedAccount == null)
                return;

            try
            {
                byte[] encrypted = Convert.FromBase64String(SelectedAccount.EncryptedCookie);
                byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                string cookieContent = Encoding.UTF8.GetString(decrypted);

                string cookiePath = GetCookiesPath();
                File.WriteAllText(cookiePath, cookieContent);

                await App.Cookies.LoadCookies();
                var user = await App.Cookies.GetAuthenticated();

                if (user != null && user.Id != 0)
                {
                    SelectedAccount.Username = user.Username;
                    SelectedAccount.DisplayName = user.Displayname;
                    Save();

                    // Force UI refresh
                    int idx = Accounts.IndexOf(SelectedAccount);
                    if (idx >= 0)
                    {
                        var temp = SelectedAccount;
                        Accounts.RemoveAt(idx);
                        Accounts.Insert(idx, temp);
                        SelectedAccount = temp;
                    }

                    MessageBox.Show($"Refreshed: {user.Username} ({user.Displayname})", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Cookie may be expired. Re-add the account.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                App.Logger.WriteLine(LOG_IDENT, $"Refreshed account: {SelectedAccount.Username}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to refresh account");
                App.Logger.WriteException(LOG_IDENT, ex);
                MessageBox.Show($"Refresh failed: {ex.Message}", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditNickname()
        {
            if (SelectedAccount == null)
                return;

            string currentNick = string.IsNullOrEmpty(SelectedAccount.Nickname) ? SelectedAccount.Username : SelectedAccount.Nickname;

            // Use Microsoft.VisualBasic InputBox for simple text input
            string? input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter a nickname for this account:\n(Leave empty to clear)",
                "Bytestrap - Edit Nickname",
                currentNick);

            if (input != null)
            {
                SelectedAccount.Nickname = input.Trim();
                Save();

                int idx = Accounts.IndexOf(SelectedAccount);
                if (idx >= 0)
                {
                    var temp = SelectedAccount;
                    Accounts.RemoveAt(idx);
                    Accounts.Insert(idx, temp);
                    SelectedAccount = temp;
                }
            }
        }

        private void ClearAllAccounts()
        {
            if (Accounts.Count == 0)
                return;

            var result = MessageBox.Show(
                $"Remove ALL {Accounts.Count} saved accounts? This cannot be undone.",
                "Bytestrap",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            Accounts.Clear();
            SelectedAccount = null;
            Save();
            OnPropertyChanged(nameof(AccountCount));
        }
    }
}
