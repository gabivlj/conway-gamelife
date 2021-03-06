﻿using System;
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
        ///
        /// </summary>
        private int numberOfWorkers = 3;

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
                // one more because we have to send the initial task and there is not a softblock
                manager = new TaskManager(numberOfWorkers + 1);
                newWorkers = false;
            }
            manager.Do((_) =>
            {
                var d = DateTime.Now;
                CellAction.CallCellActions(board, manager);
                Cell[,] newCells = CellAction.WaitNewGameState();
                TimeSpan span = DateTime.Now.Subtract(d);
                int msWaitTime = Math.Max(sleepTime - span.Milliseconds, 1);
                if (Program.DebugPerf)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Turn done in {span.Milliseconds}ms with {numberOfWorkers} threads\nWe'll wait: {msWaitTime}ms");
                    Console.ForegroundColor = ConsoleColor.Green;
                }                
                Thread.Sleep(msWaitTime);
                board.SafeUpdateCells(newCells);

            });          
        }

        #endregion
    }

    /// <summary>
    /// What CellAction does is:
    /// - Holding the callback and information that we'll send to the TaskManager.
    /// - Barrier wait until all cells are done processing.
    /// </summary>
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

        private int GetAliveNeighborsCount(int i, int j)
        {
            //List<Cell> neigh = new List<Cell>(8);
            int neigh = 0;
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
                    if (toAdd.type == CellType.Alive) neigh++;
                }
            }

            if (neigh == 9)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("bad things are happening: count is 9");
            }
            return neigh;
        }


        private void FuncGameLogic(object cellAct)
        {
            CellAction cellAction = cellAct as CellAction;
            Cell cell = cellsCompare[cellAction.y, cellAction.x];

            int numberOfAliveNeighbors = GetAliveNeighborsCount(cellAction.y, cellAction.x);
            if (cell.type == CellType.Dead && numberOfAliveNeighbors == 3)
            {
                cellsSet[cellAction.y, cellAction.x].type = CellType.Alive;                
            }
            else if (numberOfAliveNeighbors < 2 || numberOfAliveNeighbors > 3)
            {
                cellsSet[cellAction.y, cellAction.x].type = CellType.Dead;
            }
            //cellsSet[cellAction.y, cellAction.x] = cellSet;
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
