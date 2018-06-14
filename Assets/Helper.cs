using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helper
{
    public static void InitializeMatrix<T>(ref T[,] matrix, T value)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = value;
            }
        }
    }

    public static T[] GetRow<T>(T[,] matrix, int row)
    {
        int columns = matrix.GetLength(1);
        T[] array = new T[columns];

        for (int i = 0; i < columns; ++i)
        {
            array[i] = matrix[row, i];
        }

        return array;
    }

    public static int Max<T>(T[] array)
    {
        T maxValue = array.Max();
        return array.ToList().IndexOf(maxValue);
    }

    public static int Index(Vector2 state, int num)
    {
        return (int)state.x + (int)state.y * num;
    }
}
