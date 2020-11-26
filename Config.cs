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
            try
            {
                lines = File.ReadAllLines(path);
                if (lines.Length == 0 || !FillWidthHeight(lines[0]))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"[ERROR]: Error loading file at {path}... Did you remember to put your file?\n** Maybe the format isn't quite right... Check the examples!\nUsing empty grid...");
                    width = 1;
                    height = 1;
                }
            }
            catch (Exception)
            {
                width = -1;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[ERROR]: Error loading file at {path}... Did you remember to put your file?\nUsing empty grid...");
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
