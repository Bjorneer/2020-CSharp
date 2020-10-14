using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.Actions
{
    public class UpgradeTurn : Turn
    {
        int x, y;
        string upgrade;
        public UpgradeTurn(int x, int y, string s, string gameId) : base(gameId)
        {
            this.x = x;
            this.y = y;
            this.upgrade = s;
        }

        public static Turn GetBestUpgradeTurn(GameLayer gameLayer)
        {
            var state = gameLayer.GetState();
            foreach (var build in state.ResidenceBuildings)
            {
                if (build.BuildingName == "HighRise" && state.Funds > 10000 && !build.Effects.Contains("Caretaker")) 
                    return new UpgradeTurn(build.Position.x, build.Position.y, "Caretaker", state.GameId);
            }
            foreach (var b in state.ResidenceBuildings)
            {
                var x = gameLayer.GetResidenceBlueprint(b.BuildingName);
                if (b.EffectiveEnergyIn > 6 && !b.Effects.Contains("SolarPanel") && state.Funds > 20000) return new UpgradeTurn(b.Position.x, b.Position.y, "SolarPanel", state.GameId);
            }
            foreach (var b in state.ResidenceBuildings)
            {
                var x = gameLayer.GetResidenceBlueprint(b.BuildingName);
                if (b.Temperature > 29 && !b.Effects.Contains("Regulator") && state.Funds > 10000) return new UpgradeTurn(b.Position.x, b.Position.y, "Regulator", state.GameId);
            }
            foreach (var b in state.ResidenceBuildings)
            {
                var x = gameLayer.GetResidenceBlueprint(b.BuildingName);
                if (b.Temperature < 15 && !b.Effects.Contains("Insulation") && state.Funds > 30000) return new UpgradeTurn(b.Position.x, b.Position.y, "Insulation", state.GameId);
            }
            foreach (var b in state.ResidenceBuildings)
            {
                var x = gameLayer.GetResidenceBlueprint(b.BuildingName);
                if (!b.Effects.Contains("Playground") && state.Funds > 30000) return new UpgradeTurn(b.Position.x, b.Position.y, "Playground", state.GameId);
            }
            foreach (var b in state.ResidenceBuildings)
            {
                var x = gameLayer.GetResidenceBlueprint(b.BuildingName);
                if (b.Effects.Contains("Mall.2") && !b.Effects.Contains("Charger") && state.Funds > 50000) return new UpgradeTurn(b.Position.x, b.Position.y, "Charger", state.GameId);
            }
            return null;
        }

        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.BuyUpgrade(new models.Position(x, y), upgrade, _gameId);
            return null;
        }
    }
}
