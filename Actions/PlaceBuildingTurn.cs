using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.Actions
{
    public class PlaceBuildingTurn : Turn
    {
        BlueprintBuilding blueprint;
        int x, y;
        public PlaceBuildingTurn(int x, int y, BlueprintBuilding blueprint, string gameId)
            :base(gameId)
        {
            this.x = x;
            this.y = y;
            this.blueprint = blueprint;
        }
        public static Turn GetBestPlacedBuilding(GameLayer gameLayer)
        {
            Turn placeBuildingTurn = null;
            var state = gameLayer.GetState();
            if ((state.Funds < 5000 && state.ResidenceBuildings.Count > 0) || state.Turn > 650) return null;
            int spaceLeft = 0;
            foreach (var building in state.ResidenceBuildings)
            {
                spaceLeft += state.AvailableResidenceBuildings.First(x => x.BuildingName == building.BuildingName).MaxPop - building.CurrentPop;
            }
            if (state.Turn > 650 || spaceLeft > 0) return null;
            if (gameLayer.GetState().Funds < 5000 && gameLayer.GetState().ResidenceBuildings.Count > 0) return null;
            if (state.QueueHappiness > Constants.QUEUE_MAX_HAPPINESS - 5 || state.HousingQueue > Constants.LONG_QUEUE - 1)
            {
                double score = 0;
                BlueprintResidenceBuilding toBuild = null;
                foreach (var building in state.AvailableResidenceBuildings)
                {
                    if (building.ReleaseTick > state.Turn) continue;
                    double s = GetScoreForBuilding(state, building);
                    if(score < s && (building.Cost + 5000 <= state.Funds || (building.Cost <= state.Funds && state.ResidenceBuildings.Count == 0)) )
                    {
                        toBuild = building;
                        score = s;
                    }
                }
                if (toBuild == null) return null;
                var pos = GetBestPos(state);
                return new PlaceBuildingTurn(pos.x, pos.y, toBuild, state.GameId);
            }
            return placeBuildingTurn;
        }

        private static double GetScoreForBuilding(GameState state, BlueprintResidenceBuilding building)
        {
            double ret = 0;
            if (state.MinTemp < 0) ret += 10 / building.Emissivity;
            if(state.HousingQueue - 5 < building.MaxPop)
            {
                ret += 100;
            }
            else
            {
                ret -= 70;
            }
            if (state.Funds - building.Cost - building.MaintenanceCost * 2 < 10000)
            {
                ret += building.IncomePerPop * 10;
                ret -= 60;
            }
            if(state.Funds > 20000)
            {
                ret -= building.BaseEnergyNeed * 3;
                ret += building.Co2Cost / 3;
                ret += building.MaxHappiness * 30;
            }
            ret += 100 - building.Co2Cost / 5.0;
            ret -= building.BaseEnergyNeed * 5;
            ret += !state.ResidenceBuildings.Any(t => t.BuildingName == building.BuildingName) ? 50 : 0;

            return ret;
        }

        private static Position GetBestPos(GameState state)
        {
            var arr = Helper.GetUtilitiyGrid(state);
            Func<int,int,int> func = delegate (int x, int y)
            {
                int s = 0;
                for (int i = 0; i < 4; i++)
                    if (((arr[x, y] >> i) & 1) == 1)
                        s++;
                return s;
            };
            Position ret = new Position(-1, -1);
            int score = -1;
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (state.Map[i][j] == 1 || state.ResidenceBuildings.Any(x => x.Position.x == i && x.Position.y == j) || state.UtilityBuildings.Any(x => x.Position.x == i && x.Position.y == j)) continue;
                    if(score == -1 && state.Map[i][j] == 0)
                    {
                        ret.x = i;
                        ret.y = j;
                        score = func(i, j);
                    }
                    else if (state.Map[i][j] == 0 && score < func(i, j))
                    {
                        ret.x = i;
                        ret.y = j;
                        score = func(i, j);
                    }
                }
            if(score == 0)
            { //Handle no utilities
                int ma = -1;
                for (int i = 0; i < arr.GetLength(0); i++)
                    for (int j = 0; j < arr.GetLength(1); j++)
                    {
                        if (state.Map[i][j] == 1 || state.ResidenceBuildings.Any(x => x.Position.x == i && x.Position.y == j) || state.UtilityBuildings.Any(x => x.Position.x == i && x.Position.y == j)) continue;
                        int x = 0;
                        if (Helper.InGrid(i - 1, j, state) && state.Map[i - 1][j] == 1)
                        {
                            x++;
                            if (state.ResidenceBuildings.Any(x => x.Position.x == i - 1 && x.Position.y == j) || state.UtilityBuildings.Any(x => x.Position.x == i - 1 && x.Position.y == j)) x++;
                        }
                        if (Helper.InGrid(i + 1, j, state) && state.Map[i + 1][j] == 1)
                        {
                            x++;
                            if (state.ResidenceBuildings.Any(x => x.Position.x == i + 1 && x.Position.y == j) || state.UtilityBuildings.Any(x => x.Position.x == i + 1 && x.Position.y == j)) x++;
                        }
                        if (Helper.InGrid(i, j - 1, state) && state.Map[i][j - 1] == 1)
                        {
                            x++;
                            if (state.ResidenceBuildings.Any(x => x.Position.x == i && x.Position.y == j - 1) || state.UtilityBuildings.Any(x => x.Position.x == i && x.Position.y == j - 1)) x++;
                        }
                        if (Helper.InGrid(i, j + 1, state) && state.Map[i][j + 1] == 1)
                        {
                            x++;
                            if (state.ResidenceBuildings.Any(x => x.Position.x == i && x.Position.y == j + 1) || state.UtilityBuildings.Any(x => x.Position.x == i && x.Position.y == j + 1)) x++;
                        }
                        if (x > ma)
                        {
                            ma = x;
                            ret.x = i;
                            ret.y = j;
                        }
                    }

            }
            return ret;
        }
        public override Turn TakeTurn(GameLayer gameLayer)
        {
            gameLayer.StartBuild(new Position(x, y), blueprint.BuildingName, _gameId);
            return new BuildTurn(x, y, _gameId);
        }
    }
}
