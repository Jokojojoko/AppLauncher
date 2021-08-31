using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
namespace AppLauncher
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("User32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Int32 vKey);

        List<string> functions = new List<string>() {"all","add","set","del","?","q","g","chg"};
        Dictionary<string, List<string>> data;
        Dictionary<string, List<string>> gameData;
        public List<string> dict = new List<string>();
        public List<string> gameDict = new List<string>();
        static void Main(string[] args)
        {
            NotifyIcon icon = new NotifyIcon();
            icon.Icon = new System.Drawing.Icon("./icon.ico");
            icon.Visible = true;
            icon.Text = "AppLauncher 1.4";
            Console.OutputEncoding=Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            Program p = new Program();
            p.Quit();
            while (true)
            {
                p.Start();
            }
        }

        
        void Start()
        {
            ReadJson();
            ReadGameJson();
            UpdateDicts();
            dict.Sort();
            gameDict.Sort();
            string input = Hinter.ReadHintedLine(dict, gameDict, item => item);
            if (input.Length>0)
                CheckAliases(CheckFunctions(FixSpaces(SplitInput(FixRussian(ToLowercase(input)))), data),data);
        }

        void ReadJson()
        {
            if (File.Exists(@"data.json")==false)
            {
                File.WriteAllText("data.json", "{}");
            }
            using StreamReader r = new StreamReader(@"data.json");
            string json = r.ReadToEnd();
            data = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        }

        void ReadGameJson()
        {
            if (File.Exists(@"gamedata.json") == false)
            {
                File.WriteAllText("gamedata.json","{}");
            }
            using StreamReader r = new StreamReader(@"gamedata.json");
            string json = r.ReadToEnd();
            gameData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        }

        string FixRussian(string input)
        {
            Dictionary<char, char> ru_eng = new Dictionary<char, char>{['й']='q',['ц']='w',['у']='e',['к']='r',['е']='t',['н']='y',['г']='u',['ш']='i',['щ']='o',['з']='p',['х']='[',['ъ']=']',['ф']='a',['ы']='s',['в']='d',['а']='f',['п']='g',['р']='h',['о']='j',['л']='k',['д']='l',['ж']=';',['э']='\'',['я']='z',['ч']='x',['с']='c',['м']='v',['и']='b',['т']='n',['ь']='m',['б']=',',['ю']='.'};
            foreach (var letter in input)
            {
                if (ru_eng.Keys.Contains(letter))
                {
                    input = input.Replace(letter, ru_eng[letter]);
                }
            }
            return input;
        }

        string ToLowercase(string input)
        {
            return input.ToLower();
        }

        string[] SplitInput(string input)
        {
            return input.Split();
        }

        string[] FixSpaces(string[] input)
        {
            bool started = false;
            List<string> final_array = new List<string>();
            string final_string = "";
            foreach (var item in input){
                if (started)
                    final_string += " " + item;
                else if (item.Length > 1 && item[0] == '"')
                {
                    final_string = item;
                    started = true;
                }
                else final_array.Add(item);
                if (item.Length>1 && item[item.Length - 1] == '"')
                {
                    started = false;
                    final_array.Add(final_string);
                }
            }
            return final_array.ToArray();
        }

        string CheckFunctions(string[] input, Dictionary<string, List<string>> dataSet)
        {
            switch (input[0])
            {
                case "q":
                    Quit();
                    return "IS_A_FUNCTION";
                case "del":
                    Del(input, dataSet);
                    return "IS_A_FUNCTION";
                case "add":
                    Add(input, dataSet);
                    return "IS_A_FUNCTION";
                case "set":
                    Set(input, dataSet);
                    return "IS_A_FUNCTION";
                case "all":
                    All(dataSet);
                    return "IS_A_FUNCTION";
                case "?":
                    Hlp(input, dataSet);
                    return "IS_A_FUNCTION";
                case "g":
                    Games(input);
                    return "IS_A_FUNCTION";
                case "chg":
                    Change(input, dataSet);
                    return "IS_A_FUNCTION";
            }
            return input[0];
        }

        void Change(string[] input, Dictionary<string, List<string>> dataSet)
        {
            if (input.Length < 3)
            {
                Console.WriteLine("Wait for 2 arguments");
                return;
            }
            Console.WriteLine(input[1]);
            string oldPath = FindPath(input[1], dataSet);
            string newPath = input[2];
            if (oldPath == "") Console.WriteLine("No such alias");
            else
            {
                bool first = true;
                foreach (string alias in dataSet[oldPath])
                    if (first == true)
                    {
                        dataSet[newPath] = new List<string>() { alias };
                        first = false;
                    }
                    else dataSet[newPath].Add(alias);
                dataSet.Remove(oldPath);
                Save();
                Console.WriteLine(input[1] + " changed to: " + newPath);
            }
        }

        void Games(string[] input)
        {
            if (input.Length > 1)
                input = input.Skip(1).ToArray();
            else
                input[0] = "all";
            CheckAliases(CheckFunctions(input, gameData), gameData);
        }

        void Quit()
        {
            IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(h, 0);
            while (GetAsyncKeyState(0x13) != -32767)
            {
                System.Threading.Thread.Sleep(1);
            }
            ShowWindow(h, 5);
            SetForegroundWindow(h);
            System.Threading.Thread.Sleep(20);
            SetForegroundWindow(h);
            Console.Clear();
            //Environment.Exit(0);
        }

        void Save()
        {
            using StreamWriter file = File.CreateText("data.json");
            using StreamWriter gameFile = File.CreateText("gamedata.json");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, data);
            serializer.Serialize(gameFile, gameData);
            UpdateDicts();
        }
        
        public void UpdateDicts()
        {
            dict.Clear();
            foreach (var line in data)
            {
                foreach (var name in line.Value)
                {
                    dict.Add(name);
                }
            }
            gameDict.Clear();
            foreach (var line in gameData)
            {
                foreach (var name in line.Value)
                {
                    gameDict.Add(name);
                }
            }
        }

        void Alias(string input)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo(input);
            process.StartInfo.UseShellExecute = true;
            process.Start();
            Quit();
        }

        void Add(string[] input, Dictionary<string, List<string>> dataSet)
        {
            if (input.Length < 3)
                Console.WriteLine("Wait for 2 arguments");
            else
            {
                string path = input[1];
                string alias = input[2];
                string repeat = FindPath(alias, dataSet, false);
                if (repeat!="" || functions.Contains(alias))
                {
                    Console.WriteLine("Alias already defined");
                    return;
                }
                if (dataSet.ContainsKey(path))
                {
                    dataSet[path].Add(alias);
                    Console.WriteLine("added " + alias);
                }
                else
                {
                    dataSet[path] = new List<string>() {alias};
                    Console.WriteLine("created " + alias);
                }
                Save();
            }
        }

        void Del(string[] input, Dictionary<string, List<string>> dataSet)
        {
            if (input.Length < 2)
                Console.WriteLine("Wait for 1 argument");
            else
            {
                string path = FindPath(input[1], dataSet);
                if (path != "")
                {
                    dataSet[path].Remove(input[1]);
                    Console.WriteLine("deleted " + input[1]);
                    if (dataSet[path].Count == 0)
                    {
                        dataSet.Remove(path);
                        Console.WriteLine("deleted " + path);
                    }
                }
                else Console.WriteLine("No such alias");
                Save();
            }
        }

        void Set(string[] input, Dictionary<string, List<string>> dataSet)
        {
            if (input.Length < 3)
                Console.WriteLine("Wait for 2 arguments");
            else
            {
                string path = FindPath(input[1], dataSet, false);
                string alias = input[2];
                string repeat = FindPath(alias, dataSet, false);
                if (repeat != "" || functions.Contains(alias))
                {
                    Console.WriteLine("Alias already defined");
                    return;
                }
                if (dataSet.ContainsKey(path))
                {
                    dataSet[path].Add(alias);
                    Console.WriteLine("added " + alias);
                }
                else
                {
                    Console.WriteLine("No such alias - " + input[1]);
                }
                Save();
            }
        }

        void All(Dictionary<string, List<string>> dataSet)
        {
            int count = 0;
            foreach (var item in dataSet.Values)
            {
                Console.WriteLine(string.Join(" | ",item));
                count++;
            }
            Console.WriteLine("_______________");
            Console.WriteLine("Total Apps: " + count);
        }

        void Hlp(string[] input, Dictionary<string, List<string>> dataSet)
        {
            if (input.Length < 2)
                Console.WriteLine("Wait for 1 argument");
            else
            {
                string path = FindPath(input[1], dataSet);
                if (path != "")
                    Console.WriteLine(input[1] + "=" + path);
                else
                    Console.WriteLine("No such alias");
            }
        }

        string FindPath(string input, Dictionary<string, List<string>> dataSet, bool stein = true)
        {
            List<string> dictSet;
            if (dataSet == data)
                dictSet = dict;
            else
                dictSet = gameDict;
            foreach (var path in dataSet.Keys)
            {
                foreach (var alias in dataSet[path])
                {
                    if (input == alias)
                        return path;
                }
            }
            if (stein)
            {
            Dictionary<string, int> steintest = new Dictionary<string, int>();
            foreach (var word in dictSet)
            {
                steintest.Add(word,Livenstein.Distance(input, word));
            }
            string min = Livenstein.MinName(steintest);
            if (min != "")
            {
                Console.WriteLine(min);
                string path = FindPath(min, dataSet);
                return path;
            }
            }
            return "";
        }
        
        void CheckAliases(string input, Dictionary<string, List<string>> dataSet)
        {
            if (input == "IS_A_FUNCTION") return;
            string path = FindPath(input, dataSet);
            if (path != "")
                Alias(path);
            else
                Console.WriteLine("Unknown alias");
        }

    }
}
