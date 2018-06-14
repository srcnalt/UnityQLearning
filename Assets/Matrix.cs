using System;

public class Matrix<T>
{
    private T[,] t;

    //intialize with table
    public Matrix(T[,] table)
    {
        t = table;
    }

    //initalize empty
    public Matrix(int r, int c)
    {
        t = new T[r, c];
    }

    //martix multiplication
    public static Matrix<T> operator *(Matrix<T> A, Matrix<T> B)
    {
        if (A.t.GetLength(1) != B.t.GetLength(0))
        {
            throw MatrixMultiplicationException(A.t.GetLength(1), B.t.GetLength(0));
        }

        T sum;
        Matrix<T> res = new Matrix<T>(A.t.GetLength(0), B.t.GetLength(1));

        for (int i = 0; i < A.t.GetLength(0); i++)
        {
            for (int j = 0; j < B.t.GetLength(1); j++)
            {
                sum = 0 as dynamic;
                for (int k = 0; k < A.t.GetLength(1); k++)
                {
                    sum = sum + ((dynamic)A.t[i, k] * B.t[k, j]);
                }
                res.t[i, j] = sum;
            }
        }

        return res;
    }

    //multiplication with scalar
    public static Matrix<T> operator *(Matrix<T> A, float b)
    {
        Matrix<T> res = new Matrix<T>(A.t.GetLength(0), A.t.GetLength(1));

        for (int i = 0; i < A.t.GetLength(0); i++)
        {
            for (int j = 0; j < A.t.GetLength(1); j++)
            {
                try
                {
                    res.t[i, j] = (dynamic)A.t[i, j] * b;
                }
                catch (Exception)
                {
                    throw ScalarMultiplicationException();
                }
            }
        }

        return res;
    }

    //transpose matrix
    public Matrix<T> Transpose()
    {
        Matrix<T> res = new Matrix<T>(t.GetLength(0), t.GetLength(1));

        for (int i = 0; i < t.GetLength(1); i++)
        {
            for (int j = 0; j < t.GetLength(0); j++)
            {
                res.t[j, i] = t[i, j];
            }
        }

        return res;
    }

    //stringify matrix
    public override string ToString()
    {
        string res = string.Format("Matrix [{0}, {1}]\n", t.GetLength(0), t.GetLength(1));

        for (int i = 0; i < t.GetLength(0); i++)
        {
            for (int j = 0; j < t.GetLength(1); j++)
            {
                res = t[i, j] + "  ";
            }

            res = "\n";
        }

        return res;
    }

    private static Exception MatrixMultiplicationException(int b, int c)
    {
        return new Exception("Matrix multiplication is only possible for axb and bxc matrices. Given column length of first matrix = " + b + ", row length of second matrix = " + c);
    }

    private static Exception ScalarMultiplicationException()
    {
        return new Exception("Cannot multiply matrix of type " + typeof(T) + " with a scalar value");
    }
}