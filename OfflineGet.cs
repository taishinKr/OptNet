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

namespace OptNet
{
    internal class OfflineGet<T> where T : struct, INumber<T>
    {
        private readonly uint M;
        private readonly uint tildM;
        private readonly uint Ns;
        private readonly T[,] Q;
        private readonly T[,] Rw;
        private readonly T[,] A;
        private readonly T[,] B;
        private readonly T[,] C;
        private string fileNameCsv;
        private string fileNameBin;

        public T[,] P { get; }
        public T[,] Ra { get; }
        public T[,] Rb { get; }
        public T[,] Rc { get; }
        public T[] Dp { get; }
        public OfflineGet(uint M, uint tildM)
        {
            this.M = M;
            this.tildM = tildM;
            this.Ns = M + 1;

            string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            string fileNameBase = Path.Combine(baseFolder, "OptNetConst" + M.ToString() + "_" + tildM.ToString());
            fileNameCsv = fileNameBase + ".csv";
            fileNameBin = fileNameBase + ".bin";

            if (File.Exists(fileNameBin))
            {
                FileReadBin(out T[] dp, out T[,] ra, out T[,] rb, out T[,] rc);
                Dp = dp;
                Ra = ra;
                Rb = rb;
                Rc = rc;
                P = new T[M + 1, M + 1];
                Q = new T[M, tildM];
                Rw = new T[M, M];
                A = new T[M, M];
                B = new T[M, M];
                C = new T[M, M];
            }
            else if (File.Exists(fileNameCsv))
            {
                FileReadCsv(out T[] dp, out T[,] ra, out T[,] rb, out T[,] rc);
                Dp = dp;
                Ra = ra;
                Rb = rb;
                Rc = rc;
                P = new T[M + 1, M + 1];
                Q = new T[M, tildM];
                Rw = new T[M, M];
                A = new T[M, M];
                B = new T[M, M];
                C = new T[M, M];               
            }            
            else
            {
                Dp = calcDp();
                P = calcP();
                Q = calcQ();
                Rw = calcRw();
                A = calcABC(anx);
                B = calcABC(bnx);
                C = calcABC(cnx);
                Ra = GeneralMath<T>.MultiplyMatrices(A, Rw);
                Rb = GeneralMath<T>.MultiplyMatrices(B, Rw);
                Rc = GeneralMath<T>.MultiplyMatrices(C, Rw);
                // Save the data to a binary file
                FileWriteCsv();
                FileWriteBin();
            }
        }
        private void FileReadCsv(out T[] dp, out T[,] ra, out T[,] rb, out T[,] rc)
        {
            using (FileStream fs = new FileStream(fileNameCsv, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                convOneLine(sr, out dp);
                uint rowLength = (uint)(dp.Length - 1);

                ra = new T[rowLength, M + 1];
                for (uint i = 0; i < rowLength; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        ra[i, j] = row[j];
                }

                rb = new T[rowLength, M + 1];
                for (uint i = 0; i < rowLength; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rb[i, j] = row[j];
                }

                rc = new T[rowLength, M + 1];
                for (uint i = 0; i < rowLength; i++)
                {
                    convOneLine(sr, out T[] row);
                    for (uint j = 0; j < M + 1; j++)
                        rc[i, j] = row[j];
                }
            }
                
        }
        private void convOneLine(StreamReader sr, out T[] lowD)
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

                for (uint i = 0; i < rowLength; i++)
                {
                    for (uint j = 0; j < lcolumns; j++)
                        fwriter.Write(Ra[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }
                for (uint i = 0; i < rowLength; i++)
                {
                    for (uint j = 0; j < lcolumns; j++)
                        fwriter.Write(Rb[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }
                for (uint i = 0; i < rowLength; i++)
                {
                    for (uint j = 0; j < lcolumns; j++)
                        fwriter.Write(Rc[i, j].ToString() + ',');
                    fwriter.Write(Environment.NewLine);
                }

                fwriter.Flush();
                fs.Position = 0;
            }
        }

        /*private void FileReadBin(string fileNameBin, out T[] dp, out T[,] ra, out T[,] rb, out T[,] rc)
        {
            using (FileStream fs = new FileStream(fileNameBin, FileMode.Open, FileAccess.Read))
            using (BinaryReader freader = new BinaryReader(fs))
            {
                // Deserialize the object from a binary file
                int size = freader.ReadInt32();
                byte[] bytes = freader.ReadBytes(size);
                dp = new T[size / Unsafe.SizeOf<T>()];
                Buffer.BlockCopy(bytes, 0, dp, 0, size);
            }
        }*/
        private void FileReadBin(out T[] dp, out T[,] ra, out T[,] rb, out T[,] rc)
        {
            using (FileStream fs = new FileStream(fileNameBin, FileMode.Open, FileAccess.Read))
            using (BinaryReader freader = new BinaryReader(fs))
            {
                uint dpLength = convOneVar(freader, out dp);

                ra = new T[dpLength - 1, M + 1];
                convOneVar(freader, out T[] obj);                
                for ( uint i = 0; i < dpLength-1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        ra[i, j] = obj[i * (M + 1) + j];
                }

                rb = new T[dpLength - 1, M + 1];
                convOneVar(freader, out obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rb[i, j] = obj[i * (M + 1) + j];
                }

                rc = new T[dpLength - 1, M + 1];
                convOneVar(freader, out obj);
                for (uint i = 0; i < dpLength - 1; i++)
                {
                    for (uint j = 0; j < M + 1; j++)
                        rc[i, j] = obj[i * (M + 1) + j];
                }
            }
        }
        private uint convOneVar(BinaryReader br, out T[] oneVar)
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
                fwriter.Write(Unsafe.SizeOf<T>() * Ra.GetLength(0) * Ra.GetLength(1) );
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
                fwriter.Flush();
                fs.Position = 0;
            }
        }

        private T[] calcDp()
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
        private T[,] calcP()
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
        private T Qmn(uint m, uint n)
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
        private T[,] calcQ()
        {
            var q = new T[M, tildM];
            for (uint i = 0; i < M; i++)
                for (uint j = 0; j < tildM; j++)
                {
                    q[i, j] = Qmn(i + 1, j + 1);
                }
            return q;
        }
        private T[,] calcRw()
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
            var Pbar = GeneralMath<T>.ExtractSubarray(P, 1, 0, M, M + 1);

            var tmpQt = GeneralMath<T>.TransposeMatrix(Q);
            var tmpQQt = GeneralMath<T>.MultiplyMatrices(Q, tmpQt);
            var tmpInvQQt = GeneralMath<T>.InvertMatrix(tmpQQt);
            var tmpInvQQtQa = GeneralMath<T>.MultiplyMatrices(tmpInvQQt, Qa);
            var tmpInvQQtQaKa = GeneralMath<T>.MultiplyMatrices(tmpInvQQtQa, Ka);
            var tmpInvQQtQaKaPbar = GeneralMath<T>.MultiplyMatrices(tmpInvQQtQaKa, Pbar);
            return tmpInvQQtQaKaPbar;
        }
        private uint kK(uint n, T x) => (uint)Math.Floor(n * double.CreateChecked(x));
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
        private T[,] calcABC(Func<uint, T, T> abc)
        {
            var ABC = new T[Dp.Length - 1, M];
            for (uint n = 1; n <= M; n++)
                for (uint k = 1; k <= Dp.Length - 1; k++)
                {
                    ABC[k - 1, n - 1] = abc(n, Dp[k - 1]);
                }
            return ABC;
        }
        //var tmpAaRw=GeneralMath<T>.MultiplyMatrices(Aa,Rw);

    }
}
