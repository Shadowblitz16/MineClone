using System.Numerics;
using Silk.NET.OpenGL;

namespace MineClone.Core.Graphics;

public enum VertexBufferElementType
{
    Int,
    Float,
    Vector2,
    Vector3,
    Vector4,
    Matrix4
}

public readonly record struct VertexBufferElement
(
    VertexBufferElementType Type,
    uint                    Size,
    uint                    Count,
    bool                    Normalized
);