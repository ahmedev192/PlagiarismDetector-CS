using System;
using System.Collections.Generic;

namespace PlagiarismValidation
{
    public class Sort
    {
        public static void MergeSort<T>(List<T> list, Func<T, double> getKey)
        {
            MergeSort(list, 0, list.Count - 1, getKey);
        }

        private static void MergeSort<T>(List<T> list, int start, int end, Func<T, double> getKey)
        {
            if (start < end)
            {
                int mid = (start + end) / 2;
                MergeSort(list, start, mid, getKey);
                MergeSort(list, mid + 1, end, getKey);
                Merge(list, start, mid, end, getKey);
            }
        }

        private static void Merge<T>(List<T> list, int start, int mid, int end, Func<T, double> getKey)
        {
            int n1 = mid - start + 1;
            int n2 = end - mid;

            List<T> leftList = list.GetRange(start, n1);
            List<T> rightList = list.GetRange(mid + 1, n2);

            int leftIndex = 0, rightIndex = 0;
            int currentIndex = start;

            while (leftIndex < n1 && rightIndex < n2)
            {
                if (getKey(leftList[leftIndex]) >= getKey(rightList[rightIndex]))
                {
                    list[currentIndex] = leftList[leftIndex];
                    leftIndex++;
                }
                else
                {
                    list[currentIndex] = rightList[rightIndex];
                    rightIndex++;
                }
                currentIndex++;
            }

            while (leftIndex < n1)
            {
                list[currentIndex] = leftList[leftIndex];
                leftIndex++;
                currentIndex++;
            }

            while (rightIndex < n2)
            {
                list[currentIndex] = rightList[rightIndex];
                rightIndex++;
                currentIndex++;
            }
        }

        public static void MergeSort<T>(List<T> list, IComparer<T> comparer)
        {
            MergeSort(list, 0, list.Count - 1, comparer);
        }

        private static void MergeSort<T>(List<T> list, int start, int end, IComparer<T> comparer)
        {
            if (start < end)
            {
                int mid = (start + end) / 2;
                MergeSort(list, start, mid, comparer);
                MergeSort(list, mid + 1, end, comparer);
                Merge(list, start, mid, end, comparer);
            }
        }

        private static void Merge<T>(List<T> list, int start, int mid, int end, IComparer<T> comparer)
        {
            int n1 = mid - start + 1;
            int n2 = end - mid;

            List<T> leftList = list.GetRange(start, n1);
            List<T> rightList = list.GetRange(mid + 1, n2);

            int leftIndex = 0, rightIndex = 0;
            int currentIndex = start;

            while (leftIndex < n1 && rightIndex < n2)
            {
                if (comparer.Compare(leftList[leftIndex], rightList[rightIndex]) <= 0)
                {
                    list[currentIndex] = leftList[leftIndex];
                    leftIndex++;
                }
                else
                {
                    list[currentIndex] = rightList[rightIndex];
                    rightIndex++;
                }
                currentIndex++;
            }

            while (leftIndex < n1)
            {
                list[currentIndex] = leftList[leftIndex];
                leftIndex++;
                currentIndex++;
            }

            while (rightIndex < n2)
            {
                list[currentIndex] = rightList[rightIndex];
                rightIndex++;
                currentIndex++;
            }
        }










    }





}
