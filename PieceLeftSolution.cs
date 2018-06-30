using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGameResolver
{
    internal class PieceLeftSolution
    {
        public int PieceLeft { get; set; }
        public List<List<string>> Solutions { get;set; }
        public int TotalSolutions;

        public PieceLeftSolution(int pieceLeft, int totalSolutions, List<List<string>> solutions)
        {
            PieceLeft = pieceLeft;
            TotalSolutions = totalSolutions;
            Solutions = solutions;
        }

        public override string ToString()
        {
            int solutionCount = Solutions.Count();
            return string.Format(
                "{0} ({1}/{2} = {3}%)",
                PieceLeft,
                solutionCount,
                TotalSolutions,
                Math.Round(((double)solutionCount *100/TotalSolutions), 2));
        }
    }
}
