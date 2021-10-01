using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Telegram.Bot;

namespace HomeWork9
{

    class Program
    {
        static TelegramBotClient bot;
        static void Main(string[] args)
        {
            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.StartReceiving();
            Console.ReadKey();
            bot.StopReceiving();

        }

        private static string token = "****";

        public static object WeatherResponse { get; private set; }

        private static void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";
            Console.WriteLine(text);

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                Console.WriteLine(e.Message.Document.FileId);
                Console.WriteLine(e.Message.Document.FileName);
                Console.WriteLine(e.Message.Document.FileSize);

                DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);
            }

            if (e.Message.Text == null) return;

            var messageText = e.Message.Text;

            if (e.Message.Text == "/start") //начало работы
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Приветствую! Мое имя Учебный бот\n"+
                "Вам доступны следующие команды:\n"+
                "/getlistfiles - получить список сохраненных файлов\n"+
                "/weather - данные о погоде в Печоре");
            }

            if (e.Message.Text == "/getlistfiles") //получить список файлов в папке загрузки
            {
                DirectoryInfo folder = new DirectoryInfo(@"C:\Users\Екатерина\Downloads");
                foreach (FileInfo file in folder.GetFiles())
                {
                    bot.SendTextMessageAsync(e.Message.Chat.Id, file.Name+'\n');
                }

            }

            if (e.Message.Text[0] != '/')
            {
                bot.SendTextMessageAsync(e.Message.Chat.Id,
                $"{messageText}"
                );
            }

            if (e.Message.Text == "/weather") //получить температуру воздуха в Печоре
            {
                string url = "https://api.openweathermap.org/data/2.5/weather?q=" + "Pechora, RU" + "&appid=" + "8a424f124eedd1c859d12e03fff36147";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest?.GetResponse();
                string response;
                using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }

                Console.WriteLine(response);
                WeatherResponse weather = JsonConvert.DeserializeObject<WeatherResponse>(response);
                bot.SendTextMessageAsync(e.Message.Chat.Id, $"Температура воздуха в Печоре " + Convert.ToString((int)weather.Main.Temp - 273) + " градуса по Цельсию");         
            }

        }

        static async void DownLoad(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream("_" + path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }
    }   
}
