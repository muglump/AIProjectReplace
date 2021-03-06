﻿using System;
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
            if (args.Count() < 3) Console.WriteLine("Provide (1) Machine learning technique, (2) data file, (3) '1' to remove extraneous data, (4) '0' to not normalize data on removal");
            Util utility = new Util();
            Dictionary<int, Candidate> dict = new Dictionary<int, Candidate>();
            Dictionary<int, Candidate> trainDict = new Dictionary<int, Candidate>();
            Dictionary<int, Candidate> testDict = new Dictionary<int, Candidate>();
            FeatureData features = new FeatureData();
            List<bool> results = new List<bool>();
            bool normalize = true;
            if (args[3] == "0") normalize = false;

            switch (args[0])
            {
                case "Tree":
                    
                    dict = utility.csvReader(args[1]);

                    if (args[2] == "1") utility.removeExtraneousData(dict, normalize);
                    //utility.csvWriter("tgmc_cut1.csv", dict);

                    Console.Write("Starting Tree Building");
                    DecisionTrees tree = new DecisionTrees(dict, 20);
                    Console.Write("Built Tree");
                    Console.Write("Evaluating data");
                    List<double> answers = tree.runTree(dict, 20);
                    Console.Write("Completed");
                    break;

                case "FFNetwork":
                    dict = utility.csvReader(args[1]);
                    if (args[2] == "1") utility.removeExtraneousData(dict, normalize);
                    //utility.csvWriter("tgmc_cut1.csv", dict);

                    trainDict = utility.csvReader("tgmc_cut1.csv");
                    Candidate c = new Candidate();
                    trainDict.TryGetValue(1, out c);

                    bool allFeats = false;
                    //features = utility.removeMore(trainDict, "BestFeatures.txt");
                    features = utility.readFeatures("bestFeatures3.txt");
                    if (allFeats)
                    {
                        features.features.Add(new List<int>());
                        for (int j = 0; j < c.featuresRating.Count(); j++)
                        {
                            features.features[0].Add(j);
                            features.featuresVector.Add(j);
                        }
                    }

                    List<int> numLayers = new List<int>();
                    List<List<int>> NPL = new List<List<int>>();
                    List<double> Lrate = new List<double>();
                    List<int> sysT = new List<int>();
                    Random randN = new Random();
                    for (int k = 0; k < features.features.Count(); k++)
                    {
                        numLayers.Add((int)(3 * randN.NextDouble() + 1));
                        NPL.Add(new List<int>());
                        for (int j = 0; j < numLayers[k]; j++)
                        {
                            NPL[k].Add((int)(30 * randN.NextDouble() + 1));
                        }
                        Lrate.Add(0.002);
                        sysT.Add(2);
                    }

                    double CRate = 0.6;

                    EnsembleFF ff = new EnsembleFF(numLayers, NPL, features.features, Lrate, sysT);
                    ff.trainSystem(trainDict, CRate, -1);
                    ff.FFwriter("weightsFF.txt");

                    //EnsembleRBF rbf = new EnsembleRBF();
                    //rbf.RBFreader("weights.txt",-1);

                    results = ff.testSystem(testDict, CRate);
                    dict = testDict;

                    break;
                case "RBFNetwork":
                    
                    
                    dict = utility.csvReader(args[1]);

                    if (args[2] == "1") utility.removeExtraneousData(dict, normalize);
                    //utility.csvWriter("tgmc_cut1.csv", dict);

                    // EnsembleRBF(List<int> numNod, List<List<int>> features, List<double> deltaWeight, StatisticalData sd, Dictionary<int, Candidate> dict, bool use)
                    //testDict = utility.csvReader("tgmctrain_small.csv");
                    //testDict = utility.csvReader("tgmc_cut1.csv");
                    //testDict = utility.csvReader("tgmc_cut2.csv");
                    //testDict = utility.csvReader("tgmc_cut3.csv");
                    //testDict = utility.csvReader("tgmc_cutTest.csv");
                    //trainDict = utility.csvReader("tgmctrain_small.csv");
                    trainDict = utility.csvReader("tgmc_cut1.csv");
                    //trainDict = utility.csvReader("tgmc_cut2.csv");
                    //trainDict = utility.csvReader("tgmc_cut3.csv");
                    //trainDict = utility.csvReader("tgmc_cutTest.csv");
                    //trainDict = testDict;
                    testDict = trainDict;

                    //utility.dataAnalysis(testDict, true);
                    //utility.csvWriter("tgmc_cut2.csv",trainDict);
                    //trainDict = utility.randSample(testDict, 2);
                    //utility.csvWriter("tgmc_cutTest.csv",trainDict);

                    Candidate cand = new Candidate();
                    trainDict.TryGetValue(1, out cand);
                    StatisticalData sd = utility.dataAnalysis(trainDict, false);

                    bool allFeat = false;
                    //features = utility.removeMore(trainDict, "BestFeatures.txt");
                    features = utility.readFeatures("bestFeatures3.txt");
                    if (allFeat)
                    {
                        features.features.Add(new List<int>());
                        for (int j = 0; j < cand.featuresRating.Count(); j++)
                        {
                            features.features[0].Add(j);
                            features.featuresVector.Add(j);
                        }
                    }

                    List<int> numNodes = new List<int>();
                    List<double> learningRate = new List<double>();
                    Random rand = new Random();
                    for (int k = 0; k < features.features.Count(); k++)
                    {
                        numNodes.Add((int)(100*(rand.NextDouble()-0.5)+400));
                        learningRate.Add(0.005 + 0.002*(rand.NextDouble()-0.5));
                    }

                    double CR = 0.5;

                    bool testCombo = false;
                    List<List<int>> bestFeatures = new List<List<int>>();
                    if (testCombo) bestFeatures = utility.BruteForceTest(features,sd,trainDict);
                    else bestFeatures = features.features;

                    EnsembleRBF rbf = new EnsembleRBF(numNodes, bestFeatures, learningRate, sd, trainDict, true);
                    rbf.trainSystem(trainDict, CR, -1);
                    rbf.RBFwriter("weights.txt");

                    //EnsembleRBF rbf = new EnsembleRBF();
                    //rbf.RBFreader("weights.txt",-1);

                    results = rbf.testSystem(testDict, CR);
                    dict = testDict;
                    break;

                case "LogisticRegression":
                    bool train = true;
                    LogisiticRegression ai = null;
                    List<int> removed = null;

                    if (train)
                    {
                        Console.WriteLine("reading file");
                        Dictionary<int, Candidate> FullSet = utility.csvReader("tgmctrain.csv");
                        Console.WriteLine("read file");
                        bool run = true;
                        while (run)
                        {
                            // run = false;
                            if (true)
                            {
                                
                                int sizeOfTrain = 10000;
                                int numTrue = 0;
                                int numFalse = 0;
                                int ratioTtoF = 1;
                                Random gen = new Random();
                                while (dict.Count < sizeOfTrain)
                                {
                                    int randNum = gen.Next(FullSet.Count);
                                    while (randNum == 0) randNum = gen.Next(FullSet.Count);
                                    if (!dict.ContainsValue(FullSet[randNum]))
                                    {
                                        if (FullSet[randNum].isTrue && numTrue < (1000))
                                        {
                                            dict.Add(dict.Count + 1, FullSet[randNum]);
                                            numTrue++;
                                        }
                                        else if (!FullSet[randNum].isTrue && numFalse < (9000))
                                        {
                                            dict.Add(dict.Count + 1, FullSet[randNum]);
                                            numFalse++;
                                        }
                                    }

                                }
                            }
                            utility.csvWriter("randomSplitLarger.csv", dict);
                        }
                        Console.WriteLine("reading file");
                        dict = utility.csvReader("randomSplitLarge.csv");
                        Console.WriteLine("remove extraneous");
                        removed =utility.removeExtraneousData(dict, false);

                        utility.writeRemovedValues("removedLarge5k.txt", removed);
                        //removed = utility.readRemovedValues("removedLarge10k.txt");

                        ai = new LogisiticRegression(dict);
                        Console.Write("Beginning Training \n");
                        ai.train();
                        ai.writeToFile("LG5kDataAI.txt");
                        Console.Write("Finished Training \n");
                        Console.Write("AI Convergence is " + ai.convergenceValue().ToString());

                        System.IO.StreamWriter writer1 = new StreamWriter("convergenceValues5k.csv");
                        for (int k = 1; k <= ai.convergenceValue().Count; k++)
                        {
                            StringBuilder s = new StringBuilder();
                            s.Append(k + ",");
                            s.Append(ai.convergenceValue().ToList()[k - 1]);
                            if (k != ai.convergenceValue().Count) writer1.WriteLine(s.ToString());
                            else writer1.Write(s.ToString());
                        }
                        writer1.Close();
                    }
                    else
                    {
                        //dict = utility.csvReader("randomSplitLarge.csv");
                        //removed = utility.removeExtraneousData(dict, false);
                        ai = new LogisiticRegression("LG5kDataAI.txt");
                        
                    }
                    
                        double confidence = 0.99;
                        int testMode = 5;
                        switch (testMode)
                        {
                            case 1:
                                int numActualTrue = 0;
                                int numCorrectlyTrue = 0;
                                int numFalsePositive = 0;
                                int numActualFalse = 0;
                                int numFalseNegative = 0;
                                int numCorrectFalse = 0;
                                
                                foreach (KeyValuePair<int, Candidate> pair in dict)
                                {
                                    double prob = ai.probability(pair.Value.featuresRating);
                                    bool guess = (prob > confidence);
                                    if (guess)
                                    {
                                        if (pair.Value.isTrue)
                                        {
                                            numActualTrue++;
                                            numCorrectlyTrue++;
                                        }
                                        else
                                        {
                                            numActualFalse++;
                                            numFalsePositive++;
                                        }
                                    }
                                    else
                                    {
                                        if (pair.Value.isTrue)
                                        {
                                            numActualTrue++;
                                            numFalseNegative++;
                                        }
                                        else
                                        {
                                            numActualFalse++;
                                            numCorrectFalse++;
                                        }
                                    }
                                }
                                Console.WriteLine("Total actual trues: " + numActualTrue);
                                Console.WriteLine("Total actual falses: " + numActualFalse);
                                Console.WriteLine("Total correct trues: " + numCorrectlyTrue);
                                Console.WriteLine("Total false postives: " + numFalsePositive);
                                Console.WriteLine("Total correct falses: " + numCorrectFalse);
                                Console.WriteLine("Total false negatives: " + numFalseNegative);
                                break;
                            case 2:
                                numActualTrue = 0;
                                numCorrectlyTrue = 0;
                                numFalsePositive = 0;
                                numActualFalse = 0;
                                numFalseNegative = 0;
                                numCorrectFalse = 0;
                                
                                Console.WriteLine("Reading File");
                                Dictionary<int, Candidate> largeSet = utility.csvReader("tgmctrain.csv");
                                Console.WriteLine("removing extraneous");
                                removed = utility.readRemovedValues("removedLarge10k.txt");
                                utility.removeMore(largeSet, removed);
                                /* Console.WriteLine("Writing new file");
                                 utility.csvWriter("tgmctrainFilteredLarge.csv", largeSet);*/
                                foreach (KeyValuePair<int, Candidate> pair in largeSet)
                                {
                                    double prob = ai.probability(pair.Value.featuresRating);
                                    bool guess = (prob > confidence);
                                    if (guess)
                                    {
                                        if (pair.Value.isTrue)
                                        {
                                            numActualTrue++;
                                            numCorrectlyTrue++;
                                        }
                                        else
                                        {
                                            numActualFalse++;
                                            numFalsePositive++;
                                        }
                                    }
                                    else
                                    {
                                        if (pair.Value.isTrue)
                                        {
                                            numActualTrue++;
                                            numFalseNegative++;
                                        }
                                        else
                                        {
                                            numActualFalse++;
                                            numCorrectFalse++;
                                        }
                                    }
                                }
                                Console.WriteLine("Total actual trues: " + numActualTrue);
                                Console.WriteLine("Total actual falses: " + numActualFalse);
                                Console.WriteLine("Total correct trues: " + numCorrectlyTrue);
                                Console.WriteLine("Total false postives: " + numFalsePositive);
                                Console.WriteLine("Total correct falses: " + numCorrectFalse);
                                Console.WriteLine("Total false negatives: " + numFalseNegative);
                                break;
                            case 3:
                                                                
                                List<bool> answer = new List<bool>();
                                Console.WriteLine("Reading File");
                                Dictionary<int, Candidate> newSet = utility.csvReader("tgmcevaluation.csv");
                                removed = utility.readRemovedValues("removedLarge5k.txt");
                                utility.removeMore(newSet, removed);
                                //Console.WriteLine("removing extraneous");
                                //utility.removeMore(largeSet, removed);
                                //Console.WriteLine("Writing new file");
                                //utility.csvWriter("tgmctrainFiltered.csv", largeSet);
                                bool run = true;
                                while (run)
                                {
                                    // run = false;
                                    confidence = 0.99;
                                    Console.WriteLine("Evaluating at Confidence: " + confidence);
                                    foreach (KeyValuePair<int, Candidate> pair in newSet)
                                    {
                                        double prob = ai.probability(pair.Value.featuresRating);
                                        bool guess = (prob > confidence);
                                        answer.Add(guess);

                                    }
                                    /* Console.WriteLine("Total actual trues: " + numActualTrue);
                                     Console.WriteLine("Total actual falses: " + numActualFalse);
                                     Console.WriteLine("Total correct trues: " + numCorrectlyTrue);
                                     Console.WriteLine("Total false postives: " + numFalsePositive);
                                     Console.WriteLine("Total correct falses: " + numCorrectFalse);
                                     Console.WriteLine("Total false negatives: " + numFalseNegative);*/
                                    Console.WriteLine("Finished");
                                    utility.fileWriter(answer, newSet, "LogisticRegressionTestAnswer5k.txt",false);
                                }
                                break;
                            case 4:
                                LogisiticRegression ai5k = new LogisiticRegression("LogisticRegressionAiBackup20131031.txt");
                                LogisiticRegression ai10k = new LogisiticRegression("LG10kDataAI.txt");
                                LogisiticRegression ai20k = new LogisiticRegression("LG20kDataAI.txt");
                                
                                numActualTrue = 0;
                                numCorrectlyTrue = 0;
                                numFalsePositive = 0;
                                numActualFalse = 0;
                                numFalseNegative = 0;
                                numCorrectFalse = 0;
                                
                                Console.WriteLine("Reading File");
                                Dictionary<int, Candidate> trainingSet = utility.csvReader("tgmctrain.csv");
                                Console.WriteLine("removing extraneous");
                                removed = utility.readRemovedValues("removedLarge10k.txt");
                                utility.removeMore(trainingSet, removed);
                                /* Console.WriteLine("Writing new file");
                                 utility.csvWriter("tgmctrainFilteredLarge.csv", largeSet);*/
                                bool run2 = true;
                                while (run2)
                                {
                                    // run2 = false;
                                    confidence = 0.9;
                                    numActualTrue = 0;
                                    numCorrectlyTrue = 0;
                                    numFalsePositive = 0;
                                    numActualFalse = 0;
                                    numFalseNegative = 0;
                                    numCorrectFalse = 0;
                                    foreach (KeyValuePair<int, Candidate> pair in trainingSet)
                                    {
                                        double prob5k = ai5k.probability(pair.Value.featuresRating);
                                        double prob10k = ai10k.probability(pair.Value.featuresRating);
                                        double prob20k = ai20k.probability(pair.Value.featuresRating);

                                        bool guess1 = (prob5k > confidence);
                                        bool guess2 = (prob10k > confidence);
                                        bool guess3 = (prob20k > confidence);



                                        if (guess1 && guess3)
                                        {
                                            if (pair.Value.isTrue)
                                            {
                                                numActualTrue++;
                                                numCorrectlyTrue++;
                                            }
                                            else
                                            {
                                                numActualFalse++;
                                                numFalsePositive++;
                                            }
                                        }
                                        else
                                        {
                                            if (pair.Value.isTrue)
                                            {
                                                numActualTrue++;
                                                numFalseNegative++;
                                            }
                                            else
                                            {
                                                numActualFalse++;
                                                numCorrectFalse++;
                                            }
                                        }
                                    }
                                    Console.WriteLine("Total actual trues: " + numActualTrue);
                                    Console.WriteLine("Total actual falses: " + numActualFalse);
                                    Console.WriteLine("Total correct trues: " + numCorrectlyTrue);
                                    Console.WriteLine("Total false postives: " + numFalsePositive);
                                    Console.WriteLine("Total correct falses: " + numCorrectFalse);
                                    Console.WriteLine("Total false negatives: " + numFalseNegative);
                                }
                                break;
                            case 5:
                                LogisiticRegression ai5k1 = new LogisiticRegression("LogisticRegressionAiBackup20131031.txt");
                                LogisiticRegression ai10k1 = new LogisiticRegression("LG10kDataAI.txt");
                                LogisiticRegression ai20k1 = new LogisiticRegression("LG20kDataAI.txt");
                                List<bool> answer1 = new List<bool>();
                                Console.WriteLine("Reading File");
                                Dictionary<int, Candidate> newSet1 = utility.csvReader("tgmcevaluation.csv");
                                removed = utility.readRemovedValues("removedLarge5k.txt");
                                utility.removeMore(newSet1, removed);
                                //Console.WriteLine("removing extraneous");
                                //utility.removeMore(largeSet, removed);
                                //Console.WriteLine("Writing new file");
                                //utility.csvWriter("tgmctrainFiltered.csv", largeSet);
                                while (true)
                                {
                                    confidence = 0.9;
                                    answer1 = new List<bool>();
                                    Console.WriteLine("Evaluating at Confidence: " + confidence);
                                    foreach (KeyValuePair<int, Candidate> pair in newSet1)
                                    {

                                        double prob1 = ai5k1.probability(pair.Value.featuresRating);
                                        double prob2 = ai10k1.probability(pair.Value.featuresRating);
                                        double prob3 = ai20k1.probability(pair.Value.featuresRating);

                                        bool guess11 = (prob1 > confidence);
                                        bool guess12 = (prob2 > confidence);
                                        bool guess13 = (prob3 > confidence);
                                        answer1.Add(guess11 || guess13);

                                    }
                                    /* Console.WriteLine("Total actual trues: " + numActualTrue);
                                     Console.WriteLine("Total actual falses: " + numActualFalse);
                                     Console.WriteLine("Total correct trues: " + numCorrectlyTrue);
                                     Console.WriteLine("Total false postives: " + numFalsePositive);
                                     Console.WriteLine("Total correct falses: " + numCorrectFalse);
                                     Console.WriteLine("Total false negatives: " + numFalseNegative);*/
                                    Console.WriteLine("Finished");
                                    utility.fileWriter(answer1, newSet1, "LogisticRegressionTestAnswerMulti.txt",false);
                                }

    
                            default:
                                Console.WriteLine("Invalid Test");
                                break;
                        }

                    

                    break;
                //Case for combined work TODO:
                case "Combined":

                    break;
                default:
                    Console.WriteLine("Invalid parameters");
                    break;
            }

            /*
            for (int k = 0; k < dict.Count(); k++)
            {
                results.Add(false);
            }
            */

            Console.WriteLine("Done testing system - press enter to print results in 'output.txt'");
            Console.ReadLine();
        //utility.fileWriter(results, dict, "output2.txt",false);
        Console.ReadLine();
        }  
    }

    class Candidate
    {
        public int rowNumber;
        public double questionNumber;
        public List<double> featuresRating;
        public Boolean isTrue;

        public Candidate()
        {
            rowNumber = 0;
            questionNumber = 0;
            featuresRating = new List<double>();
            isTrue = false;
        }
    }

    class FeatureData
    {
        public List<int> featuresVector;
        public List<List<int>> features;

        public FeatureData()
        {
            featuresVector = new List<int>();
            features = new List<List<int>>();
        }
    }

    class StatisticalData
    {
        public List<double> meansT;
        public List<double> variancesT;
        public List<int> numTzero;
        public List<int> numTrue;
        public List<double> meansF;
        public List<double> variancesF;
        public List<int> numFzero;
        public List<int> numFalse;

        public List<double> meansTwz;
        public List<double> variancesTwz;
        public List<double> meansFwz;
        public List<double> variancesFwz;

        public StatisticalData()
        {
            meansT = new List<double>();
            variancesT = new List<double>();
            numTzero = new List<int>();
            numTrue = new List<int>();
            meansF = new List<double>();
            variancesF = new List<double>();
            numFzero = new List<int>();
            numFalse = new List<int>();

            meansTwz = new List<double>();
            variancesTwz = new List<double>();
            meansFwz = new List<double>();
            variancesFwz = new List<double>();
        }
    }


    class Util
    {
        public Dictionary<int, Candidate> csvReader(string fileName)
        {
            var reader = new StreamReader(File.OpenRead(@fileName));
            List<string> lineRead = new List<string>();
            Dictionary<int, Candidate> candidates = new Dictionary<int, Candidate>();
            int index = 1;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (!string.IsNullOrEmpty(values[0]))
                {
                    Candidate candidate = new Candidate();
                    candidate.rowNumber = Convert.ToInt32(values[0]);
                    candidate.questionNumber = Convert.ToSingle(values[1]);
                    for (int i = 2; i < values.Count(); i++)
                    {
                        if (values[i] == "true" || values[i] == "false" || values[i] == "TRUE" || values[i] == "FALSE")
                        {
                            if (values[i] == "true" || values[i] == "TRUE") candidate.isTrue = true;
                            else candidate.isTrue = false;
                            break;
                        }
                        candidate.featuresRating.Add(Convert.ToSingle(values[i]));
                    }
                    candidates.Add(index, candidate);
                    index++;
                }
            }
            reader.Close();
            return candidates;
        }

        public void csvWriter(string fileName, Dictionary<int, Candidate> candidates)
        {
            File.Delete(@fileName);
            var writer = new StreamWriter(File.OpenWrite(@fileName));
            Candidate candidate = new Candidate();
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

        public void fileWriter(List<bool> results, Dictionary<int, Candidate> candidates, string fileName, bool write)
        {
            File.Delete(@fileName);
            var writer = new StreamWriter(File.OpenWrite(@fileName));
            int i = 1;
            Candidate candidate = new Candidate();
            string stringToWrite = "";
            string f = "FALSE: ";
            string t = "TRUE: ";
            int score = 0;
            while (candidates.ContainsKey(i))
            {
                candidates.TryGetValue(i, out candidate);
                stringToWrite = candidate.rowNumber.ToString();
                if (results[i - 1] == true)
                {
                    if (candidate.isTrue == true)
                    {
                        if (write) writer.WriteLine(stringToWrite);
                        score++;
                    }
                    else
                    {
                        if (write) writer.WriteLine(f + stringToWrite);
                        score--;
                    }
                    if (!write) writer.WriteLine(stringToWrite);
                }
                else
                {
                    if (candidate.isTrue == true && write) writer.WriteLine(t + stringToWrite);
                }
                i++;
            }
            writer.Close();
            Console.WriteLine("Score: " + score);
        }

        public List<int> removeExtraneousData(Dictionary<int, Candidate> candidates,bool normalize = true)
        {
            //intialize size of array of columns
            List<List<double>> columns = new List<List<double>>();
            int columnCount = candidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<double>());
            }

            //Generate Array of Columns
            foreach (KeyValuePair<int, Candidate> pair in candidates)
            {
                Candidate c = pair.Value;
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
                double firstElement = columns[k][0];
                bool isRemove = true;
                foreach (double feature in columns[k])
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
                foreach (KeyValuePair<int, Candidate> pair in candidates)
                {
                    pair.Value.featuresRating.RemoveAt(removeData.Max());
                }
                columns.RemoveAt(removeData.Max());
                removeData.Remove(removeData.Max());
            }

            if (normalize)
            {
                List<double> maxValues = new List<double>();
                foreach (List<double> column in columns)
                {
                    double absMax;
                    if (Math.Abs(column.Min()) > Math.Abs(column.Max())) absMax = Math.Abs(column.Min());
                    else absMax = Math.Abs(column.Max());
                    if (absMax == 0.0) absMax = (double)1.0;
                    maxValues.Add(absMax);
                }

                for (int k = 0; k < maxValues.Count; k++)
                {
                    foreach (KeyValuePair<int, Candidate> candidate in candidates)
                    {
                        candidate.Value.featuresRating[k] = (candidate.Value.featuresRating[k] / maxValues[k]);
                    }
                }
            }
            return result;
        }

        public void removeMore(Dictionary<int, Candidate> canidates, List<int> rmColumns)
        {
            List<int> removeData = rmColumns.ToList();
            while (removeData.Count > 0)
            {
                foreach (KeyValuePair<int, Candidate> pair in canidates)
                {
                    pair.Value.featuresRating.RemoveAt(removeData.Max());
                }
                //columns.RemoveAt(removeData.Max());
                //Console.WriteLine("Removed Column " + removeData.Max());
                removeData.Remove(removeData.Max());
            }
        }
        public void removeMore(Dictionary<int, Candidate> canidates, string fileName)
        {
            // Assumes file is 1 indexed (like Matlab)
            var reader = new StreamReader(File.OpenRead(@fileName));
            var line = reader.ReadLine();
            var values = line.Split(',');
            List<int> keepIndex = new List<int>();
            foreach (string val in values)
            {
                keepIndex.Add(Convert.ToInt32(val) - 1);
            }
            reader.Close();

            List<int> removeIndex = new List<int>();
            for (int k = 0; k < canidates.First().Value.featuresRating.Count; k++)
            {
                removeIndex.Add(k);
            }
            foreach (int index in keepIndex)
            {
                removeIndex.Remove(index);
            }
            removeMore(canidates, removeIndex);
        }
        public List<int> readRemovedValues(String filepath)
        {
            List<int> result = new List<int>();
            System.IO.StreamReader reader = new System.IO.StreamReader(filepath);
            String line = reader.ReadLine();
            String[] values = line.Split(',');
            foreach (String value in values)
            {
                if (value == "") break;
                result.Add(Convert.ToInt32(value));
            }

            reader.Close();

            return result;
            
        }
        public void writeRemovedValues(String filepath, List<int> removed)
        {
            System.IO.StreamWriter writer1 = new StreamWriter(filepath);
            StringBuilder s = new StringBuilder();
            for (int k = 0; k < removed.Count; k++)
            {
                if (k != removed.Count - 1) s.Append(removed[k] + ",");
                else s.Append(removed[k]);
            }
            writer1.Write(s.ToString());
            writer1.Close();

        }

        public FeatureData readFeatures(string fileName)
        {
            // Assumes file is 1 indexed (like Matlab)
            FeatureData keepIndex = new FeatureData();
            var reader = new StreamReader(File.OpenRead(@fileName));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!line.Equals(""))
                {
                    var values = line.Split(',');
                    keepIndex.features.Add(new List<int>());
                    foreach (string val in values)
                    {
                        int conVal = Convert.ToInt32(val) - 1;
                        keepIndex.featuresVector.Add(conVal);
                        keepIndex.features[keepIndex.features.Count() - 1].Add(conVal);
                    }
                }
            }
            reader.Close();
            return keepIndex;
        }

        public Dictionary<int, Candidate> randSample(Dictionary<int, Candidate> candidates, int FperT)
        {
            Dictionary<int, Candidate> final = new Dictionary<int, Candidate>();
            int i = 1;
            Random random = new Random();
            int average = (int)55 / FperT;
            int index = (int)(random.NextDouble() * (2 * average)) + 1;
            int count = 0;
            foreach (KeyValuePair<int, Candidate> pair in candidates)
            {
                Candidate c = pair.Value;
                if (c.isTrue)
                {
                    final.Add(i, c);
                    i++;
                }
                else
                {
                    if (count >= index)
                    {
                        final.Add(i, c);
                        i++;
                        count = 0;
                        index = (int)(random.NextDouble() * (2 * average)) + 1;
                    }
                    count++;
                }
            }

            return final;
        }

        public StatisticalData dataAnalysis(Dictionary<int, Candidate> candidates, bool print)
        {
            List<double> meanT = new List<double>();
            List<double> standDevT = new List<double>();
            List<int> numTrue = new List<int>();
            List<int> numTzero = new List<int>();
            List<double> meanF = new List<double>();
            List<double> standDevF = new List<double>();
            List<int> numFalse = new List<int>();
            List<int> numFzero = new List<int>();
            List<double> meanTwz = new List<double>();
            List<double> standDevTwz = new List<double>();
            List<double> meanFwz = new List<double>();
            List<double> standDevFwz = new List<double>();
            Candidate candidate = new Candidate();

            //intialize size of array of columns
            List<List<double>> columns = new List<List<double>>();
            int columnCount = candidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<double>());
                meanT.Add(0);
                standDevT.Add(0);
                numTrue.Add(0);
                numTzero.Add(0);
                meanF.Add(0);
                standDevF.Add(0);
                numFalse.Add(0);
                numFzero.Add(0);
                meanTwz.Add(0);
                standDevTwz.Add(0);
                meanFwz.Add(0);
                standDevFwz.Add(0);
            }

            //Generate Array of Columns
            int length = 0;
            foreach (KeyValuePair<int, Candidate> pair in candidates)
            {
                Candidate c = pair.Value;
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
                        if (columns[i][j - 1] != 0) numTrue[i]++;
                        else numTzero[i]++;
                        meanT[i] += columns[i][j - 1];
                    }
                    else
                    {
                        if (columns[i][j - 1] != 0) numFalse[i]++;
                        else numFzero[i]++;
                        meanF[i] += columns[i][j - 1];
                    }
                    j++;
                }
            }

            for (int i = 0; i < length; i++)
            {
                meanFwz[i] = meanF[i] / (numFalse[i] + numFzero[i]);
                meanTwz[i] = meanT[i] / (numTrue[i] + numTzero[i]);
                if(numFalse[i] != 0) meanF[i] /= numFalse[i];
                if(numTrue[i] != 0) meanT[i] /= numTrue[i];
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
                        standDevTwz[i] += Math.Pow(columns[i][j - 1] - meanTwz[i], 2);
                        if(columns[i][j - 1] != 0) standDevT[i] += Math.Pow(columns[i][j - 1] - meanT[i], 2);
                    }
                    else
                    {
                        standDevFwz[i] += Math.Pow(columns[i][j - 1] - meanFwz[i], 2);
                        if(columns[i][j - 1] != 0) standDevF[i] += Math.Pow(columns[i][j - 1] - meanF[i], 2);
                    }
                    j++;
                }
            }

            for (int i = 0; i < length; i++)
            {
                standDevFwz[i] = Math.Sqrt(standDevFwz[i] / (numFalse[i] + numFzero[i]));
                if (numFalse[i] != 0) standDevF[i] = Math.Sqrt(standDevF[i] / numFalse[i]);
                else standDevF[i] = Math.Sqrt(standDevF[i]);

                standDevTwz[i] = Math.Sqrt(standDevTwz[i] / (numTrue[i] + numTzero[i]));
                if (numTrue[i] != 0) standDevT[i] = Math.Sqrt(standDevT[i] / numTrue[i]);
                else standDevT[i] = Math.Sqrt(standDevT[i]);
            }

            StatisticalData sd = new StatisticalData();
            sd.meansF = meanF;
            sd.meansFwz = meanFwz;
            sd.meansT = meanT;
            sd.meansTwz = meanTwz;
            sd.variancesF = standDevF;
            sd.variancesFwz = standDevFwz;
            sd.variancesT = standDevT;
            sd.variancesTwz = standDevTwz;
            sd.numTzero = numTzero;
            sd.numFzero = numFzero;
            sd.numTrue = numTrue;
            sd.numFalse = numFalse;

            if (print)
            {
                File.Delete("meanVStandDev.txt");
                var writer = new StreamWriter(File.OpenWrite("meanVStandDev.txt"));
                for (int i = 0; i < length; i++)
                {
                    writer.WriteLine(meanT[i].ToString() + ',' + standDevT[i].ToString() + ',' + meanF[i].ToString() + ',' + standDevF[i].ToString() + ',' + meanTwz[i].ToString() + ',' + standDevTwz[i].ToString() + ',' + meanFwz[i].ToString() + ',' + standDevFwz[i].ToString());
                }
                writer.Close();
            }

            return sd;
        }

        public List<List<int>> BruteForceTest(FeatureData fd, StatisticalData sd, Dictionary<int, Candidate> candidates)
        {
            // arraySplit
            double CR = 0.5;
            int ND = 150;
            double LR = 0.2;
            int maxNumRBF = 5;
            List<List<int>> bestFeatures = new List<List<int>>();
            bestFeatures.Add(fd.featuresVector);

            List<int> numNodes = new List<int>();
            numNodes.Add(ND);
            List<double> learningRate = new List<double>();
            learningRate.Add(LR);

            EnsembleRBF rbf = new EnsembleRBF(numNodes, bestFeatures, learningRate, sd, candidates, true);
            int mostRight = rbf.trainSystem(candidates, CR, 30);

            List<List<int>> tempFeat = new List<List<int>>();
            tempFeat.Add(new List<int>());
            Random rand = new Random();
            int output = 0;
            int totRight = 0;
            for (int k = 1; k < maxNumRBF; k++)
            {
                tempFeat.Add(new List<int>());
                numNodes.Add((int)(100 * (rand.NextDouble() - 0.5) + ND));
                learningRate.Add(LR + 0.02 * (rand.NextDouble() - 0.5));
	            for (int j = 0; j < 20; j++)
	            {
                    for (int m = 0; m <= k; m++) tempFeat[m] = new List<int>();
                    for (int m = 0; m < fd.featuresVector.Count(); m++)
                    {
                        output = (int)((k + 0.99999)*rand.NextDouble());
                        tempFeat[output].Add(fd.featuresVector[m]);
                    }
		            
                    rbf = new EnsembleRBF(numNodes, bestFeatures, learningRate, sd, candidates, true);
                    totRight = rbf.trainSystem(candidates, CR, 30);

		            if (totRight > mostRight)
		            {
			            mostRight = totRight;
                        bestFeatures = new List<List<int>>();
                        for (int m = 0; m < tempFeat.Count(); m++)
                        {
                            bestFeatures.Add(new List<int>());
                            for (int n = 0; n < tempFeat[m].Count(); n++)
                            {
                                bestFeatures[m].Add(tempFeat[m][n]);
                            }
                        }
		            }
	            }
            }
            return bestFeatures;
        }
    }
}
