using System;
using System.Collections;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;

namespace OptNet
{
    public static class ArrayExtensions
    {
        // Accept uint rows/cols because OfflineGet uses uint for M. Also provide an int overload.
        public static T[,] To2DArray<T>(this T[] source, uint rows, uint cols)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            int r = checked((int)rows);
            int c = checked((int)cols);
            if (source.Length != r * c) throw new ArgumentException("Source length does not match rows*cols.", nameof(source));
            var result = new T[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    result[i, j] = source[i * c + j];
            return result;
        }

        public static T[,] To2DArray<T>(this T[] source, int rows, int cols) =>
            source.To2DArray((uint)rows, (uint)cols);
    }
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
    public class OfflineGet<T> where T : struct, INumber<T>
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

        private ImmutableArray<decimal> DpDecimal;
        private ImmutableArray2D<decimal> PDecimal;
        private ImmutableArray2D<decimal> RaDecimal;
        private ImmutableArray2D<decimal> RbDecimal;
        private ImmutableArray2D<decimal> RcDecimal;


        private string fileNameCsv;
        private string fileNameBin;

        public OfflineGet(in uint ns)
        {
            M = ns - 1;
            TildM = M * 100;
            Ns = M + 1;

            string baseFolder = AppDomain.CurrentDomain.BaseDirectory + typeof(T).Name;
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            string fileNameBase = Path.Combine(baseFolder, "OptNetConst" + M.ToString() + "_" + TildM.ToString());
            fileNameCsv = fileNameBase + ".csv";
            fileNameBin = fileNameBase + ".bin";


            if (File.Exists(fileNameBin))
            {
                //(Dp, Ra, Rb, Rc, P) = FileReadBin(fileNameBin);
                FileReadDecimalBin();
            }
            else if (File.Exists(fileNameCsv))
            {
                //(Dp, Ra, Rb, Rc, P) = FileReadCsv(fileNameCsv);
                FileWriteDecimalCsv();
            }
            else
            {

                DpDecimal = ImmutableArray.Create(CalculateDp());
                PDecimal = ImmutableArray2D<decimal>.Create(CalculateP());
                var Q = ImmutableArray2D<decimal>.Create(CalculateQ());
                var rw = ImmutableArray2D<decimal>.Create(CalculateRw(Q.Data));
                RaDecimal = ImmutableArray2D<decimal>.Create(GeneralMath<decimal>.MultiplyMatrices(CalculateABC(Anx), rw.Data));
                RbDecimal = ImmutableArray2D<decimal>.Create(GeneralMath<decimal>.MultiplyMatrices(CalculateABC(Bnx), rw.Data));
                RcDecimal = ImmutableArray2D<decimal>.Create(GeneralMath<decimal>.MultiplyMatrices(CalculateABC(Cnx), rw.Data));

                /*Dp = ImmutableArray.Create<T>(CalculateDp());
                P = ImmutableArray2D<T>.Create(CalculateP());
                Ra = ImmutableArray2D<T>.Create(
                    GeneralMath<T>.MultiplyMatrices(CalculateABC(Anx), rw.Data));
                Rb = ImmutableArray2D<T>.Create(
                    GeneralMath<T>.MultiplyMatrices(CalculateABC(Bnx), rw.Data));
                Rc = ImmutableArray2D<T>.Create(
                    GeneralMath<T>.MultiplyMatrices(CalculateABC(Cnx), rw.Data));*/

                //FileWriteCsv();
                //FileWriteBin();
                FileWriteDecimalBin();
                FileWriteDecimalCsv();
            }
            Dp = ImmutableArray.Create<T>(DpDecimal.Select(d => (T)Convert.ChangeType(d, typeof(T))).ToArray());
            P = ImmutableArray2D<T>.Create(PDecimal.Data.Cast<decimal>().Select(d => (T)Convert.ChangeType(d, typeof(T))).ToArray().To2DArray(M + 1, M + 1));

            uint tmpRowLenth = (uint)RaDecimal.GetLength(0);
            Ra = ImmutableArray2D<T>.Create(RaDecimal.Data.Cast<decimal>().Select(d => (T)Convert.ChangeType(d, typeof(T))).ToArray().To2DArray(tmpRowLenth, M + 1));
            Rb = ImmutableArray2D<T>.Create(RbDecimal.Data.Cast<decimal>().Select(d => (T)Convert.ChangeType(d, typeof(T))).ToArray().To2DArray(tmpRowLenth, M + 1));
            Rc = ImmutableArray2D<T>.Create(RcDecimal.Data.Cast<decimal>().Select(d => (T)Convert.ChangeType(d, typeof(T))).ToArray().To2DArray(tmpRowLenth, M + 1));
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
                ConvOneLine(sr, out T[] dptmp);
                uint rowLength = (uint)(dptmp.Length - 1);
                dp = ImmutableArray.Create(dptmp);

                var rabctmp = new T[rowLength, M + 1];
                for (uint i = 0; i < rowLength; i++)
                {
                    ConvOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = row[j];
                }
                ra = ImmutableArray2D<T>.Create(rabctmp);

                for (uint i = 0; i < rowLength; i++)
                {
                    ConvOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = row[j];
                }
                rb = ImmutableArray2D<T>.Create(rabctmp);

                for (uint i = 0; i < rowLength; i++)
                {
                    ConvOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = row[j];
                }
                rc = ImmutableArray2D<T>.Create(rabctmp);

                var ptmp = new T[M + 1, M + 1];
                for (uint i = 0; i < M + 1; i++)
                {
                    ConvOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        ptmp[i, j] = row[j];
                }
                p = ImmutableArray2D<T>.Create(ptmp);
            }
            return (dp, ra, rb, rc, p);
        }
        private void ConvOneLine(in StreamReader sr, out T[] lowD)
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

        // Read decimal-specific CSV written by FileWriteDecimalCsv
        private void FileReadDecimalCsv()
        {
            using (FileStream fs = new FileStream(fileNameCsv, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                string? line = sr.ReadLine();
                if (line == null) throw new InvalidOperationException($"The file '{fileNameCsv}' is empty or does not contain any lines.");
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var dpArr = parts.Select(p => decimal.Parse(p)).ToArray();
                DpDecimal = ImmutableArray.Create(dpArr);

                int rowLength = dpArr.Length - 1;
                int cols = checked((int)(M + 1));

                decimal[,] rabctmp = new decimal[rowLength, cols];
                for (int i = 0; i < rowLength; i++)
                {
                    line = sr.ReadLine();
                    if (line == null) throw new InvalidOperationException($"Unexpected end of file while reading Ra at line {i}.");
                    parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < cols; j++)
                        rabctmp[i, j] = decimal.Parse(parts[j]);
                }
                RaDecimal = ImmutableArray2D<decimal>.Create(rabctmp);

                // Rb
                rabctmp = new decimal[rowLength, cols];
                for (int i = 0; i < rowLength; i++)
                {
                    line = sr.ReadLine();
                    if (line == null) throw new InvalidOperationException($"Unexpected end of file while reading Rb at line {i}.");
                    parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < cols; j++)
                        rabctmp[i, j] = decimal.Parse(parts[j]);
                }
                RbDecimal = ImmutableArray2D<decimal>.Create(rabctmp);

                // Rc
                rabctmp = new decimal[rowLength, cols];
                for (int i = 0; i < rowLength; i++)
                {
                    line = sr.ReadLine();
                    if (line == null) throw new InvalidOperationException($"Unexpected end of file while reading Rc at line {i}.");
                    parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < cols; j++)
                        rabctmp[i, j] = decimal.Parse(parts[j]);
                }
                RcDecimal = ImmutableArray2D<decimal>.Create(rabctmp);

                // P
                var ptmp = new decimal[cols, cols];
                for (int i = 0; i < cols; i++)
                {
                    line = sr.ReadLine();
                    if (line == null) throw new InvalidOperationException($"Unexpected end of file while reading P at line {i}.");
                    parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < cols; j++)
                        ptmp[i, j] = decimal.Parse(parts[j]);
                }
                PDecimal = ImmutableArray2D<decimal>.Create(ptmp);
            }
        }

        // Read decimal-specific binary written by FileWriteDecimalBin
        private uint ConvOneVarDecimal(in BinaryReader br, out decimal[] oneVar)
        {
            int size = br.ReadInt32();
            byte[] bytes = br.ReadBytes(size);
            uint length = (uint)(size / (sizeof(int) * 4));
            oneVar = new decimal[length];
            for (int i = 0; i < length; i++)
            {
                int startIndex = i * 16;
                int[] bits = new int[4]
                {
                    BitConverter.ToInt32(bytes, startIndex),
                    BitConverter.ToInt32(bytes, startIndex + 4),
                    BitConverter.ToInt32(bytes, startIndex + 8),
                    BitConverter.ToInt32(bytes, startIndex + 12),
                };
                oneVar[i] = new decimal(bits);
            }
            return length;
        }

        private void FileReadDecimalBin()
        {
            using (FileStream fs = new FileStream(fileNameBin, FileMode.Open, FileAccess.Read))
            using (BinaryReader freader = new BinaryReader(fs))
            {
                uint dpLength = ConvOneVarDecimal(freader, out decimal[] dptmp);
                DpDecimal = ImmutableArray.Create(dptmp);

                int rowLength = checked((int)dpLength - 1);
                int cols = checked((int)(M + 1));

                // Ra
                ConvOneVarDecimal(freader, out decimal[] obj);
                var rabctmp = new decimal[rowLength, cols];
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < cols; j++)
                        rabctmp[i, j] = obj[i * cols + j];
                }
                RaDecimal = ImmutableArray2D<decimal>.Create(rabctmp);

                // Rb
                ConvOneVarDecimal(freader, out obj);
                rabctmp = new decimal[rowLength, cols];
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < cols; j++)
                        rabctmp[i, j] = obj[i * cols + j];
                }
                RbDecimal = ImmutableArray2D<decimal>.Create(rabctmp);

                // Rc
                ConvOneVarDecimal(freader, out obj);
                rabctmp = new decimal[rowLength, cols];
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < cols; j++)
                        rabctmp[i, j] = obj[i * cols + j];
                }
                RcDecimal = ImmutableArray2D<decimal>.Create(rabctmp);

                // P
                ConvOneVarDecimal(freader, out obj);
                var ptmp = new decimal[cols, cols];
                for (int i = 0; i < cols; i++)
                {
                    for (int j = 0; j < cols; j++)
                        ptmp[i, j] = obj[i * cols + j];
                }
                PDecimal = ImmutableArray2D<decimal>.Create(ptmp);
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
                uint dpLength = ConvOneVar(freader, out T[] dptmp);


                dp = ImmutableArray.Create(dptmp);

                var rabctmp = new T[dpLength - 1, M + 1];
                ConvOneVar(freader, out T[] obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = obj[i * (M + 1) + j];
                }
                ra = ImmutableArray2D<T>.Create(rabctmp);

                ConvOneVar(freader, out obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = obj[i * (M + 1) + j];
                }
                rb = ImmutableArray2D<T>.Create(rabctmp);

                ConvOneVar(freader, out obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rabctmp[i, j] = obj[i * (M + 1) + j];
                }
                rc = ImmutableArray2D<T>.Create(rabctmp);

                var ptmp = new T[M + 1, M + 1];
                ConvOneVar(freader, out obj);
                for (uint i = 0; i < M + 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        ptmp[i, j] = obj[i * (M + 1) + j];
                }
                p = ImmutableArray2D<T>.Create(ptmp);
            }
            return (dp, ra, rb, rc, p);
        }
        private uint ConvOneVar(in BinaryReader br, out T[] oneVar)
        {
            int size = br.ReadInt32();
            byte[] bytes = br.ReadBytes(size);
            uint length;
            if (typeof(T) == typeof(decimal))
            {
                length = (uint)(size / (sizeof(int) * 4));
                oneVar = new T[length];
                for (int i = 0; i < length; i++)
                {
                    int startIndex = i * 16;
                    int[] bits =
                    [
                        BitConverter.ToInt32(bytes, startIndex),
                        BitConverter.ToInt32(bytes, startIndex + 4),
                        BitConverter.ToInt32(bytes, startIndex + 8),
                        BitConverter.ToInt32(bytes, startIndex + 12),
                    ];
                    oneVar[i] = (T)(object)new decimal(bits);
                }
            }
            else
            {
                length = (uint)(size / Unsafe.SizeOf<T>());
                oneVar = new T[length];
                for (int i = 0; i < length; i++)
                {
                    oneVar[i] = (T)(object)BitConverter.ToDouble(bytes, i * sizeof(double));
                }
            }
            return length;
        }
        private void FileWriteBin()
        {
            using (FileStream fs = new FileStream(fileNameBin, FileMode.Create, FileAccess.Write))
            using (BinaryWriter fwriter = new BinaryWriter(fs))
            {
                Action<IEnumerable<T>> writeArray = (IEnumerable<T> arr) =>
                {
                    if (typeof(T) == typeof(decimal))
                    {
                        var list = arr.Cast<decimal>().ToList();
                        fwriter.Write(list.Count * sizeof(int) * 4);
                        foreach (var item in list)
                        {
                            int[] bits = decimal.GetBits(item);
                            foreach (var b in bits)
                                fwriter.Write(b);
                        }
                    }
                    else
                    {
                        var list = arr.ToList();
                        fwriter.Write(list.Count * Unsafe.SizeOf<T>());
                        foreach (var item in list)
                        {
                            byte[] bytes = BitConverter.GetBytes((double)(object)item);
                            fwriter.Write(bytes);
                        }
                    }
                };

                writeArray(Dp);
                writeArray(Ra);
                writeArray(Rb);
                writeArray(Rc);
                writeArray(P);

                fwriter.Flush();
                fs.Position = 0;
            }
        }
        private decimal[] CalculateDp()
        {
            decimal[] dp = new decimal[M + 1];
            decimal tmpoffset = decimal.CreateChecked(1.0 / 2.0 / Ns);
            var dpSet = new SortedSet<decimal> { tmpoffset, 1 - tmpoffset };
            for (uint n = 1; n <= M; n++)
                for (uint k = 1; k <= n - 1; k++)
                    dpSet.Add(k / (decimal)n);
            dp = dpSet.ToArray();
            return dp;
        }
        private decimal[,] CalculateP()
        {
            var p = new decimal[M + 1, M + 1];
            for (uint i = 1; i <= M + 1; i++)
                for (uint j = 1; j <= M + 1; j++)
                {
                    if (i == 1)
                        p[i - 1, j - 1] = 1m / (M + 1m); // Replace T.One with T.CreateChecked(1)
                    else
                        p[i - 1, j - 1] = (decimal)(2.0 / (M + 1.0) * Math.Cos((i - 1.0) * Math.PI * ((j - 0.5) / (M + 1.0))));
                }
            return p;
        }
        private decimal Qmn(in uint m, uint n)
        {
            if (n % m != 0)
                return 0;
            else
            {
                uint div = n / m;

                if (div % 2 == 0)
                    return 0;
                else
                    return (decimal)(32 / Math.Pow(Math.PI * div, 3));
            }
        }
        private decimal[,] CalculateQ()
        {
            var q = new decimal[M, TildM];
            for (uint i = 0; i < M; i++)
                for (uint j = 0; j < TildM; j++)
                {
                    q[i, j] = Qmn(i + 1, j + 1);
                }
            return q;
        }
        private decimal[,] CalculateRw(decimal[,] Q)
        {
            var Qa = GeneralMath<decimal>.ExtractSubarray(Q, 0, 0, M, M);
            var Ka = new decimal[M, M];
            for (int i = 1; i <= M; i++)
                for (int j = 1; j <= M; j++)
                {
                    if (i == j)
                        Ka[i - 1, j - 1] = decimal.CreateChecked(-i * Math.PI);
                    else
                        Ka[i - 1, j - 1] = decimal.Zero;
                }
            var Pbar = GeneralMath<decimal>.ExtractSubarray(PDecimal.Data, 1, 0, M, M + 1);

            var tmpQt = GeneralMath<decimal>.TransposeMatrix(Q);
            var tmpQQt = GeneralMath<decimal>.MultiplyMatrices(Q, tmpQt);
            var tmpInvQQt = GeneralMath<decimal>.InvertMatrix(tmpQQt);
            var tmpInvQQtQa = GeneralMath<decimal>.MultiplyMatrices(tmpInvQQt, Qa);
            var tmpInvQQtQaKa = GeneralMath<decimal>.MultiplyMatrices(tmpInvQQtQa, Ka);
            var tmpInvQQtQaKaPbar = GeneralMath<decimal>.MultiplyMatrices(tmpInvQQtQaKa, Pbar);
            return tmpInvQQtQaKaPbar;
        }
        private uint KK(in uint n, decimal x) => (uint)Math.Floor(n * (double)x);
        private decimal Anx(uint n, decimal x)
        {
            uint k = KK(n, x);
            if (k % 2 == 0)
                return decimal.CreateChecked(-4 * n * n);
            else
                return decimal.CreateChecked(4 * n * n);
        }
        private decimal Bnx(uint n, decimal x)
        {
            uint k = KK(n, x);
            if (k % 2 == 0)
                return decimal.CreateChecked(4 * n * (2 * k + 1));
            else
                return decimal.CreateChecked(-4 * n * (2 * k + 1));
        }
        private decimal Cnx(uint n, decimal x)
        {
            uint k = KK(n, x);
            if (k % 2 == 0)
                return decimal.CreateChecked(-4 * k * (k + 1));
            else
                return decimal.CreateChecked(4 * k * (k + 1));
        }
        private decimal[,] CalculateABC(in Func<uint, decimal, decimal> abc)
        {
            var ABC = new decimal[DpDecimal.Length - 1, M];
            for (uint n = 1; n <= M; n++)
                for (int k = 1; k <= DpDecimal.Length - 1; k++)
                {
                    ABC[k - 1, n - 1] = abc(n, DpDecimal[k - 1]);
                }
            return ABC;
        }
        private void FileWriteDecimalCsv()
        {
            using (var fs = new FileStream(fileNameCsv, FileMode.Create, FileAccess.Write))
            using (var fwriter = new StreamWriter(fs))
            {
                // write DpDecimal on one line
                fwriter.Write(string.Join(',', DpDecimal.ToArray().Select(d => d.ToString())));
                fwriter.Write(Environment.NewLine);

                // helper to write 2D decimal arrays row by row
                void Write2D(ImmutableArray2D<decimal> a)
                {
                    int rows = a.GetLength(0);
                    int cols = a.GetLength(1);
                    for (int i = 0; i < rows; i++)
                    {
                        var row = new string[cols];
                        for (int j = 0; j < cols; j++)
                            row[j] = a[i, j].ToString();
                        fwriter.Write(string.Join(',', row));
                        fwriter.Write(Environment.NewLine);
                    }
                }

                Write2D(RaDecimal);
                Write2D(RbDecimal);
                Write2D(RcDecimal);
                Write2D(PDecimal);

                fwriter.Flush();
                fs.Position = 0;
            }
        }

        private void FileWriteDecimalBin()
        {
            using (var fs = new FileStream(fileNameBin, FileMode.Create, FileAccess.Write))
            using (var fwriter = new BinaryWriter(fs))
            {
                Action<IEnumerable<decimal>> writeDecimalArray = (IEnumerable<decimal> arr) =>
                {
                    var list = arr.ToList();
                    // write byte length first (count * 16)
                    fwriter.Write(list.Count * sizeof(int) * 4);
                    foreach (var item in list)
                    {
                        int[] bits = decimal.GetBits(item);
                        foreach (var b in bits)
                            fwriter.Write(b);
                    }
                };

                // flatten 2D into row-major sequence
                IEnumerable<decimal> Flatten(ImmutableArray2D<decimal> a)
                {
                    int rows = a.GetLength(0);
                    int cols = a.GetLength(1);
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            yield return a[i, j];
                }

                writeDecimalArray(DpDecimal);
                writeDecimalArray(Flatten(RaDecimal));
                writeDecimalArray(Flatten(RbDecimal));
                writeDecimalArray(Flatten(RcDecimal));
                writeDecimalArray(Flatten(PDecimal));

                fwriter.Flush();
                fs.Position = 0;
            }
        }
    }
}
