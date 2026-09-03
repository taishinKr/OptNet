using System.Numerics;

namespace OptNet
{
    public static class GeneralMath<T> where T : struct, INumber<T>
    {
        public static T[,] MultiplyMatrices(in T[,] matrixA, in T[,] matrixB)
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
        public static T InnerProduct(in T[] vectorA, in T[] vectorB)
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
        public static T[] LinearTransform(in T[,] matrix, in T[] vector)
        {
            return LinearTransform(matrix, vector, 0, matrix.GetLength(0) - 1);
        }
        public static T[] LinearTransform(in T[,] matrix, in T[] vector, in int minK, in int maxK)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (cols != vector.Length)
            {
                throw new InvalidOperationException("Matrix columns must match the vector length for transformation.");
            }

            T[] result = new T[rows];

            //for (int i = 0; i < rows; i++)
            for (int i = minK; i < maxK + 1; i++)
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
        public static T[,] InvertMatrix(in T[,] matrix)
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
        public static T[,] TransposeMatrix(in T[,] matrix)
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
        public static T[,] ExtractSubarray(in T[,] source, in uint startRow, in uint startCol, in uint numRows, in uint numCols)
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

        public static UInt32 SetFlag(ref UInt32 Flag, in UInt32 mask)
        {
            Flag |= mask;
            return Flag;
        }
        public static UInt32 ClrFlag(ref UInt32 Flag, in UInt32 mask)
        {
            Flag &= (UInt32)(~mask);
            return Flag;
        }
        public static bool IsSetFlag(in UInt32 Flag, in UInt32 mask)
        {
            if ((Flag & mask) == 0)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Groups a set of points into clusters based on a specified minimum distance.
        /// </summary>
        /// <remarks>Each point is assigned to exactly one cluster. A cluster is formed by grouping points
        /// such that every point in the cluster is within the specified minimum distance from at least one other point
        /// in the cluster.</remarks>
        /// <param name="optResult">An array of tuples where each tuple contains a point's coordinates and an associated value.</param>
        /// <param name="xMinDist">The minimum distance required to consider points as part of the same cluster.</param>
        /// <returns>A list of clusters, where each cluster is a list of tuples containing the coordinates and associated value
        /// of points that are close to each other based on the specified minimum distance.</returns>
        public static List<List<(T[][] X, T J)>> ClusterByDistance((T[][] X, T J)[] optResult, T xMinDist)
        {
            var clusters = new List<List<(T[][] X, T J)>>();
            var used = new bool[optResult.Length];

            for (int i = 0; i < optResult.Length; i++)
            {
                if (used[i]) continue;
                var cluster = new List<(T[][] X, T J)> { optResult[i] };
                used[i] = true;

                for (int j = 0; j < optResult.Length; j++)
                {
                    if (used[j]) continue;
                    bool closeToAll = true;
                    foreach (var (X, J) in cluster)
                    {
                        T dist = T.Zero;
                        for (int d = 0; d < X.Length; d++)
                        {
                            // if X[d], optResult[j].X[d] are interval, the center point is considered for calculation
                            T avgX = (X[d][0] + X[d][1]) / (T.One + T.One);
                            T avgOptResut = (optResult[j].X[d][0] + optResult[j].X[d][1]) / (T.One + T.One);

                            //T dx = X[d][0] - optResult[j].X[d][0];
                            T dx = avgX - avgOptResut;
                            dist += dx * dx;
                        }

                        dist = T.CreateChecked(Math.Sqrt(double.CreateChecked(dist)));
                        if (dist >= xMinDist)
                        {
                            closeToAll = false;
                            break;
                        }
                    }
                    if (closeToAll)
                    {
                        cluster.Add(optResult[j]);
                        used[j] = true;
                    }
                }
                clusters.Add(cluster);
            }
            return clusters;
        }
        /// <summary>
        /// Calculates the standard deviation of a subset of an array of type T
        /// , where T is a numeric type that implements INumber and IRootFunctions.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="subStart"></param>
        /// <param name="subEnd"></param>
        /// <returns></returns>
        public static T CalculateStdDev<T> (T[] array, int subStart, int subEnd) where T : INumber<T>, IRootFunctions<T>
        {
            int count = subEnd - subStart + 1;
            if (count <= 0) return T.Zero;

            // 1. 평균 계산
            T sum = T.Zero;
            for (int i = subStart; i <= subEnd; i++)
            {
                sum += array[i];
            }
            T mean = sum / T.CreateChecked(count);

            // 2. 편차 제곱합 계산
            T sumOfSquares = T.Zero;
            for (int i = subStart; i <= subEnd; i++)
            {
                T diff = array[i] - mean;
                sumOfSquares += diff * diff;
            }

            // 3. 분산 및 표준편차 계산 (제곱근 연산을 위해 T.Root / Math.Sqrt 처리)
            T variance = sumOfSquares / T.CreateChecked(count);

            // INumber<T> / IRootFunctions<T>를 지원하는 경우 T.Sqrt 사용, 
            // 그렇지 않은 경우 double 변환 후 제곱근 계산을 T로 다시 캐스팅
            if (typeof(IRootFunctions<T>).IsAssignableFrom(typeof(T)))
            {
                return T.Sqrt(variance);
            }
            else
            {
                double doubleVariance = Convert.ToDouble(variance);
                return T.CreateChecked(Math.Sqrt(doubleVariance));
            }
        }
        public static (T mean, T maxDev) CalculateMaxDev(T[] J, int start, int end)
        {
            int count = end - start + 1;

            // 구간이 유효하지 않은 경우 T의 기본값(0) 반환
            if (count <= 0 || start < 0 || end >= J.Length)
            {
                return (T.Zero, T.Zero);
            }

            // 1. 구간 평균(Mean) 계산
            T sum = T.Zero;
            for (int i = start; i <= end; i++)
            {
                sum += J[i];
            }
            T mean = sum / T.CreateChecked(count);

            // 2. 평균과의 차이값(절대 편차) 중 최댓값 구하기
            T maxDev = T.Zero;
            for (int i = start; i <= end; i++)
            {
                // T.Abs를 사용하여 절대 편차 계산 (|J[i] - mean|)
                T dev = T.Abs(J[i] - mean);

                if (dev > maxDev)
                {
                    maxDev = dev;
                }
            }

            return (mean, maxDev);
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
