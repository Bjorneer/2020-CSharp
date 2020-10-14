using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Actions
{
    public class DemolishTurn : Turn
    {
        int x, y;
        public DemolishTurn(int x, int y, string gameId) : base(gameId)
        {
            this.x = x;
            this.y = y;
        }
        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.Demolish(new models.Position(x, y), _gameId);
            return null;
        }
    }
}
