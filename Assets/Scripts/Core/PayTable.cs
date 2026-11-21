using System;
using UnityEngine;
using SlotGame.Symbols;

namespace SlotGame.Core
{
    [Serializable]
    public class WinResult
    {
        public bool isWin;
        public int matchedCount;
        public SymbolType symbol;
        public int payout;
        public bool isJackpot;
        public int scatterCount;
        public bool bonusTriggered;
    }

    public static class PayTable
    {
        public static int PayoutFor(SymbolData s, int count)
        {
            if (s == null) return 0;
            return count switch
            {
                3 => s.payout3,
                4 => s.payout4,
                5 => s.payout5,
                _ => 0
            };
        }

        public static WinResult Evaluate(SymbolData[][] grid, int bet, int freeSpinMultiplier = 1)
        {
            var result = new WinResult();
            if (grid == null || grid.Length == 0) return result;

            int reels = grid.Length;
            int rows = grid[0].Length;

            int scatters = 0;
            foreach (var reel in grid)
                foreach (var sym in reel)
                    if (sym != null && sym.isScatter) scatters++;
            result.scatterCount = scatters;
            result.bonusTriggered = scatters >= 3;

            bool allSame = true;
            SymbolData first = null;
            foreach (var reel in grid)
                foreach (var sym in reel)
                {
                    if (sym != null && sym.isWild) continue;
                    if (first == null) { first = sym; continue; }
                    if (first != null && sym.type != first.type) { allSame = false; break; }
                }
            if (allSame && first != null)
            {
                result.isJackpot = true;
                result.isWin = true;
                result.symbol = first.type;
                result.matchedCount = reels * rows;
                result.payout = bet * 50 * freeSpinMultiplier;
                return result;
            }

            int mid = rows / 2;
            var line = new SymbolData[reels];
            for (int r = 0; r < reels; r++) line[r] = grid[r][mid];

            SymbolData lineSymbol = null;
            foreach (var s in line)
            {
                if (s != null && !s.isWild) { lineSymbol = s; break; }
            }
            if (lineSymbol == null && line.Length > 0) lineSymbol = line[0];

            int match = 0;
            foreach (var s in line)
            {
                if (s == null) break;
                if (s.isWild || (lineSymbol != null && s.type == lineSymbol.type)) match++;
                else break;
            }

            if (match >= 3)
            {
                result.isWin = true;
                result.symbol = lineSymbol != null ? lineSymbol.type : SymbolType.Wild;
                result.matchedCount = match;
                result.payout = bet * PayoutFor(lineSymbol, match) * freeSpinMultiplier;
            }
            return result;
        }
    }
}