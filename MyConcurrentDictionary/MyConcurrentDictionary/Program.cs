using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Concurrency;
using System.IO;
using System.Runtime.InteropServices;

namespace MyConcurrentDictionary
{
    internal class Program
    {
        private static int THREAD_COUNT = 1;
        private static int TOTAL_INSERTS = 3000000;
        private static int LOOP_COUNT = TOTAL_INSERTS / THREAD_COUNT;
        private static int STRING_LENGTH = 20;

        private static string fileName = string.Empty;

        private static Concurrency.ConcurrentDictionary Dict = null;
        private static string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz 0123456789";

        private static ConcurrentBag<double> insertTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> searchTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> deleteTimes = new ConcurrentBag<double>();

        static void Main(string[] args)
        {
            bool bInserts = true;

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++) 
                {
                    switch (i)
                    {
                        case 0:
                            if (args[i].ToLower().CompareTo("build") == 0)
                            {
                                bInserts = false;
                            }
                            break;
                        case 1:
                            TOTAL_INSERTS = Int32.Parse(args[i]);
                            LOOP_COUNT = TOTAL_INSERTS / THREAD_COUNT;
                            break;
                        case 2:
                            fileName = args[i];
                            break;
                    }
                }
            }

            try
            {
                if (bInserts)
                {
                    using (Dict = new Concurrency.ConcurrentDictionary())
                    {
                        InsertTest();
                    }
                    Dict = null;
                }
                else
                {
                    BuildTest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void InsertTest()
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(fileName))
                {
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        writer.WriteLine($"Thread Count:  {THREAD_COUNT}");
                        writer.WriteLine($"String Length: {STRING_LENGTH}");
                        writer.WriteLine($"Start Time: {DateTime.Now}");
                        writer.WriteLine();
                        writer.WriteLine();
                        writer.WriteLine(",Inserts,,,Search,,,Size,Min,Max,,Run Time");
                    }
                }
            }

            Console.WriteLine("===================================================================================");
            Console.WriteLine("================================ Insert Test ======================================");
            Console.WriteLine("===================================================================================");

            Stopwatch sw = Stopwatch.StartNew();
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

            Stopwatch sw2 = Stopwatch.StartNew();
            int count = Dict.Size();
            sw2.Stop();

            Stopwatch sw3 = Stopwatch.StartNew();
            int min = Dict.Min();
            sw3.Stop();

            Stopwatch sw4 = Stopwatch.StartNew();
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
            if (!String.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(fileName))
                {
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        writer.WriteLine($"Thread Count:  1");
                        writer.WriteLine($"String Length: {STRING_LENGTH}");
                        writer.WriteLine($"Start Time: {DateTime.Now}");
                        writer.WriteLine();
                        writer.WriteLine();
                        writer.WriteLine(",Builds,,,Deletes,,Run Time");
                    }
                }
            }

            Console.WriteLine("===================================================================================");
            Console.WriteLine("================================ Build Test =======================================");
            Console.WriteLine("===================================================================================");

            Stopwatch sw = Stopwatch.StartNew();

            List<Tuple<string, int>> list = BuildList();
            Stopwatch sw1 = Stopwatch.StartNew();
            using (Dict = ConcurrentDictionary.Build(list))
            {
                sw1.Stop();


                for (int i = 0; i < 10; i++)
                {
                    // not found
                    string scrambleStr = new string(chars.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
                    Random rnd = new Random();
                    string key = RandomString(scrambleStr, STRING_LENGTH, rnd);
                    Stopwatch swDeletes = Stopwatch.StartNew();
                    try
                    {
                        Dict.Delete(key);
                    }
                    catch (Exception)
                    {
                        // Delete throws an exception if the
                        // key is not found. We are doing that
                        // deliberately here, so we just
                        // swallow the exception
                    }
                    swDeletes.Stop();
                    deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);

                    // first
                    key = Dict.SearchValue(true, false);
                    swDeletes = Stopwatch.StartNew();
                    Dict.Delete(key);
                    swDeletes.Stop();
                    deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);

                    // last
                    key = Dict.SearchValue(false, true);
                    swDeletes = Stopwatch.StartNew();
                    Dict.Delete(key);
                    swDeletes.Stop();
                    deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);

                    // middle
                    key = Dict.SearchValue(false, false);
                    swDeletes = Stopwatch.StartNew();
                    Dict.Delete(key);
                    swDeletes.Stop();
                    deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);
                }

                sw.Stop();
                TimeSpan totalTime = new TimeSpan(sw.ElapsedTicks);
                TimeSpan insertTime = new TimeSpan(sw1.ElapsedTicks);
                double maxDelete = deleteTimes.Max();

                Console.WriteLine($"Total Time:         {totalTime}");
                Console.WriteLine($"Insert Time:        {insertTime.TotalSeconds}");
                Console.WriteLine($"Max Delete seconds: {maxDelete}");
                Console.WriteLine();
                Console.WriteLine();

                if (!String.IsNullOrEmpty(fileName))
                {
                    using (StreamWriter writer = File.AppendText(fileName))
                    {
                        writer.WriteLine($"{TOTAL_INSERTS},{insertTime.TotalSeconds},,{TOTAL_INSERTS},{maxDelete},,{totalTime}");
                    }
                }
            }
            Dict = null;
        }

        private static string RandomString(string startString, int length, Random random)
        {
            return new string(Enumerable.Repeat(startString, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void Insert()
        {
            try
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

                    Stopwatch sw = Stopwatch.StartNew();
                    Dict.Insert(key, val);
                    sw.Stop();
                    insertTimes.Add((double)sw.ElapsedTicks / 10000000);
                }

                foreach (string key in keys)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    int val2 = Dict.Search(key);
                    sw.Stop();
                    searchTimes.Add((double)sw.ElapsedTicks / 10000000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
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
    }
}
