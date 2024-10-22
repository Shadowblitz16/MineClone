using System.Numerics;

namespace MineClone.Core.Graphics;

public class IndexBuffer(uint[] indices) : Disposable
{
    private readonly IndexBufferData _data = GraphicService.Instance.IndexBufferCreate(indices);
    public ref readonly IndexBufferData Data => ref _data;

    public void Use()
    {
        GraphicService.Instance.IndexBufferUse(in Data);
    }
    protected override void OnDispose()
    {
        base.OnDispose();
        GraphicService.Instance.IndexBufferDestroy(in Data);
    }
}

public readonly record struct IndexBufferData(uint Id, uint ElementSize, uint ElementCount)
{
    
}