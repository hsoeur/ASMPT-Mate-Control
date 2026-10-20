using Asm.As.Oib.SiplaceSetupCenter.Contracts.Data;
using LO_ASM_MATE_CONTROL.Core;
using LO_ASM_MATE_CONTROL.Data;
using LO_ASM_MATE_CONTROL.MVVM.ViewModels;
using LO_ASM_MATE_CONTROL.MVVM.Views;
using LO_ASM_MATE_CONTROL.Services.ApiData;
using LO_ASM_MATE_CONTROL.Services.AsmXmlFiles;
using LO_ASM_MATE_CONTROL.Services.Commanders;
using LO_ASM_MATE_CONTROL.Services.Navigation;
using LO_ASM_MATE_CONTROL.Services.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace LO_ASM_MATE_CONTROL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File("LOASMMATECONTROL_FUNCTIONNAL_LOGS.txt")
            .CreateLogger();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Fatal((Exception)args.ExceptionObject, "Unhandled exception");
            };


            DispatcherUnhandledException += (sender, args) =>
            {
                Log.Fatal(args.Exception, "DispatcherUnhandledException");
                args.Handled = true;
            };


            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Fatal(args.Exception, "UnobservedTaskException");
            };

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<HttpClient>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<PackagingUnitManagementViewModel>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<AsmVisualizationDatabaseViewModel>();
            services.AddSingleton<ExitSubContractingViewModel>();
            services.AddSingleton<IPackagingUnitProvider, PackagingUnitProvider>();
            services.AddSingleton<AsmDatabaseContext>();
            services.AddSingleton<IProgramExecutioner, ProgramExecutioner>();
            services.AddSingleton<IAsmFileGen, AsmFileGen>();
            services.AddSingleton<IApiService, ApiService>();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>()
            });

            services.AddSingleton<Func<Type, ViewModel>>(ServiceProvider => viewModelType => (ViewModel)ServiceProvider.GetRequiredService(viewModelType));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Getting MainWindow instance from the service provider

            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            navigationService.NavigateTo<HomeViewModel>();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // Showing the MainWindow
            mainWindow.Show();
            base.OnStartup(e);
        }
    }


}
