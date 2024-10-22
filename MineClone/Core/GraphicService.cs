using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;

namespace MineClone.Core;
using MineClone.Core.Graphics;
public class GraphicService : Disposable
{
    private readonly GL _gl;
    private uint                               _stride = 0;
    private readonly List<VertexBufferElement> _elements = new();
    private readonly VertexArrayData           _defaultVao;
    private readonly VertexBufferData          _defaultVbo;
    private readonly IndexBufferData           _defaultIbo;
    private readonly ShaderData                _defaultShader;
    private VertexArrayData                    _currentVao;
    private VertexBufferData                   _currentVbo;
    private IndexBufferData                    _currentIbo;
    private ShaderData                         _currentShader;
    
    private readonly record struct Attribute(VertexAttribPointerType Type, uint Index, uint Size, uint Count, bool Normalized);
    private GraphicService()
    {
        WindowService.Instance.Verify();
        _gl = WindowService.Instance;

        _defaultVao    = VertexArrayCreate();
        _defaultVbo    = VertexBufferCreate([]);
        _defaultIbo    = IndexBufferCreate([]);
        _defaultShader = ShaderCreate(Shader.DefaultVertexShader, Shader.DefaultFragmentShader);
    }
    private static Lazy<GraphicService> Lazy { get; } = new(() => new());
    public static GraphicService Instance => Lazy.Value;
    
    // Misc
    public void Clear(Color color)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.ClearColor(color);
    }
    
    // VertexArray
    public ref readonly VertexArrayData VertexArrayDefault()
    {
        return ref _defaultVao;
    }
    public ref readonly VertexArrayData VertexArrayCurrent()
    {
        return ref _currentVao;
    }
    public ref readonly VertexArrayData VertexArrayCreate ()
    {
        var data = new VertexArrayData(_gl.GenVertexArray());
        VertexArrayUse(in data);
        return ref VertexArrayCurrent();
    }
    public void VertexArrayUse              (ref readonly VertexArrayData data)
    {
        if (_currentVao == data) return;
        _currentVao = data;
        _gl.BindVertexArray(data.Id);
    }
    public void VertexArrayDestroy          (ref readonly VertexArrayData data)
    {
        if (_currentVao == data) VertexArrayUse(in VertexArrayDefault());
        _gl.DeleteVertexArray(data.Id);
    }

    // VertexArray Utils
    private uint                             GetStride  () => _stride;
    private IEnumerable<VertexBufferElement> GetElements() => _elements;
    private static VertexAttribPointerType   GetAttributeType (VertexBufferElementType type)
    {
        return type switch
        {
            VertexBufferElementType.Int     => VertexAttribPointerType.Int,
            VertexBufferElementType.Float   => VertexAttribPointerType.Float,
            VertexBufferElementType.Vector2 => VertexAttribPointerType.Float,
            VertexBufferElementType.Vector3 => VertexAttribPointerType.Float,
            VertexBufferElementType.Vector4 => VertexAttribPointerType.Float,
            VertexBufferElementType.Matrix4 => VertexAttribPointerType.Float,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    private static uint                      GetAttributeSize (VertexBufferElementType type)
    {
        unsafe
        {
            return type switch
            {
                VertexBufferElementType.Int     => (uint)sizeof(int    ),
                VertexBufferElementType.Float   => (uint)sizeof(float  ),
                VertexBufferElementType.Vector2 => (uint)sizeof(float  ),
                VertexBufferElementType.Vector3 => (uint)sizeof(float  ),
                VertexBufferElementType.Vector4 => (uint)sizeof(float  ),
                VertexBufferElementType.Matrix4 => (uint)sizeof(float  ),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
    private static uint                      GetAttributeCount(VertexBufferElementType type)
    {
        unsafe
        {
            return type switch
            {
                VertexBufferElementType.Int     => 1,
                VertexBufferElementType.Float   => 1,
                VertexBufferElementType.Vector2 => 2,
                VertexBufferElementType.Vector3 => 3,
                VertexBufferElementType.Vector4 => 4,
                VertexBufferElementType.Matrix4 => 16,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
    
    
    // IndexBuffer
    public ref readonly IndexBufferData IndexBufferDefault    ()
    {
        return ref _defaultIbo;
    }
    public ref readonly IndexBufferData IndexBufferCurrent    ()
    {
        return ref _currentIbo;
    }
    public ref readonly IndexBufferData IndexBufferCreate     (in uint[] indices)
    {
        var data = new IndexBufferData(_gl.GenBuffer(), sizeof(uint), (uint)indices.Length);
        IndexBufferUse(ref data);
        _gl.BufferData(GLEnum.ElementArrayBuffer, new ReadOnlySpan<uint>(indices), GLEnum.StaticDraw);
        return ref IndexBufferCurrent();
    }
    public void         IndexBufferUse        (ref readonly IndexBufferData data)
    {
        if (_currentIbo == data) return;
        _currentIbo = data;
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, data.Id);
    }
    public void         IndexBufferDestroy    (ref readonly IndexBufferData data)
    {
        if (_currentIbo == data) IndexBufferUse(in IndexBufferDefault());
        _gl.DeleteBuffer(data.Id);
    }
    public void         IndexBufferDraw       (ref readonly IndexBufferData data, uint first=0, uint count=0)
    {
        unsafe
        {
            IndexBufferUse(data);
            first = uint.Max(0, first == 0 ? 0 : first);
            count = uint.Min(data.ElementCount, count == 0 ? data.ElementCount : count);
            _gl.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, (void*)first);
        }
    }
    public void         IndexBufferDrawCurrent(uint first=0, uint count=0)
    {
        IndexBufferDraw(in IndexBufferCurrent(), first, count);
    }
    
    // VertexBuffer
    public ref readonly VertexBufferData VertexBufferDefault()
    {
        return ref _defaultVbo;
    }
    public ref readonly VertexBufferData VertexBufferCurrent()
    {
        return ref _currentVbo;
    }
    public ref readonly VertexBufferData VertexBufferCreate (in Vertex[] vertices)
    {
        unsafe
        {
            var data = new VertexBufferData(_gl.GenBuffer(), (uint)sizeof(Vertex), (uint)vertices.Length);
            VertexBufferUse(ref data);
            _gl.BufferData(GLEnum.ArrayBuffer, new ReadOnlySpan<Vertex>(vertices), GLEnum.StaticDraw);
            
            VertexBufferAttributesBegin(in data);
            VertexBufferAttribute      (in data, VertexBufferElementType.Vector3, false);
            VertexBufferAttribute      (in data, VertexBufferElementType.Vector4, false);
            VertexBufferAttributesEnd  (in data);
            return ref VertexBufferCurrent();
        }
    }
    public void VertexBufferUse             (ref readonly VertexBufferData data)
    {
        if (_currentVbo == data) return;
        _currentVbo = data;
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, data.Id);
    }
    public void VertexBufferDestroy         (ref readonly VertexBufferData data)
    {
        if (_currentVbo == data) VertexBufferUse(in VertexBufferDefault());
        _gl.DeleteBuffer(data.Id);
    }
    public void VertexBufferDraw            (ref readonly VertexBufferData data, uint first=0, uint count=0)
    {
        VertexBufferUse(in data);
        first = uint.Max(0, first == 0 ? 0 : first);
        count = uint.Min(data.ElementCount, count == 0 ? data.ElementCount : count);
        _gl.DrawArrays(PrimitiveType.Triangles, (int)first, count);
    }
    public void VertexBufferDrawCurrent     (uint first=0, uint count=0)
    {
        VertexBufferDraw(in VertexBufferCurrent(), first, count);
    }
    public void VertexBufferAttribute       (ref readonly VertexBufferData data, VertexBufferElementType type, bool normalized)
    {
        VertexBufferUse(in data);
        var size  = GetAttributeSize (type);
        var count = GetAttributeCount(type);
        _elements.Add(new(type, size, count, normalized));
        _stride += size * count;
    }
    public void VertexBufferAttributesEnd   (ref readonly VertexBufferData data)
    {
        VertexBufferUse(in data);
        unsafe
        {
            uint index   = 0;
            uint offset  = 0;
            var stride   = GetStride();
            var elements = GetElements();
            foreach (var element in elements)
            {
                _gl.EnableVertexAttribArray(index);
                _gl.VertexAttribPointer    (index, (int)element.Count, GetAttributeType(element.Type), element.Normalized, stride, (void*)offset);
                index++;
                offset += element.Count * element.Size;
            }
        }
    }
    public void VertexBufferAttributesBegin (ref readonly VertexBufferData data)
    {
        VertexBufferUse(in data);
        _stride = 0;
        _elements.Clear();
    }

    // Shader
    public ref readonly ShaderData ShaderDefault()
    {
        return ref _defaultShader;
    }
    public ref readonly ShaderData ShaderCurrent()
    {
        return ref _currentShader;
    }
    public ref readonly ShaderData ShaderCreate (string vertexShader, string fragmentShader)
    {
        var vId = CompileShader(vertexShader  , ShaderType.VertexShader);
        var fId = CompileShader(fragmentShader, ShaderType.FragmentShader);
        var id  = new ShaderData(LinkProgram(vId, fId));
        ShaderUse(in id);
        return ref ShaderCurrent();
    }
    public void ShaderUse    (ref readonly ShaderData data)
    {
        if (_currentShader == data) return;
        _currentShader = data;
        _gl.UseProgram(data.Id);
    }
    public void ShaderDestroy(ref readonly ShaderData data)
    {
        if (_currentShader == data) ShaderUse(in ShaderDefault());
        _gl.DeleteProgram(data.Id);
    }
    public void ShaderUniform(ref readonly ShaderData data, string name, int value)
    {
        ShaderUse(in data);
        var location = _gl.GetUniformLocation(data.Id, name);
        if (location != -1) _gl.Uniform1(location, value);
        else throw new Exception($"Shader uniform {name} does not exist.");
    }
    public void ShaderUniform(ref readonly ShaderData data, string name, float value)
    {
        ShaderUse(in data);
        var location = _gl.GetUniformLocation(data.Id, name);
        if (location != -1) _gl.Uniform1(location, value);
        else throw new Exception($"Shader uniform {name} does not exist.");
    }
    public void ShaderUniform(ref readonly ShaderData data, string name, Vector2 value)
    {
        ShaderUse(in data);
        var location = _gl.GetUniformLocation(data.Id, name);
        if (location != -1) _gl.Uniform2(location, value.X, value.Y);
        else throw new Exception($"Shader uniform {name} does not exist.");
    }
    public void ShaderUniform(ref readonly ShaderData data, string name, Vector3 value)
    {
        ShaderUse(in data);
        var location = _gl.GetUniformLocation(data.Id, name);
        if (location != -1) _gl.Uniform3(location, value.X, value.Y, value.Z);
        else throw new Exception($"Shader uniform {name} does not exist.");
    }
    public void ShaderUniform(ref readonly ShaderData data, string name, Vector4 value)
    {
        ShaderUse(in data);
        var location = _gl.GetUniformLocation(data.Id, name);
        if (location != -1) _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        else throw new Exception($"Shader uniform {name} does not exist.");
    }
    public void ShaderUniform(ref readonly ShaderData data, string name, Matrix4x4 value)
    {
        unsafe
        {
            ShaderUse(in data);
            var location = _gl.GetUniformLocation(data.Id, name);
            if (location != -1) _gl.UniformMatrix4(location, 1, true, (float*)&value);
            else throw new Exception($"Shader uniform {name} does not exist.");
        }
    }

    // Shader Utils
    private uint CompileShader(string source, ShaderType type)
    {
        var id = _gl.CreateShader(type);
        _gl.ShaderSource(id, source);
        _gl.CompileShader(id);
        _gl.GetShader(id, ShaderParameterName.CompileStatus, out var ok);

        if (ok != 0) return id;
        
        var log = _gl.GetShaderInfoLog(id);
        throw type switch
        {
            ShaderType.VertexShader   => new Exception($"Vertex Shader compilation failed with log: {log}"),
            ShaderType.FragmentShader => new Exception($"Fragment Shader compilation failed with log: {log}"),
            _                         => new Exception($"Unknown Shader compilation failed with log: {log}")
        };
    }
    private uint LinkProgram  (uint vId, uint fId)
    {
        var id = _gl.CreateProgram();
        _gl.AttachShader(id, vId);
        _gl.AttachShader(id, fId);
        _gl.LinkProgram (id);
        _gl.GetProgram  (id, ProgramPropertyARB.LinkStatus, out var ok);

        if (ok != 0) return id;
        
        var log = _gl.GetProgramInfoLog(id);
        throw new Exception($"Shader linking failed with log: {log}");
    }
    
}


