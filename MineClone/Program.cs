// See https://aka.ms/new-console-template for more information

using System.Numerics;
using MineClone.Core;
using MineClone.Core.Graphics;
using Silk.NET.SDL;
using Vertex = MineClone.Core.Graphics.Vertex;

var _vertices = new Vertex[]
{
    new(new(-0.5f, -0.5f,  0.0f), new(0.9f, 0.8f, 0.2f, 1)),
    new(new( 0.0f,  0.5f,  0.0f), new(0.2f, 0.9f, 0.8f, 1)),
    new(new( 0.5f, -0.5f, +0.5f), new(0.9f, 0.2f, 0.8f, 1)),
};

// var _indices = new uint[]
// {
//     0, 1, 2,
//     2, 3, 1
// };

// var translation = new Vector3(0, 0, 0);
// var rotation    = Quaternion.Identity;
// var scale       = Vector3.One;
//
// var view       = Matrix4x4.CreateLookAt(new Vector3(0, 0, 20), new Vector3(0, 0, 0), Vector3.UnitY);
// var projection = WindowService.Instance.GetProjection();
// var transform  = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) *
//                  Matrix4x4.CreateTranslation(translation);
//


WindowService.Instance.Load   += (_) =>
{
    GraphicService.Instance.VertexBufferCreate(_vertices);
};
WindowService.Instance.Render += (_) => {
    //GraphicService.Instance.Clear(System.Drawing.Color.Black);

    //_shader?.Uniform("uTransform", Matrix4x4.Identity);
    //_shader?.Uniform("uProjection", Matrix4x4.Identity);
    //_shader?.Uniform("uView", Matrix4x4.Identity);
    GraphicService.Instance.VertexBufferDrawCurrent();
};

WindowService.Instance.Show();