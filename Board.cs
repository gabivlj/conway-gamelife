
public enum CellType
{
    ALIVE,
    DEAD,
}



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
        if (type == CellType.ALIVE)
        {
            return Blue;
        }
        return Red;
    }
}

public class Board
{
    #region Private

    private int width;
    private int height;
    private Cell[,] cells;
    private IView view;

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

    public Board(IView view, string config = "/")
    {
        width = 30;
        height = 30;
        cells = new Cell[width, height];
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                if (i % 2 == 0) 
                    cells[i, j] = new Cell(CellType.ALIVE);
                else cells[i, j] = new Cell(CellType.DEAD);
            }
                
        }
        this.view = view;
    }

    public Cell[,] BoardCopy()
    { 
        return cells.Clone() as Cell[,];
    }

    public void Start()
    {
        view.DrawGrid(cells);
        view.SetResolution(100f);
    }

    public void Update()
    {

    }

    #endregion
}