using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;


namespace WatsonCompetitionCode
{
    class LogisiticRegression
    {
        private Dictionary<int, Candidate> trainingDataSet;
        private DenseVector theta;
        private DenseVector J;
        private DenseMatrix x;
        private DenseVector y;
        private DenseMatrix hessianResult;
        public const int MAX_ITERATIONS= 7;
        private int m;
        private int n;
        


        public LogisiticRegression(Dictionary<int,Candidate> data)
        {
            this.trainingDataSet = data;
            int numberFeatures = trainingDataSet[1].featuresRating.Count;

            //Initialize xdata matrix
            DenseVector[] xdata;
            xdata = new DenseVector[trainingDataSet.Count];
            //xdata[0] = new DenseVector(numberFeatures,1); 
            int k = 0;
           

            //intialize y data
            List<double> ydata = new List<double>();
            

            //fill x and y data from dictionary
            foreach (KeyValuePair<int,Candidate> candidate in trainingDataSet)
            {
                List<double> intermediate = candidate.Value.featuresRating.ToList();
                intermediate.Insert(0, 1);
                xdata[k] = new DenseVector(intermediate.ToArray());
                if (candidate.Value.isTrue) ydata.Add(1);
                else ydata.Add(0);
                k++;
            }

            //populate fields with data
            x = DenseMatrix.OfRowVectors(xdata);
            y = new DenseVector(ydata.ToArray());

            m = x.RowCount;
            n = x.ColumnCount-1;

            //Intialize fitting parameters, theta
            theta = new DenseVector(m+1,1);
            /*test code
            DenseVector z;
            z = x * theta;
            double[] test1;
            test1 = new double[3] {2,3,4};
            double[] test2;
            test2 = new double[3] {4,3,2};
            DenseVector test1V = new DenseVector(test1);
            DenseVector test2V = new DenseVector(test2);
            DenseVector result = (DenseVector) test1V.PointwiseMultiply(test2V);*/
            


            //Console.ReadLine();
        }

        private DenseVector sigmoid(DenseVector z)
        {
            List<double> vector = new List<double>();
            foreach (double element in z)
            {
                vector.Add((double)(1.0 / (1.0 + Math.Exp(-element))));
            }
            return new DenseVector(vector.ToArray());
        }
        public double probability(List<double> features)
        {
            return 0;
        }
        private DenseVector gradient(DenseMatrix xMat, DenseVector hVec, DenseVector yVec)
        {
            DenseVector grad = (DenseVector)(xMat.Transpose().Divide(m) * (hVec - yVec));
            return grad;
        }
        private DenseMatrix hessian(DenseMatrix xMat, DenseVector hVec)
        {
            DenseMatrix result;
            DiagonalMatrix diag = DiagonalMatrix.Identity(hVec.Count);
            diag.SetDiagonal(hVec);
            DiagonalMatrix diagMinus1 = DiagonalMatrix.Identity(hVec.Count);
            diagMinus1.SetDiagonal(1-hVec);
            result = (DenseMatrix) ((xMat.Transpose().Divide(m)) * diag * diagMinus1 * xMat);
            return result;
        }
        public void train(int iterations = MAX_ITERATIONS)
        {
            J = new DenseVector(iterations, 0);
            for (int k = 0; k < iterations; k++)
            {
                DenseVector z = x * theta;
                DenseVector h = sigmoid(z);
                DenseVector grad = gradient(x, h, y);
                DenseMatrix H = hessian(x, h);

                //Calculate J for testing convergence
                J[k] = (double) (y.Negate().PointwiseMultiply((DenseVector) log(h)) - (1-y).PointwiseMultiply( (DenseVector) log((DenseVector)(1-h)))).Sum();
                theta = (DenseVector) (theta - H.Inverse()*grad);
            }
        }
        private DenseVector log(DenseVector vec)
        {
            List<double> result = new List<double>();
            foreach (double element in vec)
            {
                result.Add((double)Math.Log(element));
            }
            return new DenseVector(result.ToArray());
        }
        
    }
    
}

