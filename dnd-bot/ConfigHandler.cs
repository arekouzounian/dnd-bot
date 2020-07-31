using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using Newtonsoft.Json;

namespace dnd_bot
{
    class ConfigHandler
    {
        private Config conf;
        private string configPath;

        struct Config
        {
            public string token;
        }

        public ConfigHandler()
        {
            conf = new Config()
            {
                token = ""
            };
        }

        public async Task FillConfig()
        {
            configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json").Replace(@"\", @"\\");
            Console.WriteLine(configPath);

            if (!File.Exists(configPath))
            {
                using (StreamWriter sw = File.AppendText(configPath))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(conf));
                }

                Console.WriteLine("WARNING! New Config initialized. Need to fill in values before running commands.");
                throw new Exception("NO CONFIG AVAILABLE. Go to executable path and fill out newly created file.");
            }

            using (StreamReader reader = new StreamReader(configPath))
            {
                conf = JsonConvert.DeserializeObject<Config>(reader.ReadLine());
            }


            await Task.CompletedTask;
        }

        public string GetToken()
        {
            return conf.token;
        }
    }
}