using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotNet.Actions
{
    class BuildTurn : Turn
    {
        int _x;
        int _y;
        public BuildTurn(int x, int y, string gameId) 
            : base(gameId)
        {
            _x = x;
            _y = y;
        }
        public override Turn TakeTurn(GameLayer gameLayer)
        {
            var pos = new models.Position(_x, _y);
            gameLayer.Build(pos, _gameId);
            Turn turn = null;
            foreach (var building in gameLayer.GetState().ResidenceBuildings)
            {
                if(building.Position.ToString() ==  pos.ToString() && building.BuildProgress != 100)
                {
                    turn = new BuildTurn(_x, _y, _gameId);
                }
            }
            foreach (var building in gameLayer.GetState().UtilityBuildings)
            {
                if (building.Position.ToString() == pos.ToString() && building.BuildProgress != 100)
                {
                    turn = new BuildTurn(_x, _y, _gameId);
                }
            }
            return turn;
        }
    }
}
