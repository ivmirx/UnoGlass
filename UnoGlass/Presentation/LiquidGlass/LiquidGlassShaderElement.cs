namespace UnoGlass.Presentation.LiquidGlass;

using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

/// <summary>
/// SKCanvasElement that uses SKSL runtime shaders for lens refraction.
/// Demonstrates that Uno 6+ Skia backend fully supports SKRuntimeEffect.
/// </summary>
public sealed class LiquidGlassShaderElement : SKCanvasElement
{
    // Lens refraction SKSL shader (simplified from LiquidGlassAvaloniaUI)
    private const string LensShaderSource = """
        uniform shader content;
        uniform float2 size;
        uniform float cornerRadius;
        uniform float refractionHeight;
        uniform float refractionAmount;

        float sdRoundedRect(float2 coord, float2 halfSize, float radius) {
            float2 q = abs(coord) - halfSize + float2(radius);
            return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radius;
        }

        float2 gradSdRoundedRect(float2 coord, float2 halfSize, float radius) {
            float2 q = abs(coord) - halfSize + float2(radius);
            if (q.x >= 0.0 || q.y >= 0.0) {
                return sign(coord) * normalize(max(q, 0.0));
            } else {
                float gx = step(q.y, q.x);
                return sign(coord) * float2(gx, 1.0 - gx);
            }
        }

        float circleMap(float x) {
            x = clamp(x, 0.0, 1.0);
            return 1.0 - sqrt(1.0 - x * x);
        }

        half4 main(float2 coord) {
            float2 halfSize = size * 0.5;
            float2 centered = coord - halfSize;
            float sd = sdRoundedRect(centered, halfSize, cornerRadius);
            float h = max(refractionHeight, 0.001);

            // Inside flat area: no refraction
            if (-sd >= h) {
                return content.eval(coord);
            }

            // Edge area: apply lens refraction
            sd = min(sd, 0.0);
            float d = circleMap(1.0 - (-sd / h)) * refractionAmount;
            float gradRadius = min(cornerRadius * 1.5, min(halfSize.x, halfSize.y));
            float2 grad = gradSdRoundedRect(centered, halfSize, gradRadius);
            float2 refracted = coord + d * grad;

            return content.eval(refracted);
        }
        """;

    private static SKRuntimeEffect? _lensEffect;
    private static bool _shaderLoadAttempted;
    private static string? _shaderError;

    public float BlurRadius { get; set; } = 20f;
    public float CornerRadius { get; set; } = 16f;
    public float RefractionHeight { get; set; } = 12f;
    public float RefractionAmount { get; set; } = 24f;

    /// <summary>
    /// Optional explicit backdrop image. If null, renders a test gradient.
    /// </summary>
    public SKImage? BackdropImage { get; set; }

    private static void EnsureShaderLoaded()
    {
        if (_shaderLoadAttempted) return;
        _shaderLoadAttempted = true;

        _lensEffect = SKRuntimeEffect.CreateShader(LensShaderSource, out _shaderError);
        if (_lensEffect == null)
        {
            System.Diagnostics.Debug.WriteLine($"[LiquidGlassShader] SKSL compile error: {_shaderError}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[LiquidGlassShader] SKSL shader compiled successfully!");
        }
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        float width = (float)area.Width;
        float height = (float)area.Height;
        if (width <= 0 || height <= 0) return;

        EnsureShaderLoaded();

        var rect = new SKRect(0, 0, width, height);
        float clampedRadius = MathF.Min(CornerRadius, MathF.Min(width, height) * 0.5f);

        // Step 1: Get or create backdrop image
        using var backdropImage = BackdropImage ?? CreateTestBackdrop((int)width, (int)height);
        if (backdropImage == null) return;

        // Step 2: Apply blur to backdrop
        using var blurredBackdrop = ApplyBlur(backdropImage, BlurRadius);

        // Step 3: Apply lens refraction shader (if available)
        SKImage finalImage;
        if (_lensEffect != null && blurredBackdrop != null)
        {
            var refracted = ApplyLensShader(blurredBackdrop, width, height, clampedRadius);
            finalImage = refracted ?? blurredBackdrop;
        }
        else
        {
            // Fallback: just use blurred image
            finalImage = blurredBackdrop ?? backdropImage;
        }

        // Step 4: Draw result clipped to rounded rect
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(new SKRoundRect(rect, clampedRadius));

        canvas.Save();
        canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
        canvas.DrawImage(finalImage, 0, 0);
        canvas.Restore();

        // Step 5: Add glass overlay (tint + highlight)
        DrawGlassOverlay(canvas, rect, clampedRadius);

        // Step 6: Draw border
        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 80),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f
        };
        canvas.DrawRoundRect(new SKRoundRect(rect, clampedRadius), borderPaint);

        // Debug: show shader status
        if (_shaderError != null)
        {
            using var errorPaint = new SKPaint { Color = SKColors.Red, TextSize = 10 };
            canvas.DrawText("Shader error - see debug output", 4, 12, errorPaint);
        }
    }

    private SKImage CreateTestBackdrop(int width, int height)
    {
        // Simulate the portion of page gradient visible behind this button.
        // The page gradient goes from orange (top-left) to teal (bottom-right).
        // Since this button is roughly centered, we sample the middle portion.
        // We create a much larger virtual gradient and offset into it.

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var c = surface.Canvas;

        // Simulate a ~800x800 page gradient, and this button is near the center
        // So we offset the gradient to show the middle portion
        float pageSize = 800f;
        float offsetX = (pageSize - width) / 2;
        float offsetY = (pageSize - height) / 2 + 100; // Offset down a bit (button is below center)

        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(-offsetX, -offsetY),
                new SKPoint(pageSize - offsetX, pageSize - offsetY),
                [
                    new SKColor(255, 120, 80),   // Orange
                    new SKColor(200, 100, 150),  // Pink
                    new SKColor(100, 150, 200),  // Blue
                    new SKColor(80, 180, 160)    // Teal
                ],
                [0f, 0.33f, 0.66f, 1f],
                SKShaderTileMode.Clamp)
        };
        c.DrawRect(new SKRect(0, 0, width, height), paint);

        return surface.Snapshot();
    }

    private SKImage? ApplyBlur(SKImage source, float radius)
    {
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height));
        var c = surface.Canvas;

        using var blurFilter = SKImageFilter.CreateBlur(radius, radius);
        using var paint = new SKPaint { ImageFilter = blurFilter };
        c.DrawImage(source, 0, 0, paint);

        return surface.Snapshot();
    }

    private SKImage? ApplyLensShader(SKImage source, float width, float height, float radius)
    {
        if (_lensEffect == null) return null;

        try
        {
            // Create shader from source image
            using var sourceShader = source.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

            // Set up uniforms
            var uniforms = new SKRuntimeEffectUniforms(_lensEffect)
            {
                ["size"] = new[] { width, height },
                ["cornerRadius"] = radius,
                ["refractionHeight"] = RefractionHeight,
                ["refractionAmount"] = -RefractionAmount  // Negative for inward refraction
            };

            // Set up children (the backdrop shader)
            var children = new SKRuntimeEffectChildren(_lensEffect)
            {
                ["content"] = sourceShader
            };

            // Create the effect shader
            using var effectShader = _lensEffect.ToShader(uniforms, children);
            if (effectShader == null)
            {
                System.Diagnostics.Debug.WriteLine("[LiquidGlassShader] Failed to create effect shader");
                return null;
            }

            // Render to surface
            using var surface = SKSurface.Create(new SKImageInfo((int)width, (int)height));
            var c = surface.Canvas;

            using var paint = new SKPaint { Shader = effectShader, IsAntialias = true };
            c.DrawRect(new SKRect(0, 0, width, height), paint);

            return surface.Snapshot();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LiquidGlassShader] Exception: {ex.Message}");
            return null;
        }
    }

    private void DrawGlassOverlay(SKCanvas canvas, SKRect rect, float radius)
    {
        // Subtle white tint
        using var tintPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 20),
            IsAntialias = true
        };
        canvas.DrawRoundRect(new SKRoundRect(rect, radius), tintPaint);

        // Top edge highlight
        using var highlightPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, RefractionHeight),
                [new SKColor(255, 255, 255, 100), new SKColor(255, 255, 255, 0)],
                SKShaderTileMode.Clamp)
        };

        using var clipPath = new SKPath();
        clipPath.AddRoundRect(new SKRoundRect(rect, radius));
        canvas.Save();
        canvas.ClipPath(clipPath);
        canvas.DrawRect(new SKRect(0, 0, rect.Width, RefractionHeight), highlightPaint);
        canvas.Restore();
    }
}
