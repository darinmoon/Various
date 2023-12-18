﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Concurrency;
using System.IO;

namespace MyConcurrentDictionary
{
    internal class Program
    {
        private static int THREAD_COUNT = 10;
        private static int TOTAL_INSERTS = 100000;
        private static int LOOP_COUNT = TOTAL_INSERTS / THREAD_COUNT;
        private static int STRING_LENGTH = 20;

        private static string fileName = string.Empty;

        private static Concurrency.ConcurrentDictionary Dict = null;
        private static string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz 0123456789";

        private static ConcurrentBag<double> insertTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> searchTimes = new ConcurrentBag<double>();

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++) 
                {
                    switch (i)
                    {
                        case 0:
                            TOTAL_INSERTS = Int32.Parse(args[i]);
                            LOOP_COUNT = TOTAL_INSERTS / THREAD_COUNT;
                            break;
                        case 1:
                            fileName = args[i];
                            CreateFile();
                            break;
                    }
                }
            }

            try
            {
                using (Dict = new Concurrency.ConcurrentDictionary()) // TOTAL_INSERTS))
                {
                    InsertTest();
                }
                Dict = null;

                //BuildTest();

                int x;
                x = 0;
            }
            catch (Exception ex)
            {
                int x;
                x = 0;
            }
        }

        private static void InsertTest()
        {
            Console.WriteLine("===================================================================================");
            Console.WriteLine("================================ Insert Test ======================================");
            Console.WriteLine("===================================================================================");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Thread[] threads = new Thread[THREAD_COUNT];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ThreadStart(Insert));
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            Console.WriteLine($"Threads:            {threads.Length}");
            Console.WriteLine($"Total Inserts:      {TOTAL_INSERTS}");

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            sw.Stop();

            // not found
            string scrambleStr = new string(chars.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            Random rnd = new Random();
            string key = RandomString(scrambleStr, STRING_LENGTH, rnd);
            //int val1 = Dict.Search(key);

            // first
            //key = Dict.SearchValue(true, false);
            //int val2 = Dict.Search(key);

            //// last
            //key = Dict.SearchValue(false, true);
            //int val3 = Dict.Search(key);

            // middle
            //key = Dict.SearchValue(false, false);
            //int val4 = Dict.Search(key);

            //key = Dict.SearchValue(false, false);
            //Dict.Delete(key);
            //key = Dict.SearchValue(false, true);
            //Dict.Delete(key);
            //key = Dict.SearchValue(true, false);
            //Dict.Delete(key);
            //count = Dict.Size();

            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            int count = Dict.Size();
            sw2.Stop();

            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            int min = Dict.Min();
            sw3.Stop();

            Stopwatch sw4 = new Stopwatch();
            sw4.Start();
            int max = Dict.Max();
            sw4.Stop();

            TimeSpan size = new TimeSpan(sw2.ElapsedTicks);
            TimeSpan minTime = new TimeSpan(sw3.ElapsedTicks);
            TimeSpan maxTime = new TimeSpan(sw4.ElapsedTicks);
            double maxInsert = insertTimes.Max();
            double maxSearch = searchTimes.Max();

            TimeSpan totalTime = new TimeSpan(sw.ElapsedTicks);

            Console.WriteLine($"Total Time:         {totalTime}");
            Console.WriteLine($"Max Insert seconds: {maxInsert}");
            Console.WriteLine($"Max Search seconds: {maxSearch}");
            Console.WriteLine($"Size:               {count},         seconds: {size.TotalSeconds}");
            Console.WriteLine($"Min:                {min},   seconds: {minTime.TotalSeconds}");
            Console.WriteLine($"Max:                {max},    seconds: {maxTime.TotalSeconds}");
            Console.WriteLine();
            Console.WriteLine();

            if (!String.IsNullOrEmpty(fileName))
            {
                using (StreamWriter writer = File.AppendText(fileName))
                {
                    writer.WriteLine($"{TOTAL_INSERTS},{maxInsert},,{TOTAL_INSERTS},{maxSearch},,{TOTAL_INSERTS},{size.TotalSeconds},{minTime.TotalSeconds},{maxTime.TotalSeconds},,{totalTime}");
                }
            }

            count = 0;
        }

        private static void BuildTest()
        {
            Console.WriteLine("===================================================================================");
            Console.WriteLine("================================ Build Test =======================================");
            Console.WriteLine("===================================================================================");

            List<Tuple<string, int>> list = BuildList();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Dict = ConcurrentDictionary.Build(list);
            sw.Stop();

            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            int count = Dict.Size();
            sw2.Stop();

            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            int min = Dict.Min();
            sw3.Stop();

            Stopwatch sw4 = new Stopwatch();
            sw4.Start();
            int max = Dict.Max();
            sw4.Stop();

            Console.WriteLine($"Threads:            1");
            Console.WriteLine($"Build Time:         {TOTAL_INSERTS},         seconds: {(double)sw.ElapsedTicks / 10000000}");
            Console.WriteLine($"Size:               {count},         seconds: {(double)sw2.ElapsedTicks / 10000000}");
            Console.WriteLine($"Min:                {min},   seconds: {(double)sw3.ElapsedTicks / 10000000}");
            Console.WriteLine($"Max:                {max},    seconds: {(double)sw4.ElapsedTicks / 10000000}");
            Console.WriteLine();
            Console.WriteLine();

            Dict.Dispose();
            Dict = null;
            count = 0;
        }

        private static string RandomString(string startString, int length, Random random)
        {
            return new string(Enumerable.Repeat(startString, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void Insert()
        {
            List<string> keys = new List<string>();
            string scrambleStr = new string(chars.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            Random rnd = new Random();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                string key = RandomString(scrambleStr, STRING_LENGTH, rnd);
                int val = rnd.Next(int.MinValue, int.MaxValue);
                if (i % 20 == 0)
                {
                    keys.Add(key);
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Dict.Insert(key, val);
                sw.Stop();
                insertTimes.Add((double)sw.ElapsedTicks / 10000000);
            }

            foreach (string key in keys)
            {
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                int val2 = Dict.Search(key);
                sw.Stop();
                searchTimes.Add((double)sw.ElapsedTicks / 10000000);
            }
        }

        private static List<Tuple<string, int>> BuildList()
        {
            List<Tuple<string, int>> list = new List<Tuple<string, int>>();
            string scrambleStr = new string(chars.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            Random rnd = new Random();
            for (int i = 0; i < TOTAL_INSERTS; i++)
            {
                list.Add(new Tuple<string, int>(RandomString(scrambleStr, STRING_LENGTH, rnd), rnd.Next(int.MinValue, int.MaxValue)));
            }
            return list;
        }

        private static void CreateFile()
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(fileName))
                {
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        sw.WriteLine($"Thread Count:  {THREAD_COUNT}");
                        sw.WriteLine($"String Length: {STRING_LENGTH}");
                        sw.WriteLine($"Start Time: {DateTime.Now}");
                        sw.WriteLine();
                        sw.WriteLine();
                        sw.WriteLine(",Inserts,,,Search,,,Size,Min,Max,,Running Time");
                    }
                }
            }
        }
    }
}
