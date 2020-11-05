using System;
using System.IO;
using utilsconway;

namespace testconway
{
    public class Config
    {
        private int width  = 0;
        private int height = 0;

        public int Width
        {
            get => width;
        }

        public int Height
        {
            get => height;
        }

        private string[] lines;

        public Config(string path = "./data.txt")
        {
            lines = File.ReadAllLines(path);
            if (lines.Length == 0 || !FillWidthHeight(lines[0]))
            {
                throw new ApplicationException("bad file format: needed `width` and `height` parameters separated by coma on first line");
            }            
        }

        private bool FillWidthHeight(string line)
        {
            try
            {
                string[] numbersUnparsed = line.Split(',');
                if (numbersUnparsed.Length != 2)
                {
                    return false;
                }
                width  = int.Parse(numbersUnparsed[0].Trim());
                height = int.Parse(numbersUnparsed[1].Trim());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetLine(int y)
        {
            if (y + 1 >= lines.Length)
            {
                return Str.Repeat(" ", width);
            }
            return lines[y + 1];
        }

        public Cell[,] LoadCells()
        {
            Cell[,] cells = new Cell[height, width];
            for (int i = 0; i < height; i++)
            {
                string line = GetLine(i);
                for (int j = 0; j < width; j++)
                {
                    if (j >= line.Length || line[j] == ' ')
                    {
                        cells[i, j] = new Cell(CellType.Dead);
                        continue;
                    }
                    cells[i, j] = new Cell(CellType.Alive);
                }
            }
            return cells;
        }

    }
}
