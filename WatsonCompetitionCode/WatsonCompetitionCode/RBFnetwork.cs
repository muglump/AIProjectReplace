using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonCompetitionCode
{
    class RBFnetwork
    {
        private int numNodes;
        private int numFeatures;
        private double learningRate;
        private List<List<double>> mean;
        private List<List<double>> variance;
        private List<double> weight;
        private List<double> BF;

        // Random initialization for means, variances, etc.
        public RBFnetwork(int numNod, int numFeat, double deltaWeight)
        {
            numNodes = numNod;
            numFeatures = numFeat;
            learningRate = deltaWeight;

            mean = new List<List<double>>();
            variance = new List<List<double>>();
            weight = new List<double>();
            BF = new List<double>();
            Random random = new Random();
            for (int k = 0; k < numNodes; k++)
            {
                mean.Add(new List<double>());
                variance.Add(new List<double>());
                for (int j = 0; j < numFeatures; j++)
                {
                    mean[k].Add(1.1 * random.NextDouble() - 0.1);
                    variance[k].Add(random.NextDouble() * 5);
                }
                weight.Add(1);
                BF.Add(0);
            }
        }

        // Pre-initialized rather than random means, variances, etc. (can do training or leave as is)
        public RBFnetwork(int numNod, int numFeat, double deltaWeight, List<List<double>> means, List<List<double>> variances, List<double> weights)
        {
            numNodes = numNod;
            numFeatures = numFeat;
            mean = means;
            variance = variances;
            weight = weights;
            learningRate = deltaWeight;

            for (int k = 0; k < numNodes; k++)
            {
                BF.Add(0);
            }
        }

        public void RBFreader(string fileName)
        {
            var reader = new StreamReader(File.OpenRead(@fileName));
            List<string> lineRead = new List<string>();
            var line = reader.ReadLine();
            numNodes = Convert.ToInt32(line);
            line = reader.ReadLine();
            numFeatures = Convert.ToInt32(line);
            line = reader.ReadLine();
            learningRate = (float)Convert.ToDouble(line);

            mean = new List<List<double>>();
            variance = new List<List<double>>();
            weight = new List<double>();
            BF = new List<double>();

            var values = line.Split(',');
            int i = 0;
            int numLine = 0;
            int done = numNodes;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                values = line.Split(',');
                switch (i)
                {
                    case 0:
                        BF.Add(0); // Initialize once
                        mean.Add(new List<double>());
                        for (int k = 0; k < values.Count(); k++)
                        {
                            mean[numLine].Add(Convert.ToDouble(values[k]));
                        }
                        numLine++;
                        break;
                    case 1:
                        variance.Add(new List<double>());
                        for (int k = 0; k < values.Count(); k++)
                        {
                            variance[numLine].Add(Convert.ToDouble(values[k]));
                        }
                        numLine++;
                        break;
                    case 2:
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
                }
            }
            reader.Close();
        }

        public void RBFwriter(string fileName)
        {
            File.Delete(@fileName);
            var writer = new StreamWriter(File.OpenWrite(@fileName));
            writer.WriteLine(numNodes);
            writer.WriteLine(numFeatures);
            writer.WriteLine(learningRate);

            System.Text.StringBuilder strToWrite = new System.Text.StringBuilder();
            for (int k = 0; k < numNodes; k++)
            {
                strToWrite.Clear();
                strToWrite.Append(mean[k][0].ToString());
                for (int j = 1; j < numFeatures; j++)
                {
                    strToWrite.Append(',' + mean[k][j].ToString());
                }
                writer.WriteLine(strToWrite.ToString());
            }

            for (int k = 0; k < numNodes; k++)
            {
                strToWrite.Clear();
                strToWrite.Append(variance[k][0].ToString());
                for (int j = 1; j < numFeatures; j++)
                {
                    strToWrite.Append(',' + variance[k][j].ToString());
                }
                writer.WriteLine(strToWrite.ToString());
            }

            strToWrite.Clear();
            strToWrite.Append(weight[0].ToString());
            for (int k = 1; k < numNodes; k++)
            {
                strToWrite.Append(',' + weight[k].ToString());
            }
            writer.WriteLine(strToWrite.ToString());

            writer.Close();
        }

        private float basisFunction(List<double> input, int hidUnNum)
        {
            double result = 0;
            for (int k = 0; k < input.Count(); k++)
            {
                result -= Math.Pow(input[k] - mean[hidUnNum][k], 2) / (2 * Math.Pow(variance[hidUnNum][k], 2));
            }

            return (float)Math.Exp(result);
        }

        private double useSystem(List<double> features)
        {
            double output = 0;
            double bfout = 0;
            for (int k = 0; k < numNodes; k++)
            {
                bfout = basisFunction(features, k);
                BF[k] = bfout;
                output += bfout * weight[k];
            }

            //System.Console.WriteLine("Out: {0:F20}", output);
            return output;
        }

        public void trainSystem(Dictionary<int, Candidate> candidates)
        {
            List<List<double>> deltaMean = new List<List<double>>();
            List<List<double>> deltaVariance = new List<List<double>>();
            List<double> deltaWeight = new List<double>();
            int i = 0;
            double checker = 0;
            float candCheck = 0;
            Candidate candidate = new Candidate();
            int djdy = -1;
            double dHUdm = 1;
            double dHUdv = 1;
            int numOffT = 1;
            int numOffF = 1;

            for (int k = 0; k < numNodes; k++)
            {
                deltaMean.Add(new List<double>());
                deltaVariance.Add(new List<double>());
                for (int j = 0; j < numFeatures; j++)
                {
                    deltaMean[k].Add(0);
                    deltaVariance[k].Add(0);
                }
                deltaWeight.Add(0);
            }

            // learning
            while (numOffT != 0 || numOffF != 0)
            {
                numOffT = 0;
                numOffF = 0;
                i = 1;
                while (candidates.ContainsKey(i))
                {
                    candidates.TryGetValue(i, out candidate);
                    checker = useSystem(candidate.featuresRating);
                    if (candidate.isTrue) candCheck = 1;
                    else candCheck = 0;

                    if (Math.Abs(checker - candCheck) >= 0.1)
                    {
                        if (candCheck == 1) numOffT++;
                        else numOffF++;

                        if (checker > candCheck) djdy = -1;
                        else djdy = 1;

                        for (int k = 0; k < numNodes; k++)
                        {
                            for (int j = 0; j < numFeatures; j++)
                            {
                                dHUdm = ((mean[k][j] - candidate.featuresRating[j]) / (Math.Pow(variance[k][j], 2)));
                                dHUdv = (Math.Pow(candidate.featuresRating[j] - mean[k][j], 2) / Math.Pow(variance[k][j], 3)); //-1 / variance[k][j];
                                deltaMean[k][j] = learningRate * djdy * weight[k] * BF[k] * dHUdm;
                                deltaVariance[k][j] = learningRate * djdy * weight[k] * BF[k] * dHUdv;

                                //mean[k][j] += learningRate * djdy * weight[k] * BF[k] * dHUdm;
                                //variance[k][j] += learningRate * djdy * weight[k] * BF[k] * dHUdv;
                            }
                            deltaWeight[k] = learningRate * djdy * BF[k];
                            //weight[k] += learningRate * djdy * BF[k];
                        }
                    }
                    i++;
                }

                // update weights/values
                if (numOffT != 0 || numOffF != 0)
                {
                    for (int k = 0; k < numNodes; k++)
                    {
                        for (int j = 0; j < numFeatures; j++)
                        {
                            mean[k][j] += deltaMean[k][j];
                            variance[k][j] += deltaVariance[k][j];
                        }
                        weight[k] += deltaWeight[k];
                    }
                }
            }
            Console.WriteLine("I'm done training!\n");
        }

        public List<bool> testSystem(Dictionary<int, Candidate> candidates)
        {
            List<bool> results = new List<bool>();
            int i = 1;
            double checker = 0;
            Candidate candidate = new Candidate();
            while (candidates.ContainsKey(i))
            {
                candidates.TryGetValue(i, out candidate);
                checker = useSystem(candidate.featuresRating);

                if (checker >= 0.7) results.Add(true);
                else results.Add(false);

                i++;
            }
            return results;
        }
    }
}
