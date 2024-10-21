using System.Drawing;
using Silk.NET.OpenGL;

namespace MineClone.Core;

public class GraphicService
{
    private readonly GL _gl;
    private GraphicService()
    {
        WindowService.Instance.Verify();
        _gl = WindowService.Instance;
    }
    private static Lazy<GraphicService> Lazy => new(() => new());
    public static GraphicService Instance => Lazy.Value;


    public void Clear()
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _gl.ClearColor(Color.Black);
    }
}