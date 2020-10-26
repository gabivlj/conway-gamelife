
public enum CellType
{
    ALIVE,
    DEAD,
}

public struct Cell
{
    public CellType type;

    public Cell(CellType type)
    {
        this.type = type;
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
        width = 256;
        height = 256;
        cells = new Cell[width, height];
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                cells[i, j] = new Cell(CellType.ALIVE);
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
        view.SetResolution(100f, width, height);
    }

    public void Update()
    {

    }

    #endregion
}