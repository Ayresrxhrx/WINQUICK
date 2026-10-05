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
    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e); var services=new ServiceCollection(); var databasePath=System.IO.Path.Combine(AppContext.BaseDirectory,"WinQuick.db"); services.AddDbContext<WinQuickDbContext>(o=>o.UseSqlite($"Data Source={databasePath}")); services.AddWinQuickInfrastructure(); _services=services.BuildServiceProvider();
        try
        {
            using(var scope=_services.CreateScope()) await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            using var loginScope=_services.CreateScope(); var login=new LoginWindow(loginScope.ServiceProvider.GetRequiredService<AuthenticationService>()); MainWindow=login;
            if(login.ShowDialog()!=true||login.AuthenticatedUser is null){Shutdown();return;}
            var mainScope=_services.CreateScope(); var db=mainScope.ServiceProvider.GetRequiredService<WinQuickDbContext>();
            var window=new MainWindow(mainScope.ServiceProvider.GetRequiredService<UserManagementService>(),db,mainScope.ServiceProvider.GetRequiredService<ISaleService>(),mainScope.ServiceProvider.GetRequiredService<IProductService>(),login.AuthenticatedUser.CompanyId,login.AuthenticatedUser.UserId); MainWindow=window; window.Show();
        }
        catch(Exception ex){System.Windows.MessageBox.Show($"Não foi possível iniciar o WINQUICK.\n\n{ex.Message}","WINQUICK",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);Shutdown(1);}
    }
    protected override void OnExit(System.Windows.ExitEventArgs e){_services?.Dispose();base.OnExit(e);}
}
