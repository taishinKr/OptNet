using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Drawing.Design;
using System.Collections.Immutable;
using System.Collections;

namespace OptNet
{
    public class ImmutableArray2D<TValue> : IEnumerable<TValue>
    {
        private readonly TValue[,] _data;
        public TValue[,] Data { get { return _data; } }
        private ImmutableArray2D(TValue[,] data)
        {
            _data = data;
        }
        public TValue this[int row, int col] => _data[row, col];
        public int GetLength(int dimension) => _data.GetLength(dimension);
        public static ImmutableArray2D<TValue> Create(TValue[,] data)
        {
            return new ImmutableArray2D<TValue>((TValue[,])data.Clone());
        }
        // Implement IEnumerable<TValue>
        public IEnumerator<TValue> GetEnumerator()
        {
            for (int i = 0; i < _data.GetLength(0); i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    yield return _data[i, j];
                }
            }
        }

        // Explicit implementation for non-generic IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    internal class OfflineGet<T> where T : struct, INumber<T>
    {
        // Immutable fields
        public uint M { get; }
        public uint TildM { get; }
        public uint Ns { get; }
        public ImmutableArray<T> Dp { get; }
        public ImmutableArray2D<T> P { get; }
        public ImmutableArray2D<T> Ra { get; }
        public ImmutableArray2D<T> Rb { get; }
        public ImmutableArray2D<T> Rc { get; }

        private string fileNameCsv;
        private string fileNameBin;

        public OfflineGet(in uint ns)
        {
            M = ns - 1;
            TildM = M * 100;
            Ns = M + 1;

            string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            string fileNameBase = Path.Combine(baseFolder, "OptNetConst" + M.ToString() + "_" + TildM.ToString());
            fileNameCsv = fileNameBase + ".csv";
            fileNameBin = fileNameBase + ".bin";


            if (File.Exists(fileNameBin))
            {
                (Dp, Ra, Rb, Rc, P) = FileReadBin(fileNameBin);
            }
            else if (File.Exists(fileNameCsv))
            {
                (Dp, Ra, Rb, Rc, P) = FileReadCsv(fileNameCsv);
            }
            else
            {
                Dp = ImmutableArray.Create<T>(CalculateDp());
                P = ImmutableArray2D<T>.Create(CalculateP());
                var Q = ImmutableArray2D<T>.Create(CalculateQ());
                var rw = ImmutableArray2D<T>.Create(CalculateRw(Q.Data));
                Ra = ImmutableArray2D<T>.Create(
                    GeneralMath<T>.MultiplyMatrices(CalculateABC(anx), rw.Data));
                Rb = ImmutableArray2D<T>.Create(
                    GeneralMath<T>.MultiplyMatrices(CalculateABC(bnx), rw.Data));
                Rc = ImmutableArray2D<T>.Create(
                    GeneralMath<T>.MultiplyMatrices(CalculateABC(cnx), rw.Data));

                FileWriteCsv();
                FileWriteBin();
            }
        }
        private (ImmutableArray<T>, ImmutableArray2D<T>, ImmutableArray2D<T>, ImmutableArray2D<T>, ImmutableArray2D<T>) FileReadCsv(string fileName)
        {
            ImmutableArray<T> dp;
            ImmutableArray2D<T> p;
            ImmutableArray2D<T> ra;
            ImmutableArray2D<T> rb;
            ImmutableArray2D<T> rc;

            using (FileStream fs = new FileStream(fileNameCsv, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                convOneLine(sr, out T[] dptmp);
                uint rowLength = (uint)(dptmp.Length - 1);
                dp = ImmutableArray.Create(dptmp);

                var rabctmp = new T[rowLength, M + 1];
                for (uint i = 0; i < rowLength; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = row[j];
                }
                ra = ImmutableArray2D<T>.Create(rabctmp);

                for (uint i = 0; i < rowLength; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = row[j];
                }
                rb = ImmutableArray2D<T>.Create(rabctmp);

                for (uint i = 0; i < rowLength; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = row[j];
                }
                rc = ImmutableArray2D<T>.Create(rabctmp);

                var ptmp = new T[M + 1, M + 1];
                for (uint i = 0; i < M + 1; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        ptmp[i, j] = row[j];
                }
                p = ImmutableArray2D<T>.Create(ptmp);
            }
            return (dp, ra, rb, rc, p);
        }
        private void convOneLine(in StreamReader sr, out T[] lowD)
        {
            string? line;
            if ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                uint rowLength = (uint)parts.Length;
                lowD = new T[rowLength];
                foreach (var part in parts)
                {
                    try
                    {
                        // Use Convert.ChangeType to handle parsing for generic type T  
                        lowD[Array.IndexOf(parts, part)] = (T)Convert.ChangeType(part, typeof(T));
                    }
                    catch (FormatException)
                    {
                        throw new InvalidOperationException($"Unable to parse parts '{part}' to type {typeof(T)}.");
                    }
                }

            }
            else
                throw new InvalidOperationException($"The file '{fileNameCsv}' is empty or does not contain any lines.");
        }
        private void FileWriteCsv()
        {
            using (FileStream fs = new FileStream(fileNameCsv, FileMode.Create, FileAccess.Write))
            using (StreamWriter fwriter = new StreamWriter(fs))
            {
                foreach (var item in Dp)
                    fwriter.Write(item.ToString() + ',');
                fwriter.Write(Environment.NewLine);

                uint rowLength = (uint)Ra.GetLength(0);
                uint lcolumns = (uint)Ra.GetLength(1);

                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < lcolumns; j++)
                        fwriter.Write(Ra[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < lcolumns; j++)
                        fwriter.Write(Rb[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < lcolumns; j++)
                        fwriter.Write(Rc[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }
                for (int i = 0; i < M + 1; i++)
                {
                    for (int j = 0; j < M + 1; j++)
                        fwriter.Write(P[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }

                fwriter.Flush();
                fs.Position = 0;
            }
        }
        private (ImmutableArray<T>, ImmutableArray2D<T>, ImmutableArray2D<T>, ImmutableArray2D<T>, ImmutableArray2D<T>) FileReadBin(string fileName)
        {
            ImmutableArray<T> dp;
            ImmutableArray2D<T> p;
            ImmutableArray2D<T> ra;
            ImmutableArray2D<T> rb;
            ImmutableArray2D<T> rc;

            using (FileStream fs = new FileStream(fileNameBin, FileMode.Open, FileAccess.Read))
            using (BinaryReader freader = new BinaryReader(fs))
            {
                uint dpLength = convOneVar(freader, out T[] dptmp);


                dp = ImmutableArray.Create(dptmp);

                var rabctmp = new T[dpLength - 1, M + 1];
                convOneVar(freader, out T[] obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = obj[i * (M + 1) + j];
                }
                ra = ImmutableArray2D<T>.Create(rabctmp);

                convOneVar(freader, out obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = obj[i * (M + 1) + j];
                }
                rb = ImmutableArray2D<T>.Create(rabctmp);

                convOneVar(freader, out obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = obj[i * (M + 1) + j];
                }
                rc = ImmutableArray2D<T>.Create(rabctmp);

                var ptmp = new T[M + 1, M + 1];
                convOneVar(freader, out obj);
                for (uint i = 0; i < M + 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        ptmp[i, j] = obj[i * (M + 1) + j];
                }
                p = ImmutableArray2D<T>.Create(ptmp);
            }
            return (dp, ra, rb, rc, p);
        }
        private uint convOneVar(in BinaryReader br, out T[] oneVar)
        {
            int size = br.ReadInt32();
            byte[] bytes = br.ReadBytes(size);
            uint length = (uint)(size / Unsafe.SizeOf<T>());
            oneVar = new T[length];
            Buffer.BlockCopy(bytes, 0, oneVar, 0, size);
            return length;
        }
        private void FileWriteBin()
        {
            using (FileStream fs = new FileStream(fileNameBin, FileMode.Create, FileAccess.Write))
            using (BinaryWriter fwriter = new BinaryWriter(fs))
            {
                // Serialize the object to a binary file
                dynamic obj = Dp;
                fwriter.Write(Unsafe.SizeOf<T>() * Dp.Length);
                foreach (var item in obj)
                {
                    byte[] bytes = BitConverter.GetBytes(item);
                    fwriter.Write(bytes);
                }
                obj = Ra;
                fwriter.Write(Unsafe.SizeOf<T>() * Ra.GetLength(0) * Ra.GetLength(1));
                foreach (var item in obj)
                {
                    byte[] bytes = BitConverter.GetBytes(item);
                    fwriter.Write(bytes);
                }
                obj = Rb;
                fwriter.Write(Unsafe.SizeOf<T>() * Rb.GetLength(0) * Rb.GetLength(1));
                foreach (var item in obj)
                {
                    byte[] bytes = BitConverter.GetBytes(item);
                    fwriter.Write(bytes);
                }
                obj = Rc;
                fwriter.Write(Unsafe.SizeOf<T>() * Rc.GetLength(0) * Rc.GetLength(1));
                foreach (var item in obj)
                {
                    byte[] bytes = BitConverter.GetBytes(item);
                    fwriter.Write(bytes);
                }
                obj = P;
                fwriter.Write(Unsafe.SizeOf<T>() * P.GetLength(0) * P.GetLength(1));
                foreach (var item in obj)
                {
                    byte[] bytes = BitConverter.GetBytes(item);
                    fwriter.Write(bytes);
                }
                fwriter.Flush();
                fs.Position = 0;
            }
        }
        private T[] CalculateDp()
        {
            T[] dp = new T[M + 1];
            T tmpoffset = T.CreateChecked(1.0 / 2.0 / Ns);
            var dpSet = new SortedSet<T> { tmpoffset, T.One - tmpoffset };
            for (uint n = 1; n <= M; n++)
                for (uint k = 1; k <= n - 1; k++)
                    dpSet.Add(T.CreateChecked(k / (double)n));
            dp = dpSet.ToArray();
            return dp;
        }
        private T[,] CalculateP()
        {
            var p = new T[M + 1, M + 1];
            for (uint i = 1; i <= M + 1; i++)
                for (uint j = 1; j <= M + 1; j++)
                {
                    if (i == 1)
                        p[i - 1, j - 1] = T.One / (T.CreateChecked(M) + T.One); // Replace T.One with T.CreateChecked(1)
                    else
                        p[i - 1, j - 1] = T.CreateChecked(2.0 / (double)(M + 1) * Math.Cos((i - 1) * Math.PI * ((j - 0.5) / (M + 1.0))));
                }
            return p;
        }
        private T Qmn(in uint m, uint n)
        {
            if (n % m != 0)
                return T.CreateChecked(0);
            else
            {
                uint div = n / m;

                if (div % 2 == 0)
                    return T.CreateChecked(0);
                else
                    return T.CreateChecked(32 / Math.Pow(Math.PI * div, 3));
            }
        }
        private T[,] CalculateQ()
        {
            var q = new T[M, TildM];
            for (uint i = 0; i < M; i++)
                for (uint j = 0; j < TildM; j++)
                {
                    q[i, j] = Qmn(i + 1, j + 1);
                }
            return q;
        }
        private T[,] CalculateRw(T[,] Q)
        {
            var Qa = GeneralMath<T>.ExtractSubarray(Q, 0, 0, M, M);
            var Ka = new T[M, M];
            for (int i = 1; i <= M; i++)
                for (int j = 1; j <= M; j++)
                {
                    if (i == j)
                        Ka[i - 1, j - 1] = T.CreateChecked(-i * Math.PI);
                    else
                        Ka[i - 1, j - 1] = T.Zero;
                }
            var Pbar = GeneralMath<T>.ExtractSubarray(P.Data, 1, 0, M, M + 1);

            var tmpQt = GeneralMath<T>.TransposeMatrix(Q);
            var tmpQQt = GeneralMath<T>.MultiplyMatrices(Q, tmpQt);
            var tmpInvQQt = GeneralMath<T>.InvertMatrix(tmpQQt);
            var tmpInvQQtQa = GeneralMath<T>.MultiplyMatrices(tmpInvQQt, Qa);
            var tmpInvQQtQaKa = GeneralMath<T>.MultiplyMatrices(tmpInvQQtQa, Ka);
            var tmpInvQQtQaKaPbar = GeneralMath<T>.MultiplyMatrices(tmpInvQQtQaKa, Pbar);
            return tmpInvQQtQaKaPbar;
        }
        private uint kK(in uint n, T x) => (uint)Math.Floor(n * double.CreateChecked(x));
        private T anx(uint n, T x)
        {
            uint k = kK(n, x);
            if (k % 2 == 0)
                return T.CreateChecked(-4 * n * n);
            else
                return T.CreateChecked(4 * n * n);
        }
        private T bnx(uint n, T x)
        {
            uint k = kK(n, x);
            if (k % 2 == 0)
                return T.CreateChecked(4 * n * (2 * k + 1));
            else
                return T.CreateChecked(-4 * n * (2 * k + 1));
        }
        private T cnx(uint n, T x)
        {
            uint k = kK(n, x);
            if (k % 2 == 0)
                return T.CreateChecked(-4 * k * (k + 1));
            else
                return T.CreateChecked(4 * k * (k + 1));
        }
        private T[,] CalculateABC(in Func<uint, T, T> abc)
        {
            var ABC = new T[Dp.Length - 1, M];
            for (uint n = 1; n <= M; n++)
                for (int k = 1; k <= Dp.Length - 1; k++)
                {
                    ABC[k - 1, n - 1] = abc(n, Dp[k - 1]);
                }
            return ABC;
        }
    }
}
