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
        public int numNodes;
        public int numFeatures;
        public double learningRate;
        public double checkRate;
        public double goalNum;
        public List<List<double>> mean;
        public List<List<double>> deltaMean;
        public List<List<double>> variance;
        public List<List<double>> deltaVariance;
        public List<double> weight;
        public List<double> deltaWeight;
        public List<double> BF;
        public List<int> featureList;

        // Random initialization for means, variances, etc.
        public RBFnetwork(int numNod, double deltaWeight, List<int> features, double goalNumber, StatisticalData sd, Dictionary<int, Candidate> dict, bool use)
        {
            numNodes = numNod;
            numFeatures = features.Count();
            learningRate = deltaWeight;
            checkRate = 0.5*goalNumber; // max 1
            goalNum = goalNumber;

            mean = new List<List<double>>();
            variance = new List<List<double>>();
            weight = new List<double>();
            Random random = new Random();
            Candidate c = new Candidate();
            featureList = features;

            if (use)
            {
                List<int> index = new List<int>();
                int total = dict.Count();
                int numNodesFalse = (3*numNodes) / 5;
                int numNodesTrue = numNodes - numNodesFalse;
                int indexF = 0;
                int indexT = 0;
                int numTrue = total / 61;
                int numFalse = (60*total)/61;
                double trueRatio = numTrue / numNodesTrue;
                double falseRatio = numFalse/numNodesFalse;
                int nextIndexT = (int)(trueRatio * random.NextDouble() + 1);
                int nextIndexF = (int)(falseRatio * random.NextDouble() + 1);
                int i = 1;
                int countT = 0;
                int countF = 0;
                List<int> addIfNec = new List<int>();

                while (dict.ContainsKey(i) && (countT + countF) < numNodes)
                {
                    dict.TryGetValue(i, out c);
                    if (c.isTrue)
                    {
                        if (indexT >= nextIndexT)
                        {
                            indexT = 0;
                            numTrue = (total - i) / 61;
                            nextIndexT = (int)(trueRatio * random.NextDouble() + 1);
                            numNodesTrue--;
                            if (numNodesTrue < 0) continue;
                            trueRatio = numTrue / (numNodesTrue+1);
                            index.Add(i);
                            countT++;
                        }
                        else
                        {
                            if (indexT == nextIndexT / 2) addIfNec.Add(i);
                            indexT++;
                        }
                    }
                    else
                    {
                        if (indexF >= nextIndexF)
                        {
                            
                            indexF = 0;
                            numFalse = (60 * (total - i)) / 61;
                            nextIndexF = (int)(falseRatio * random.NextDouble() + 1);
                            numNodesFalse--;
                            if (numNodesFalse < 0) continue;
                            falseRatio = numFalse / (numNodesFalse + 1);
                            index.Add(i);
                            countF++;
                        }
                        else indexF++;
                    }

                    i++;
                }

                int currentCount = index.Count();
                for (int k = 0; k < numNodes - currentCount; k++)
                {
                    index.Add(addIfNec[addIfNec.Count()-k-1]);
                }

                //double v = 0;
                for (int k = 0; k < numNodes; k++)
                {
                    mean.Add(new List<double>());
                    variance.Add(new List<double>());
                    dict.TryGetValue(index[k], out c);
                    for (int j = 0; j < numFeatures; j++)
                    {
                        mean[k].Add(c.featuresRating[features[j]]);

                        //if (c.isTrue) v = Math.Abs(sd.variancesT[features[j]] + 0.2 * (random.NextDouble() - 0.1));
                        //else v = Math.Abs(sd.variancesF[features[j]] + 0.2 * (random.NextDouble() - 0.1));
                        //if (v < 0.01) v = 0.01;
                        //variance[k].Add(v);

                        variance[k].Add(4*random.NextDouble()+0.001);
                    }
                    if (c.isTrue) weight.Add(0.7+0.3*random.NextDouble());
                    else weight.Add(-(0.7+0.3*random.NextDouble()));
                }
            }
            else
            {
                for (int k = 0; k < numNodes; k++)
                {
                    mean.Add(new List<double>());
                    variance.Add(new List<double>());
                    for (int j = 0; j < numFeatures; j++)
                    {
                        mean[k].Add(1.1 * random.NextDouble() - 0.1);
                        variance[k].Add(3 * random.NextDouble() + 0.001);
                    }
                    weight.Add(2 * random.NextDouble() - 1);
                }
            }
            genInit();
        }

        // Pre-initialized rather than random means, variances, etc. (can do training or leave as is)
        public RBFnetwork(int numNod, int numFeat, double deltaWeight, double goalNumber, List<int> features, List<List<double>> means, List<List<double>> variances, List<double> weights)
        {
            numNodes = numNod;
            numFeatures = numFeat;
            mean = means;
            variance = variances;
            weight = weights;
            learningRate = deltaWeight;
            checkRate = 0.5*goalNumber;
            goalNum = goalNumber;
            featureList = features;
            genInit();
        }

        private void genInit()
        {
            deltaMean = new List<List<double>>();
            deltaVariance = new List<List<double>>();
            deltaWeight = new List<double>();
            BF = new List<double>();

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
                BF.Add(0);
            }
        }

        public void RBFwriter(StreamWriter writer)
        {
            System.Text.StringBuilder strToWrite = new System.Text.StringBuilder();

            writer.WriteLine(numNodes);
            writer.WriteLine(numFeatures);
            writer.WriteLine(learningRate);

            strToWrite.Clear();
            strToWrite.Append(featureList[0].ToString());
            for (int k = 1; k < numFeatures; k++)
            {
                strToWrite.Append(',' + featureList[k].ToString());
            }
            writer.WriteLine(strToWrite);

            for (int k = 0; k < numNodes; k++)
            {
                strToWrite.Clear();
                strToWrite.Append(mean[k][0].ToString());
                for (int j = 1; j < numFeatures; j++)
                {
                    strToWrite.Append(',' + mean[k][j].ToString());
                }
                writer.WriteLine(strToWrite);
            }

            for (int k = 0; k < numNodes; k++)
            {
                strToWrite.Clear();
                strToWrite.Append(variance[k][0].ToString());
                for (int j = 1; j < numFeatures; j++)
                {
                    strToWrite.Append(',' + variance[k][j].ToString());
                }
                writer.WriteLine(strToWrite);
            }

            strToWrite.Clear();
            strToWrite.Append(weight[0].ToString());
            for (int k = 1; k < numNodes; k++)
            {
                strToWrite.Append(',' + weight[k].ToString());
            }
            writer.WriteLine(strToWrite);
        }

        private double basisFunction(List<double> input, int hidUnNum)
        {
            double result = 0;
            for (int k = 0; k < numFeatures; k++)
            {
                result -= Math.Pow(input[featureList[k]] - mean[hidUnNum][k], 2) / (2 * Math.Pow(variance[hidUnNum][k], 2));
            }

            return Math.Exp(result);
        }

        private double useSystem(List<double> features)
        {
            double output = 0;
            double bfout = 0;
            for (int k = 0; k < numNodes; k++)
            {
                bfout = basisFunction(features, k);
                if (bfout == 0) bfout = 0.01;
                BF[k] = bfout;
                output += bfout * weight[k];
            }
            return output;
        }

        public List<double> trainSystem(Dictionary<int, Candidate> dict)
        {
            List<double> output = new List<double>();
            bool doChange;
            double TratioF = (double) 4; // 1/60
            double djdyInt = -1;
            double dHUdm = 1;
            double dHUdv = 1;
            Candidate candidate = new Candidate();

            double tempOut = 0;
            for (int k = 0; k < dict.Count(); k++)
            {
                dict.TryGetValue(k + 1, out candidate);
                tempOut = useSystem(candidate.featuresRating);
                if (tempOut > goalNum) tempOut = goalNum;
                else if (tempOut < -goalNum) tempOut = -goalNum;
                output.Add(tempOut);

                if (candidate.isTrue == true)
                {
                    if (output[k] < (goalNum - checkRate)) doChange = true;
                    else doChange = false;
                }
                else
                {
                    if (output[k] > (-goalNum + 2 * checkRate)) doChange = true;
                    else doChange = false;
                }

                if (doChange)
                {
                    if (candidate.isTrue) djdyInt = (goalNum - output[k]) * TratioF;
                    else djdyInt = -goalNum - output[k];

                    for (int m = 0; m < numNodes; m++)
                    {
                        for (int j = 0; j < numFeatures; j++)
                        {
                            dHUdm = (candidate.featuresRating[featureList[j]] - mean[m][j]) / (Math.Pow(variance[m][j], 2));
                            dHUdv = (Math.Pow(candidate.featuresRating[featureList[j]] - mean[m][j], 2) / Math.Pow(variance[m][j], 3));
                            if (k == 0)
                            {  
                                deltaMean[m][j] = learningRate * djdyInt * weight[m] * BF[m] * dHUdm;
                                deltaVariance[m][j] = learningRate * djdyInt * weight[m] * BF[m] * dHUdv;
                            }
                            else
                            {
                                deltaMean[m][j] += learningRate * djdyInt * weight[m] * BF[m] * dHUdm;
                                deltaVariance[m][j] += learningRate * djdyInt * weight[m] * BF[m] * dHUdv;
                            }
                        }
                        deltaWeight[m] = learningRate * djdyInt * BF[m];
                    }
                }
            }

            updateSystem();

            if (learningRate > 0.15) learningRate -= 0.1;
            else if (learningRate > 0.015) learningRate -= 0.01;
            else if (learningRate > 0.0025) learningRate -= 0.002;
            else if (learningRate > 0.00025) learningRate -= 0.0001;
            else if (learningRate > 0.000025) learningRate -= 0.00001;
            else learningRate = 0.000025;

            if (checkRate > 0.4 * goalNum) checkRate -= 0.05 * goalNum;
            else if (checkRate > 0.25 * goalNum) checkRate -= 0.02 * goalNum;
            else checkRate = 0.25;    

            return output;
        }

        private void updateSystem()
        {
            // update weights/values
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

        public List<double> testSystem(Dictionary<int, Candidate> dict)
        {
            List<double> output = new List<double>();
            int i = 1;
            Candidate candidate = new Candidate();

            // find current outputs of network
            double tempOut;
            while (dict.ContainsKey(i))
            {
                dict.TryGetValue(i, out candidate);
                tempOut = useSystem(candidate.featuresRating);
                if (tempOut > goalNum) tempOut = goalNum;
                else if (tempOut < -goalNum) tempOut = -goalNum;
                output.Add(tempOut);
                i++;
            }

            return output;
        }
    }
}
