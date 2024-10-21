using System.Diagnostics;
using System.Numerics;
using System.Text;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace MineClone.Core;

public sealed class WindowService
{
    private bool _running;
    private readonly IWindow       _window;
    private readonly GL            _gl;
    private readonly IInputContext _input;
    private float   _lastDelta;
    private Vector2 _lastSize;
    private Vector2 _lastPosition;
    private Vector2 _lastMousePosition;
    private Vector2 _lastMouseWheel;
    private WindowService()
    {
        _window = Window.Create(WindowOptions.Default with
        {
            Size = new(800, 600),
            Title = "MineClone",
            IsVisible = false,
        });
        _window.Initialize();
        _window.Update  += (d) => {
            _lastDelta = (float)d;
            Update?.Invoke(this);
        };
        _window.Render  += (d) => {
            _lastDelta = (float)d;
            GraphicService.Instance.Clear();
            Render?.Invoke(this);
        };
        _window.Closing += ( ) => {
            Unload?.Invoke(this);
        };
        _window.Move    += (v) => {
            var vector = new Vector2(v.X, v.Y);
            var delta  =  vector - _lastPosition;
            Debug.Assert((_lastPosition + delta) == vector, "Bad Window Position");
            Moved?.Invoke(this, _lastPosition, delta);
            _lastPosition = vector;
        };
        _window.Resize  += (v) => {
            var vector = new Vector2(v.X, v.Y);
            var delta  =  vector - _lastSize;
            Debug.Assert((_lastSize + delta) == vector, "Bad Window Size");
            Resized?.Invoke(this, _lastSize, _lastSize - vector);
            _lastSize = vector;
        };

        _gl   = _window.CreateOpenGL();
        _input = _window.CreateInput();
        foreach (var keyboard in _input.Keyboards)
        {
            keyboard.KeyUp   += (device, key, value) => {
                KeyUp?.Invoke(this, key);
            };
            keyboard.KeyDown += (device, key, value) => {
                KeyDown?.Invoke(this, key);
            };
            keyboard.KeyChar += (device, character) => {
                KeyChar?.Invoke(this, character);
            };
        }
        foreach (var mouse    in _input.Mice     )
        {
            mouse.MouseUp   += (device, button) => {
                MouseUp?.Invoke(this, button);
            };
            mouse.MouseDown += (device, button) => {
                MouseDown?.Invoke(this, button);
            };
            mouse.MouseMove += (device, v) => {
                var vector = new Vector2(v.X, v.Y);
                var delta  = vector - _lastMousePosition;
                Debug.Assert((_lastMousePosition + delta) == vector, "Bad Mouse Position");
                MouseMoved?.Invoke(this, _lastMousePosition, delta);
                _lastMousePosition = vector;
            };
            mouse.Scroll    += (device, d) => {
                var delta  = new Vector2(d.X, d.Y);
                var vector = _lastMouseWheel + delta;
                Debug.Assert((_lastMouseWheel + delta) == vector, "Bad Wheel Position");
                MouseScrolled?.Invoke(this, _lastMouseWheel, delta);
                _lastMouseWheel = vector;
            };
        }

        Load?.Invoke(this);
    }

    private static Lazy<WindowService> Lazy => new(() => new());
    public static WindowService Instance => Lazy.Value;

    public float GetDelta() => _lastDelta;
    public void Verify()
    {
        Debug.Assert(_window.IsInitialized);
    }

    public void Show()
    {
        _window.IsVisible = true;
        if (_running) return;
        
        _running = true;
        _window.Run();
    }
    public void Hide()
    {
        _window.IsVisible = false;
    }

    public void Restore ()
    {
        _window.WindowState = WindowState.Normal;
    }
    public void Minimize()
    {
        _window.WindowState = WindowState.Minimized;
    }
    public void Maximize()
    {
        _window.WindowState = WindowState.Maximized;
    }
    
    public event WindowServiceDelegate? Load;
    public event WindowServiceDelegate? Unload;
    public event WindowServiceDelegate? Update;
    public event WindowServiceDelegate? Render;

    public event WindowServiceDeltaDelegate<Vector2>? Moved;
    public event WindowServiceDeltaDelegate<Vector2>? Resized;

    public event WindowServiceValueDelegate<Key>?         KeyUp;
    public event WindowServiceValueDelegate<Key>?         KeyDown;
    public event WindowServiceValueDelegate<char>?        KeyChar;

    public event WindowServiceValueDelegate<MouseButton>? MouseUp;
    public event WindowServiceValueDelegate<MouseButton>? MouseDown;
    public event WindowServiceDeltaDelegate<Vector2    >? MouseMoved;
    public event WindowServiceDeltaDelegate<Vector2    >? MouseScrolled;

    public static implicit operator GL(WindowService service)
    {
        return service._gl;
    }
}

public delegate void WindowServiceDelegate           (WindowService service);
public delegate void WindowServiceValueDelegate<in T>(WindowService service, T value);
public delegate void WindowServiceDeltaDelegate<in T>(WindowService service, T last, T delta);