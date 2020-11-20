
using System;
using testconway;


public enum CellType
{
    Alive,
    Dead,
}

/// <summary>
/// Cell data structure which holds all the information that the View and Game logic needs
/// </summary>
public struct Cell
{
    public CellType type;
    public static readonly byte[] Red = { 255, 0, 0 };
    public static readonly byte[] Blue = { 0, 255, 0 };

    public Cell(CellType type)
    {
        this.type = type;
    }

    public byte[] ToColor()
    {
        return type == CellType.Alive ? Blue : Red;
    }
}

public class Board
{
    #region Private

    private volatile bool cellsUpdated = false;
    private int width;
    private int height;
    private Cell[,] cells;
    private IView view;
    private Game game;
    private bool reset = false;
    private string path;

    #endregion

    #region Properties

    public Cell[,] Cells
    {
        get
        {
            return cells;
        }
    }

    #endregion

    #region Public

    public void AddThread()
    {        
        game.NumberOfWorkers = Math.Min(64, game.NumberOfWorkers + 1);
        Console.WriteLine($"current threads: {game.NumberOfWorkers}");
    }

    public void RemoveThread()
    {
        game.NumberOfWorkers = Math.Max(1, game.NumberOfWorkers - 1);
        Console.WriteLine($"current threads: {game.NumberOfWorkers}");
    }

    public Board(IView view, string configPath = "./data.txt")
    {
        path = configPath;
        LoadConfig(path);
        game   = new Game(this);
        this.view = view;
    }

    private void LoadConfig(string configPath)
    {
        Config configData = new Config(configPath);
        width = configData.Width;
        height = configData.Height;
        cells = configData.LoadCells();
    }
    
    public void SafeUpdateCells(Cell[,] cells)
    {
        this.cells = cells;
        cellsUpdated = true;
    }

    public Cell[,] BoardCopy()
    { 
        return cells.Clone() as Cell[,];
    }

    /// <summary>
    /// On Start draw
    /// </summary>
    public void Start()
    {
        view.DrawGrid(cells);
        view.SetResolution(100f);

        game.DoTurn(500);
    }

    /// <summary>
    /// It's gonna be called by the view each frame
    /// </summary>
    public void Update()
    {        
        if (!cellsUpdated)
        {            
            return;
        }
        if (reset)
        {
            LoadConfig(path);
            reset = false;
            view.DrawGrid(cells);
            game.DoTurn(1000);
            cellsUpdated = false;
            return;
        }        
        game.DoTurn(300);
        view.DrawGrid(cells);
        cellsUpdated = false;
    }

    public void Reset()
    {
        reset = true;
    }

    #endregion
}