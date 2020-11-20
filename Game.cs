using System;
using System.Collections.Generic;
using System.Threading;

namespace testconway
{
    public class Game
    {

        #region Properties

        public int NumberOfWorkers
        {
            set
            {
                numberOfWorkers = value;
                newWorkers = true;
            }

            get => numberOfWorkers;
        }
        #endregion

        #region Private
        private Board board;
        private TaskManager manager;

        /// <summary>
        /// How many threads do you want
        /// </summary>
        private int numberOfWorkers = 8;

        private bool newWorkers = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a Game class which will be in charge of initializing game turns in a multithreaded way.
        ///
        ///
        /// </summary>
        /// <param name="board"></param>
        public Game(Board board)
        {
            this.board = board;
            manager = new TaskManager(numberOfWorkers);            
        }

        /// <summary>
        /// Starts a new turn async and will call board.SafeUpdateCells with the new cells when it finishes
        /// </summary>
        /// <param name="sleepTime"></param>
        public void DoTurn(int sleepTime)
        {
            if (newWorkers)
            {
                manager.Finish();
                manager = new TaskManager(numberOfWorkers);
                newWorkers = false;
            }
            new Thread((_) =>
            {
                Thread.Sleep(sleepTime);
                CellAction.CallCellActions(board, manager);
                Cell[,] newCells = CellAction.WaitNewGameState();                
                board.SafeUpdateCells(newCells);
            }).Start();            
        }

        #endregion
    }


    class CellAction
    {



        #region Private Variables

        private Cell[,] cellsCompare;
        private int y = 0;
        private int x = 0;

        static private volatile Cell[,] cellsSet;
        static private Semaphore cellResources;
        static private int size;

        #endregion

        #region Internal Local Logic 

        private CellAction(Cell[,] cellsCompare, int i, int j, TaskManager manager)
        {
            this.cellsCompare = cellsCompare;
            this.y = i;
            this.x = j;
            manager.Do(FuncGameLogic, this);
        }

        private List<Cell> GetAliveNeighbors(int i, int j)
        {
            List<Cell> neigh = new List<Cell>(8);
            int maxCol = Math.Min(cellsCompare.GetLength(1), j + 2);
            int minCol = Math.Max(0, j - 1);
            int maxRow = Math.Min(cellsCompare.GetLength(0), i + 2);
            int minRow = Math.Max(0, i - 1);
            for (int row = minRow; row < maxRow; ++row)
            {
                for (int col = minCol; col < maxCol; ++col)
                {
                    if (row == i && col == j) continue;
                    Cell toAdd = cellsCompare[row, col];
                    if (toAdd.type == CellType.Alive) neigh.Add(toAdd);
                }
            }

            if (neigh.Count == 9) Console.WriteLine("bad things are happening: count is 9");
            return neigh;
        }


        private void FuncGameLogic(object cellAct)
        {
            CellAction cellAction = cellAct as CellAction;
            Cell cell = cellsCompare[cellAction.y, cellAction.x];
            Cell cellSet = cellsCompare[cellAction.y, cellAction.x];
            int numberOfAliveNeighbors = GetAliveNeighbors(cellAction.y, cellAction.x).Count;
            if (cell.type == CellType.Dead && numberOfAliveNeighbors == 3)
            {                
                cellSet.type = CellType.Alive;                
            }
            else if (numberOfAliveNeighbors < 2 || numberOfAliveNeighbors > 3)
            {                
                cellSet.type = CellType.Dead;
            }
            cellsSet[cellAction.y, cellAction.x] = cellSet;
            cellResources.Release();
        }

        #endregion


        #region Public API for Multithreading

        /// <summary>
        /// Does a conway turn in a multithreaded manner
        /// </summary>
        /// <param name="board"></param>
        /// <param name="taskManager"></param>
        /// <returns></returns>
        public static CellAction[] CallCellActions(Board board, TaskManager taskManager)
        {
            Cell[,] cellsOriginal = board.Cells;
            cellsSet = board.BoardCopy();
            size = board.Cells.GetLength(0) * board.Cells.GetLength(1);
            cellResources = new Semaphore(0, size);
            CellAction[] actions = new CellAction[size];

            for (int i = 0, y = 0; i < size; y++)
            {
                for (int x = 0; i < size && x < board.Cells.GetLength(1); ++x, ++i)
                {
                    actions[i] = new CellAction(cellsOriginal, y, x, taskManager);
                }
            }

            return actions;
        }


        /// <summary>
        /// Waits all the cell actions to finish and returns the new game state
        /// </summary>
        /// <returns></returns>
        public static Cell[,] WaitNewGameState()
        {           
            for (int j = 0; j < size; j++)
            {
                cellResources.WaitOne();               
            }
            size = 0;
            return cellsSet;
        }

        #endregion
    }

}
