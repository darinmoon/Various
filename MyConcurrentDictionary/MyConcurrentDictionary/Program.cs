using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using Concurrency;
using System.IO;

namespace MyConcurrentDictionary
{
    internal class Program
    {
        #region Private Static Constants

        private static int THREAD_COUNT = 10;
        private static int TOTAL_INSERTS = 3000000;
        private static int LOOP_COUNT = TOTAL_INSERTS / THREAD_COUNT;
        private static int STRING_LENGTH = 20;
        private static string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz 0123456789";

        #endregion

        #region Private Static Variables

        private static string fileName = string.Empty;
        private static ConcurrentDictionary Dict = null;

        private static ConcurrentBag<double> insertTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> searchTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> deleteTimes = new ConcurrentBag<double>();

        #endregion

        #region Public Static Methods

        static void Main(string[] args)
        {
            bool bInserts = true;

            // process the arguments if any
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++) 
                {
                    switch (i)
                    {
                        case 0:
                            // inserts or builds
                            if (args[i].ToLower().CompareTo("build") == 0)
                            {
                                bInserts = false;
                            }
                            break;
                        case 1:
                            // how many are we inserting into the dictionary
                            TOTAL_INSERTS = Int32.Parse(args[i]);
                            LOOP_COUNT = TOTAL_INSERTS / THREAD_COUNT;
                            break;
                        case 2:
                            // filename to save the timings to
                            fileName = args[i];
                            break;
                    }
                }
            }

            try
            {
                if (bInserts)
                {
                    // Test the inserts, searchs, size, min, and max
                    using (Dict = new Concurrency.ConcurrentDictionary())
                    {
                        InsertTest();
                    }
                    Dict = null;
                }
                else
                {
                    // Test the builds and deletes
                    BuildTest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Private Methods

        private static void InsertTest()
        {
            // write the test header to the file
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

            // initialize the threads array
            Thread[] threads = new Thread[THREAD_COUNT];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ThreadStart(Insert));
            }

            // start all of the threads
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            Console.WriteLine($"Threads:            {threads.Length}");
            Console.WriteLine($"Total Inserts:      {TOTAL_INSERTS}");

            // block until all of the threads are done
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            // Test the size method
            Stopwatch sw2 = Stopwatch.StartNew();
            int count = Dict.Size();
            sw2.Stop();

            // test the min method
            Stopwatch sw3 = Stopwatch.StartNew();
            int min = Dict.Min();
            sw3.Stop();

            // test the max method
            Stopwatch sw4 = Stopwatch.StartNew();
            int max = Dict.Max();
            sw4.Stop();

            // gather and output the results
            TimeSpan size = new TimeSpan(sw2.ElapsedTicks);
            TimeSpan minTime = new TimeSpan(sw3.ElapsedTicks);
            TimeSpan maxTime = new TimeSpan(sw4.ElapsedTicks);
            double maxInsert = insertTimes.Max();
            double maxSearch = searchTimes.Max();

            sw.Stop();
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
        }

        private static void BuildTest()
        {
            // write the test header to the file
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

            // load the list of tuples
            List<Tuple<string, int>> list = BuildList();
            Stopwatch sw1 = Stopwatch.StartNew();

            // Test the Build method
            using (Dict = ConcurrentDictionary.Build(list))
            {
                sw1.Stop();

                // loop to test different cases of the Delete method
                for (int i = 0; i < 10; i++)
                {
                    // not found
                    string scrambleStr = new string(CHARACTERS.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
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

                // gather and output the results
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

        // Generat a random string of the given length
        private static string RandomString(string startString, int length, Random random)
        {
            return new string(Enumerable.Repeat(startString, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Thread method for inserts
        private static void Insert()
        {
            try
            {
                List<string> keys = new List<string>();
                // we need to start each thread with a scrambled version of the CHARACTERS string
                // from which to generate random strings so that we don't end up with a bunch of duplicates
                string scrambleStr = new string(CHARACTERS.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
                Random rnd = new Random();
                for (int i = 0; i < LOOP_COUNT; i++)
                {
                    // generate a random string and number to insert
                    string key = RandomString(scrambleStr, STRING_LENGTH, rnd);
                    int val = rnd.Next(int.MinValue, int.MaxValue);

                    // save a few keys to search for later
                    if (i % 20 == 0)
                    {
                        keys.Add(key);
                    }

                    // Insert
                    Stopwatch sw = Stopwatch.StartNew();
                    Dict.Insert(key, val);
                    sw.Stop();
                    insertTimes.Add((double)sw.ElapsedTicks / 10000000);
                }

                // Test the Search method by searching for the
                // strings saved previously
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

        // Builds a list of tuples of random strings and integers
        private static List<Tuple<string, int>> BuildList()
        {
            List<Tuple<string, int>> list = new List<Tuple<string, int>>();
            string scrambleStr = new string(CHARACTERS.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            Random rnd = new Random();
            for (int i = 0; i < TOTAL_INSERTS; i++)
            {
                list.Add(new Tuple<string, int>(RandomString(scrambleStr, STRING_LENGTH, rnd), rnd.Next(int.MinValue, int.MaxValue)));
            }
            return list;
        }

        #endregion
    }
}
