using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace OptNet
{
    internal static class GeneralMath<T> where T : struct, INumber<T>
    {
        public  static T[,] MultiplyMatrices(T[,] matrixA, T[,] matrixB)
        {
            int rowsA = matrixA.GetLength(0);
            int colsA = matrixA.GetLength(1);
            int rowsB = matrixB.GetLength(0);
            int colsB = matrixB.GetLength(1);

            if (colsA != rowsB)
            {
                throw new InvalidOperationException("Matrix dimensions are not compatible for multiplication.");
            }

            T[,] result = new T[rowsA, colsB];

            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    T sum = T.Zero;
                    for (int k = 0; k < colsA; k++)
                    {
                        sum += matrixA[i, k] * matrixB[k, j];
                    }
                    result[i, j] = sum;
                }
            }

            return result;
        }
        public static T InnerProduct(T[] vectorA, T[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
            {
                throw new InvalidOperationException("Vectors must have the same length to compute the inner product.");
            }

            T result = T.Zero;

            for (int i = 0; i < vectorA.Length; i++)
            {
                result += vectorA[i] * vectorB[i];
            }

            return result;
        }
        public static T[] LinearTransform(T[,] matrix, T[] vector)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (cols != vector.Length)
            {
                throw new InvalidOperationException("Matrix columns must match the vector length for transformation.");
            }

            T[] result = new T[rows];

            for (int i = 0; i < rows; i++)
            {
                T sum = T.Zero;
                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j] * vector[j];
                }
                result[i] = sum;
            }

            return result;
        }
        public static T[,] InvertMatrix(T[,] matrix)
        {
            int n = matrix.GetLength(0);

            if (n != matrix.GetLength(1))
            {
                throw new InvalidOperationException("Matrix must be square to compute its inverse.");
            }

            // Create an augmented matrix [matrix | identity]
            T[,] augmented = new T[n, 2 * n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = matrix[i, j];
                }
                augmented[i, i + n] = T.One; // Add identity matrix
            }

            // Perform Gaussian elimination
            for (int i = 0; i < n; i++)
            {
                // Make the diagonal element 1
                T diagElement = augmented[i, i];
                if (diagElement == T.Zero)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                }

                for (int j = 0; j < 2 * n; j++)
                {
                    augmented[i, j] /= diagElement;
                }

                // Make other elements in the column 0
                for (int k = 0; k < n; k++)
                {
                    if (k == i) continue;

                    T factor = augmented[k, i];
                    for (int j = 0; j < 2 * n; j++)
                    {
                        augmented[k, j] -= factor * augmented[i, j];
                    }
                }
            }

            // Extract the inverse matrix from the augmented matrix
            T[,] inverse = new T[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    inverse[i, j] = augmented[i, j + n];
                }
            }

            return inverse;
        }
        public static T[,] TransposeMatrix(T[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            T[,] transposed = new T[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            }

            return transposed;
        }
        public static T[,] ExtractSubarray(T[,] source, uint startRow, uint startCol, uint numRows, uint numCols)
        {
            if (startRow < 0 || startCol < 0 || startRow + numRows > source.GetLength(0) || startCol + numCols > source.GetLength(1))
            {
                throw new ArgumentOutOfRangeException("Subarray dimensions are out of bounds.");
            }

            T[,] subarray = new T[numRows, numCols];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    subarray[i, j] = source[startRow + i, startCol + j];
                }
            }

            return subarray;
        }
        
        /*public static BigInteger Sqrt(BigInteger value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Cannot compute the square root of a negative number.");
            }

            if (value == 0 || value == 1)
            {
                return value;
            }

            BigInteger left = 0;
            BigInteger right = value;
            BigInteger mid;

            while (right - left > 1)
            {
                mid = (left + right) / 2;

                if (mid * mid <= value)
                {
                    left = mid;
                }
                else
                {
                    right = mid;
                }
            }

            return left;
        }*/
    }
}
