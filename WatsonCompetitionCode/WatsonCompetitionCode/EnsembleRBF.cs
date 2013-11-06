using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonCompetitionCode
{
    class EnsembleRBF
    {
        public int numRBF;
        public int goalNum;
        public double learnRate;
        public List<RBFnetwork> rbf;
        public List<double> weights;

        public EnsembleRBF()
        {
            numRBF = 0;
            goalNum = 1;
            learnRate = 0.02;
            rbf = new List<RBFnetwork>();
            weights = new List<double>();
        }

        public EnsembleRBF(List<int> numNod, List<int> numFeat, List<double> deltaWeight, StatisticalData sd, Dictionary<int, Candidate> dict, bool use)
        {
            List<int> features = new List<int>();
            weights = new List<double>();
            rbf = new List<RBFnetwork>();
            Candidate c = new Candidate();
            dict.TryGetValue(1, out c);
            Random random = new Random();
            int rand = 0;

            numRBF = numNod.Count();
            goalNum = 1;
            learnRate = 0.02;
            for (int k = 0; k < numNod.Count(); k++)
            {
                features = new List<int>();
                for (int j = 0; j < numFeat[k]; j++)
                {
                    rand = (int)(numFeat[k] * random.NextDouble());
                    if (!features.Contains(rand)) features.Add(rand);
                    else j--;
                }
                rbf.Add(new RBFnetwork(numNod[k], deltaWeight[k], features, goalNum, sd, dict, use));
                weights.Add(1 / (double)numRBF);
            }
        }

        public EnsembleRBF(List<int> numNod, List<List<int>> features, List<double> deltaWeight, StatisticalData sd, Dictionary<int, Candidate> dict, bool use)
        {
            learnRate = 0.02;
            goalNum = 1;
            numRBF = features.Count();
            rbf = new List<RBFnetwork>();
            weights = new List<double>();
            Random random = new Random();
            for (int k = 0; k < numRBF; k++)
            {
                rbf.Add(new RBFnetwork(numNod[k], deltaWeight[k], features[k], goalNum, sd, dict, use));
            }
            for (int k = 0; k < numRBF; k++) weights.Add(1 / (double)numRBF);
        }

        public void RBFreader(string fileName, int numRBFdes) // negative number of RBF desired means read entire file
        {
            int numNodes;
            int numFeatures;
            double learningRate;
            var reader = new StreamReader(File.OpenRead(@fileName));

            List<List<double>> mean;
            List<List<double>> variance;
            List<double> weight;
            List<int> feature;

            int i = 0;
            int numLine = 0;
            int done;
            bool stop = false;

            if (numRBFdes < 1) numRBFdes = 0xFFFF;

            while (!reader.EndOfStream && numRBF < numRBFdes)
            {
                mean = new List<List<double>>();
                variance = new List<List<double>>();
                weight = new List<double>();
                feature = new List<int>();

                var line = reader.ReadLine();
                weights.Add(Convert.ToDouble(line));
                line = reader.ReadLine();
                numNodes = Convert.ToInt32(line);
                line = reader.ReadLine();
                numFeatures = Convert.ToInt32(line);
                line = reader.ReadLine();
                learningRate = (float)Convert.ToDouble(line);

                var values = line.Split(',');
                i = 0;
                numLine = 0;
                done = 1;
                stop = false;
                while (!reader.EndOfStream && !stop)
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    switch (i)
                    {
                        case 0:
                            for (int k = 0; k < values.Count(); k++)
                            {
                                feature.Add(Convert.ToInt32(values[k]));
                            }
                            numLine++;
                            done = 1;
                            break;
                        case 1:
                            mean.Add(new List<double>());
                            for (int k = 0; k < values.Count(); k++)
                            {
                                mean[numLine].Add(Convert.ToDouble(values[k]));
                            }
                            numLine++;
                            done = numNodes;
                            break;
                        case 2:
                            variance.Add(new List<double>());
                            for (int k = 0; k < values.Count(); k++)
                            {
                                variance[numLine].Add(Convert.ToDouble(values[k]));
                            }
                            numLine++;
                            done = numNodes;
                            break;
                        case 3:
                            for (int k = 0; k < values.Count(); k++)
                            {
                                weight.Add(Convert.ToDouble(values[k]));
                            }
                            done = 1;
                            numLine++;
                            break;
                        default:
                            break;
                    }
                    if (numLine == done)
                    {
                        numLine = 0;
                        i++;
                        if (i == 4)
                        {
                            i = 0;
                            numRBF++;
                            rbf.Add(new RBFnetwork(numNodes, numFeatures, learningRate, goalNum, feature, mean, variance, weight));
                            stop = true;
                        }
                    }
                }
            }
            reader.Close();
        }

        public void RBFwriter(string fileName)
        {
            File.Delete(@fileName);
            StreamWriter writer = new StreamWriter(File.OpenWrite(@fileName));
            for (int j = 0; j < numRBF; j++)
            {
                writer.WriteLine(weights[j]);
                rbf[j].RBFwriter(writer);
            }
            writer.Close();
        }

        public int trainSystem(Dictionary<int, Candidate> candidates, double CR, int numIterations)
        {
            Candidate candidate = new Candidate();
            List<double> deltaW = new List<double>();
            List<double> candCheck = new List<double>();
            List<List<double>> checker = new List<List<double>>();
            for (int k = 0; k < numRBF; k++)
            {
                checker.Add(new List<double>());
                deltaW.Add(0);
            }
            
            int i = 1;
            int numCandTrue = 0;
            int numCandFalse = 0;
            while (candidates.ContainsKey(i))
            {
                candidates.TryGetValue(i, out candidate);
                if (candidate.isTrue)
                {
                    candCheck.Add(goalNum);
                    numCandTrue++;
                }
                else
                {
                    candCheck.Add(-goalNum);
                    numCandFalse++;
                }
                i++;
            }

            // learning
            int totalWrong = candidates.Count();
            int totalRight = 0;
            double djdy = -1;
            
            int numOffT = 1;
            int numOffF = 1;
            bool doChange = false;
            double checkRate = CR*goalNum; // max of 1
            int count = 0;
            double output = 0;
            double prevLearnRate = 1;
            double prevCheckRate = 1;
            double TratioF = (double) 4; // 1/60

            if (numIterations < 0) numIterations = 0xFFFF;

            bool done = false;
            int numPoints = 0;
            int numFT = 0;
            while (((totalWrong != 0 || rbf[0].checkRate > 0.1) && (count < numIterations)) && !done)
            {
                //done = true;
                prevLearnRate = rbf[0].learningRate;
                prevCheckRate = rbf[0].checkRate;
                numOffT = 0;
                numOffF = 0;
                numFT = 0;
                for (int k = 0; k < numRBF; k++)
                {
                    checker[k] = rbf[k].trainSystem(candidates);
                }

                for (int k = 0; k < candidates.Count(); k++)
                {
                    output = 0;
                    candidates.TryGetValue(k+1, out candidate);
                    for (int j = 0; j < numRBF; j++)
                    {
                        output += checker[j][k] * weights[j];
                        //if (checker[j][k] > goalNum - 2*checkRate) output += weights[j];
                        //else if (checker[j][k] < (goalNum+2*checkRate)) output -= weights[j];
                        // if (checker[j][k] > checkRate) output += 1;
                    }
                    
                    if (candCheck[k] == goalNum)
                    {
                        //if (output > 0) doChange = false;
                        //else doChange = true;
                        if (output < (goalNum - checkRate)) doChange = true;
                        else doChange = false;
                        //if (output >= (double)numRBF / 2) doChange = false;
                        //else doChange = true;
                    }
                    else
                    {
                        //if (output < 0) doChange = false;
                        //else doChange = true;
                        if (output > goalNum - checkRate) numFT++;
                        if (output > (-goalNum + 2*checkRate)) doChange = true;
                        else doChange = false;
                        //if (output < (double)numRBF / 2) doChange = false;
                        //else doChange = true;
                    }

                    if (doChange)
                    {
                        djdy = candCheck[k] - output;
                        if (candCheck[k] == goalNum)
                        {
                            numOffT++;
                            djdy *= TratioF;
                        }
                        else numOffF++; 

                        for (i = 0; i < numRBF; i++)
                        {
                            deltaW[i] = learnRate * djdy * checker[i][k];
                        }
                    }
                }

                double totalW = 0;
                for (int k = 0; k < numRBF; k++)
                {
                    weights[k] += deltaW[k];
                    totalW += weights[k];
                }

                if (learnRate > 0.15) learnRate -= 0.1;
                else if (learnRate > 0.015) learnRate -= 0.01;
                else if (learnRate > 0.0025) learnRate -= 0.002;
                else if (learnRate > 0.00025) learnRate -= .0001;
                else if (learnRate > 0.000025) learnRate -= 0.00001;
                else learnRate = 0.000025;

                for (int k = 0; k < numRBF; k++) weights[k] /= totalW;

                totalWrong = numOffT + numOffF;
                totalRight = candidates.Count() - totalWrong;
                count++;
                numPoints = (numCandTrue - numOffT) - numFT;
                Console.WriteLine(count + ": !T=" + numOffT + ", !F=" + numOffF + ", C=" + totalRight + ", P=" + numPoints + "\t\t%=" + (double) 100*totalRight / (totalRight + totalWrong));
                Console.WriteLine("\tLr=" + prevLearnRate + ", Cr=" + prevCheckRate);
            }
            Console.WriteLine("I'm done training!\n");
            return totalRight;
        }

        public List<bool> testSystem(Dictionary<int, Candidate> candidates,double accuracy)
        {
            List<bool> results = new List<bool>();
            List<List<double>> output = new List<List<double>>();
            for (int k = 0; k < numRBF; k++)
            {
                output.Add(rbf[k].testSystem(candidates));
            }

            double count;
            for (int k = 0; k < candidates.Count(); k++)
            {
                count = 0;
                for (int j = 0; j < numRBF; j++)
                {
                    count += output[j][k] * weights[j];
                }
                if (count > accuracy * goalNum) results.Add(true);
                else results.Add(false);
            }

            return results;
        }
    }
}
