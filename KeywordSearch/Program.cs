using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeywordSearch
{
    public class Program
    {
        static List<OutputList> CalData = new List<OutputList>();
        static List<OutputList> ResultData = new List<OutputList>();
        static Bm boyer = new Bm();
        static string path = @"product.txt";
        static string[] lines = null;
        static string[] allkey;
        static void Main(string[] args)
        {
            try
            {
                readData();
                if (lines != null)
                {
                    Console.Write("Product Search - Input your keyword (s) : ");
                    string key = Console.ReadLine();

                    char[] delimeter = { ' ' };
                    allkey = key.Split(delimeter, StringSplitOptions.RemoveEmptyEntries);

                    //Search
                    Search();

                    //Show Result
                    Console.WriteLine("Search Result is:"+Environment.NewLine);
                    foreach (OutputList item in ResultData)
                    {
                        Console.WriteLine("- "+lines[item.ID]);
                    }
                    Console.WriteLine(Environment.NewLine+"- " + ResultData.Count + " product(s) matched");
                    Console.Read();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program Error : " + ex.Message + " :" + ex.Data);
                Console.Read();
            }

        }

        static void readData()
        {
            lines = null;
            if (File.Exists(path))
            {
                lines = System.IO.File.ReadAllLines(path);
            }
            else Console.WriteLine("Error, Product file not found");
            
        }
        
        static void Search()
        {
            int i = 0;
            foreach (string item in lines)
            {
                List<int> data = new List<int>();
                int j = 0;
                foreach (string patt in allkey)
                {
                   //foreach pattern from key word will be checked in here.
                    int[] Value = boyer.BM_Matcher(item.ToLower(), patt.ToLower());
                    if (Value.Length > 0)
                    {
                        data.Add(Value[0]);
                    }
                    j++;
                }
                if (data.Count > 0)
                {
                    //add data in list for do sorting
                    OutputList insertData = new OutputList();
                    insertData.ID = i;
                    insertData.firstWord = data[0];
                    int start = data[0];
                    if (data.Count > 1)
                    {
                        for (int p = 1; p < data.Count; p++)
                        {
                            start = start - data[p];
                        }
                        insertData.MinDistance = Math.Abs(start);
                    }
                    CalData.Add(insertData);
                }
                i++;
            }

            Sorting();
        }

        static void Sorting()
        {
            CalData = CalData.OrderBy(a => a.firstWord).ToList();
            List<int> SubListFtWord = CalData.Select(a => a.firstWord).Distinct().ToList();
            foreach (int item in SubListFtWord)
            {
                List<OutputList> SubList = CalData.Where(a => a.firstWord == item).ToList();
                SubList = SubList.OrderBy(b => b.MinDistance).ToList();
                ResultData.AddRange(SubList);
            }
            
        }

    }

    //boyer-moore-algorithm
    public class Bm
    {
        public Kmp _kmp = new Kmp();
        public int[] Compute_Last_Occurence_Function(string pattern)
        {
            const int sigma_size = 128;
            int m = pattern.Length;
            int[] lambda = new int[sigma_size];
            for (int i = 0; i < lambda.Length; i++)
            {
                lambda[i] = -1;
            }
            for (int j = 0; j < m; j++)
            {
                lambda[(int)pattern[j]] = j;
            }
            return lambda;
        }
        public int[] Compute_Good_Suffix_Function(string pattern)
        {
            int m = pattern.Length;
            var pi = _kmp.ComputePrefixFunction(pattern);
            var P_0 = Reverse(pattern);
            var pi_0 = _kmp.ComputePrefixFunction(P_0);
            int[] gamma = new int[m + 1];
            int j;
            for (j = 0; j <= m; j++)
            {
                gamma[j] = (m - 1) - pi[m - 1];
            }
            for (int i = 0; i < m; i++)
            {
                j = (m - 1) - pi_0[i];
                if (gamma[j] > i - pi_0[i])
                {
                    gamma[j] = i - pi_0[i];
                }
            }
            return gamma;
        }

        private string Reverse(string text)
        {
            if (text == null)
                return null;
            char[] array = text.ToCharArray();
            Array.Reverse(array);
            return new String(array);
        }

        public int[] BM_Matcher(string Text, string Pattern)
        {
            int n = Text.Length;
            int m = Pattern.Length;
            int[] lambda = Compute_Last_Occurence_Function(Pattern);
            int[] gamma = Compute_Good_Suffix_Function(Pattern);
            int s = 0;
            List<int> validshifts = new List<int>();

            while (s <= n - m)
            {
                int j = m - 1;
                while (j >= 0 && Pattern[j] == Text[s + j])
                {
                    j--;
                }
                if (j < 0)
                {
                    validshifts.Add(s);
                    s = s + gamma[0];
                }
                else
                {
                    s = s + Math.Max(gamma[j], j - lambda[Text[s + j]]);
                }
            }
            return validshifts.ToArray();
        }
    }
    //Knuth-Morris-Pratt_Algorithm
    public class Kmp
    {
        public void kmp_search(string P, string T)
        {
            int n = T.Length;
            int m = P.Length;
            int[] pi = ComputePrefixFunction(P);
            int q = 0;

            for (int i = 1; i <= n; i++)
            {
                while (q > 0 && P[q] != T[i - 1])
                {
                    q = pi[q - 1];
                }
                if (P[q] == T[i - 1]) { q++; }
                if (q == m)
                {
                    //Record a match was found here  
                    q = pi[q - 1];
                }
            }
        }

        public int[] ComputePrefixFunction(string P)
        {
            int m = P.Length;
            int[] pi = new int[m];
            int k = 0;
            pi[0] = 0;

            for (int q = 1; q < m; q++)
            {
                while (k > 0 && P[k] != P[q]) { k = pi[k]; }

                if (P[k] == P[q]) { k++; }
                pi[q] = k;
            }
            return pi;
        }  
    }

    public class OutputList
    {
        public int ID { get; set; }
        public int firstWord { get; set; }
        public int MinDistance { get; set; }

        public OutputList()
        {
            MinDistance = int.MaxValue;
        }
    }

}
