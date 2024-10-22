using System.Numerics;
using Silk.NET.OpenGL;

namespace MineClone.Core.Graphics;
public class VertexBuffer(Vertex[] vertices) : Disposable
{
    private readonly VertexBufferData _data = GraphicService.Instance.VertexBufferCreate(vertices);
    public ref readonly VertexBufferData Data => ref _data;

    public void Use()
    {
        GraphicService.Instance.VertexBufferUse(in Data);
    }
    protected override void OnDispose()
    {
        base.OnDispose();
        GraphicService.Instance.VertexBufferDestroy(in Data);
    }
}


public readonly record struct VertexBufferData(uint Id, uint ElementSize, uint ElementCount)
{
    
}