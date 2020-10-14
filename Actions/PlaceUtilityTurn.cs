using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotNet.Actions
{
    public class PlaceUtilityTurn : Turn
    {
        BlueprintUtilityBuilding bluePrint;
        public int x, y;
        public PlaceUtilityTurn(int x, int y, BlueprintUtilityBuilding bp, string gameId) : base(gameId)
        {
            bluePrint = bp;
            this.x = x;
            this.y = y;
        }
        private static Position GetBestPos(int[,] uarr, GameLayer gameLayer, int radius, string bname)
        {
            int bs = 0;
            Position pos = new Position(-1, -1);
            for (int i = 0; i < gameLayer.GetState().Map.Length; i++)
                for (int j = 0; j < gameLayer.GetState().Map[i].Length; j++)
                {
                    if(Helper.CanBuild(i, j, gameLayer.GetState()))
                    {
                        int s = 0;
                        foreach (var building in gameLayer.GetState().ResidenceBuildings)
                        {
                            if(Helper.Distance(new Position(i, j), building.Position) <= radius)
                            {
                                s++;
                            }
                            if(bname == "Mall")
                            {
                                if (Helper.Distance(new Position(i, j), building.Position) <= 1)
                                {
                                    s++;
                                }
                            }
                        }
                        if(bname == "WindTurbine")
                        {
                            foreach (var building in gameLayer.GetState().UtilityBuildings)
                            {
                                if (building.EffectiveEnergyIn == 0) continue;
                                if (Helper.Distance(new Position(i, j), building.Position) <= radius)
                                {
                                    s++;
                                }
                            }
                        }
                        if (s > bs)
                        {
                            bs = s;
                            pos.x = i;
                            pos.y = j;
                        }
                    }
                
                }
            if (bs == 0) return null;
            return pos;
        }
        public static Turn GetBestPlaceUtilityTurn(GameLayer gameLayer)
        {
            var state = gameLayer.GetState();
            var uArr = Helper.GetUtilitiyGrid(state);
            if(state.AvailableUtilityBuildings.Any(t => t.BuildingName == "Mall") && state.AvailableUtilityBuildings.Find(t => t.BuildingName == "Mall").ReleaseTick < state.Turn && !state.UtilityBuildings.Any(x => x.BuildingName == "Mall"))
            {
                var bluep = gameLayer.GetUtilityBlueprint("Mall");
                Position pos = GetBestPos(uArr, gameLayer, 3, "Mall");
                if (pos != null && state.Funds > bluep.Cost + 5000)
                {
                    return new PlaceUtilityTurn(pos.x, pos.y, bluep, state.GameId);
                }
            }
            if (state.AvailableUtilityBuildings.Any(t => t.BuildingName == "Park") && state.AvailableUtilityBuildings.Find(t => t.BuildingName == "Park").ReleaseTick < state.Turn && !state.UtilityBuildings.Any(x => x.BuildingName == "Park"))
            {
                var bluep = gameLayer.GetUtilityBlueprint("Park");
                Position pos = GetBestPos(uArr, gameLayer, 2, "Park");
                if (pos != null && state.Funds > bluep.Cost + 5000)
                {
                    return new PlaceUtilityTurn(pos.x, pos.y, bluep, state.GameId);
                }
            }
            if (state.AvailableUtilityBuildings.Any(t => t.BuildingName == "WindTurbine") && state.AvailableUtilityBuildings.Find(t => t.BuildingName == "WindTurbine").ReleaseTick < state.Turn && !state.UtilityBuildings.Any(x => x.BuildingName == "WindTurbine"))
            {
                var bluep = gameLayer.GetUtilityBlueprint("WindTurbine");
                Position pos = GetBestPos(uArr, gameLayer, 2, "WindTurbine");
                if (pos != null && state.Funds > bluep.Cost + 5000)
                {
                    return new PlaceUtilityTurn(pos.x, pos.y, bluep, state.GameId);
                }
            }
            return null;
        }
        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.StartBuild(new Position(x, y), bluePrint.BuildingName, _gameId);
            return new BuildTurn(x, y, _gameId);
        }
    }
}
