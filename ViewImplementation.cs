using System;

public interface IView
{
    public void DrawGrid(Cell[,] cells);
    public void SetResolution(float res, int width = -1, int height = -1);
}