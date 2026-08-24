using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Serilog;
using Valorant_Instalocker.Main;
using Valorant_Instalocker.Views;

namespace Valorant_Instalocker
{
    public partial class MainWindow : Window
    {
        private string _currentTab = nameof(BtnNavDashboard);
        private readonly DashboardView _dashboard = new();
        private MacrosView? _mapRules;
        private SettingsView? _settings;
        private AboutView? _about;

        public MainWindow()
        {
            InitializeComponent();
            AppStateManager.Initialize();
            _dashboard.OpenMapRulesRequested += (_, _) => NavigateTo(BtnNavMacros);
            ShowContent(_dashboard, false);
            SetActiveNavigation(BtnNavDashboard);
        }

        private void SwitchTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button clickedButton || clickedButton.Name == _currentTab) return;

            NavigateTo(clickedButton);
        }

        private void NavigateTo(Button clickedButton)
        {

            try
            {
                UserControl view = clickedButton.Name switch
                {
                    nameof(BtnNavDashboard) => _dashboard,
                    nameof(BtnNavMacros) => _mapRules ??= new MacrosView(),
                    nameof(BtnNavSettings) => _settings ??= new SettingsView(),
                    nameof(BtnNavAbout) => _about ??= new AboutView(),
                    _ => _dashboard
                };

                _currentTab = clickedButton.Name;
                SetActiveNavigation(clickedButton);
                ShowContent(view, AppStateManager.CurrentUserSettings.EnableAnimations);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[MainWindow] Navigation failed for {CurrentTab}.", _currentTab);
            }
        }

        private void SetActiveNavigation(Button active)
        {
            foreach (var button in new[] { BtnNavDashboard, BtnNavMacros, BtnNavSettings, BtnNavAbout })
                button.Tag = button == active ? "Active" : null;
        }

        private void ShowContent(UserControl view, bool animate)
        {
            MainContent.Content = view;
            if (!animate)
            {
                view.Opacity = 1;
                view.RenderTransform = Transform.Identity;
                return;
            }

            view.Opacity = 0;
            view.RenderTransform = new TranslateTransform(0, 6);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            view.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)) { EasingFunction = easing });
            ((TranslateTransform)view.RenderTransform).BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(6, 0, TimeSpan.FromMilliseconds(180)) { EasingFunction = easing });
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) ToggleMaximize();
            else if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void ToggleMaximize() => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
