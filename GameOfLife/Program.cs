using System;

namespace GameOfLife
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            GameOfLife sim = new GameOfLife(20, 30);
            sim.GameRun();
        }
    }
}
