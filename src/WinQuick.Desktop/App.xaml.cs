using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Products;
using WinQuick.Application.Sales;
using WinQuick.Application.Security;
using WinQuick.Infrastructure;
using WinQuick.Infrastructure.Persistence;

namespace WinQuick.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;
    private IServiceScope? _mainScope;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        // The login window is temporary. Keep the application alive while it is open
        // so closing the login dialog cannot terminate the process before MainWindow is created.
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

        base.OnStartup(e);

        try
        {
            var services = new ServiceCollection();
            var databasePath = System.IO.Path.Combine(AppContext.BaseDirectory, "WinQuick.db");

            services.AddDbContext<WinQuickDbContext>(options =>
                options.UseSqlite($"Data Source={databasePath}"));

            services.AddWinQuickInfrastructure();
            _services = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

            using (var scope = _services.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<DatabaseInitializer>()
                    .InitializeAsync();
            }

            // Do NOT assign the login dialog to Application.MainWindow.
            // Doing so with the default WPF shutdown mode can make the application
            // exit immediately when the dialog closes successfully.
            using (var loginScope = _services.CreateScope())
            {
                var login = new LoginWindow(
                    loginScope.ServiceProvider.GetRequiredService<AuthenticationService>());

                var loginResult = login.ShowDialog();

                if (loginResult != true || login.AuthenticatedUser is null)
                {
                    Shutdown(0);
                    return;
                }

                var authenticatedUser = login.AuthenticatedUser;

                _mainScope = _services.CreateScope();
                var mainServices = _mainScope.ServiceProvider;
                var db = mainServices.GetRequiredService<WinQuickDbContext>();

                var window = new MainWindow(
                    mainServices.GetRequiredService<UserManagementService>(),
                    db,
                    mainServices.GetRequiredService<ISaleService>(),
                    mainServices.GetRequiredService<IProductService>(),
                    authenticatedUser.CompanyId,
                    authenticatedUser.UserId);

                MainWindow = window;
                ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                window.Show();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Não foi possível iniciar o WINQUICK.\n\n{ex}",
                "WINQUICK",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _mainScope?.Dispose();
        _services?.Dispose();
        base.OnExit(e);
    }
}
