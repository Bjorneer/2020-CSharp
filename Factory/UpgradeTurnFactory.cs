using DotNet.Actions;
using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.Factory
{
    class UpgradeTurnFactory
    {

        internal static Bundle GetBestUpgradeTurn(Upgrade upgrade, GameLayer layer)
        {
            switch (upgrade.Name)
            {
                case "Caretaker":
                    return GetBestCaretaker(upgrade, layer);
                case "Charger":
                    return GetBestCharger(upgrade, layer);
                case "SolarPanel":
                    return GetBestSolarPanel(upgrade, layer);
                case "Insulation":
                    return GetBestInsulation(upgrade, layer);
                case "Playground":
                    return GetBestPlayground(upgrade, layer);
                case "Regulator":
                    return GetBestRegulator(upgrade, layer);
                default:
                    return null;
            }
        }
        private static Bundle GetBestRegulator(Upgrade upgrade, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = new Position(0, 0);
            return null;
            var b = new Bundle
            {
                EnergyNeed = 0,
                ExtraCost = 0,
                PotentialScore = 0,
                TotalIncome = 0,
                Turn = new UpgradeTurn(pos.x, pos.y, upgrade.Name, state.GameId),
                UpfrontCost = 1250
            };
            return b;
        }
        private static Bundle GetBestInsulation(Upgrade upgrade, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = new Position(0, 0);
            BuiltResidenceBuilding building = null;
            return null;
        }
        private static Bundle GetBestPlayground(Upgrade upgrade, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = new Position(0, 0);
            BuiltResidenceBuilding building = null;
            int turnsleft = state.MaxTurns - state.Turn;
            int popAffected = 0;
            foreach (var build in state.ResidenceBuildings)
            {
                if(!build.Effects.Contains(upgrade.Name) && popAffected < layer.GetResidenceBlueprint(build.BuildingName).MaxPop)
                {
                    building = build;
                    popAffected = layer.GetResidenceBlueprint(build.BuildingName).MaxPop;
                    pos = build.Position;
                }
            }
            if (building == null) return null;
            var b = new Bundle
            {
                EnergyNeed = 0,
                ExtraCost = 0,
                PotentialScore = popAffected * turnsleft * 0.16 / 10 + 0.007 * popAffected * turnsleft,
                TotalIncome = 0,
                Turn = new UpgradeTurn(pos.x, pos.y, upgrade.Name, state.GameId),
                UpfrontCost = 5200
            };
            return b;
        }
        private static Bundle GetBestSolarPanel(Upgrade upgrade, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = new Position(0, 0);
            BuiltResidenceBuilding building = null;
            int turnsleft = state.MaxTurns - state.Turn;
            double energySaved = 0;
            foreach (var build in state.ResidenceBuildings)
            {
                if (!build.Effects.Contains(upgrade.Name) && (build.EffectiveEnergyIn - (build.Effects.Contains("WindTurbine") ? 3.4 : 0) + (build.Effects.Contains("Charger") ? 1.8 : 0)) > energySaved)
                {
                    building = build;
                    pos = build.Position;
                    energySaved = Math.Max(Math.Min(build.EffectiveEnergyIn - (build.Effects.Contains("WindTurbine") ? 3.4 : 0) + (build.Effects.Contains("Charger") ? 1.8 : 0), 3.4), 0);
                }
            }
            if (building == null) return null;
            double consu = Helper.GetEnergyConsumation(layer) - energySaved;
            var levelNow = Helper.GetEnergyLevel(layer, consu);
            var levelPrev = Helper.GetEnergyLevel(layer);

            double score = energySaved * levelNow.TonCo2PerMwh * turnsleft;


            var b = new Bundle
            {
                EnergyNeed = -energySaved,
                ExtraCost = 0,
                PotentialScore = score,
                TotalIncome = energySaved * levelNow.CostPerMwh * turnsleft,
                Turn = new UpgradeTurn(pos.x, pos.y, upgrade.Name, state.GameId),
                UpfrontCost = 6800
            };
            return b;
        }
        private static Bundle GetBestCharger(Upgrade upgrade, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = new Position(0, 0);
            BuiltResidenceBuilding building = null;
            int turnsleft = state.MaxTurns - state.Turn;
            int popAffected = 0;
            foreach (var build in state.ResidenceBuildings)
            {
                if (build.Effects.Contains("Mall.2") && !build.Effects.Contains(upgrade.Name) && popAffected < layer.GetResidenceBlueprint(build.BuildingName).MaxPop)
                {
                    building = build;
                    pos = build.Position;
                    popAffected = layer.GetResidenceBlueprint(build.BuildingName).MaxPop;
                }
            }
            if (building == null) return null;
            double consu = Helper.GetEnergyConsumation(layer) + 1.8;
            var levelNow = Helper.GetEnergyLevel(layer, consu);


            var b = new Bundle
            {
                EnergyNeed = 1.8,
                ExtraCost = 1.8 * turnsleft * levelNow.CostPerMwh,
                PotentialScore = popAffected * 0.016 * turnsleft - 1.8 * turnsleft * levelNow.TonCo2PerMwh,
                TotalIncome = 0,
                Turn = new UpgradeTurn(pos.x, pos.y, upgrade.Name, state.GameId),
                UpfrontCost = 3400
            };
            return b;
        }
        private static Bundle GetBestCaretaker(Upgrade upgrade, GameLayer layer)
        {
            var state = layer.GetState();
            Position pos = new Position(0, 0);
            BuiltResidenceBuilding building = null;
            int turnsleft = state.MaxTurns - state.Turn;
            double save = 0;
            foreach (var build in state.ResidenceBuildings)
            {
                double maintCost = layer.GetResidenceBlueprint(build.BuildingName).MaintenanceCost;
                double decayRate = layer.GetResidenceBlueprint(build.BuildingName).DecayRate + build.Effects.Where(t => t == "Charger" || t == "Regulator" || t == "Playground" || t == "SolarPanel" ||t == "Insulation").Count() * 0.2; 
                if (!build.Effects.Contains(upgrade.Name) && save < maintCost * Math.Ceiling(turnsleft /  (55 / decayRate)) - maintCost * Math.Ceiling(turnsleft * 0.25 / (55 / decayRate)) - 6 * turnsleft - 3500)
                {
                    building = build;
                    pos = build.Position;
                    save = maintCost * Math.Ceiling(turnsleft / (55 / decayRate)) - maintCost * Math.Ceiling(turnsleft * 0.25 / (55 / decayRate)) - 6 * turnsleft - 3500;
                }
            }
            if (building == null) return null;
            var b = new Bundle
            {
                EnergyNeed = 0,
                ExtraCost = 6 * turnsleft,
                PotentialScore = 0,
                TotalIncome = save + 6 * turnsleft + 3500,
                Turn = new UpgradeTurn(pos.x, pos.y, upgrade.Name, state.GameId),
                UpfrontCost = 3500
            };
            return b;
        }
    }
}
