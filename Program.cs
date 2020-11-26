using System;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace testconway
{
    class Program
    {
        public static bool DebugPerf = false;
        public const string FILE_PATH = "./data.txt";

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            Console.WriteLine("Number Of Processors: {0}", Environment.ProcessorCount);
            Console.WriteLine("Instructions: \n- Movement with wasd.\n- '+' To add one thread.\n- '-' To remove a thread.\n- 'r' to Reset.\n- Scroll to zoom\n- Txt files are loaded from the bin/Debug or bin/Release folders\n- Press space to activate/deactivate debugging of time");
 
            using (var window = new Window())
            {
                window.Run();
            }
        }

    }

    class Window : GameWindow, IView
    {
        #region Private variables
        private Board board;
        private float resolution = 76.0f;
        private int margin = 12;
        private float offsetX = 0.0f;
        private float offsetY = 0.0f;

        #endregion

        public Window()
            : base(600, 600)
        {
            //VSync = VSyncMode.On;
            MouseWheel += new EventHandler<MouseWheelEventArgs>(OnMouseWheels);

        }

        #region Draw interface

        public void SetResolution(float res = -1)
        {
            resolution = res == -1 ? resolution : res;
            DrawGrid(board.Cells);
        }

        public void DrawGrid(Cell[,] cells)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-resolution + offsetX, resolution + offsetX, -resolution + offsetY, resolution + offsetY, -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            DrawGridGL(cells);
            SwapBuffers();            
        }

        /// <summary>
        /// Updates the grid view from the board
        /// </summary>
        private void DrawGridGL(Cell[,] cells)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            int ySize = 0;
            for (int i = cells.GetLength(0)-1; i >= 0; i--)
            {
                int xSize = 0;
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    GL.Begin(PrimitiveType.Polygon);
                    GL.Color3(cells[i,j].ToColor());
                    GL.Vertex3(-resolution + xSize, -resolution + ySize, 0.0);
                    GL.Vertex3(-resolution + 10 + xSize, -resolution + ySize, 0.0);
                    GL.Vertex3(-resolution + 10 + xSize, -resolution + 10 + ySize, 0.0);
                    GL.Vertex3(-resolution + xSize, -resolution + 10 + ySize, 0.0);
                    xSize += margin;
                    GL.End();
                }
                ySize += margin;
            }
            GL.Flush();
        }

        #endregion

        #region OpenGL LifeCycle

        const int speed = 10;
        protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (e.KeyChar == 'r' || e.KeyChar == 'R')
            {
                Console.WriteLine("Reseting...");
                board.Reset();
                return;
            }
            if (e.KeyChar == '+')
            {
                board.AddThread();
                return;
            }
            if (e.KeyChar == '-')
            {
                board.RemoveThread();
                return;
            }
            if (e.KeyChar == ' ')
            {
                Program.DebugPerf = !Program.DebugPerf;
                return;
            }
            if (e.KeyChar == 'D' || e.KeyChar == 'd')
                offsetX += speed;
            if (e.KeyChar == 'A' || e.KeyChar == 'a')
                offsetX -= speed;
            if (e.KeyChar == 'W' || e.KeyChar == 'w')
                offsetY += speed;
            if (e.KeyChar == 'S' || e.KeyChar == 's')
                offsetY -= speed;            
            SetResolution(resolution);
        }

        private void OnKeyboardEvents(object sender, KeyboardKeyEventArgs args)
        {

        }

        private void OnMouseWheels(object sender, MouseWheelEventArgs args)
        {
            
            SetResolution(resolution + args.Delta);
        }        

        protected override void OnLoad(System.EventArgs e)
        {
            GL.ClearColor(0.1f, 0.2f, 0.3f, 1f);            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (board == null)
            {
                board = new Board(this, Program.FILE_PATH);
                board.Start();
            }
            board.Update();
        }

        

        #endregion

    }
}
