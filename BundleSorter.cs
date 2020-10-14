using DotNet.Actions;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DotNet
{
    public class BundleSorter
    {
        private static double ScoreBundle(GameLayer layer, Bundle bundle)
        {
            double score = 0;
            var state = layer.GetState();
            int avSpace = 0;
            int turnsleft = state.MaxTurns - state.Turn;
            double income = Helper.GetIncome(layer);
            if(income < 600 && state.Funds < 70000 && turnsleft > 100 && bundle.TotalIncome != 0)
            {
                score += Math.Min(1000 / (0.25 * (bundle.TotalIncome / turnsleft)), 1500);
            }
            foreach (var building in state.ResidenceBuildings)
            {
                avSpace += (layer.GetResidenceBlueprint(building.BuildingName).MaxPop - building.CurrentPop);
            }
            if(bundle.Residence != null)
            {
                score += Math.Min((state.HousingQueue - avSpace) * bundle.Residence.MaxPop * 4, 4000); 
            }
            if ((state.Funds - bundle.UpfrontCost < 40000 || income < 400) && bundle.Upgrade != null)
            {
                return -100;
            }
            score += bundle.PotentialScore;
            return score;
        }

        internal static Bundle GetBestBundle(List<Bundle> possibleMoves, GameLayer layer)
        {
            var state = layer.GetState();
            possibleMoves = possibleMoves.Where(t => t.UpfrontCost <= state.Funds).ToList();
            possibleMoves.Sort((a, b) =>
            {
                double sa = ScoreBundle(layer, a);
                double sb = ScoreBundle(layer, b);
                if (sa > sb) return -1;
                return 1;
            });
            return possibleMoves.First();
        }
    }
}
