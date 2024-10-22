using System.Numerics;
using System.Runtime.InteropServices;

namespace MineClone.Core.Graphics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex(Vector3 position, Vector4 color)
{
    public readonly Vector3 Position = position;
    public readonly Vector4 Color    = color;
} 