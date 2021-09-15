using System;
using System.Windows;

namespace CambridgeOneSolver.Models
{
    class ErrorMessages
    {
        static readonly Func<string, MessageBoxResult> sendMessage = MessageBox.Show;
        public static void NoDriver() => sendMessage("Отсутствует файл chromedriver.exe, который нужен для корректной работы. Положите его в папку с программой.");
        public static void NoDataURL() => sendMessage("В тесте нет вопросов с выбором/вводом ответа или вы не выбрали тест.");
        //public static void NoDataURL() => sendMessage("Зайдите в тест и дождитесь его загрузки.");

        public static void NoInternet() => sendMessage("Отсутствует подключение к интернету.");
        public static void NoValidQuizzes() => sendMessage("В этом тесте нет вопросов с выбором ответа.");
        public static void NoAnswersRecieved() => sendMessage("Не удалось получить ответы для этого теста.");
        public static void ApiServerConnectionError() => sendMessage("Сервер недоступен. Напишите в группу.");

    }
}
