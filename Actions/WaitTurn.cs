using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Actions
{
    public class WaitTurn : Turn
    {
        Turn _nextTurn;
        public WaitTurn(Turn nextTurn, string gameId) : base(gameId)
        {
            _nextTurn = nextTurn;
        }
        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.Wait(_gameId);
            if (_nextTurn != null) Program.StackedTurns.Push(_nextTurn);
            return null;
        }
    }
}
