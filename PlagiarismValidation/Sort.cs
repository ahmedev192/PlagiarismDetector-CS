using System;
using System.Collections.Generic;

namespace PlagiarismValidation
{
    public class Sort
    {
        public static void MGSort<T>(List<T> lst, Func<T, double> getKey)
        {
            MGSort(lst, 0, lst.Count - 1, getKey);
        }

        private static void MGSort<T>(List<T> lst, int start, int end, Func<T, double> getKey)
        {
            if (start < end)
            {
                int mid = (start + end) / 2;
                MGSort(lst, start, mid, getKey);
                MGSort(lst, mid + 1, end, getKey);
                MG(lst, start, mid, end, getKey);
            }
        }

        private static void MG<T>(List<T> lst, int start, int mid, int end, Func<T, double> getKey)
        {
            int n1 = mid - start + 1;
            int n2 = end - mid;

            List<T> leftList = lst.GetRange(start, n1);
            List<T> rightList = lst.GetRange(mid + 1, n2);

            int leftIDX = 0, rightIDX = 0;
            int currentIDX = start;

            while (leftIDX < n1 && rightIDX < n2)
            {
                if (getKey(leftList[leftIDX]) >= getKey(rightList[rightIDX]))
                {
                    lst[currentIDX] = leftList[leftIDX];
                    leftIDX++;
                }
                else
                {
                    lst[currentIDX] = rightList[rightIDX];
                    rightIDX++;
                }
                currentIDX++;
            }

            while (leftIDX < n1)
            {
                lst[currentIDX] = leftList[leftIDX];
                leftIDX++;
                currentIDX++;
            }

            while (rightIDX < n2)
            {
                lst[currentIDX] = rightList[rightIDX];
                rightIDX++;
                currentIDX++;
            }
        }

















        public static void MGSort<T>(List<T> lst, IComparer<T> comparer)
        {
            MGSort(lst, 0, lst.Count - 1, comparer);
        }

        private static void MGSort<T>(List<T> lst, int start, int end, IComparer<T> comparer)
        {
            if (start < end)
            {
                int mid = (start + end) / 2;
                MGSort(lst, start, mid, comparer);
                MGSort(lst, mid + 1, end, comparer);
                MG(lst, start, mid, end, comparer);
            }
        }

        private static void MG<T>(List<T> lst, int start, int mid, int end, IComparer<T> comparer)
        {
            int n1 = mid - start + 1;
            int n2 = end - mid;

            List<T> leftList = lst.GetRange(start, n1);
            List<T> rightList = lst.GetRange(mid + 1, n2);

            int leftIDX = 0, rightIDX = 0;
            int currentIDX = start;

            while (leftIDX < n1 && rightIDX < n2)
            {
                if (comparer.Compare(leftList[leftIDX], rightList[rightIDX]) > 0) // Changed to >= for descending order
                {
                    lst[currentIDX] = leftList[leftIDX];
                    leftIDX++;
                }
                else if (comparer.Compare(leftList[leftIDX], rightList[rightIDX]) == 0)
                {
                    if (typeof(T) == typeof(Edge))
                    {
                        int result = GetEntryIndex(leftList[leftIDX], rightList[rightIDX]);
                        if (result == 0)
                        {
                            lst[currentIDX] = leftList[leftIDX];
                            leftIDX++;
                        }
                        else
                        {
                            lst[currentIDX] = rightList[rightIDX];
                            rightIDX++;
                        }

                    }
                    else
                    {
                        lst[currentIDX] = leftList[leftIDX];
                        leftIDX++;
                    }


                }
                else
                {
                    lst[currentIDX] = rightList[rightIDX];
                    rightIDX++;
                }
                currentIDX++;
            }

            while (leftIDX < n1)
            {
                lst[currentIDX] = leftList[leftIDX];
                leftIDX++;
                currentIDX++;
            }

            while (rightIDX < n2)
            {
                lst[currentIDX] = rightList[rightIDX];
                rightIDX++;
                currentIDX++;
            }
        }


        private static int GetEntryIndex<T>(T EDG1, T EDG2)
        {
            Edge edge1 = EDG1 as Edge;
            Edge edge2 = EDG2 as Edge;

            if (GlobalVariables.similarityMap[(edge1.V1, edge1.V2)].IDX > GlobalVariables.similarityMap[(edge2.V1, edge2.V2)].IDX)
                return 1;
            else
                return 0;

        }


    }






}