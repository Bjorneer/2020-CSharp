using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;

namespace DotNet.Actions
{
    public class MaintananceTurn : Turn
    {
        int x, y;
        public MaintananceTurn(int x, int y, string gameId) 
            : base(gameId)
        {
            this.x = x;
            this.y = y;
        }

        public static Turn GetBestMaintanance(GameLayer gameLayer)
        {
            var state = gameLayer.GetState();
            int health = 100;
            Position pos = null;
            foreach (var building in state.ResidenceBuildings)
            {
                if(building.Health < health && state.Funds >= gameLayer.GetResidenceBlueprint(building.BuildingName).MaintenanceCost)
                {
                    pos = building.Position;
                    health = building.Health;
                }
            }
            return health > Constants.LOW_HEALTH + 3 ? null: new MaintananceTurn(pos.x, pos.y, state.GameId);
        }

        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.Maintenance(new models.Position(x, y), _gameId);
            return null;
        }
    }
}
