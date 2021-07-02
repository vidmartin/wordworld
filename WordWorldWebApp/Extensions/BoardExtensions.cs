using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordWorldWebApp.DataStructures;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Extensions
{
    public static class BoardExtensions
    {
        public static async Task<string> ScanAsync(this Board board, Rect scanArea)
        {
            var stringBuilder = new StringBuilder();

            await board.DoAsync(() =>
            {
                for (int y = 0; y < scanArea.w; y++)
                {
                    for (int x = 0; x < scanArea.w; x++)
                    {
                        stringBuilder.Append(board.At((x + scanArea.x, y + scanArea.y)));
                    }
                }
            });

            return stringBuilder.ToString();
        }
    }
}
