using DotNet.Actions;
using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace DotNet.Factory
{
    public class PlaceBuildingTurnFactory
    {
        private static Position GetBestResidencePosition(GameLayer layer)
        {
            var state = layer.GetState();
            var arr = Helper.GetUtilitiyGrid(state);
            Func<int, int, int> func = delegate (int x, int y)
            {
                int s = 0;
                for (int i = 0; i < 5; i++)
                    if (((arr[x, y] >> i) & 1) == 1)
                        s++;
                return s;
            };
            Position ret = new Position(-1, -1);
            int score = 0;
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (state.Map[i][j] != 0 || state.ResidenceBuildings.Any(x => x.Position.x == i && x.Position.y == j) || state.UtilityBuildings.Any(x => x.Position.x == i && x.Position.y == j)) continue;
                    if (score < func(i, j))
                    {
                        ret.x = i;
                        ret.y = j;
                        score = func(i, j);
                    }
                }
            if (score == 0 && ret.x == -1)
            {
                Position bestUtilityPos = GetBestUtilityPosition(layer, layer.GetUtilityBlueprint("Mall"));
                if (bestUtilityPos == null) return null;
                for (int radius = 1; radius <= 3; radius++)
                {
                    for (int k = -3; k < 4; k++)
                    {
                        for (int c = -3; c < 4; c++)
                        {
                            int i = bestUtilityPos.x;
                            int j = bestUtilityPos.y;
                            if (Math.Abs(k) + Math.Abs(c) > radius || !Helper.InGrid(i + k, j + c, state) || state.Map[i + k][j + c] != 0) continue;
                            if (!state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c) && !state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c))
                            {
                                ret.x = i + k;
                                ret.y = j + c;
                            }
                        }
                    }
                    if (ret.x != -1) break;
                }
            }
            if (ret.x == -1) return null;
            return ret;
        }

        internal static Bundle GetBestResidenceBundle(BlueprintResidenceBuilding building, GameLayer layer)
        {
            var state = layer.GetState();
            int turnsLeft = (state.MaxTurns - state.Turn - (int)Math.Ceiling(100.0 / building.BuildSpeed) - 1);
            var pos = GetBestResidencePosition(layer);
            if (pos == null)
            {
                return null;
            }
            double potentialScore = (building.MaxHappiness + Constants.AVG_EFFECT_HAPPINESS_INCREASE) * building.MaxPop * turnsLeft * Constants.AVG_POP_HAPPINESS_PRECENT / 10;
            if (!state.ResidenceBuildings.Any(t => t.BuildingName == building.BuildingName))
                potentialScore += turnsLeft * Constants.TARGET_END_POP_COUNT * Constants.AVG_POP_HAPPINESS * Constants.AVG_POP_HAPPINESS_PRECENT * 0.1 / 30;
            //FIXA DET UNDER DENNA
            double avgTemp = Math.Max(state.MinTemp, state.MinTemp + (state.MaxTemp - state.MinTemp) / 2 - 4);
            double avgEnergyIn = Math.Max(building.BaseEnergyNeed, ((21 - avgTemp) * building.Emissivity - Constants.DEGREES_PER_POP * building.MaxPop) / Constants.DEGREES_PER_EXCESS_MWH + building.BaseEnergyNeed);
            if(state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2))
            {
                avgEnergyIn = Math.Max(0, avgEnergyIn - 3.4);
            }
            if(state.Funds > 50000)
            {
                avgEnergyIn = Math.Max(0, avgEnergyIn - 3.4);
            }

            var el = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer));
            var elAfter = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer) + avgEnergyIn);
            if(el.CostPerMwh != elAfter.CostPerMwh)
            {
                potentialScore -= (Helper.GetEnergyConsumation(layer) + avgEnergyIn) * elAfter.TonCo2PerMwh * turnsLeft + (Helper.GetEnergyConsumation(layer)) * el.TonCo2PerMwh * turnsLeft;
            }
            else
            {
                potentialScore -= avgEnergyIn * elAfter.TonCo2PerMwh * turnsLeft;
            }

            potentialScore += 15 * building.MaxPop;
            potentialScore -= building.MaxPop * Constants.CO2_PER_POP * turnsLeft;
            potentialScore -= building.Co2Cost;

            Bundle bundle = new Bundle
            {
                TotalIncome = building.IncomePerPop * building.MaxPop * turnsLeft,
                ExtraCost = building.MaintenanceCost * turnsLeft / Math.Ceiling(55.0 / (building.DecayRate + Constants.AVG_DECAY_INCREASE)) + elAfter.CostPerMwh * avgEnergyIn * turnsLeft,
                UpfrontCost = building.Cost,
                PotentialScore = potentialScore,
                Turn = new PlaceBuildingTurn(pos.x, pos.y,building, state.GameId),
                EnergyNeed = avgEnergyIn
            };
            return bundle;
        }

        private static Position GetBestUtilityPosition(GameLayer layer, BlueprintUtilityBuilding building)
        {
            int radius = 2;
            if (building.BuildingName == "Mall") radius = 3;

            var state = layer.GetState();
            var arr = Helper.GetUtilitiyGrid(state);
            Position ret = new Position(-1, -1);
            int fd = 0;
            int sd = 0;
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (state.Map[i][j] != 0 || state.ResidenceBuildings.Any(x => x.Position.x == i && x.Position.y == j) || state.UtilityBuildings.Any(x => x.Position.x == i && x.Position.y == j)) continue;
                    int cnt = 0;
                    int avSpace = 0;
                    for (int k = -3; k < 4; k++)
                    {
                        for (int c = -3; c < 4; c++)
                        {
                            if (Math.Abs(k) + Math.Abs(c) > radius || !Helper.InGrid(i + k, j + c, state) || state.Map[i + k][j + c] != 0 || (k == 0 && c == 0)) continue;
                            if (state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c && !x.Effects.Any(t => t.StartsWith(building.BuildingName))) ||
                                (building.BuildingName == "WindTurbine" &&
                                state.UtilityBuildings.Any(x => x.Position.x == i + k &&
                                x.Position.y == j + c && x.BuildingName != "WindTurbine" &&
                                !x.Effects.Contains(building.BuildingName))))
                            {
                                cnt++;
                            }
                            else if (!state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c) && !state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c)
                                && (arr[i + k, j + c] & Helper.UtilityToInt[building.BuildingName]) != Helper.UtilityToInt[building.BuildingName])
                            {
                                avSpace++;
                            }
                            else if (state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c && building.BuildingName != "WindTurbine")) avSpace--;
                        }
                    }
                    if(cnt > fd)
                    {
                        fd = cnt;
                        sd = avSpace;
                        ret.x = i;
                        ret.y = j;
                    }
                    else if(cnt == fd && avSpace > sd)
                    {
                        fd = cnt;
                        sd = avSpace;
                        ret.x = i;
                        ret.y = j;
                    }
                }
            if (ret.x == -1) return null; // NO POSSIBLE POSITION
            return ret;
        }

        internal static Bundle GetBestUtilityBundle(BlueprintUtilityBuilding building, GameLayer layer)
        {
            switch (building.BuildingName)
            {
                case "WindTurbine":
                    return GetBestWindTurbineBundle(building, layer);
                case "Mall":
                    return GetBestMallBundle(building, layer);
                case "Park":
                    return GetBestParkBundle(building, layer);
                default:
                    return null;
            }
        }

        private static Bundle GetBestParkBundle(BlueprintUtilityBuilding building, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = GetBestUtilityPosition(layer, building);
            if (pos == null) return null;
            int turnsLeft = state.MaxTurns - state.Turn - 5;

            var arr = Helper.GetUtilitiyGrid(layer.GetState());
            int popAffected = 0;
            int avalibleSpace = 0;
            for (int k = -3; k < 4; k++)
            {
                for (int c = -3; c < 4; c++)
                {
                    int i = pos.x;
                    int j = pos.y;
                    if (Math.Abs(k) + Math.Abs(c) > 2 || !Helper.InGrid(i + k, j + c, state) || state.Map[i + k][j + c] == 1 || (k == 0 && j == 0)) continue;
                    if (state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c && !x.Effects.Contains(building.BuildingName)))
                    {
                        popAffected += layer.GetResidenceBlueprint(state.ResidenceBuildings.First(x => x.Position.x == i + k && x.Position.y == j + c).BuildingName).MaxPop;
                    }
                    else if (!state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c) && !state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c) &&
                        (arr[i + k, j + c] & Helper.UtilityToInt[building.BuildingName]) != Helper.UtilityToInt[building.BuildingName])
                    {
                        avalibleSpace++;
                    }
                    else if(state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c))
                    {
                        avalibleSpace--;
                    }
                }
            }

            EnergyLevel el = Helper.GetEnergyLevel(layer);
            EnergyLevel elAfter = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer) + Math.Max(0, 2.4 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 2.4 : 0)));
            double potentialScore = -10 + (popAffected + avalibleSpace * 30.0 / 6.0) * 0.007 * turnsLeft + 0.11 * (popAffected + avalibleSpace * 30.0 / 6.0)* turnsLeft / 10;
            if(!state.UtilityBuildings.Any(t => t.BuildingName == building.BuildingName))
            {
                potentialScore += 15 * 0.15 * turnsLeft / 2;
            }
            else
            {
                potentialScore -= 20 * Constants.AVG_POP_HAPPINESS * turnsLeft / 20.0;
            }
            if (el.CostPerMwh != elAfter.CostPerMwh)
            {
                potentialScore -= (elAfter.TonCo2PerMwh - el.TonCo2PerMwh) * Helper.GetEnergyConsumation(layer);
            }
            potentialScore -= turnsLeft * elAfter.TonCo2PerMwh * Math.Max(0, 2.4 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 2.4 : 0));
            if (Helper.GetPossibleMorePop(layer, 1) + Helper.GetCurrentMaxPop(layer) < Constants.TARGET_END_POP_COUNT) return null;
            Bundle bundle = new Bundle
            {
                UpfrontCost = building.Cost,
                TotalIncome = 0,
                ExtraCost = el.CostPerMwh * (2.4 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 2.4 : 0)) * turnsLeft,
                Turn = new PlaceUtilityTurn(pos.x, pos.y, building, layer.GetState().GameId),
                PotentialScore = potentialScore,
                EnergyNeed = Math.Max(0, 2.4 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 2.4 : 0))
            };
            return bundle;
        }

        private static Bundle GetBestMallBundle(BlueprintUtilityBuilding building, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = GetBestUtilityPosition(layer, building);
            if (pos == null) return null;
            int turnsLeft = state.MaxTurns - state.Turn - 8;
            if (state.UtilityBuildings.Any(t => t.BuildingName == "Mall")) return null;
            var arr = Helper.GetUtilitiyGrid(state);
            int popAffected = 0;
            int popOne = 0;
            int avalibleSpace = 0;
            int avSpaceOne = 0;
            for (int k = -3; k < 4; k++)
            {
                for (int c = -3; c < 4; c++)
                {
                    int i = pos.x;
                    int j = pos.y;
                    if (Math.Abs(k) + Math.Abs(c) > 3 || !Helper.InGrid(i + k, j + c, state) || state.Map[i + k][j + c] == 1 || (k == 0 && c == 0)) continue;
                    if (state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c && !x.Effects.Any(t => t.StartsWith(building.BuildingName))))
                    {
                        popAffected += layer.GetResidenceBlueprint(state.ResidenceBuildings.Find(x => x.Position.x == i + k && x.Position.y == j + c).BuildingName).MaxPop;
                        if ((Math.Abs(k) + Math.Abs(c) == 1)) popOne += layer.GetResidenceBlueprint(state.ResidenceBuildings.Find(x => x.Position.x == i + k && x.Position.y == j + c).BuildingName).MaxPop;
                    }
                    else if (!state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c) && !state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c)
                        && (arr[i + k, j + c] & Helper.UtilityToInt[building.BuildingName]) != Helper.UtilityToInt[building.BuildingName])
                    {
                        avalibleSpace++;
                        if ((Math.Abs(k) + Math.Abs(c) == 1)) avSpaceOne++;
                    }
                    else if (state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c)) avalibleSpace--;
                }
            }

            EnergyLevel el = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer));
            EnergyLevel elAfter = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer) + 8 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 3.4 : 0));
            
            double potentialScore = -200 - (popAffected - popOne) * 0.009 / 2 * turnsLeft - (Math.Max(0, (avalibleSpace - avSpaceOne)) * 15.0 / 10.0) * 0.009 / 2 * turnsLeft + 0.12 * (popAffected + avalibleSpace * 25.0 / 3.0) * turnsLeft / 10;
            //potentialScore -= 25 * Constants.AVG_POP_HAPPINESS * turnsLeft / 20.0;
            if (!state.UtilityBuildings.Any(t => t.BuildingName == building.BuildingName))
            {
                potentialScore += 15 * 0.15 * turnsLeft / 2;
            }
            if (el.CostPerMwh != elAfter.CostPerMwh)
            {
                potentialScore -= (elAfter.TonCo2PerMwh - el.TonCo2PerMwh) * Helper.GetEnergyConsumation(layer) * turnsLeft;
            }
            potentialScore -= turnsLeft * elAfter.TonCo2PerMwh * (8 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 3.4 : 0));

            if (Helper.GetPossibleMorePop(layer, 1) + Helper.GetCurrentMaxPop(layer) < Constants.TARGET_END_POP_COUNT) return null;
            Bundle bundle = new Bundle
            {
                UpfrontCost = building.Cost,
                TotalIncome = Math.Pow(0.5, state.UtilityBuildings.Where(t => t.BuildingName == building.BuildingName).Count()) * 240 * turnsLeft,
                ExtraCost = elAfter.CostPerMwh * (8 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 3.4 : 0)) * turnsLeft,
                Turn = new PlaceUtilityTurn(pos.x, pos.y, building, layer.GetState().GameId),
                PotentialScore = potentialScore,
                EnergyNeed = 8 - (state.UtilityBuildings.Any(t => t.BuildingName == "WindTurbine" && Math.Abs(pos.x - t.Position.x) + Math.Abs(pos.y - t.Position.y) <= 2) ? 3.4 : 0)
            };
            return bundle;
        }

        private static Bundle GetBestWindTurbineBundle(BlueprintUtilityBuilding building, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = GetBestUtilityPosition(layer, building);
            if (pos == null) return null;
            int turnsLeft = state.MaxTurns - state.Turn - 5;

            double energySaved = 0;
            int avalibleSpace = 0;
            for (int k = -2; k < 3; k++)
            {
                for (int c = -2; c < 3; c++)
                {
                    int i = pos.x;
                    int j = pos.y;
                    if (Math.Abs(k) + Math.Abs(c) > 2 || !Helper.InGrid(i + k, j + c, state) || state.Map[i + k][j + c] == 1 || (k == 0 && c == 0)) continue;
                    if (state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c && !x.Effects.Contains(building.BuildingName)))
                    {
                        BuiltBuilding b = state.ResidenceBuildings.First(x => x.Position.x == i + k && x.Position.y == j + c);
                        energySaved += Math.Min(3.4, Math.Max(b.EffectiveEnergyIn - (b.Effects.Contains("SolarPanel") ? 3.4 : 0), 0));
                    }
                    if (state.UtilityBuildings.Any(x => x.Position.x == i + k &&
                        x.Position.y == j + c && x.BuildingName != "WindTurbine" &&
                        !x.Effects.Contains(building.BuildingName)))
                    {
                        BuiltBuilding b = state.UtilityBuildings.First(x => x.Position.x == i + k && x.Position.y == j + c);
                        energySaved += Math.Min(3.4, Math.Max(b.EffectiveEnergyIn - (b.Effects.Contains("SolarPanel") ? 3.4 : 0), 0));
                    }
                    else if (!state.ResidenceBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c) && !state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c))
                    {
                        avalibleSpace++;
                    }
                    else if (state.UtilityBuildings.Any(x => x.Position.x == i + k && x.Position.y == j + c)) avalibleSpace--;
                }
            }
            EnergyLevel el = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer));
            EnergyLevel elAfter = Helper.GetEnergyLevel(layer, Helper.GetEnergyConsumation(layer) - energySaved);
            double potentialScore = -25;
            potentialScore -= 25 * Constants.AVG_POP_HAPPINESS * turnsLeft / 20.0;
            if (!state.UtilityBuildings.Any(t => t.BuildingName == building.BuildingName))
            {
                potentialScore += 15 * 0.15 * turnsLeft / 2;
            }
            potentialScore += el.TonCo2PerMwh * Helper.GetEnergyConsumation(layer) * turnsLeft - (elAfter.TonCo2PerMwh * (Helper.GetEnergyConsumation(layer) - energySaved)) * turnsLeft;
            if (Helper.GetPossibleMorePop(layer, 1) + Helper.GetCurrentMaxPop(layer) < Constants.TARGET_END_POP_COUNT) return null;

            Bundle bundle = new Bundle
            {
                UpfrontCost = building.Cost,
                TotalIncome = turnsLeft * elAfter.CostPerMwh * energySaved,//saved on electicity
                ExtraCost = 0,
                Turn = new PlaceUtilityTurn(pos.x, pos.y, building, layer.GetState().GameId),
                PotentialScore = potentialScore,
                EnergyNeed = -energySaved
            };
            return bundle;
        }
    }
}
//score = population * 15 + happiness / 10 - co2
//newTemp = indoorTemp + (effectiveEnergyIn - baseEnergyNeed) * degreesPerExcessMwh + degreesPerPop * currentPop - (indoorTemp - outdoorTemp) * emissivity
