using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotNet.Actions
{
    public abstract class Turn
    {
        protected string _gameId;
        public Turn(string gameId)
        {
            this._gameId = gameId;
        }
        public abstract Turn TakeTurn(GameLayer gameLayer);
    }
}
