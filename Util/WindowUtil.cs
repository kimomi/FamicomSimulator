using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace FamicomSimulator.Util
{
    internal static class WindowUtil
    {
        private static GameWindow? _window;
        private static int _vao, _vbo, _shaderProgram, _texture;
        private static Random _random = new Random();
        private static int _width = 800, _height = 600;
        private static Action? _updateData;

        // 顶点数据：矩形四个角坐标和纹理坐标
        static readonly float[] _vertices =
        {
            1f,  1f, 0.0f, 1.0f, 1.0f,  // top right
            1f, -1f, 0.0f, 1.0f, 0.0f,  // bottom right
            -1f, -1f, 0.0f, 0.0f, 0.0f, // bottom left
            -1f,  1f, 0.0f, 0.0f, 1.0f  // top left
        };

        public static void Show(int w, int h, string title, Action update)
        {
            _width = w;
            _height = h;
            _updateData = update;

            var settings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(_width, _height),
                Title = title,
                MinimumClientSize = new Vector2i(_width, _height),  // 禁用窗口大小调整
                MaximumClientSize = new Vector2i(_width, _height),  // 禁用窗口大小调整
            };

            using (_window = new GameWindow(GameWindowSettings.Default, settings))
            {
                _window.Load += OnLoad;
                _window.RenderFrame += OnRenderFrame;
                _window.Run();
            }
        }

        public static void Close()
        {
            _window?.Close();
        }

        private static void OnLoad()
        {
            // 创建 VAO 和 VBO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsage.StaticDraw);

            // 创建并编译着色器
            _shaderProgram = CreateShaderProgram();

            // 设置顶点属性指针
            GL.EnableVertexAttribArray(0); // Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1); // Texture coordinates
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            // 创建纹理
            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2d, _texture);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        public static void DrawData(uint[] data)
        {
            UpdateTexture(data);
        }

        private static void OnRenderFrame(FrameEventArgs args)
        {
            _updateData?.Invoke();

            // 清屏
            GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // 使用着色器并绘制矩形
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4); // 使用 TriangleFan 绘制矩形

            // 交换缓冲区
            _window?.SwapBuffers();
        }

        private static int CreateShaderProgram()
        {
            // 顶点着色器代码
            string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aTexCoord;
            out vec2 TexCoords;
            void main()
            {
                gl_Position = vec4(aPosition, 1.0);
                TexCoords = aTexCoord;
            }
        ";

            // 片段着色器代码
            string fragmentShaderSource = @"
            #version 330 core
            in vec2 TexCoords;
            out vec4 FragColor;
            uniform sampler2D texture1;
            void main()
            {
                FragColor = texture(texture1, TexCoords);
            }
        ";

            // 创建和编译着色器
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            // 链接着色器程序
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            // 删除着色器
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            GL.UseProgram(program);
            GL.Uniform1i(GL.GetUniformLocation(program, "texture1"), 0); // 绑定纹理单元 0
            return program;
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            // 检查编译错误
            GL.GetShaderi(shader, ShaderParameterName.CompileStatus, out int success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(shader, out string info);
                throw new Exception($"{type} compilation failed: {info}");
            }

            return shader;
        }

        private static uint[] GenerateRandomColors(int width, int height)
        {
            uint[] colors = new uint[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                byte r = (byte)_random.Next(0, 256);
                byte g = (byte)_random.Next(0, 256);
                byte b = (byte)_random.Next(0, 256);
                byte a = (byte)_random.Next(0, 256);
                colors[i] = (uint)(r << 24 | g << 16 | b << 8 | a); // RGBA 格式
            }
            return colors;
        }

        private static void UpdateTexture(uint[] colors)
        {
            GL.BindTexture(TextureTarget.Texture2d, _texture);
            GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, colors);
        }
    }
}
