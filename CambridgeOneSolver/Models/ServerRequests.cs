﻿using CambridgeOneSolver.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CambridgeOneSolver.Models
{
    class ServerRequests
    {
        static readonly string ServerURL = "http://138.124.180.37:4567/";

        #region Json properties
        [JsonProperty("data")]
        public string[] Data { get; set; }
        [JsonProperty("accountStatus")]
        public string AccountStatus { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("displayMessage")]
        public string DisplayMessage { get; set; } = " ";

        [JsonProperty("messageBody")]
        public string[] DontatorsMessageBody { get; set; }
        [JsonProperty("messageDate")]
        public string[] DontatorsMessageDate { get; set; }
        #endregion

        // почта ведется для идентификации количества пользователей на каждом курсе
        // и среднего количества выполненных тестов, никакой мисис этих данных не получает
        public static async Task<ServerRequests> Asnwers(string DataLink, string Email, string Version)
        {
            string request = ServerURL + $"?responseType=getTasks&link={DataLink}&email={Email}&version={Version}";
            var client = new HttpClient();
            var result = await client.GetStringAsync(request);
            return JsonConvert.DeserializeObject<ServerRequests>(result);
        }
        public static async Task<ServerRequests> Donators(string Email, string Version)
        {
            string request = ServerURL + $"?responseType=getDonators&email={Email}&version={Version}";
            var client = new HttpClient();
            var result = await client.GetStringAsync(request);
            return JsonConvert.DeserializeObject<ServerRequests>(result);
        }
    }
}
