using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Serilog;

namespace Valorant_Instalocker.Main.Helpers
{
    public static class AgentUIHelper
    {
        public static Border CreateAgentCard(string imagePath, string name, MouseButtonEventHandler clickHandler, Style cardStyle)
        {
            var card = new Border { Style = cardStyle, Tag = name, ToolTip = name };
            try
            {
                var bitmap = LoadBitmap(imagePath);
                var content = new Grid();
                content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(27) });

                var image = new Image { Source = bitmap, Stretch = Stretch.Uniform, Margin = new Thickness(5, 5, 5, 0) };
                var label = new TextBlock
                {
                    Text = name,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 11,
                    Foreground = (Brush)Application.Current.FindResource("TextMuted")
                };
                Grid.SetRow(label, 1);
                content.Children.Add(image);
                content.Children.Add(label);
                card.Child = content;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[AgentUI] Image failed for {AgentName}.", name);
                card.Child = new TextBlock { Text = name, Margin = new Thickness(8), TextWrapping = TextWrapping.Wrap };
            }

            card.MouseLeftButtonDown += clickHandler;
            return card;
        }

        private static BitmapImage LoadBitmap(string path)
        {
            var uri = Uri.TryCreate(path, UriKind.Absolute, out var absolute)
                ? absolute
                : new Uri(Path.GetFullPath(path), UriKind.Absolute);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 96;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
