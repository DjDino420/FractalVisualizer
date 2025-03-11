using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

class Program
{
    static void Main()
    {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "Mandelbrot Haladó",
            API = ContextAPI.OpenGL,
            APIVersion = new Version(3, 3),
            WindowState = WindowState.Normal
        };

        using var window = new Game(nativeWindowSettings);
        window.Run();
    }
}