#nullable enable

using System;
using Windows.Graphics.Effects;
using Windows.Graphics.Effects.Interop;

namespace UnoGlass.Presentation.LiquidGlass;

/// <summary>
/// Gaussian blur effect for use with Composition API.
/// Implements IGraphicsEffectD2D1Interop to work with CompositionEffectBrush.
/// </summary>
internal class GaussianBlurEffect : IGraphicsEffect, IGraphicsEffectSource, IGraphicsEffectD2D1Interop
{
    // Direct2D Gaussian Blur Effect CLSID
    private static readonly Guid EffectId = new("1FEB6D69-2FE6-4AC9-8C58-1D7F93E7A6A5");

    private string _name = "GaussianBlurEffect";

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// The source to blur. Set to CompositionBackdropBrush for backdrop blur.
    /// </summary>
    public IGraphicsEffectSource? Source { get; set; }

    /// <summary>
    /// Blur radius in pixels. Default is 3.0f.
    /// </summary>
    public float BlurAmount { get; set; } = 3.0f;

    public Guid GetEffectId() => EffectId;

    public void GetNamedPropertyMapping(string name, out uint index, out GraphicsEffectPropertyMapping mapping)
    {
        switch (name)
        {
            case nameof(BlurAmount):
                index = 0;
                mapping = GraphicsEffectPropertyMapping.Direct;
                break;
            default:
                index = 0xFF;
                mapping = (GraphicsEffectPropertyMapping)0xFF;
                break;
        }
    }

    public object? GetProperty(uint index)
    {
        return index switch
        {
            0 => BlurAmount,
            _ => null
        };
    }

    public uint GetPropertyCount() => 1;
    public IGraphicsEffectSource? GetSource(uint index) => Source;
    public uint GetSourceCount() => 1;
}
