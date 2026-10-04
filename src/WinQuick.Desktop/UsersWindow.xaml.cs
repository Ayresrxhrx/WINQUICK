using WinQuick.Application.Security;
using WinQuick.Core.Security;

namespace WinQuick.Desktop;

public partial class UsersWindow : System.Windows.Window
{
    private readonly UserManagementService _service;
    private readonly Guid _companyId;

    public UsersWindow(UserManagementService service, Guid companyId)
    {
        InitializeComponent();
        _service = service;
        _companyId = companyId;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        UsersGrid.ItemsSource = await _service.ListAsync(_companyId);
        RoleBox.ItemsSource = await _service.ListRolesAsync(_companyId);
    }

    private async void Create_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ErrorText.Visibility = System.Windows.Visibility.Collapsed;
        if (RoleBox.SelectedItem is not Role role)
        {
            ErrorText.Text = "Seleccione um perfil.";
            ErrorText.Visibility = System.Windows.Visibility.Visible;
            return;
        }
        try
        {
            await _service.CreateAsync(new CreateUserCommand(_companyId, UsernameBox.Text, DisplayNameBox.Text, PasswordBox.Password, role.Id));
            UsernameBox.Clear();
            DisplayNameBox.Clear();
            PasswordBox.Clear();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
            ErrorText.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
