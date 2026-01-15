using Microsoft.UI.Xaml.Media;
using UnoGlass.Presentation.LiquidGlass;

namespace UnoGlass.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.DataContext<MainViewModel>((page, vm) => page
            .NavigationCacheMode(NavigationCacheMode.Required)
            .Content(new Grid()
                .SafeArea(SafeArea.InsetMask.VisibleBounds)
                .Children(
                    // Gradient background
                    new Border()
                        .Background(new LinearGradientBrush()
                        {
                            StartPoint = new Windows.Foundation.Point(0, 0),
                            EndPoint = new Windows.Foundation.Point(1, 1),
                            GradientStops =
                            {
                                new GradientStop { Color = Windows.UI.Color.FromArgb(255, 255, 120, 80), Offset = 0 },
                                new GradientStop { Color = Windows.UI.Color.FromArgb(255, 200, 100, 150), Offset = 0.3 },
                                new GradientStop { Color = Windows.UI.Color.FromArgb(255, 100, 150, 200), Offset = 0.6 },
                                new GradientStop { Color = Windows.UI.Color.FromArgb(255, 80, 180, 160), Offset = 1 }
                            }
                        }),

                    // Content
                    new StackPanel()
                        .VerticalAlignment(VerticalAlignment.Center)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Spacing(16)
                        .MaxWidth(400)
                        .Padding(24)
                        .Children(
                            // Title
                            new TextBlock()
                                .Text("Liquid Glass Effects")
                                .FontSize(28)
                                .FontWeight(Microsoft.UI.Text.FontWeights.Bold)
                                .HorizontalAlignment(HorizontalAlignment.Center)
                                .Foreground(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)))
                                .Margin(0, 0, 0, 8),

                            new TextBlock()
                                .Text("Three approaches to glass effects in Uno Platform")
                                .FontSize(14)
                                .Opacity(0.8)
                                .HorizontalAlignment(HorizontalAlignment.Center)
                                .Foreground(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)))
                                .Margin(0, 0, 0, 24),

                            // Button 1: AcrylicBrush
                            new Grid()
                                .Height(56)
                                .Children(
                                    new Border()
                                        .CornerRadius(16)
                                        .Background(new AcrylicBrush()
                                        {
                                            AlwaysUseFallback = false,
                                            TintColor = Windows.UI.Color.FromArgb(255, 255, 255, 255),
                                            TintOpacity = 0,
                                            TintLuminosityOpacity = 0
                                        }),
                                    new Border()
                                        .CornerRadius(16)
                                        .BorderBrush(new SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 255, 255)))
                                        .BorderThickness(1.5)
                                        .Child(
                                            new TextBlock()
                                                .Text("Acrylic Glass")
                                                .FontSize(18)
                                                .HorizontalAlignment(HorizontalAlignment.Center)
                                                .VerticalAlignment(VerticalAlignment.Center)
                                                .Foreground(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)))
                                        )
                                ),

                            // Button 2: Skia Glass
                            new Grid()
                                .Height(56)
                                .Children(
                                    new LiquidGlassElement()
                                    {
                                        BlurRadius = 20,
                                        CornerRadius = 16,
                                        RefractionHeight = 12,
                                        RefractionAmount = 24,
                                        HighlightAngle = -MathF.PI / 4,
                                        HighlightFalloff = 3
                                    },
                                    new TextBlock()
                                        .Text("Skia Glass")
                                        .FontSize(18)
                                        .HorizontalAlignment(HorizontalAlignment.Center)
                                        .VerticalAlignment(VerticalAlignment.Center)
                                        .Foreground(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)))
                                ),

                            // Button 3: Composition Glass
                            BuildCompositionGlassButton(),

                            // Button 4: SKSL Shader Glass (lens refraction via SKRuntimeEffect)
                            new Grid()
                                .Height(56)
                                .Children(
                                    new LiquidGlassShaderElement()
                                    {
                                        BlurRadius = 20,
                                        CornerRadius = 16,
                                        RefractionHeight = 12,
                                        RefractionAmount = 24
                                    },
                                    new TextBlock()
                                        .Text("SKSL Shader Glass")
                                        .FontSize(18)
                                        .HorizontalAlignment(HorizontalAlignment.Center)
                                        .VerticalAlignment(VerticalAlignment.Center)
                                        .Foreground(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)))
                                )
                        )
                )));
    }

    static Grid BuildCompositionGlassButton()
    {
        var blurHost = new Border()
            .CornerRadius(16);

        blurHost.Loaded += (s, e) =>
        {
            CompositionGlassHelper.ApplyBackdropBlur(
                blurHost,
                blurAmount: 30f,
                tintColor: Windows.UI.Color.FromArgb(255, 255, 255, 255),
                tintOpacity: 0.15f);
        };

        return new Grid()
            .Height(56)
            .Children(
                blurHost,
                new Border()
                    .CornerRadius(16)
                    .BorderBrush(new SolidColorBrush(Windows.UI.Color.FromArgb(80, 255, 255, 255)))
                    .BorderThickness(1.5),
                new TextBlock()
                    .Text("Composition Glass")
                    .FontSize(18)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Foreground(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)))
            );
    }
}
