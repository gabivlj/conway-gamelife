﻿using System;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace testconway
{
    class Program
    {
        static string l = "";
        static double st = 0;


        static void Main(string[] args)
        {
            //Console.Clear();
            const double nIters = 100000;
            var taskManager = new TaskManager(16, 1000);
            taskManager.Do(() =>
            {
                for (int i = 1; i <= nIters; i++)
                {
                    int capture = i;
                    taskManager.Do(() =>
                    {
                        int capt2 = capture;
                        //Thread.Sleep(i);
                        lock(l)
                        {
                            st += capt2;
                        }
                    });
                }
            });
            Console.WriteLine("start check...");
            while (st != 5000050000)
            {
                Console.WriteLine($"mmm {st}");
                Thread.Sleep(150);                
            }
            Console.WriteLine($"finished {st}");
            taskManager.Finish();

            //using (var window = new Window())
            //{
            //    window.Run();
            //}
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

        public void SetResolution(float res)
        {
            resolution = res;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-resolution + offsetX, resolution + offsetX, -resolution + offsetY, resolution + offsetY, -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            DrawGrid(board.Cells);
            SwapBuffers();
        }

        /// <summary>
        /// Updates the grid view
        /// </summary>
        public void DrawGrid(Cell[,] cells)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            
            int ySize = 0;
            for (int i = 0; i < cells.GetLength(0); i++)
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
            if (e.KeyChar == 'D' || e.KeyChar == 'd')
                offsetX += speed;
            if (e.KeyChar == 'A' || e.KeyChar == 'a')
                offsetX -= speed;
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
            board = new Board(this);
            board.Start();            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {            
            board.Update();
        }

        

        #endregion

    }
}
