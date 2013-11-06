using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonCompetitionCode
{
    class FFnetwork
    {
        public int numLayers;
        public List<int> nodesPerLayer;
        public List<List<List<double>>> weights;
        public List<List<List<double>>> deltaWeight;
        public List<List<double>> input;
        public List<List<double>> output;
        public List<List<double>> bias;
        public List<List<double>> deltaBias;
        public List<int> features;
        public int sysType;
        public double learningRate;
        public int goalNumPos;
        public int goalNumNeg;
        public double checkRate;

        public FFnetwork(int numL, List<int> NPL, List<int> feat, double LR, int sysT)
        {
            numLayers = numL;
            nodesPerLayer = NPL;
            features = feat;
            sysType = sysT;
            learningRate = LR;
            bias = new List<List<double>>();
            deltaBias = new List<List<double>>();
            deltaWeight = new List<List<List<double>>>();
            weights = new List<List<List<double>>>();
            Random random = new Random();
            for (int k = 0; k < numLayers+1; k++)
            {
                weights.Add(new List<List<double>>());
                deltaWeight.Add(new List<List<double>>());
                bias.Add(new List<double>());
                deltaBias.Add(new List<double>());
                if (k == 0)
                {
                    for (int i = 0; i < nodesPerLayer[k]; i++)
                    {
                        bias[k].Add(2 * random.NextDouble() - 1);
                        deltaBias[k].Add(0);
                        weights[k].Add(new List<double>());
                        deltaWeight[k].Add(new List<double>());
                        for (int j = 0; j < features.Count(); j++)
                        {
                            weights[k][i].Add(2*random.NextDouble() - 1);
                            deltaWeight[k][i].Add(0);
                        }
                    }
                }
                else if (k == numLayers)
                {
                    bias[k].Add(2 * random.NextDouble() - 1);
                    deltaBias[k].Add(0);
                    weights[k].Add(new List<double>());
                    deltaWeight[k].Add(new List<double>());
                    for (int j = 0; j < nodesPerLayer[k - 1]; j++)
                    {
                        weights[k][0].Add(2 * random.NextDouble() - 1);
                        deltaWeight[k][0].Add(0);
                    }
                }
                else
                {
                    for (int i = 0; i < nodesPerLayer[k]; i++)
                    {
                        bias[k].Add(2 * random.NextDouble() - 1);
                        deltaBias[k].Add(0);
                        weights[k].Add(new List<double>());
                        deltaWeight[k].Add(new List<double>());
                        for (int j = 0; j < nodesPerLayer[k - 1]; j++)
                        {
                            weights[k][i].Add(2 * random.NextDouble() - 1);
                            deltaWeight[k][i].Add(0);
                        }
                    }
                }
            }
            setGoals(sysT);
        }

        public FFnetwork(int numFeatures)
        {
            numLayers = 1;
            nodesPerLayer = new List<int>();
            nodesPerLayer.Add(1);
            bias = new List<List<double>>();
            bias.Add(new List<double>());
            bias[0].Add(1);
            sysType = 2;
            learningRate = 0.2;
            
            weights = new List<List<List<double>>>();
            weights.Add(new List<List<double>>());
            weights[0].Add(new List<double>());
            for (int k = 0; k < numFeatures; k++)
            { 
                weights[0][0].Add(1);
            }
            setGoals(sysType);
        }

        public FFnetwork(int numL, int sysT, double LR, List<int> NPL, List<int> feats, List<List<List<double>>> w, List<List<double>> b)
        {
            numLayers = numL;
            learningRate = LR;
            nodesPerLayer = NPL;
            features = feats;
            weights = w;
            bias = b;
            setGoals(sysT);
        }

        private double actFunc(double x, int funcChoice)
        {
            double output = 0;
            switch(funcChoice)
            {
                case 1: // Sigmoid
                    output = (double) 1/(1+Math.Exp(-x));
                    break;
                case 2: // Tanh
                    output = (double) (Math.Exp(x)-Math.Exp(-x))/(Math.Exp(x)+Math.Exp(-x));
                    break;
                case 3: // Step
                    if (x >= 0) output = 1;
                    else output = -1;
                    break;
                default: // Identity
                    output = x;
                    break;
            }
            return output;
        }

        private double actFuncDeriv(double x, int funcChoice)
        {
            double output = 0;
            switch (funcChoice)
            {
                case 1: // Sigmoid
                    output = (double)Math.Exp(-x) / Math.Pow(1 + Math.Exp(-x),2);
                    break;
                case 2: // Tanh
                    output = (double)Math.Pow(Math.Exp(x) - Math.Exp(-x),2) / Math.Pow(Math.Exp(x) + Math.Exp(-x),2);
                    break;
                case 3: // Step
                    output = 0;
                    break;
                default: // Identity
                    output = 1;
                    break;
            }
            return output;
        }

        private void setGoals(int funcChoice)
        {
            goalNumPos = 1;
            switch (funcChoice)
            {
                case 1: // sigmoid
                    goalNumNeg = 0;
                    break;
                default: // tanh, step, Identity
                    goalNumNeg = -1;
                    break;
            }
            checkRate = (double)(Math.Abs(goalNumPos) + Math.Abs(goalNumNeg)) / 4;

            output = new List<List<double>>();
            input = new List<List<double>>();
            for (int k = 0; k < numLayers + 1; k++)
            {
                output.Add(new List<double>());
                input.Add(new List<double>());
                if (k == 0)
                {
                    for (int i = 0; i < nodesPerLayer[k]; i++)
                    {
                        input[k].Add(0);
                        output[k].Add(0);
                    }
                }
                else if (k == numLayers)
                {
                    input[k].Add(0);
                    output[k].Add(0);
                }
                else
                {
                    for (int i = 0; i < nodesPerLayer[k]; i++)
                    {
                        input[k].Add(0);
                        output[k].Add(0);
                    }
                }
            }
        }

        public void RBFwriter(StreamWriter writer)
        {
            System.Text.StringBuilder strToWrite = new System.Text.StringBuilder();

            writer.WriteLine(numLayers);
            writer.WriteLine(sysType);
            writer.WriteLine(learningRate);

            strToWrite.Clear();
            strToWrite.Append(features[0].ToString());
            for (int k = 1; k < features.Count(); k++)
            {
                strToWrite.Append(',' + features[k].ToString());
            }
            writer.WriteLine(strToWrite);

            strToWrite.Clear();
            strToWrite.Append(nodesPerLayer[0].ToString());
            for (int k = 1; k < nodesPerLayer.Count(); k++)
            {
                strToWrite.Append(',' + nodesPerLayer[k].ToString());    
            }
            writer.WriteLine(strToWrite);

            for (int k = 0; k <= numLayers; k++)
            {
                strToWrite.Clear();
                strToWrite.Append(weights[k][0][0].ToString());
                
                if (k == 0)
                {
                    for (int j = 0; j < nodesPerLayer[k]; j++)
                    {
                        for (int m = 1; m < features.Count(); m++)
                        {
                            strToWrite.Append(',' + weights[k][j][m]);
                        }
                    }
                }
                else if (k == numLayers)
                {
                    for (int m = 1; m < nodesPerLayer[k-1]; m++)
                    {
                        strToWrite.Append(',' + weights[k][0][m]);
                    }
                }
                else
                {
                    for (int j = 0; j < nodesPerLayer[k]; j++)
                    {
                        for (int m = 1; m < nodesPerLayer[k-1]; m++)
                        {
                            strToWrite.Append(',' + weights[k][j][m]);
                        }
                    }
                }
                writer.WriteLine(strToWrite);
            }

            for (int k = 0; k <= numLayers; k++)
            {
                strToWrite.Clear();
                strToWrite.Append(bias[k][0].ToString());
                if (k != numLayers)
                {
                    for (int m = 1; m < nodesPerLayer[k]; m++)
                    {
                        strToWrite.Append(',' + bias[k][m].ToString());
                    }
                }
                writer.WriteLine(strToWrite);
            }
        }

        private void useSystem(List<double> feats)
        {
            double sum = 0;
            for (int k = 0; k < numLayers + 1; k++)
            {
                if (k == 0)
                {
                    for (int i = 0; i < nodesPerLayer[k]; i++)
                    {
                        sum = 0;
                        for (int j = 0; j < features.Count(); j++)
                        {
                            sum += feats[features[j]] * weights[k][i][j];
                        }
                        sum += bias[k][i];
                        input[k][i] = sum;
                        sum = actFunc(sum, sysType);
                        output[k][i] = sum;
                    }
                }
                else if (k == numLayers)
                {
                    sum = 0;
                    for (int j = 0; j < output[k-1].Count(); j++)
                    {
                        sum += output[k - 1][j] * weights[k][0][j];
                    }
                    sum += bias[k][0];
                    input[k][0] = sum;
                    sum = actFunc(sum, sysType);
                    output[k][0] = sum;
                }
                else
                {
                    for (int i = 0; i < nodesPerLayer[k]; i++)
                    {
                        sum = 0;
                        for (int j = 0; j < output[k - 1].Count(); j++)
                        {
                            sum += output[k - 1][j] * weights[k][i][j];
                        }
                        sum += bias[k][i];
                        input[k][i] = sum;
                        sum = actFunc(sum, sysType);
                        output[k][i] = sum;
                    }
                }                
            }
        }

        public List<double> trainSystem(Dictionary<int, Candidate> dict)
        {
            List<double> outputTS = new List<double>();
            List<double> errorNow = new List<double>();
            List<double> errorPast = new List<double>();
            bool doChange;
            double TratioF = (double)1; // 1/60
            Candidate candidate = new Candidate();

            List<List<double>> tempOutList = new List<List<double>>();
            double tempOut = 0;
            double djdyInt = 0;
            for (int k = 0; k < dict.Count(); k++)
            {
                dict.TryGetValue(k + 1, out candidate);
                useSystem(candidate.featuresRating);
                tempOut = output[output.Count()][0];
                if (tempOut > goalNumPos) tempOut = goalNumPos;
                else if (tempOut < goalNumNeg) tempOut = goalNumNeg;
                outputTS.Add(tempOut);

                if (candidate.isTrue == true)
                {
                    if (tempOut < (goalNumPos - checkRate)) doChange = true;
                    else doChange = false;
                }
                else
                {
                    if (tempOut > (goalNumNeg + 2 * checkRate)) doChange = true;
                    else doChange = false;
                }

                if (doChange)
                {
                    if (candidate.isTrue) djdyInt = (goalNumPos - tempOut) * TratioF;
                    else djdyInt = goalNumNeg - tempOut;

                    double curErr;
                    for (int j = numLayers; j >= 0; j--)
                    {
                        if (j == numLayers)
                        {
                            errorNow.Add(djdyInt);
                        }
                        else
                        {
                            for (int m = 0; m < nodesPerLayer[j]; m++)
                            {
                                curErr = 0;
                                for (int n = 0; n < errorPast.Count(); n++)
                                {
                                    curErr += errorPast[n] * weights[j][m][n];
                                    if (k == 0) deltaWeight[j][m][n] = learningRate * errorPast[n] * actFuncDeriv(input[j+1][m], sysType) * output[j][m];
                                    else deltaWeight[j][m][n] += learningRate * errorPast[n] * actFuncDeriv(input[j + 1][m], sysType) * output[j][m];
                                }
                                errorNow.Add(curErr);
                                if (k == 0) deltaBias[j][m] = learningRate * curErr;
                                else deltaBias[j][m] += learningRate * curErr;
                            }
                        }
                        errorPast = errorNow.ToList();
                        errorNow = new List<double>();
                    }

                }
            }

            updateSystem(deltaWeight, deltaBias);

            if (learningRate > 0.15) learningRate -= 0.1;
            else if (learningRate > 0.015) learningRate -= 0.01;
            else if (learningRate > 0.0025) learningRate -= 0.002;
            else if (learningRate > 0.00025) learningRate -= .0001;
            else learningRate = 0.00025;

            return outputTS;
        }

        private void updateSystem(List<List<List<double>>> dw, List<List<double>> db)
        {
            for (int k = 0; k < numLayers + 1; k++)
            {
                if (k == 0)
                {
                    for (int m = 0; m < nodesPerLayer[k]; m++)
                    {
                        bias[k][m] += db[k][m];
                        for (int j = 0; j < features.Count(); j++)
                        {
                            weights[k][m][j] += dw[k][m][j];
                        }
                    }
                }
                else if (k == numLayers)
                {
                    for (int m = 0; m < 2; m++)
                    {
                        bias[k][m] += db[k][m];
                        for (int j = 0; j < weights[k][m].Count(); j++)
                        {
                            weights[k][m][j] += dw[k][m][j];
                        }
                    }
                }
                else
                {
                    for (int m = 0; m < nodesPerLayer[k]; m++)
                    {
                        bias[k][m] += db[k][m];
                        for (int j = 0; j < weights[k][m].Count(); j++)
                        {
                            weights[k][m][j] += dw[k][m][j];
                        }
                    }
                }
            }
        }

        public List<double> testSystem(Dictionary<int, Candidate> dict)
        {
            List<double> outputTS = new List<double>();
            int i = 1;
            Candidate candidate = new Candidate();

            // find current outputs of network
            double tempOut;
            int lastIndex = 0;
            while (dict.ContainsKey(i))
            {
                dict.TryGetValue(i, out candidate);
                useSystem(candidate.featuresRating);
                lastIndex = output.Count() - 1;
                if (output[lastIndex][0] > checkRate) tempOut = 1;
                else tempOut = -1; 
                outputTS.Add(tempOut);
                i++;
            }

            return outputTS;
        }
    }
}
