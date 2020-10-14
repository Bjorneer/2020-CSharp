using DotNet.models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNet.Actions
{
    class AdjustEnergyTurn : Turn
    {
        int _x, _y;
        double en;
        public AdjustEnergyTurn(int x, int y, double energy, string gameId) : base(gameId)
        {
            _x = x;
            _y = y;
            en = energy;
        }

        private static double GetNextTemp(BuiltResidenceBuilding building, GameLayer gameLayer)
        {
            return
                building.Temperature +
                (building.EffectiveEnergyIn - gameLayer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed) * Constants.DEGREES_PER_EXCESS_MWH +
                Constants.DEGREES_PER_POP * building.CurrentPop -
                (building.Temperature - gameLayer.GetState().CurrentTemp) * (gameLayer.GetResidenceBlueprint(building.BuildingName).Emissivity * (building.Effects.Contains("Insulation") ? 0.6 : 1));
        }
        //newTemp = indoorTemp + (effectiveEnergyIn - baseEnergyNeed) * degreesPerExcessMwh + degreesPerPop * currentPop - (indoorTemp - outdoorTemp) * emissivity
        private static double OptimalEnergy(double targetIndoorTemp, BuiltResidenceBuilding building, GameLayer layer, double outTemp)
        {
            var state = layer.GetState();
            double A = (state.MaxTemp - state.MinTemp) / 2.0;
            double B = Math.PI * 2 / 183.0;
            double C = A + state.MinTemp;
            double baseEnergy = layer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed + (building.Effects.Contains("Charger") ? 1.8 : 0);
            double ret = 
                (((targetIndoorTemp - outTemp) * (layer.GetResidenceBlueprint(building.BuildingName).Emissivity * (building.Effects.Contains("Insulation") ? 0.6 : 1))) - 
                Constants.DEGREES_PER_POP * building.CurrentPop) / Constants.DEGREES_PER_EXCESS_MWH + 
                baseEnergy;
            return ret;
        }
        private static double GetNextEnergy(BuiltResidenceBuilding building, GameLayer layer)
        {
            var state = layer.GetState();
            double A = (state.MaxTemp - state.MinTemp) / 2.0;
            double B = Math.PI * 2 / 183.0;
            double C = A + state.MinTemp;
            double minEnergy = layer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed + ((building.Effects.Contains("Charger") ? 1.8 : 0));
            if((((state.Turn + 10) % 193) < 87))
            {
                return OptimalEnergy(22.5, building, layer, Math.Max(state.CurrentTemp, state.MinTemp));
            }
            else
            {
                return OptimalEnergy(19.5, building, layer, Math.Min(state.CurrentTemp, state.MaxTemp));
            }
        }

        public static Turn GetBestEnergyAdjustment(GameLayer gameLayer, double deviation)
        {
            var state = gameLayer.GetState();
            if (state.Funds < 150) return null;
            List<BuiltResidenceBuilding> list = new List<BuiltResidenceBuilding>(state.ResidenceBuildings);
            list.Sort((a, b) => {
                if (Math.Abs(a.Temperature - 21) > Math.Abs(b.Temperature - 21)) return -1;
                return 1;
            });
            foreach (var building in list)
            {
                if (building.BuildProgress != 100) continue;
                if (building.Temperature > 21 + deviation || building.Temperature < 21 - deviation)
                {
                    double temp = building.Temperature;
                    double nextTemp = GetNextTemp(building, gameLayer);
                    //if (Math.Abs(nextTemp - 21) < Math.Abs(temp - 21)) continue;
                    double newEnergy = GetNextEnergy(building, gameLayer);
                    //double newEnergy = 
                    //    Math.Max(gameLayer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed, 
                    //        ((21 - temp) * gameLayer.GetResidenceBlueprint(building.BuildingName).Emissivity - 
                    //        Constants.DEGREES_PER_POP * gameLayer.GetResidenceBlueprint(building.BuildingName).MaxPop) / Constants.DEGREES_PER_EXCESS_MWH +
                    //        gameLayer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed);
                    if(newEnergy < gameLayer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed + (building.Effects.Contains("Charger") ? 1.8 : 0))
                    {
                        newEnergy = gameLayer.GetResidenceBlueprint(building.BuildingName).BaseEnergyNeed + (building.Effects.Contains("Charger") ? 1.8 : 0) + 0.01;
                        Program.StackedTurns.Push(new AdjustEnergyTurn(building.Position.x, building.Position.y, newEnergy, state.GameId));
                        if (!building.Effects.Contains("Regulator") && state.Funds > 1250)
                        {
                            return new UpgradeTurn(building.Position.x, building.Position.y, "Regulator", state.GameId);
                        }
                        continue;
                    }
                    if (Math.Abs(newEnergy - building.RequestedEnergyIn) < 0.3)
                        continue;
                    return new AdjustEnergyTurn(building.Position.x, building.Position.y, newEnergy, state.GameId);
                }
            }
            return null;
        }

        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.AdjustEnergy(new models.Position(_x, _y), en, _gameId);
            return null;
        }
    }
}
