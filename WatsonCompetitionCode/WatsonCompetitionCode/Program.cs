using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonCompetitionCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Util utility = new Util();
            Dictionary<int, Canidate> dict = utility.csvReader(args[1]);
            utility.removeExtraneouData(dict);
            switch (args[0])
            {
                case "Tree":
                    
                    Console.Write("Starting Tree Building");
                    DecisionTrees tree = new DecisionTrees(dict, 20);
                    Console.Write("Built Tree");
                    Console.Write("Evaluating data");
                    List<float> answers = tree.runTree(dict, 20);
                    Console.Write("Completed");
                    break;

                case "RBFNetwork":
                    
                    List<bool> results = new List<bool>();
                    bool RBF = true;
                    bool DecTree = false;
                    bool LogReg = false;
                    Dictionary<int, Canidate> newDict = new Dictionary<int, Canidate>();
                    Canidate cand = new Canidate();
                    dict.TryGetValue(5, out cand);
                    RBFnetwork rbf = new RBFnetwork(150, cand.featuresRating.Count(), 0.1);
                    newDict = dict;
                    rbf.trainSystem(newDict);
                    break;

                case "LogisticRegression":
                    LogisiticRegression ai = new LogisiticRegression(dict);
                    ai.train();
                    break;
                default:
                    Console.WriteLine("Invalid parameters");
                    break;
            }
        }

       
    }

    class Canidate
    {
        public int rowNumber;
        public float questionNumber;
        public List<float> featuresRating;
        public Boolean isTrue;

        public Canidate()
        {
            rowNumber = 0;
            questionNumber = 0;
            featuresRating = new List<float>();
            isTrue = false;
        }
    }


    class Util
    {
        public Dictionary<int, Canidate> csvReader(string fileName)
        {
            var reader = new StreamReader(File.OpenRead(@fileName));
            List<string> lineRead = new List<string>();
            Dictionary<int, Canidate> canidates = new Dictionary<int, Canidate>();
            int index = 1;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (!string.IsNullOrEmpty(values[0]))
                {
                    Canidate canidate = new Canidate();
                    canidate.rowNumber = Convert.ToInt32(values[0]);
                    canidate.questionNumber = Convert.ToSingle(values[1]);
                    for (int i = 2; i < values.Count(); i++)
                    {
                        if (values[i] == "true" || values[i] == "false" || values[i] == "TRUE" || values[i] == "FALSE")
                        {
                            if (values[i] == "true" || values[i] == "TRUE") canidate.isTrue = true;
                            else canidate.isTrue = false;
                            break;
                        }
                        canidate.featuresRating.Add(Convert.ToSingle(values[i]));
                    }
                    canidates.Add(index, canidate);
                    index++;
                }
            }
            reader.Close();
            return canidates;
        }

        public void csvWriter(string fileName, Dictionary<int, Canidate> candidates)
        {
            File.Delete(@fileName);
            var writer = new StreamWriter(File.OpenWrite(@fileName));
            Canidate candidate = new Canidate();
            string stringToWrite = "";
            int i = 1;
            while (candidates.ContainsKey(i))
            {
                candidates.TryGetValue(i, out candidate);
                stringToWrite = candidate.rowNumber.ToString() + ',';
                stringToWrite += candidate.questionNumber.ToString() + ',';
                for (int k = 0; k < candidate.featuresRating.Count(); k++)
                {
                    stringToWrite += candidate.featuresRating[k].ToString() + ',';
                }
                if (candidate.isTrue) stringToWrite += "TRUE";
                else stringToWrite += "FALSE";
                writer.WriteLine(stringToWrite);
                i++;
            }
            writer.Close();
        }

        public void fileWriter(List<bool> results, Dictionary<int, Canidate> candidates, string fileName)
        {
            File.Delete(@fileName);
            var writer = new StreamWriter(File.OpenWrite(@fileName));
            int i = 1;
            Canidate candidate = new Canidate();
            string stringToWrite = "";
            string f = "FALSE: ";
            string t = "TRUE: ";
            while (candidates.ContainsKey(i))
            {
                candidates.TryGetValue(i, out candidate);
                stringToWrite = candidate.rowNumber.ToString();
                if (results[i - 1] == true)
                {
                    if (candidate.isTrue == true) writer.WriteLine(stringToWrite);
                    else writer.WriteLine(f + stringToWrite);
                    // writer.WriteLine(stringToWrite);
                }
                else // for debuging only (remove for final program)
                {
                    if (candidate.isTrue == true) writer.WriteLine(t + stringToWrite);
                }
                i++;
            }
            writer.Close();
        }

        public List<int> removeExtraneouData(Dictionary<int, Canidate> canidates)
        {
            //intialize size of array of columns
            List<List<float>> columns = new List<List<float>>();
            int columnCount = canidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<float>());
            }

            //Generate Array of Columns
            foreach (KeyValuePair<int, Canidate> pair in canidates)
            {
                Canidate c = pair.Value;
                int length = c.featuresRating.Count;
                for (int i = 0; i < length; i++)
                {
                    columns[i].Add(c.featuresRating[i]);
                }
            }

            //Add columns to remove that contain same data
            List<int> removeData = new List<int>();
            for (int k = 0; k < columnCount - 1; k++)
            {
                for (int i = k + 1; i < columnCount; i++)
                {
                    if (columns[k].Sum() == columns[i].Sum())
                    {
                        bool isRemove = true;
                        for (int j = 0; j < columns[k].Count; j++)
                        {
                            if (columns[k][j] != columns[i][j])
                            {
                                isRemove = false;
                                break;
                            }
                        }
                        if (isRemove && !removeData.Contains(i))
                        {
                            removeData.Add(i);
                        }
                    }
                }
            }
            //Add columns that have all same values (without doubling up on previous addition)
            for (int k = 0; k < columns.Count; k++)
            {
                float firstElement = columns[k][0];
                bool isRemove = true;
                foreach (float feature in columns[k])
                {
                    if (firstElement != feature)
                    {
                        isRemove = false;
                        break;
                    }
                }
                if (isRemove && !removeData.Contains(k))
                {
                    removeData.Add(k);
                }
            }

            List<int> result = removeData.ToList();

            while (removeData.Count > 0)
            {
                foreach (KeyValuePair<int, Canidate> pair in canidates)
                {
                    pair.Value.featuresRating.RemoveAt(removeData.Max());
                }
                columns.RemoveAt(removeData.Max());
                //Console.WriteLine("Removed Column " + removeData.Max());
                removeData.Remove(removeData.Max());
            }

            List<float> maxValues = new List<float>();
            foreach (List<float> column in columns)
            {
                float absMax;
                if (Math.Abs(column.Min()) > Math.Abs(column.Max())) absMax = Math.Abs(column.Min());
                else absMax = Math.Abs(column.Max());
                if (absMax == 0.0) absMax = (float)1.0;
                maxValues.Add(absMax);
            }

            for (int k = 0; k < maxValues.Count; k++)
            {
                foreach (KeyValuePair<int, Canidate> canidate in canidates)
                {
                    canidate.Value.featuresRating[k] = (canidate.Value.featuresRating[k] / maxValues[k]);
                }
            }

            return result;
        }

        public void removeMore(Dictionary<int, Canidate> canidates, string fileName)
        {
            // Assumes file is 1 indexed (like Matlab)
            var reader = new StreamReader(File.OpenRead(@fileName));
            var line = reader.ReadLine();
            var values = line.Split(',');
            List<int> keepIndex = new List<int>();
            foreach (string val in values)
            {
                keepIndex.Add(Convert.ToInt32(val));
            }
            reader.Close();

            //intialize size of array of columns
            List<List<float>> columns = new List<List<float>>();
            int columnCount = canidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<float>());
            }

            //Generate Array of Columns
            foreach (KeyValuePair<int, Canidate> pair in canidates)
            {
                Canidate c = pair.Value;
                int length = c.featuresRating.Count;
                for (int i = 0; i < length; i++)
                {
                    columns[i].Add(c.featuresRating[i]);
                }
            }

            //Add columns to remove that contain same data
            List<int> removeData = new List<int>();
            for (int k = 0; k < columnCount - 1; k++)
            {
                if (!keepIndex.Contains(k + 1))
                {
                    removeData.Add(k);
                }
            }

            List<int> result = removeData.ToList();
            while (removeData.Count > 0)
            {
                foreach (KeyValuePair<int, Canidate> pair in canidates)
                {
                    pair.Value.featuresRating.RemoveAt(removeData.Max());
                }
                columns.RemoveAt(removeData.Max());
                //Console.WriteLine("Removed Column " + removeData.Max());
                removeData.Remove(removeData.Max());
            }
        }

        public Dictionary<int, Canidate> randSample(Dictionary<int, Canidate> candidates, int FperT)
        {
            Dictionary<int, Canidate> final = new Dictionary<int, Canidate>();
            int i = 1;
            Random random = new Random();
            int average = (int)55 / FperT;
            int index = (int)(random.NextDouble() * (2 * average));
            int count = 0;
            foreach (KeyValuePair<int, Canidate> pair in candidates)
            {
                Canidate c = pair.Value;
                if (c.isTrue)
                {
                    final.Add(i, c);
                    i++;
                }
                else
                {
                    if (count == index)
                    {
                        final.Add(i, c);
                        i++;
                        count = 0;
                        index = (int)(random.NextDouble() * (2 * average));
                    }
                    count++;
                }
            }

            return final;
        }

        public void dataAnalysis(Dictionary<int, Canidate> candidates)
        {
            List<double> meanT = new List<double>();
            List<double> standDevT = new List<double>();
            int numTrue = 0;
            List<double> meanF = new List<double>();
            List<double> standDevF = new List<double>();
            int numFalse = 0;
            Canidate candidate = new Canidate();

            //intialize size of array of columns
            List<List<double>> columns = new List<List<double>>();
            int columnCount = candidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<double>());
                meanT.Add(0);
                standDevT.Add(0);
                meanF.Add(0);
                standDevF.Add(0);
            }

            //Generate Array of Columns
            int length = 0;
            foreach (KeyValuePair<int, Canidate> pair in candidates)
            {
                Canidate c = pair.Value;
                length = c.featuresRating.Count;
                for (int i = 0; i < length; i++)
                {
                    columns[i].Add(c.featuresRating[i]);
                }
            }

            // find means
            int j = 0;
            for (int i = 0; i < length; i++)
            {
                j = 1;
                while (candidates.ContainsKey(j))
                {
                    candidates.TryGetValue(j, out candidate);
                    if (candidate.isTrue)
                    {
                        if (i == 0) numTrue++;
                        meanT[i] += columns[i][j - 1];
                    }
                    else
                    {
                        if (i == 0) numFalse++;
                        meanF[i] += columns[i][j - 1];
                    }
                    j++;
                }
            }

            for (int i = 0; i < length; i++)
            {
                meanF[i] /= numFalse;
                meanT[i] /= numTrue;
            }

            // find standard deviation
            for (int i = 0; i < length; i++)
            {
                j = 1;
                while (candidates.ContainsKey(j))
                {
                    candidates.TryGetValue(j, out candidate);
                    if (candidate.isTrue)
                    {
                        standDevT[i] += Math.Pow(columns[i][j - 1] - meanT[i], 2);
                    }
                    else
                    {
                        numFalse++;
                        standDevF[i] += Math.Pow(columns[i][j - 1] - meanF[i], 2);
                    }
                    j++;
                }
            }

            for (int i = 0; i < length; i++)
            {
                standDevF[i] = Math.Sqrt(standDevF[i] / numFalse);
                standDevT[i] = Math.Sqrt(standDevT[i] / numTrue);
            }

            File.Delete("meanVStandDev.txt");
            var writer = new StreamWriter(File.OpenWrite("meanVStandDev.txt"));
            for (int i = 0; i < length; i++)
            {
                writer.WriteLine(meanT[i].ToString() + ',' + standDevT[i].ToString() + ',' + meanF[i].ToString() + ',' + standDevF[i].ToString());
            }
            writer.Close();
        }
    }
}
