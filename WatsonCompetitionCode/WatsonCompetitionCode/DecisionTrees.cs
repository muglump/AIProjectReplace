using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;



namespace WatsonCompetitionCode
{
    class DecisionTrees
    {
        TreeNode startNode;

        public DecisionTrees(Dictionary<int, Candidate> candidates, int branchesPerNode)
        {
            List<List<double>> rows = new List<List<double>>();
            List<bool> rowTruth = new List<bool>();
            //Pull apart the dictionary because its easier to work with this way.
            //Additionally fill a 2D List of all rows and columns, and a List of the row truths
            List<Candidate> listCandidates = new List<Candidate>();
            foreach (Candidate value in candidates.Values)
            {
                listCandidates.Add(value);
                rows.Add(value.featuresRating);
                rowTruth.Add(value.isTrue);
            }

            //We need to try and reduce the Branching factor, so we call the following function. Additionally we now will only have integers
            Console.Write("Reducing");
            List<List<int>> newRows = reduceBranching(rows, branchesPerNode);
            Console.Write("Reduced");
            //Now we are done with the preprocessing and need to actually start building the items we need to create the id3Tree
            //Convert to int[][] and bool[] because that is what the tree needs to work
            foreach (List<int> listw in newRows)
            {
                foreach (int itemw in listw)
                {
                    Console.Write("_" + itemw + "_");
                }
                Console.WriteLine(" ");
            }
            List<int> truth = new List<int>();
            foreach (bool value in rowTruth)
            {
                if (value)
                {
                    truth.Add(1);
                }
                else
                {
                    truth.Add(0);
                }
            }

            for (int i = 0; i < newRows.Count; i++)
            {
                newRows[i].Add(truth[i]);
            }


            //Get the number of possible values for each column
            List<int> colValCounts = getColValueCount(newRows);
            int count = colValCounts.Count;

            //Run the training data
            Console.Write("Training");
            startNode = new TreeNode();
            for (int i = 0; i < newRows.Count; i++)
            {
                
                startNode.addNodes(newRows[i], 0);
            }

            Console.WriteLine("Built");
        }

        public List<double> runTree(Dictionary<int, Candidate> candidates, int branchesPerNode)
        {
            List<double> validAnswer = new List<double>();
            List<List<double>> rows = new List<List<double>>();
            //Pull apart the dictionary because its easier to work with this way.
            //Additionally fill a 2D List of all rows and columns
            List<Candidate> listCandidates = new List<Candidate>();
            
            foreach (Candidate value in candidates.Values)
            {
                listCandidates.Add(value);
                rows.Add(value.featuresRating);
                
            }

            //We need to try and reduce the Branching factor, so we call the following function. Additionally we now will only have integers
            List<List<int>> newRows = reduceBranching(rows, branchesPerNode);

            //Now we are done with the preprocessing and need to actually start building the items we need to create the id3Tree
            //Convert to int[][] and bool[] because that is what the tree needs to work

            for (int i = 0; i < newRows.Count; i++)
            {
                bool answer = startNode.run(newRows[i], 0);
                if (answer == true)
                {
                    //if(!validAnswer.Contains(listCandidates[i].questionNumber)){
                        validAnswer.Add(listCandidates[i].questionNumber);
                    //}
                }
            }
            return validAnswer;
        }

        //This method returns an array that contains the number of branches a column can has
        private List<int> getColValueCount(List<List<int>> cols)
        {
            List<int> columnCount = new List<int>();
            for (int i = 0; i < cols[0].Count; i++)
            {
                List<int> columnVals = new List<int>();
                for (int j = 0; j < cols.Count; j++)
                {
                    if (!columnVals.Contains(cols[j][i]))
                    {
                        columnVals.Add(cols[j][i]);
                    }
                }
                columnCount.Add(columnVals.Count);
            }
            return columnCount;
        }




        //This function takes a 2D list and reduces the variation in columns such that only and allowed number of values
        private List<List<int>> reduceBranching(List<List<double>> rows, int branchesPerNode)
        {
            List<List<int>> newRows = new List<List<int>>();
            //start at the first column and loop through the column values by row
            
            for(int rowindex = 0; rowindex < rows.Count; rowindex++){
                newRows.Add(new List<int>());
            }
            for(int columnindex=0; columnindex < rows[0].Count; columnindex++){
                List<double> columnValues = new List<double>();
                
                for (int rowindex = 0; rowindex < rows.Count; rowindex++)
                {
                    columnValues.Add(rows[rowindex][columnindex]);
                }
                
                //No that we have and array with all of the values for a column check the variation, if it is less then or equal to the allow amount we proceed,
                //otherwise we will need to normalize the values
                List<int> newColumn = new List<int>();
            
                //Reduce the variation in values
                
                newColumn = reduce(columnValues, branchesPerNode);
                
                for (int rowindex = 0; rowindex < rows.Count; rowindex++)
                {
                    newRows[rowindex].Add(newColumn[rowindex]);
                }
                //newRows.Add(newColumn);
            }
            return newRows;

        }

        //This method reduces the number of values such that they are <= branches number of different values
        //To do this we sort the list, and split it into branches number of sublists.
        //The median is then found for each sublist, and values are assigned by the current values relationship to those values
        private List<int> reduce(List<double> column, int branches)
        {
            List<double> values = new List<double>();
            foreach(double value in column)
            {
                if (!values.Contains(value))
                {
                    values.Add(value);
                }
            }
            if (values.Count == 1)
            {
                List<int> newList = new List<int>();
                foreach (double value in column)
                {
                    newList.Add(Convert.ToInt32(value));
                }
            }
            column.Sort();
            //Figure out how to divide the items, this may result in slightly unequal arrays but it happend to be the best way to divide 
            int divider = column.Count / branches - 1;
            List<List<double>> listOfSubList = new List<List<double>>();
            int lowerbound = 0;
            int upperbound = divider;
            int step = 0;
            //Break the list into the properly sized subLists
            while (step < branches)
            {
                if (step + 1 == branches)
                {
                    upperbound = column.Count - 1;
                }
                List<double> sublist = new List<double>();
                for (int i = lowerbound; lowerbound <= upperbound; lowerbound++)
                {
                    sublist.Add(i);
                }
                listOfSubList.Add(sublist);
                lowerbound = upperbound + 1;
                upperbound += upperbound;
                step += 1;
            }

            //Find the means of each sublist
            List<double> means = new List<double>();
            foreach (List<double> list in listOfSubList)
            {
                double value = 0;
                for (int k = 0; k < list.Count; k++)
                {
                    value += list[k];
                }
                value = value / list.Count;
                means.Add(value);
            }

            //Now loop through all of the values in the columns and assing a value based off relationship to means values
            List<int> newColumn = new List<int>();
            Console.WriteLine("Looking at n Values: " + column.Count);
            
            foreach (double value in column)
            {
                for (int j = 0; j < means.Count; j++)
                {
                    if (value <= means[j])
                    {
                        newColumn.Add(j);
                        continue;
                    }
                    if (j + 1 == means.Count)
                    {
                        if (value >= means[j])
                        {
                            newColumn.Add(j + 1);
                            continue;
                        }
                    }
                }
            }

            Console.WriteLine("Returning n element " + newColumn.Count);
            return newColumn;
        }



        //Checks the number of values in the List, if there are more then allowedNum returns true, otherwise false;
        private bool nValues(List<double> vals, int allowedNum)
        {
            List<double> values = new List<double>();
            foreach (double item in vals)
            {
                if (!values.Contains(item))
                {
                    values.Add(item);
                }
            }
            return values.Count > allowedNum;
        }


            
    }


    class TreeNode
    {
        List<int> comparators;
        Dictionary<int, TreeNode> successors;
        bool end;

        public TreeNode()
        {
            comparators = new List<int>();
            successors = new Dictionary<int, TreeNode>();
        }

        public TreeNode(bool value)
        {
            end = value;
        }

        public void tostring(int index){
            Console.WriteLine("Index: " + index + " numberOfNextNodes: " + comparators.Count);
            foreach (TreeNode node in successors.Values)
            {
                node.tostring(index + 1);
            }
        }

        public bool run(List<int> row, int index)
        {
            TreeNode node;
            successors.TryGetValue(row[index], out node);
            if (node.successors == null)
            {
                return node.end;
            }
            else
            {
               return node.run(row, index + 1);
            }
        }

        public void addNode(int comparator){
            if(!comparators.Contains(comparator)){
                comparators.Add(comparator);
                successors.Add(comparator, new TreeNode());
            }
        }

        public void addNodes(List<int> values, int index)
        {
            
            if (!comparators.Contains(values[index]))
            {
                comparators.Add(values[index]);
                if (values.Count == index + 2)
                {
                    bool value;
                    if (values[index] == 0)
                    {
                        value = false;
                    }
                    else
                    {
                        value = true;
                    }
                    TreeNode node = new TreeNode(value);
                    successors.Add(values[index], node);
                }
                else
                {
                    TreeNode node = new TreeNode();
                    successors.Add(values[index], node);
                    node.addNodes(values, index + 1);
                }

            }
            else
            {
                TreeNode node;
                successors.TryGetValue(values[index], out node);
                if (node.successors == null)
                {
                    return;
                }
                node.addNodes(values, index + 1);
            }
            
        }

        public TreeNode successor(int comparator)
        {
            TreeNode successer;
            for (int i = 0; i < comparators.Count; i++)
            {
                if (comparator == comparators[i])
                {
                    
                    successors.TryGetValue(i, out successer);
                    return successer;

                }  
            }
            successors.TryGetValue(0, out successer);
            return successer;
        }
    }

    
}
