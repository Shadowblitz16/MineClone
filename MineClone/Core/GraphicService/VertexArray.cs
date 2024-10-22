namespace MineClone.Core.Graphics;
using MineClone.Core;


public class VertexArray() : Disposable
{
    private readonly VertexArrayData _data = GraphicService.Instance.VertexArrayCreate();
    public ref readonly VertexArrayData Data => ref _data;

    public void Use()
    {
        GraphicService.Instance.VertexArrayUse(in Data);
    }
    protected override void OnDispose()
    {
        base.OnDispose();
        GraphicService.Instance.VertexArrayDestroy(in Data);
    }
}

public readonly record struct VertexArrayData(uint Id);

