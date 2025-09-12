using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Winerr.NET.Core.Configs;

namespace Winerr.NET.Core.Interfaces
{
    public interface IRenderer
    {
        Image<Rgba32> Generate(ErrorConfig config);
    }
}