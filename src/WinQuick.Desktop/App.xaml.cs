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
        // The login window is temporary. Keep the application alive explicitly
        // until the authenticated MainWindow has been created and shown.
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

            using (var initializationScope = _services.CreateScope())
            {
                await initializationScope.ServiceProvider
                    .GetRequiredService<DatabaseInitializer>()
                    .InitializeAsync();
            }

            AuthenticatedUser? authenticatedUser;

            // Keep the login scope isolated from the main application scope.
            // The login dialog is modal and is allowed to close only after a successful login.
            using (var loginScope = _services.CreateScope())
            {
                var login = new LoginWindow(
                    loginScope.ServiceProvider.GetRequiredService<AuthenticationService>());

                var loginResult = login.ShowDialog();
                authenticatedUser = loginResult == true ? login.AuthenticatedUser : null;
            }

            if (authenticatedUser is null)
            {
                Shutdown(0);
                return;
            }

            // Create the main application scope only after login has completely finished.
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

            // Assign MainWindow before changing ShutdownMode or showing it.
            MainWindow = window;
            ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            window.Show();
            window.Activate();
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
