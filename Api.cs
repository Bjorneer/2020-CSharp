using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotNet.models;

namespace DotNet
{
    public class Api
    {
        private const string BasePath = "https://game.considition.com/api/game/";
        private readonly HttpClient _client = new HttpClient {BaseAddress = new Uri(BasePath)};

        public Api(string apiKey)
        {
            _client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<GameState> NewGame(string map)
        {
            var data = new StringContent(map, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("new", data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine();
                Console.WriteLine("Fatal Error: could not start a new game");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameState>(result);
        }

        public async Task<GameStateResponse> StartGame(string gameId = null)
        {
            var response = await _client.GetAsync("start?GameId=" + gameId);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not start the game");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<GameStateResponse> StartBuild(string foundation, string gameId = null)
        {
            var data = new StringContent(foundation, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("action/startBuild?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not start build");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<GameStateResponse> Build(string pos, string gameId = null)
        {
            var data = new StringContent(pos, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("action/Build?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                Console.WriteLine("Fatal Error: could not build");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<GameStateResponse> Demolish(string pos, string gameId = null)
        {
            var data = new StringContent(pos, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("action/Demolish?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not build");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<GameStateResponse> Maintenance(string pos, string gameId = null)
        {
            var data = new StringContent(pos, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("action/Maintenance?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not maintain");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<GameStateResponse> BuyUpgrade(string body, string gameId = null)
        {
            var data = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("action/BuyUpgrade?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not buy upgrade");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<GameStateResponse> AdjustEnergy(string body, string gameId = null)
        {
            var data = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("action/AdjustEnergy?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not adjust energy");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }


        public async Task<GameStateResponse> Wait(string gameId = null)
        {
            var data = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("action/Wait?GameId=" + gameId, data);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not wait");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }

        public async Task<ScoreResponse> GetScore(string gameId)
        {
            //Client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            var response = await _client.GetAsync("score?GameId?" + gameId);

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ScoreResponse>(result);
        }

        public async Task<GameState> GetGameInfo(string gameId)
        {
            var response = await _client.GetAsync("gameInfo?GameId?" + gameId);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not get game info");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameState>(result);
        }

        public async Task<GameStateResponse> GetGameState(string gameId)
        {
            var response = await _client.GetAsync("gameState?GameId?" + gameId);

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not get game state");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<GameStateResponse>(result);
        }
        
        public async Task<List<GamesResponse>> GetGames()
        {
            var response = await _client.GetAsync("games");

            var result = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Exception:" + result);
                Console.WriteLine(response.Content);
                Console.WriteLine("Fatal Error: could not get games");
                Environment.Exit(1);
            }

            return JsonConvert.DeserializeObject<List<GamesResponse>>(result);
        }

        public async Task EndGame(string gameId = null)
        {
            var response = await _client.GetAsync("end?GameId?" + gameId);
            Console.WriteLine(await response.Content.ReadAsStringAsync());


            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                Console.WriteLine("Fatal Error: could not end game");
            }
        }
    }
}