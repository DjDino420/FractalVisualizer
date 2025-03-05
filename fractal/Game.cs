using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework; // Explicit using

class Game : GameWindow
{
    private int shaderProgram;
    private int vao;
    private int resolutionLocation, centerLocation, zoomLocation, maxIterationsLocation;

    private Vector2 center = new Vector2(-0.5f, 0.0f); // Fraktál középpontja
    private float zoom = 1.0f;                        // Kezdeti nagyítás
    private int maxIterations = 100;                  // Iterációk száma

    public Game(NativeWindowSettings settings) : base(GameWindowSettings.Default, settings) { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);

        // Shader fájlok útvonala
        string vertexShaderPath = Path.Combine("shaders", "mandelbrot.vert");
        string fragmentShaderPath = Path.Combine("shaders", "mandelbrot.frag");

        // Shaderek betöltése fájlból
        shaderProgram = CreateShader(
            File.ReadAllText(vertexShaderPath),
            File.ReadAllText(fragmentShaderPath)
        );

        GL.UseProgram(shaderProgram);

        // Uniform változók lokációjának lekérése
        resolutionLocation = GL.GetUniformLocation(shaderProgram, "resolution");
        centerLocation = GL.GetUniformLocation(shaderProgram, "center");
        zoomLocation = GL.GetUniformLocation(shaderProgram, "zoom");
        maxIterationsLocation = GL.GetUniformLocation(shaderProgram, "maxIterations");

        // Vertex array létrehozása (fullscreen quad)
        float[] vertices = {
            -1f, -1f, 1f, -1f, -1f, 1f,
            -1f, 1f, 1f, -1f, 1f, 1f
        };

        vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    private int CreateShader(string vertexCode, string fragmentCode)
    {
        int vertex = CompileShader(ShaderType.VertexShader, vertexCode);
        int fragment = CompileShader(ShaderType.FragmentShader, fragmentCode);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }

    private int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Shader error ({type}): {infoLog}");
        }

        return shader;
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(shaderProgram);
        GL.Uniform2(resolutionLocation, new Vector2(Size.X, Size.Y));
        GL.Uniform2(centerLocation, center);
        GL.Uniform1(zoomLocation, zoom);
        GL.Uniform1(maxIterationsLocation, maxIterations);

        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        var input = KeyboardState;

        // Explicit hivatkozás a Keys típusra
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)) Close();
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) center.Y += 0.01f / zoom;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) center.Y -= 0.01f / zoom;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) center.X -= 0.01f / zoom;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) center.X += 0.01f / zoom;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Q)) zoom *= 1.02f;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.E)) zoom /= 1.02f;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up)) maxIterations += 10;
        if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down)) maxIterations = Math.Max(10, maxIterations - 10);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, Size.X, Size.Y);
        base.OnResize(e);
    }
}