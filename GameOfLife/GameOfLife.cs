using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class GameOfLife
    {
        int nRow;
        int nCol;
        bool[,] currentGen;
        bool[,] nextGen;
        Task processTask;

        bool runSimulation = true;
        int n = 0;

        public GameOfLife(int row, int col)
        {
            nRow = row;
            nCol = col;
            currentGen = new bool[row, col];
            nextGen = new bool[row, col];
        }

        public void GameRun()
        {
            InitGame();
            Console.CancelKeyPress += (sender, args) =>
            {
                runSimulation = false;
                Console.WriteLine("\n👋 Ending simulation.");
            };


            // let's give our console
            // a good scrubbing

            Console.Clear();
            // Displaying the grid 
            while (runSimulation)
            {
                Console.Clear();
                Print();
                BeginGeneration();
                Wait();
                Update();
                Wait();
                Interlocked.Increment(ref n);
                Console.Clear();
            }
            GC.Collect();
        }

        private void InitGame()
        {
            var random = new Random();
            for (var row = 0; row < nRow; row++)
            {
                for (var col = 0; col < nCol; col++)
                {
                    currentGen[row, col] = random.Next(2) == 1;
                }
            }
        }

        public void Update()
        {
            if (this.processTask != null && this.processTask.IsCompleted)
            {
                // when a generation has completed
                this.currentGen = this.nextGen;
            }
        }

        public void BeginGeneration()
        {
            if (this.processTask == null || (this.processTask != null && this.processTask.IsCompleted))
            {
                // only begin the generation if the previous process was completed
                this.processTask = this.ProcessGeneration();
            }
        }

        public void Wait()
        {
            if (this.processTask != null)
            {
                this.processTask.Wait();
            }
        }


        private Task ProcessGeneration()
        {
            return Task.Factory.StartNew(() =>
            {
                Parallel.For(0, nRow, x =>
                {
                    for (int y = 0; y < nCol; y++)
                    {
                        int numberOfNeighbors = 0;

                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                if (i == 0 && j == 0) continue;
                                numberOfNeighbors += GetNeighborAlive(currentGen, x, y, i, j);
                            }


                        bool shouldLive = false;
                        bool isAlive = currentGen[x, y];

                        if (isAlive && (numberOfNeighbors == 2 || numberOfNeighbors == 3))
                        {
                            shouldLive = true;
                        }
                        else if (!isAlive && numberOfNeighbors == 3) // zombification
                        {
                            shouldLive = true;
                        }

                        nextGen[x, y] = shouldLive;
                    }
                });
            });
        }


        private int GetNeighborAlive(bool[,] current, int x, int y, int offsetx, int offsety)
        {
            int result = 0;

            int proposedOffsetX = x + offsetx;
            int proposedOffsetY = y + offsety;
            bool outOfBounds = proposedOffsetX < 0 || proposedOffsetX >= nRow | proposedOffsetY < 0 || proposedOffsetY >= nCol;
            if (!outOfBounds)
            {
                result = current[x + offsetx, y + offsety] ? 1 : 0;
            }
            return result;
        }

        private void Print(int timeout = 500)
        {
            //Console.WriteLine("Generation " + n);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Generation " + n + "\n");
            for (var row = 0; row < nRow; row++)
            {
                for (var col = 0; col < nCol; col++)
                {
                    var cell = currentGen[row, col];
                    stringBuilder.Append(cell == true ? "*" : ".");
                }
                stringBuilder.Append("\n");
            }

            Console.BackgroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.Write(stringBuilder.ToString());
            Thread.Sleep(timeout);
        }
    }

}
