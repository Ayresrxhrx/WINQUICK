using WinQuick.Application.Security;

namespace WinQuick.Desktop;

public partial class LoginWindow : System.Windows.Window
{
    private readonly AuthenticationService _authentication;

    public AuthenticatedUser? AuthenticatedUser { get; private set; }

    public LoginWindow(AuthenticationService authentication)
    {
        InitializeComponent();
        _authentication = authentication;
        Loaded += (_, _) => PasswordBox.Focus();
    }

    private async void Login_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ErrorText.Visibility = System.Windows.Visibility.Collapsed;
        var result = await _authentication.LoginAsync(UsernameBox.Text, PasswordBox.Password);
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
}
