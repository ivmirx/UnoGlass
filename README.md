# UnoGlass

A proof-of-concept exploring glass visual effects in Uno Platform. Started as an attempt to port [LiquidGlassAvaloniaUI](https://github.com/KaranocaVe/LiquidGlassAvaloniaUI). The SKSL shader pipeline works, but true backdrop capture remains challenging.

## Avalonia vs Uno Platform: Rendering Architecture

According to Claude, two frameworks expose SkiaSharp differently:

| Aspect | Avalonia | Uno Platform |
|--------|----------|--------------|
| **Custom rendering** | `ICustomDrawOperation` hooks into render pipeline | `SKCanvasElement` renders in isolation |
| **Backdrop access** | Can snapshot content rendered *before* the current element | No direct access to previously rendered content |
| **SKSL Shaders** | Full support via `SKRuntimeEffect` | Full support via `SKRuntimeEffect` |

The key difference: Avalonia's `ICustomDrawOperation.Render()` runs during the render pass, allowing it to capture what's already been drawn. Uno's `SKCanvasElement.RenderOverride()` gets a fresh canvas.

LiquidGlassAvaloniaUI achieves its effect through:
1. Capturing the backdrop via `RenderTargetBitmap` mid-render
2. Applying SKSL shaders for lens refraction (SDF-based edge distortion)
3. Chromatic aberration via 7-channel RGB dispersion

## Four Approaches Demonstrated

### 1. AcrylicBrush (WinUI Built-in)
Uses WinUI's native `AcrylicBrush` for backdrop blur.
- **How**: `AlwaysUseFallback=false`, `TintOpacity=0`, `TintLuminosityOpacity=0`
- **Result**: Works, blurs in-app content behind the element
- **Limitation**: On iOS/Android/macOS, can only be applied to elements with **no children**

### 2. Skia Glass (SKCanvasElement)
Custom SkiaSharp rendering with gradient overlays and edge highlights.
- **How**: Subclass `SKCanvasElement`, draw frosted layers, edge gradients, inner shadows
- **Result**: Looks glassy but static - simulated effect without actual backdrop
- **Limitation**: No backdrop capture; purely decorative

### 3. Composition Glass (Composition API)
Uses WinUI Composition API with `CompositionBackdropBrush` + `GaussianBlurEffect`.
- **How**: Create effect brush from backdrop, apply to sprite visual
- **Result**: True backdrop blur of in-app content
- **Limitation**: Some Composition features not fully implemented in Uno

### 4. SKSL Shader Glass (SKRuntimeEffect)
Uses actual SKSL shaders for lens refraction, similar to Avalonia approach.
- **How**: Compile SKSL shader with `SKRuntimeEffect`, apply SDF-based lens distortion
- **Result**: Real lens refraction effect with blur and highlights
- **Limitation**: Backdrop must be provided explicitly (simulated or captured separately)

## The Backdrop Capture Challenge

The main obstacle to a full Avalonia-style port is capturing the actual backdrop. Potential solutions:

1. **RenderTargetBitmap**: Capture parent content before drawing glass element
2. **CompositionDrawingSurface**: Use Composition API to sample backdrop
3. **Explicit backdrop**: Pass a known background image/gradient to the shader

The SKSL shader pipeline itself works correctly in Uno 6+ with the unified Skia renderer.

