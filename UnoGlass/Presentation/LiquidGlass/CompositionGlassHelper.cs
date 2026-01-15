using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace UnoGlass.Presentation.LiquidGlass;

/// <summary>
/// Helper to create composition-based glass effects using CompositionBackdropBrush + GaussianBlurEffect.
/// This is the "proper WinUI way" to do backdrop blur.
/// </summary>
public static class CompositionGlassHelper
{
    /// <summary>
    /// Applies a backdrop blur effect to an element using Composition API.
    /// </summary>
    /// <param name="element">The element to apply the effect to</param>
    /// <param name="blurAmount">Blur radius in pixels (default 20)</param>
    /// <param name="tintColor">Optional tint color overlay</param>
    /// <param name="tintOpacity">Tint opacity 0-1 (default 0.1)</param>
    public static void ApplyBackdropBlur(
        FrameworkElement element,
        float blurAmount = 20f,
        Windows.UI.Color? tintColor = null,
        float tintOpacity = 0.1f)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        // Get element size for the visual
        var size = new System.Numerics.Vector2((float)element.ActualWidth, (float)element.ActualHeight);
        if (size.X <= 0 || size.Y <= 0)
        {
            // If not yet measured, use reasonable defaults
            size = new System.Numerics.Vector2(400, 56);
        }

        // Create the backdrop brush (captures content behind)
        var backdropBrush = compositor.CreateBackdropBrush();

        // Create Gaussian blur effect with backdrop as source
        var blurEffect = new GaussianBlurEffect
        {
            Name = "Blur",
            BlurAmount = blurAmount,
            Source = new CompositionEffectSourceParameter("backdrop")
        };

        // Create effect factory and brush
        var effectFactory = compositor.CreateEffectFactory(blurEffect);
        var effectBrush = effectFactory.CreateBrush();
        effectBrush.SetSourceParameter("backdrop", backdropBrush);

        // Create a sprite visual to host the effect
        var blurVisual = compositor.CreateSpriteVisual();
        blurVisual.Brush = effectBrush;
        blurVisual.Size = size;

        // Add tint overlay if specified
        if (tintColor.HasValue && tintOpacity > 0)
        {
            var tintVisual = compositor.CreateSpriteVisual();
            var color = tintColor.Value;
            tintVisual.Brush = compositor.CreateColorBrush(
                Windows.UI.Color.FromArgb((byte)(tintOpacity * 255), color.R, color.G, color.B));
            tintVisual.Size = size;

            // Create container to hold both blur and tint
            var container = compositor.CreateContainerVisual();
            container.Children.InsertAtTop(blurVisual);
            container.Children.InsertAtTop(tintVisual);
            container.Size = size;

            ElementCompositionPreview.SetElementChildVisual(element, container);
        }
        else
        {
            ElementCompositionPreview.SetElementChildVisual(element, blurVisual);
        }

        // Update size when element resizes
        element.SizeChanged += (s, e) =>
        {
            var newSize = new System.Numerics.Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
            blurVisual.Size = newSize;
        };
    }
}
