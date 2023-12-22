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
        private static int TOTAL_INSERTS = 100000;
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
        private static ConcurrentBag<double> sizeTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> minTimes = new ConcurrentBag<double>();
        private static ConcurrentBag<double> maxTimes = new ConcurrentBag<double>();

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

            // Now test Size, Min, and Max
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ThreadStart(TestSizeMinMax));
            }

            // start all of the threads
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            // block until all of the threads are done
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            // gather and output the results
            int count = Dict.Size();
            int min = Dict.Min();
            int max = Dict.Max();
            double sizeMax = sizeTimes.Max();
            double minMax = minTimes.Max();
            double maxMax = maxTimes.Max();
            double maxInsert = insertTimes.Max();
            double maxSearch = searchTimes.Max();

            sw.Stop();
            TimeSpan totalTime = new TimeSpan(sw.ElapsedTicks);

            Console.WriteLine($"Total Time:         {totalTime}");
            Console.WriteLine($"Max Insert seconds: {maxInsert}");
            Console.WriteLine($"Max Search seconds: {maxSearch}");
            Console.WriteLine($"Size:               {count},         seconds: {sizeMax}");
            Console.WriteLine($"Min:                {min},   seconds: {minMax}");
            Console.WriteLine($"Max:                {max},    seconds: {maxMax}");
            Console.WriteLine();
            Console.WriteLine();

            if (!String.IsNullOrEmpty(fileName))
            {
                using (StreamWriter writer = File.AppendText(fileName))
                {
                    writer.WriteLine($"{TOTAL_INSERTS},{maxInsert},,{TOTAL_INSERTS},{maxSearch},,{TOTAL_INSERTS},{sizeMax},{minMax},{maxMax},,{totalTime}");
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

                // we need lists of values to test deletions.
                // the difficult is making sure each thread
                // has different vaules for all 3 types of deletes
                // (first in bucket, last in bucket, middle of bucket)
                List<string>[] deleteFirstValues = new List<string>[THREAD_COUNT];
                List<string>[] deleteLastValues = new List<string>[THREAD_COUNT];
                List<string>[] deleteMiddleValues = new List<string>[THREAD_COUNT];

                // initialize the threads array
                Thread[] threads = new Thread[THREAD_COUNT];
                for (int i = 0; i < threads.Length; i++)
                {
                    // get lists of values to delete for each thread
                    deleteFirstValues[i] = new List<string>();
                    deleteLastValues[i] = new List<string>();
                    deleteMiddleValues[i] = new List<string>();

                    // Load values for the threads to delete
                    // making sure that there are no duplicates
                    // among any of the lists
                    for (int j = 0; j < 10; j++)
                    {
                        deleteFirstValues[i].Add(GetUniqueDictValue((j + i + 1) * 5, deleteFirstValues, deleteLastValues, deleteMiddleValues));
                        deleteLastValues[i].Add(GetUniqueDictValue((j + i + 2) * 307, deleteFirstValues, deleteLastValues, deleteMiddleValues));
                        deleteMiddleValues[i].Add(GetUniqueDictValue((j + i + 3) * 823, deleteFirstValues, deleteLastValues, deleteMiddleValues));
                    }

                    int idx = i;
                    threads[i] = new Thread(() => TestDelete(deleteFirstValues[idx], deleteLastValues[idx], deleteMiddleValues[idx]));
                }

                // start all of the threads
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Start();
                }

                // block until all of the threads are done
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i].Join();
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

        private static string GetUniqueDictValue(int idx, List<string>[] firsts, List<string>[] lasts, List<string>[] middles)
        {
            string val = null;
            int i = 0;
            bool duplicate = true;

            while (duplicate)
            {
                // reset the flag and get a value
                duplicate = false;
                val = Dict.SearchValue(true, false, idx + i++);

                // verify that the value isn't already in the "firsts" lists
                for (int j = 0; j < firsts.Length; j++)
                {
                    var list = firsts[j];
                    if (list != null)
                    {
                        foreach (string s in list)
                        {
                            if (s.CompareTo(val) == 0)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                        if (duplicate)
                        {
                            continue;
                        }
                    }
                }
                // make sure the value isn't already in the "lasts" lists
                for (int j = 0; j < lasts.Length; j++)
                {
                    var list = lasts[j];
                    if (list != null)
                    {
                        foreach (string s in list)
                        {
                            if (s.CompareTo(val) == 0)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                        if (duplicate)
                        {
                            continue;
                        }
                    }
                }
                // make sure the value isn't already in the "middles" lists
                for (int j = 0; j < middles.Length; j++)
                {
                    var list = middles[j];
                    if (list != null)
                    {
                        foreach (string s in list)
                        {
                            if (s.CompareTo(val) == 0)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                        if (duplicate)
                        {
                            continue;
                        }
                    }
                }
            }
            return val;
        }

        private static void TestDelete(List<string> firstValues, List<string> lastValues, List<string> middleValues)
        {
            // not found
            string scrambleStr = new string(CHARACTERS.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
            Random rnd = new Random();
            Stopwatch swDeletes = null;

            // loop to test different cases of the Delete method
            for (int i = 0; i < 10; i++)
            {
                string key = RandomString(scrambleStr, STRING_LENGTH, rnd);
                swDeletes = Stopwatch.StartNew();
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
            }

            foreach (string key in firstValues)
            {
                // first
                //key = Dict.SearchValue(true, false);
                swDeletes = Stopwatch.StartNew();
                Dict.Delete(key);
                swDeletes.Stop();
                deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);
            }

            foreach (string key in lastValues)
            {
                // last
                //key = Dict.SearchValue(false, true);
                swDeletes = Stopwatch.StartNew();
                Dict.Delete(key);
                swDeletes.Stop();
                deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);
            }

            foreach (string key in middleValues)
            {
                // middle
                //key = Dict.SearchValue(false, false);
                swDeletes = Stopwatch.StartNew();
                Dict.Delete(key);
                swDeletes.Stop();
                deleteTimes.Add((double)swDeletes.ElapsedTicks / 10000000);
            }
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
                Stopwatch sw;
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
                    sw = Stopwatch.StartNew();
                    Dict.Insert(key, val);
                    sw.Stop();
                    insertTimes.Add((double)sw.ElapsedTicks / 10000000);
                }

                // Test the Search method by searching for the
                // strings saved previously
                foreach (string key in keys)
                {
                    sw = Stopwatch.StartNew();
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

        private static void TestSizeMinMax()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    // Test the size method
                    Stopwatch sw = Stopwatch.StartNew();
                    int count = Dict.Size();
                    sw.Stop();
                    sizeTimes.Add((double)sw.ElapsedTicks / 10000000);

                    // test the min method
                    sw = Stopwatch.StartNew();
                    int min = Dict.Min();
                    sw.Stop();
                    minTimes.Add((double)sw.ElapsedTicks / 10000000);

                    // test the max method
                    sw = Stopwatch.StartNew();
                    int max = Dict.Max();
                    sw.Stop();
                    maxTimes.Add((double)sw.ElapsedTicks / 10000000);
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
