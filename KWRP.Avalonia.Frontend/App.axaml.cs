using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using KWRP.Avalonia.Backend.Services;
using KWRP.Avalonia.Frontend.Models.Stores;
using KWRP.Avalonia.Frontend.Services;
using KWRP.Avalonia.Frontend.ViewModels;
using KWRP.Avalonia.Frontend.ViewModels.EditorPage;
using KWRP.Avalonia.Frontend.ViewModels.StartUpPage;
using KWRP.Avalonia.Frontend.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Serilog;
using System.Threading.Tasks;
using System;
using MsBox.Avalonia;
using KWRP.Avalonia.Frontend.Models.Settings;
using KWRP.Infra.CSV;
using KWRP.Avalonia.Frontend.Services.LaneArrangement;
using KWRP.Avalonia.Frontend.Services.LaneArrangement.Evaluate;
using KWRP.Avalonia.Backend.Model.Modlules.LaneIntegration;
using KWRP.Avalonia.Frontend.Services.Activity;
using MsBox.Avalonia.Enums;
using KWRP.Avalonia.Backend.Constants;
using KWRP.Avalonia.Frontend.Services.Dxf;
using Avalonia.Interactivity;
using System.Threading;
using KWRP.Backend.Services;
using KWRP.Frontend.Services;

namespace KWRP.Avalonia.Frontend
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            // 例外処理ハンドラの設定
            SetExceptionHandlers();

            // IoCコンテナの設定
            var collection = new ServiceCollection();
            collection.AddCommonService();
            collection.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
            });
            _serviceProvider = collection.BuildServiceProvider();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
#if !DEBUG
            if (!SingletonApplication.Start())
            {
                System.Environment.Exit(0);
            }
#endif

            // アプリケーションの起動
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                // 起動時のログ
                _serviceProvider.GetRequiredService<ILogService>().LogInfo($"#############__区割りシステムを起動します{(KWRPConfigs.IsDevMode ? "(開発者モード)" : "")}__#############");

                // アプリケーションウィンドウを初期化
                desktop.MainWindow = _serviceProvider.GetRequiredService<Window>();

                // 起動時、終了時の処理を登録
                desktop.MainWindow.Loaded += async (sender, e) => await OnWindowLoaded(desktop.MainWindow, e);
                desktop.MainWindow.Closing += async (sender, e) => await OnWindowClosing(desktop.MainWindow, e);

                // ウィンドウに写す初期表示画面の設定
                var startUp = _serviceProvider.GetRequiredService<NavigationStore>();
                startUp.SetViewModel(_serviceProvider.GetRequiredService<StartUpViewModel>());

                var startPage = _serviceProvider.GetRequiredService<NavigationPageStore>();
                startPage.SetViewModel(_serviceProvider.GetRequiredService<TopPageViewModel>());
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task OnWindowLoaded(Window mainWindow, RoutedEventArgs e)
        {
            if (mainWindow?.DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.LoadDxfHistoryAsync();
            }
        }

        private bool isClosingConfirmed = false;
        private async Task OnWindowClosing(Window mainWindow, WindowClosingEventArgs e)
        {
            if (isClosingConfirmed)
            {
                return;
            }

            if (mainWindow?.DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.SaveLaneArrangementConfigAsync();
                await viewModel.SaveDxfHistoryAsync();

                var machineChanged = _serviceProvider.GetRequiredService<MachineStore>().IsPropertyChanged;
                if (machineChanged)
                {
                    e.Cancel = true;
                    var messageBox = MessageBoxManager.GetMessageBoxStandard(
                        title: "設定の保存",
                        text: "重機設定値が変更されています。保存しますか?",
                        ButtonEnum.YesNoCancel,
                        Icon.Question);

                    var result = await messageBox.ShowWindowDialogAsync(mainWindow);

                    switch (result)
                    {
                        case ButtonResult.Yes:
                            await viewModel.SaveMachineConfigAsync();
                            isClosingConfirmed = true;
                            mainWindow.Close();
                            break;
                        case ButtonResult.No:
                            isClosingConfirmed = true;
                            mainWindow.Close();
                            break;
                        case ButtonResult.Cancel:
                            break;
                    }
                }
            }
        }



        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }

        #region 例外処理

        private void SetExceptionHandlers()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var message = $"予期せぬエラーが発生しました。続けて発生する場合は開発者に報告してください。";
            if (exception != null) message += $"\n({exception.Message} @ {exception.TargetSite?.Name})";

            Log.Fatal(exception, $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" +
                                 $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff")}|システムに予期せぬエラーが発生しました cu\n" +
                                 $"続けて発生する場合は開発者に報告してください。\n" +
                                 $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

            Environment.Exit(1);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception.InnerException as Exception;
            if (exception != null)
            {
                ConfirmUnhandledException(exception, "バックグラウンドタスク")
                    .ContinueWith(task =>
                    {
                        if (task.Result)
                        {
                            e.SetObserved();
                        }
                        else
                        {
                            Environment.Exit(1);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                Environment.Exit(1);
            }
        }

        async Task<bool> ConfirmUnhandledException(Exception e, string sourceName)
        {
            var message = $"予期せぬエラーが発生しました。続けて発生する場合は開発者に報告してください。\nプログラムの実行を継続しますか？";
            if (e != null) message += $"\n({e.Message} @ {e.TargetSite?.Name})";

            Log.Fatal(e, $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff")}|システムに予期せぬエラーが発生しました tu\n" +
                         $"続けて発生する場合は開発者に報告してください。\n" +
                         $"{message}\n" +
                         $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

            var messageBox = MessageBoxManager
                .GetMessageBoxStandard(
                    $"未処理例外 ({sourceName})",
                    message,
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo);

            var result = await messageBox.ShowWindowAsync();
            return result == MsBox.Avalonia.Enums.ButtonResult.Yes;
        }

        #endregion
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddCommonService(this IServiceCollection services)
        {
            // app
            services.AddSingleton<ConfigPathInfo>();

            // viewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<StartUpViewModel>();
            services.AddTransient<TopPageViewModel>();
            services.AddTransient<SettingPageViewModel>();
            services.AddTransient<MachinePageViewModel>();
            services.AddTransient<LogPageViewModel>();

            services.AddTransient<EditorLayoutViewModel>();
            services.AddTransient<LaneCreationPanelViewModel>();
            services.AddTransient<WorkAreaEditorPanelViewModel>();
            services.AddTransient<WorkAreaCutEditorPanelViewModel>();
            services.AddTransient<ActivityEditorViewModel>();
            services.AddTransient<DxfPageViewModel>();
            services.AddSingleton<CanvasViewModel>();

            // services
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ICanvasService, CanvasService>();
            services.AddSingleton<IDataLoader, CompactionAreaLoader>();
            services.AddSingleton<PolygonCsvParser>();
            services.AddSingleton<ILaneArrangementService, FindStartLaneArrangementPairToleranceService>();
            services.AddSingleton<ILaneArrangementEvaluationService, LaneArrangementDiffEvaluationService>();
            services.AddSingleton<DfsLaneIntegrator>();
            services.AddSingleton<IDirectionArrowService, DirectionArrowService>();
            services.AddSingleton<IWorkAreaService, WorkAreaService>();
            services.AddSingleton<IWorkAreaCutService, WorkAreaCutService>();
            services.AddSingleton<IDragRectService, DragRectService>();
            services.AddSingleton<ILaneArrangementParameterService, LaneArrangementParameterService>();
            services.AddSingleton<CaptureService>();
            services.AddSingleton<CadScriptService>();
            services.AddSingleton<IActivityService, ActivityByRollerHeadingService>();
            services.AddSingleton<IKWRPApplicationService, KWRPApplicationService>();
            services.AddSingleton<DxfConvertService>();
            services.AddSingleton<DxfHistoryStorageService>();
            services.AddSingleton<ILanguageService, LanguageService>();

            // stores
            services.AddSingleton<NavigationStore>();
            services.AddSingleton<NavigationPageStore>();
            services.AddSingleton<NavigationTwoPanelStore>();
            services.AddSingleton<CanvasItemStore>();
            services.AddSingleton<NotificationStore>();
            services.AddSingleton<LogStore>();
            services.AddSingleton<CanvasStateStore>();
            services.AddSingleton<MachineStore>();
            services.AddSingleton<ParameterStore>();
            services.AddSingleton<WorkAreaStore>();
            services.AddSingleton<ActivityStore>();
            services.AddSingleton<ApplicationStore>();
            services.AddSingleton<DxfStore>();

            services.AddSingleton<Window, MainWindow>(p =>
            {
                return new MainWindow
                {
                    DataContext = p.GetRequiredService<MainWindowViewModel>()
                };
            });
        }
    }

    public static class SingletonApplication
    {
        private static Mutex? mutex;

        public static bool Start()
        {
            mutex = new Mutex(true, "kajima_kwrp", out bool createdNew);
            return createdNew;
        }

        public static void Stop()
        {
            mutex?.ReleaseMutex();
            mutex = null;
        }
    }
}