using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNet.Actions;
using DotNet.Handler;
using DotNet.models;


namespace DotNet
{
    public static class Program
    {
        //09bbef64-b059-4d2b-ab6d-5dd19c496781 Example game

        private const string ApiKey = "53409ca2-6832-406b-a97d-f86831a337f7";           // TODO: Enter your API key
        // The different map names can be found on considition.com/rules
        private const string Map = "Visby";     // TODO: Enter your desired map
        private static readonly GameLayer GameLayer = new GameLayer(ApiKey);

        public static void Main(string[] args)
        {
            Helper.INIT();
            try
            {
                GameLayer.EndGame();
            }
            catch (Exception e){ }
            var gameId = GameLayer.NewGame(Map);
            Console.WriteLine($"Starting game: {GameLayer.GetState().GameId}");
            GameLayer.StartGame(gameId);
            var state = GameLayer.GetState();
            while (GameLayer.GetState().Turn < GameLayer.GetState().MaxTurns)
            {
                //Console.WriteLine(GameLayer.GetState().CurrentTemp);
                NewTakeTurn();
                //take_turn(gameId);
                foreach (var message in GameLayer.GetState().Messages) Console.WriteLine(message);
                foreach (var error in GameLayer.GetState().Errors) Console.WriteLine("Error: " + error);
            }
            Console.WriteLine($"Done with game: {GameLayer.GetState().GameId}");
            Console.WriteLine(GameLayer.GetScore(gameId).FinalScore);
        }


        public static Stack<Turn> StackedTurns = new Stack<Turn>();
        private static void take_turn(string gameId) // OLD
        {
            if (StackedTurns.Count > 0)
            {
                Turn nextTurn = StackedTurns.Pop().TakeTurn(GameLayer);
                if (nextTurn != null) StackedTurns.Push(nextTurn);
                return;
            }
            Turn turn = MaintananceTurn.GetBestMaintanance(GameLayer);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }
            turn = AdjustEnergyTurn.GetBestEnergyAdjustment(GameLayer, 1.5);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }
            turn = PlaceUtilityTurn.GetBestPlaceUtilityTurn(GameLayer);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }
            turn = PlaceBuildingTurn.GetBestPlacedBuilding(GameLayer);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }
            turn = UpgradeTurn.GetBestUpgradeTurn(GameLayer);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }
            GameLayer.Wait();
        }


        private static void NewTakeTurn()
        {
            Turn turn = MaintananceTurn.GetBestMaintanance(GameLayer);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }
            turn = AdjustEnergyTurn.GetBestEnergyAdjustment(GameLayer, 2);
            if (turn != null)
            {
                turn = turn.TakeTurn(GameLayer);
                if (turn != null) StackedTurns.Push(turn);
                return;
            }

            if (StackedTurns.Count > 0)
            {
                Turn nextTurn = StackedTurns.Pop().TakeTurn(GameLayer);
                if (nextTurn != null) StackedTurns.Push(nextTurn);
                return;
            }
            List<Bundle> possibleMoves = new List<Bundle>();
            possibleMoves.AddRange(TurnFactory.GetBestPlaceBuildingTurnBundles(GameLayer));
            possibleMoves.AddRange(TurnFactory.GetBestUpgradesTurnBundles(GameLayer));
            possibleMoves.AddRange(TurnFactory.GetBestPlaceUtilitiesturnBundles(GameLayer));
            possibleMoves.Add(TurnFactory.GetWaitTurnBundle(GameLayer));

            Bundle bestTurn = BundleSorter.GetBestBundle(possibleMoves, GameLayer);
            turn = bestTurn.Turn.TakeTurn(GameLayer);
            if (turn != null) StackedTurns.Push(turn);
        }
    }

    public class Bundle
    {
        public double UpfrontCost { get; set; }
        public double ExtraCost { get; set; }
        public double TotalIncome { get; set; }
        public double PotentialScore { get; set; }
        public double EnergyNeed { get; set; }
        public Turn Turn { get; set; }
        public BlueprintResidenceBuilding Residence { get; set; }
        public BlueprintUtilityBuilding Utility { get; set; }
        public Upgrade Upgrade { get; set; }
        public bool IsWait { get; set; }
    }
}