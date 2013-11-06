using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonCompetitionCode
{
    class EnsembleFF
    {
        public int numFF;
        public int goalNum;
        public double learnRate;
        public List<FFnetwork> ff;
        public List<double> weights;

        public EnsembleFF()
        {
            numFF = 0;
            goalNum = 1;
            learnRate = 0.02;
            ff = new List<FFnetwork>();
            weights = new List<double>();
        }
        
        public EnsembleFF(List<int> numLayers, List<List<int>> NPL, List<int> numFeat, List<double> deltaWeight, List<int> sysT)
        {
            List<int> features = new List<int>();
            weights = new List<double>();
            ff = new List<FFnetwork>();
            Random random = new Random();
            int rand = 0;

            numFF = numLayers.Count();
            goalNum = 1;
            learnRate = 0.2;
            for (int k = 0; k < numFF; k++)
            {
                features = new List<int>();
                for (int j = 0; j < numFeat[k]; j++)
                {
                    rand = (int)(numFeat[k] * random.NextDouble());
                    if (!features.Contains(rand)) features.Add(rand);
                    else j--;
                }
                ff.Add(new FFnetwork(numLayers[k], NPL[k], features, deltaWeight[k], sysT[k]));
                weights.Add(1 / (double)numFF);
            }
        }

        public EnsembleFF(List<int> numLayers, List<List<int>> NPL, List<List<int>> features, List<double> deltaWeight, List<int> sysT)
        {
            learnRate = 0.2;
            goalNum = 1;
            numFF = features.Count();
            ff = new List<FFnetwork>();
            weights = new List<double>();
            Random random = new Random();
            for (int k = 0; k < numFF; k++)
            {
                ff.Add(new FFnetwork(numLayers[k], NPL[k], features[k], deltaWeight[k], sysT[k]));
            }
            for (int k = 0; k < numFF; k++) weights.Add(1 / (double)numFF);
        }

        public void FFreader(string fileName, int numRBFdes) // negative number of FF desired means read entire file
        {
            int numLayers;
            int sysT;
            double LR;
            List<int> NPL;
            List<int> feats;
            List<List<List<double>>> w;
            List<List<double>> b;

            var reader = new StreamReader(File.OpenRead(@fileName));

            int i = 0;
            int numLine = 0;
            int done;
            bool stop = false;

            if (numRBFdes < 1) numRBFdes = 0xFFFF;

            while (!reader.EndOfStream && numFF < numRBFdes)
            {
                NPL = new List<int>();
                feats = new List<int>();
                w = new List<List<List<double>>>();
                b = new List<List<double>>();

                var line = reader.ReadLine();
                weights.Add(Convert.ToDouble(line));
                line = reader.ReadLine();
                numLayers = Convert.ToInt32(line);
                line = reader.ReadLine();
                sysT = Convert.ToInt32(line);
                line = reader.ReadLine();
                LR = (float)Convert.ToDouble(line);

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
                                feats.Add(Convert.ToInt32(values[k]));
                            }
                            numLine++;
                            done = 1;
                            break;
                        case 1:
                            for (int k = 0; k < values.Count(); k++)
                            {
                                NPL.Add(Convert.ToInt32(values[k]));
                            }
                            numLine++;
                            done = 1;
                            break;
                        case 2:
                            w.Add(new List<List<double>>());

                            int counter = -1;
                            if (numLine == 0)
                            {
                                for (int k = 0; k < values.Count(); k++)
                                {
                                    if (k % feats.Count() == 0)
                                    {
                                        counter++;
                                        w[numLine].Add(new List<double>());
                                    }
                                    w[numLine][counter].Add(Convert.ToDouble(values[k]));
                                }
                            }
                            else if (numLine == numLayers)
                            {
                                w[numLine].Add(new List<double>());
                                for (int k = 0; k < values.Count(); k++)
                                {
                                    w[numLine][0].Add(Convert.ToDouble(values[k]));
                                }
                            }
                            else
                            {
                                for (int k = 0; k < values.Count(); k++)
                                {
                                    if (k % NPL[numLine-1] == 0)
                                    {
                                        counter++;
                                        w[numLine].Add(new List<double>());
                                    }
                                    w[numLine][counter].Add(Convert.ToDouble(values[k]));
                                }
                            }

                            numLine++;
                            done = numLayers+1;
                            break;
                        case 3:
                            b.Add(new List<double>());
                            for (int k = 0; k < values.Count(); k++)
                            {
                                b[numLine].Add(Convert.ToDouble(values[k]));
                            }
                            done = numLayers+1;
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
                            numFF++;
                            ff.Add(new FFnetwork(numLayers, sysT, LR, NPL, feats, w, b));
                            stop = true;
                        }
                    }
                }
            }
            reader.Close();
        }

        public void FFwriter(string fileName)
        {
            File.Delete(@fileName);
            StreamWriter writer = new StreamWriter(File.OpenWrite(@fileName));
            for (int j = 0; j < numFF; j++)
            {
                writer.WriteLine(weights[j]);
                ff[j].RBFwriter(writer);
            }
            writer.Close();
        }

        public int trainSystem(Dictionary<int, Candidate> candidates, double CR, int numIterations)
        {
            Candidate candidate = new Candidate();
            List<double> deltaW = new List<double>();
            List<double> candCheck = new List<double>();
            List<List<double>> checker = new List<List<double>>();
            for (int k = 0; k < numFF; k++)
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
            double TratioF = (double)1; // 1/60

            if (numIterations < 0) numIterations = 0xFFFF;

            bool done = false;
            int numPoints = 0;
            int numFT = 0;
            while (((totalWrong != 0 || ff[0].checkRate > 0.1) && (count < numIterations)) && !done)
            {
                //done = true;
                prevLearnRate = ff[0].learningRate;
                prevCheckRate = ff[0].checkRate;
                numOffT = 0;
                numOffF = 0;
                numFT = 0;
                for (int k = 0; k < numFF; k++)
                {
                    checker[k] = ff[k].trainSystem(candidates);
                }

                for (int k = 0; k < candidates.Count(); k++)
                {
                    output = 0;
                    candidates.TryGetValue(k+1, out candidate);
                    for (int j = 0; j < numFF; j++)
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

                        for (i = 0; i < numFF; i++)
                        {
                            deltaW[i] = learnRate * djdy * checker[i][k];
                        }
                    }
                }

                double totalW = 0;
                for (int k = 0; k < numFF; k++)
                {
                    weights[k] += deltaW[k];
                    totalW += weights[k];
                }

                if (learnRate > 0.15) learnRate -= 0.1;
                else if (learnRate > 0.015) learnRate -= 0.01;
                else if (learnRate > 0.0025) learnRate -= 0.002;
                else if (learnRate > 0.00025) learnRate -= .0001;
                else learnRate = 0.00025;

                for (int k = 0; k < numFF; k++) weights[k] /= totalW;

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
            for (int k = 0; k < numFF; k++)
            {
                output.Add(ff[k].testSystem(candidates));
            }

            double count;
            for (int k = 0; k < candidates.Count(); k++)
            {
                count = 0;
                for (int j = 0; j < numFF; j++)
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
