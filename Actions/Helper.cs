using DotNet.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Transactions;

namespace DotNet.Actions
{
    public static class Helper
    {

        public static void INIT()
        {
            UtilityToInt.Add("Mall", 1);
            UtilityToInt.Add("Park", 2);
            UtilityToInt.Add("WindTurbine", 4);
        }
        public static Dictionary<string, int> UtilityToInt = new Dictionary<string, int>();
        public static bool InGrid(int x, int y, GameState state)
        {
            return x >= 0 && y >= 0 && state.Map.Length > x && state.Map[x].Length > y;
        }
        public static bool CanBuild(int x, int y, GameState state)
        {
            if (!InGrid(x, y, state)) return false;
            if (state.Map[x][y] == 1) return false;
            if (state.ResidenceBuildings.Any(t => t.Position.x == x && t.Position.y == y) || state.UtilityBuildings.Any(t => t.Position.x == x && t.Position.y == y)) return false;
            return true;
        }
        public static int Distance(Position a, Position b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
        public static int[,] GetUtilitiyGrid(GameState state)
        {
            int[,] arr = new int[state.Map.Length, state.Map[0].Length];
            foreach (var building in state.UtilityBuildings)
            {
                if (building.BuildingName == "Mall")
                {
                    for (int i = 0; i < arr.GetLength(0); i++)
                        for (int j = 0; j < arr.GetLength(1); j++)
                        {
                            if (Math.Abs(building.Position.x - i) + Math.Abs(building.Position.y - j) <= 3)
                                arr[i, j] |= 1;
                            if (Math.Abs(building.Position.x - i) + Math.Abs(building.Position.y - j) <= 1)
                                arr[i, j] |= 8;
                        }
                }
                if (building.BuildingName == "Park")
                {
                    for (int i = 0; i < arr.GetLength(0); i++)
                        for (int j = 0; j < arr.GetLength(1); j++)
                            if (Math.Abs(building.Position.x - i) + Math.Abs(building.Position.y - j) <= 2)
                                arr[i, j] |= 2;
                }
                if (building.BuildingName == "WindTurbine")
                {
                    for (int i = 0; i < arr.GetLength(0); i++)
                        for (int j = 0; j < arr.GetLength(1); j++)
                            if (Math.Abs(building.Position.x - i) + Math.Abs(building.Position.y - j) <= 2)
                                arr[i, j] |= 4;
                }
            }
            return arr;
        }
        public static double GetEnergyConsumation(GameLayer layer)
        {
            double r = 0;
            foreach (var b in layer.GetState().ResidenceBuildings)
            {
                r += Math.Max(0, b.RequestedEnergyIn - (b.Effects.Contains("SolarPanel") ? 3.4 : 0) - (b.Effects.Contains("WindTurbine") ? 3.4 : 0) + (b.Effects.Contains("Charger") ? 1.8 : 0));
            }
            foreach (var b in layer.GetState().UtilityBuildings)
            {
                r += Math.Max(0, b.EffectiveEnergyIn - (b.Effects.Contains("SolarPanel") ? 3.4 : 0) - (b.Effects.Contains("WindTurbine") ? 3.4 : 0));
            }
            return r;
        }
        public static EnergyLevel GetEnergyLevel(GameLayer layer)
        {
            return GetEnergyLevel(layer, GetEnergyConsumation(layer));
        }
        public static EnergyLevel GetEnergyLevel(GameLayer layer, double energyConsumation)
        {
            var levels = layer.GetState().EnergyLevels;
            for (int i = levels.Count - 1; i >= 0; i--)
            {
                if (levels[i].EnergyThreshold <= energyConsumation) return levels[i];
            }
            return levels[0];
        }
        public static int GetCurrentMaxPop(GameLayer layer)
        {
            int ret = 0;
            foreach (var build in layer.GetState().ResidenceBuildings)
            {
                ret += layer.GetResidenceBlueprint(build.BuildingName).MaxPop;
            }
            return ret;
        }
        public static int GetPossibleMorePop(GameLayer layer, int spacesToRemove)
        {
            int ret = 0;
            var state = layer.GetState();
            for (int i = 0; i < state.Map.GetLength(0); i++)
            {
                for (int j = 0;j < state.Map[i].Length; j++)
                {
                    if(state.Map[i][j] == 0 && !state.ResidenceBuildings.Any(t => t.Position.x == i && t.Position.y == j) && !state.UtilityBuildings.Any(t => t.Position.x == i && t.Position.y == j))
                    {
                        ret += state.AvailableResidenceBuildings.Where(t => t.ReleaseTick < 400 || state.Turn >= t.ReleaseTick).Max(t => t.MaxPop);
                    }
                }
            }
            ret -= spacesToRemove * state.AvailableResidenceBuildings.Where(t => t.ReleaseTick < 400 || state.Turn >= t.ReleaseTick).Max(t => t.MaxPop);
            return ret;
        }
        public static double GetIncome(GameLayer layer)
        {
            double income = 0;
            var el = GetEnergyLevel(layer);
            var arr = GetUtilitiyGrid(layer.GetState());
            foreach (var building in layer.GetState().ResidenceBuildings)
            {
                income -= el.CostPerMwh * 
                    Math.Max(0, 
                        building.RequestedEnergyIn -
                        (building.Effects.Contains("WindTurbine") ? 3.4 : 0) - 
                        (building.Effects.Contains("SolarPanel") ? 3.4 : 0) + 
                        (building.Effects.Contains("Charger") ? 1.8 : 0));
                income += layer.GetResidenceBlueprint(building.BuildingName).IncomePerPop * layer.GetResidenceBlueprint(building.BuildingName).MaxPop;
                income -= (building.Effects.Contains("Caretaker") ? 6 : 0);
            }
            int mallcnt = 0;
            foreach (var util in layer.GetState().UtilityBuildings)
            {
                if(util.BuildingName == "Mall")
                {
                    income += 240 * Math.Pow(0.5, mallcnt);
                    mallcnt++;
                    income -= el.CostPerMwh *
                        Math.Max(0,
                                util.EffectiveEnergyIn -
                                (util.Effects.Contains("WindTurbine") ? 3.4 : 0) -
                                (util.Effects.Contains("SolarPanel") ? 3.4 : 0) +
                                (util.Effects.Contains("Charger") ? 1.8 : 0));
                }
                else if(util.BuildingName == "Park")
                {
                    income -= el.CostPerMwh *
                        Math.Max(0,
                            util.EffectiveEnergyIn -
                            (util.Effects.Contains("WindTurbine") ? 3.4 : 0) -
                            (util.Effects.Contains("SolarPanel") ? 3.4 : 0) +
                            (util.Effects.Contains("Charger") ? 1.8 : 0));
                }
            }

            return income;
        }
    }
}
