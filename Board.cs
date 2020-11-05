
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

    public Board(IView view, string configPath = "./data.txt")
    {
        Config configData = new Config(configPath);
        width  = configData.Width;
        height = configData.Height;
        cells  = configData.LoadCells();
        game   = new Game(this);
        this.view = view;
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
    /// TODO Initialize threads and run the game there, also send messages here to draw grid
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
        view.DrawGrid(cells);
        game.DoTurn(100);
        cellsUpdated = false;
    }

    #endregion
}