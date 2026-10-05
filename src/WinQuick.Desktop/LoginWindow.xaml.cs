using WinQuick.Application.Security;

namespace WinQuick.Desktop;

public partial class LoginWindow : System.Windows.Window
{
    private readonly AuthenticationService _authentication;
    private bool _isLoggingIn;

    public AuthenticatedUser? AuthenticatedUser { get; private set; }

    public LoginWindow(AuthenticationService authentication)
    {
        InitializeComponent();
        _authentication = authentication;
        Loaded += (_, _) => PasswordBox.Focus();
    }

    private void LoginInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter && e.Key != System.Windows.Input.Key.Return)
            return;

        e.Handled = true;
        LoginButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
    }

    private async void Login_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoggingIn)
            return;

        _isLoggingIn = true;
        LoginButton.IsEnabled = false;
        ErrorText.Visibility = System.Windows.Visibility.Collapsed;

        try
        {
            var username = UsernameBox.Text?.Trim() ?? string.Empty;
            var password = PasswordBox.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Introduza o utilizador e a senha.";
                ErrorText.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            var result = await _authentication.LoginAsync(username, password);
            if (result is null)
            {
                ErrorText.Text = "Utilizador ou senha incorrectos.";
                ErrorText.Visibility = System.Windows.Visibility.Visible;
                PasswordBox.Clear();
                PasswordBox.Focus();
                return;
            }

            AuthenticatedUser = result;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Não foi possível iniciar sessão: {ex.Message}";
            ErrorText.Visibility = System.Windows.Visibility.Visible;
        }
        finally
        {
            _isLoggingIn = false;
            if (IsVisible)
                LoginButton.IsEnabled = true;
        }
    }
}
