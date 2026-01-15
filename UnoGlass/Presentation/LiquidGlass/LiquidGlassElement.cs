namespace UnoGlass.Presentation.LiquidGlass;

using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

/// <summary>
/// Custom SKCanvasElement that renders a liquid glass effect.
/// Simplified version that creates a frosted glass appearance without complex shaders.
/// </summary>
public sealed class LiquidGlassElement : SKCanvasElement
{
    /// <summary>Gaussian blur radius.</summary>
    public float BlurRadius { get; set; } = 20f;

    /// <summary>Corner radius for the rounded rectangle shape.</summary>
    public float CornerRadius { get; set; } = 16f;

    /// <summary>Height of the refraction band at edges (in pixels).</summary>
    public float RefractionHeight { get; set; } = 12f;

    /// <summary>Strength of the lens refraction effect.</summary>
    public float RefractionAmount { get; set; } = 24f;

    /// <summary>Light direction angle for highlight (radians). Default: top-left (-π/4).</summary>
    public float HighlightAngle { get; set; } = -MathF.PI / 4;

    /// <summary>Highlight falloff exponent. Higher = sharper highlight.</summary>
    public float HighlightFalloff { get; set; } = 3f;

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        float width = (float)area.Width;
        float height = (float)area.Height;
        if (width <= 0 || height <= 0) return;

        var rect = new SKRect(0, 0, width, height);
        float maxRadius = MathF.Min(width, height) * 0.5f;
        float clampedRadius = MathF.Min(CornerRadius, maxRadius);
        var roundRect = new SKRoundRect(rect, clampedRadius);

        // Step 1: Draw semi-transparent frosted base
        using var basePaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 25),
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateBlur(BlurRadius * 0.5f, BlurRadius * 0.5f)
        };
        canvas.DrawRoundRect(roundRect, basePaint);

        // Step 2: Draw glass surface with vertical gradient (lighter at top)
        using var surfacePaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(width * 0.5f, 0),
                new SKPoint(width * 0.5f, height),
                [
                    new SKColor(255, 255, 255, 50),
                    new SKColor(255, 255, 255, 15)
                ],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRoundRect(roundRect, surfacePaint);

        // Step 3: Draw edge highlight (top-left light source)
        DrawEdgeHighlight(canvas, rect, clampedRadius);

        // Step 4: Draw inner shadow at bottom for depth
        DrawInnerShadow(canvas, rect, clampedRadius);

        // Step 5: Draw subtle border
        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 60),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f
        };
        canvas.DrawRoundRect(roundRect, borderPaint);
    }

    private void DrawEdgeHighlight(SKCanvas canvas, SKRect rect, float radius)
    {
        float width = rect.Width;
        float height = rect.Height;

        // Top edge highlight
        using var topHighlightPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, RefractionHeight),
                [
                    new SKColor(255, 255, 255, 80),
                    new SKColor(255, 255, 255, 0)
                ],
                SKShaderTileMode.Clamp)
        };

        using var topPath = new SKPath();
        topPath.AddRoundRect(new SKRoundRect(rect, radius), SKPathDirection.Clockwise);

        canvas.Save();
        canvas.ClipPath(topPath);
        canvas.DrawRect(new SKRect(0, 0, width, RefractionHeight), topHighlightPaint);
        canvas.Restore();

        // Left edge highlight
        using var leftHighlightPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(RefractionHeight, 0),
                [
                    new SKColor(255, 255, 255, 60),
                    new SKColor(255, 255, 255, 0)
                ],
                SKShaderTileMode.Clamp)
        };

        canvas.Save();
        canvas.ClipPath(topPath);
        canvas.DrawRect(new SKRect(0, 0, RefractionHeight, height), leftHighlightPaint);
        canvas.Restore();
    }

    private void DrawInnerShadow(SKCanvas canvas, SKRect rect, float radius)
    {
        float width = rect.Width;
        float height = rect.Height;
        float shadowHeight = RefractionHeight * 0.8f;

        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, height - shadowHeight),
                new SKPoint(0, height),
                [
                    new SKColor(0, 0, 0, 0),
                    new SKColor(0, 0, 0, 20)
                ],
                SKShaderTileMode.Clamp)
        };

        using var clipPath = new SKPath();
        clipPath.AddRoundRect(new SKRoundRect(rect, radius));

        canvas.Save();
        canvas.ClipPath(clipPath);
        canvas.DrawRect(new SKRect(0, height - shadowHeight, width, height), shadowPaint);
        canvas.Restore();
    }
}
