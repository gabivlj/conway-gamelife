using System;
using System.Threading;

namespace testconway
{
    public class Game
    {

        #region Private
        private volatile Board board;
        private TaskManager manager;

        /// <summary>
        /// How many threads do you want
        /// </summary>
        const int N_WORKERS = 16;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a Game class which will be in charge of initializing game turns in a multithreaded way.
        ///
        /// </summary>
        /// <param name="board"></param>
        public Game(Board board)
        {
            this.board = board;
            manager = new TaskManager(N_WORKERS);            
        }

        /// <summary>
        /// Starts a new turn async and will call board.SafeUpdateCells with the new cells when it finishes
        /// </summary>
        /// <param name="sleepTime"></param>
        public void DoTurn(int sleepTime)
        {
            manager.Do((_) =>
            {
                CellAction.CallCellActions(board, manager);
                Cell[,] newCells = CellAction.WaitNewGameState();
                //Thread.Sleep(sleepTime);
                board.SafeUpdateCells(newCells);
            });
            
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

        private void FuncGameLogic(object cellAct)
        {
            CellAction cellAction = cellAct as CellAction;
            cellsSet[cellAction.y, cellAction.x].type = cellAction.
                cellsCompare[cellAction.y, cellAction.x].type == CellType.Alive ? CellType.Dead : CellType.Alive;

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
