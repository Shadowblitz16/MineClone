using System.Collections.Immutable;
using System.Numerics;
using Silk.NET.OpenGL;

namespace MineClone.Core.Graphics;
public class Shader(string vertexSource, string fragmentSource) : Disposable
{
    public static readonly string DefaultVertexShader = @"
#version 430 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;

//uniform mat4 uTransform;
//uniform mat4 uProjection;
//uniform mat4 uView;

out vec4 fColor;
void main()
{
    gl_Position = vec4(aPosition, 1.0);//uProjection * uView * (uTransform * vec4(aPosition, 1.0));
    fColor = aColor;
}";
    public static readonly string DefaultFragmentShader = @"
#version 430 core
in vec4 fColor;

out vec4 Color;

void main()
{
    Color = fColor;
}";

    private readonly ShaderData _data = GraphicService.Instance.ShaderCreate(vertexSource, fragmentSource);
    public ref readonly ShaderData Data => ref _data;


    public Shader() : this(DefaultVertexShader, DefaultVertexShader)
    {
        
    }
    
    public void Use()
    {
        GraphicService.Instance.ShaderUse(in Data);
    }

    public void Uniform(string name, int value)
    {
        GraphicService.Instance.ShaderUniform(in Data, name, value);
    }
    public void Uniform(string name, float value)
    {
        GraphicService.Instance.ShaderUniform(in Data, name, value);
    }
    public void Uniform(string name, Vector2 value)
    {
        GraphicService.Instance.ShaderUniform(in Data, name, value);
    }
    public void Uniform(string name, Vector3 value)
    {
        GraphicService.Instance.ShaderUniform(in Data, name, value);
    }
    public void Uniform(string name, Vector4 value)
    {
        GraphicService.Instance.ShaderUniform(in Data, name, value);
    }
    public void Uniform(string name, Matrix4x4 value)
    {
        GraphicService.Instance.ShaderUniform(in Data, name, value);
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        GraphicService.Instance.ShaderDestroy(in Data);
    }
}

public readonly record struct ShaderData(uint Id);