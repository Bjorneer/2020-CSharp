using DotNet.Actions;
using DotNet.Factory;
using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.Handler
{
    public static class TurnFactory
    {
        public static IEnumerable<Bundle> GetBestPlaceBuildingTurnBundles(GameLayer gameLayer)
        {
            foreach (var building in gameLayer.GetState().AvailableResidenceBuildings)
            {
                if (building.ReleaseTick > gameLayer.GetState().Turn) continue;
                var b = PlaceBuildingTurnFactory.GetBestResidenceBundle(building,  gameLayer);
                if (b == null)
                {
                    if (gameLayer.GetState().HousingQueue > Constants.LONG_QUEUE)
                    {
                        BuiltResidenceBuilding bui = null;
                        double best = 200;
                        foreach (var build in gameLayer.GetState().ResidenceBuildings)
                        {
                            if (!build.CanBeDemolished || build.CurrentPop != gameLayer.GetResidenceBlueprint(build.BuildingName).MaxPop || gameLayer.GetState().ResidenceBuildings.Where(t => t.BuildingName == build.BuildingName).Count() == 1) continue;
                            if(best > gameLayer.GetResidenceBlueprint(build.BuildingName).MaxHappiness * gameLayer.GetResidenceBlueprint(build.BuildingName).MaxPop)
                            {
                                best = gameLayer.GetResidenceBlueprint(build.BuildingName).MaxHappiness * gameLayer.GetResidenceBlueprint(build.BuildingName).MaxPop;
                                bui = build;
                            }
                        }
                        if(bui != null && best < 30 && gameLayer.GetState().Funds > 15000 && gameLayer.GetState().Turn < 550)
                        {
                            Console.WriteLine("Demolish");

                            Program.StackedTurns.Push(new DemolishTurn(bui.Position.x, bui.Position.y, gameLayer.GetState().GameId));
                        }
                    }
                    break;
                }
                b.Residence = building;
                yield return b;

            }
        }

        internal static IEnumerable<Bundle> GetBestPlaceUtilitiesturnBundles(GameLayer gameLayer)
        {
            foreach (var building in gameLayer.GetState().AvailableUtilityBuildings)
            {
                if (building.ReleaseTick > gameLayer.GetState().Turn) continue;
                var b = PlaceBuildingTurnFactory.GetBestUtilityBundle(building, gameLayer);
                if (b != null)
                {
                    b.Utility = building;
                    yield return b;
                }
            }
        }

        internal static IEnumerable<Bundle> GetBestUpgradesTurnBundles(GameLayer gameLayer)
        {
            foreach (var upgrade in gameLayer.GetState().AvailableUpgrades)
            {
                Bundle b = UpgradeTurnFactory.GetBestUpgradeTurn(upgrade, gameLayer);
                if (b != null)
                {
                    b.Upgrade = upgrade;
                    yield return b;
                }
            }
        }

        internal static Bundle GetWaitTurnBundle(GameLayer gameLayer)
        {
            var b = new Bundle
            {
                EnergyNeed = 0,
                ExtraCost = 0,
                PotentialScore = 0,
                TotalIncome = 0,
                Turn = new WaitTurn(null, gameLayer.GetState().GameId),
                UpfrontCost = -1000,
                IsWait = true
            };
            return b;
        }
    }
}
