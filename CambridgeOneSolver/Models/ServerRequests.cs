using CambridgeOneSolver.Infrastructure;
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
        [JsonProperty("tasksTag")]
        public int[] TasksTag { get; set; }
        [JsonProperty("accountStatus")]
        public string AccountStatus { get; set; }
        [JsonProperty("displayMessage")]
        public string DisplayMessage { get; set; } = null;

        [JsonProperty("messageBody")]
        public string[] DontatorsMessageBody { get; set; }
        [JsonProperty("messageDate")]
        public string[] DontatorsMessageDate { get; set; }
        #endregion

        /*
         * Некоторых может смутить отправка почты на сервер.
         * Так как на группу подписываются не все пользователи программы, то я никаким образом не могу
         * определить их точное количество. Ради интереса я ввел почту как аналитику.
         * Чтобы вы поняли размеры проблемы: количество пользователей превышает участников группы почти в 4 раза!
         * С ее помощью можно узнать следущие инсайды аудитории программы:
         * Общее число пользователей, соотношение по курсам, среднее количество выполненных тестов.
         * Более того, некоторые пользователи не из МИСиС
         * Никакая из почт не будет передана кому-либо. Даже я сам не открываю этот файл напрямую для просмотра
         */
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
