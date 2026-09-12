using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;

namespace OptNet
{
    public class OptParam<T> where T : struct, INumber<T>, ITrigonometricFunctions<T>, IExponentialFunctions<T>, IRootFunctions<T>, IPowerFunctions<T>
    {
        public string Name { get; set; }
        public Func<T[], uint, T> J { get; set; }
        public double TolJ { get; set; } //
        public double TolX { get; set; } // tolerance(ratio, %) for the input vector, X, to be considered as converged.
        public uint DimN { get; set; }
        public uint[] Ns { get; set; }
        public T[] MinX { get; set; }
        public T[] MaxX { get; set; }
        public OptMD<T>.StaMaskEnum Mode { get; set; }
        public bool IsScalable { get; set; } = true; // flag to indicate whether the optimization problem is scalable or not, default is true.
        public (T[] X, T J)[] Optima { get; set; } = []; //Array.Empty<(T[], T)>();

        public OptParam()
        {
            Name = "Default";
            var testFcn = new TestFcn<T>();
            J = testFcn.JTest2D;
            TolJ = 0.001;
            TolX = 0.001;
            DimN = 2;
            Ns = [11, 11];
            MinX = [T.Zero, T.Zero];
            MaxX = [T.One, T.One];
            Mode = OptMD<T>.StaMaskEnum.Done; // Default value, should be set properly later.            
        }

        public OptParam( OptParam<T> optparm, uint dimN)
        {
            if (optparm.IsScalable && dimN != 0) // UI의 combobox에서 dimN을 변경한 경우
            {
                this.Optima = optparm.Optima.Select(o =>
                {
                    var newX = new T[dimN];

                    if (dimN < o.X.Length)
                    {
                        for (int i = 0; i < dimN; i++)
                            newX[i] = o.X[i];
                    }
                    else
                    {
                        // 기존 값 복사
                        for (int i = 0; i < o.X.Length; i++)
                            newX[i] = o.X[i];
                        // 나머지 값은 X[0]로 채우기
                        for (int i = o.X.Length; i < dimN; i++)
                            newX[i] = o.X[0];
                    }
                    if (optparm.Name == "Styblinski­Tang")
                        return (newX, T.CreateChecked(-39.16616570 * dimN));
                    else
                        return (newX, o.J);
                }).ToArray();
            }
            else // UI의 combobox에서 dimN을 변경하지 않은 경우나 Ns가 변경된 경우
            {
                Optima = optparm.Optima
                    .Select(o => (o.X, o.J))
                    .ToArray();
            }

            this.Name = optparm.Name;
            this.J = optparm.J;            
            this.TolJ = optparm.TolJ;
            this.TolX = optparm.TolX < 0.0 ? 0.001 : optparm.TolX; // default value if tolX is negative
            var dim = dimN==0 ? optparm.DimN : dimN;
            this.DimN = dim;
            this.Ns = new uint[dim];
            this.MinX = new T[dim];
            this.MaxX = new T[dim];
            this.IsScalable = optparm.IsScalable;
            for(int i = 0; i < dim; i++)
            {
                this.Ns[i] = optparm.Ns[0];                
                this.MinX[i] = optparm.MinX[0];
                this.MaxX[i] = optparm.MaxX[0];
            }
            this.Mode = optparm.Mode;
        }
        public OptParam(
            in string Name,
            in Func<T[], uint, T> J,
            in double TolJ,
            in double TolX,
            in uint DimN,
            in uint[] Ns,
            in T[] MinX,
            in T[] MaxX,
            in OptMD<T>.StaMaskEnum Mode)
        {
            this.Name = Name;
            this.J = J;
            this.TolJ = TolJ;
            this.TolX = TolX < 0.0 ? 0.001 : TolX; // default value if tolX is negative
            this.DimN = DimN;
            this.Ns = Ns;
            this.MinX = MinX;
            this.MaxX = MaxX;
            this.Mode = Mode;
        }
    }
    // 상태를 저장할 클래스 (mutable 컬렉션 사용)
    public class StateSnapshot<T>
    {
        public uint[]? Ns { get; set; }
        public T[]? MinX { get; set; }
        public T[]? MaxX { get; set; }
        public T[]? DX { get; set; }
        public T[]? DXHalf { get; set; }
        public uint Nfe { get; set; }
        public List<T>? SampleJtot { get; set; }
        //public T? TolX { get; set; }
        public (T[][] X, T J)[] OptResultReal { get; set; } 
    }


    public class OptMD<T> where T : struct, INumber<T>, ITrigonometricFunctions<T>, IExponentialFunctions<T>, IRootFunctions<T>, IPowerFunctions<T>
    {
        private readonly uint dimN;
        private ImmutableArray<uint> Ns;
        private ImmutableArray<uint> orginNs;
        private ImmutableArray<T> minX;
        private ImmutableArray<T> maxX;
        private ImmutableArray<T> dX;
        private ImmutableArray<T> dXHalf;
        private ImmutableList<T> sampleJtot;
        // 'mode' is different from 'mState', which is used for internal state management.
        // 'mode' is dedicated to specifying the optimization goal (Min or Max) and should not be combined.
        private StaMaskEnum mode;//private readonly StaMaskEnum mode;
        private ImmutableDictionary<uint, OfflineGet<T>> lutConstant;

        private static readonly Lock _locker = new();//new Lock();
        private static readonly PCQueue pcQ = new(Environment.ProcessorCount); // new PCQueue(Environment.ProcessorCount);

        private uint[] inputXIdx;
        private T[] inputX;
        public (T[][] X, T J)[] optResult = [];
        public (T[][] X, T J)[] optResultReal = [];

        public T TolJ { get; set; } = T.CreateChecked(0.1); // tolerance(ratio) for the objective function, J, to be considered as converged.
        private T spanJ = T.Zero; // span of the objective function, J, from the initial sampling. 
        public T TolJReal => spanJ * TolJ; // tolerance(ratio) for the objective function, J, to be considered as converged.
                                           // used in the adjustResult() and in Opt1D() to filter
                                           // and to determine whether the candidate solution is within the tolerance of the optimal solution.
        public T TolX { get; set; } = T.CreateChecked(0.1); // tolerance(ratio) for the input vector, X, to be considered as converged.
        private T spanX = T.Zero;
        public T TolXReal => spanX * TolX; // tolerance(ratio) for the input vector, X
                                           // used in the GeneralMath<T>.ClusterByDistance
                                           // to cluster the results based on the distance between the input vectors, X,
        public T SpanJ => spanJ;
        public T SpanX => spanX;
        public T[] MinX => [.. minX]; // minX.ToArray();
        public T[] MaxX => [.. maxX]; // maxX.ToArray();
        public T[] DXHalf => [.. dXHalf]; // dXHalf.ToArray();
        public uint DimN => dimN;
        //public uint[] Nsample => [.. Ns]; // Assuming all dimensions have the same number of samples
        public uint[] Nsample
        {
            get => Ns.ToArray(); // ImmutableArray → 배열 변환
            set => Ns = ImmutableArray.Create(value); // 배열 → ImmutableArray 변환
        }
        public uint Nfe { get; set; } = 0;
        public StaMaskEnum Mode { get => mode; set => mode = value; }

        public double[][] IntvX { get; set; } // minX, maxX
        public Func<T[], T> J;
        public ConcurrentQueue<string> DisplayMsgQueue;
        [Flags]
        public enum StaMaskEnum : UInt32 {
            Done = 1, Min = 1 << 1, Max = 1 << 2, Bottom = 1 << 3,
            MThread = 1 << 4, SampleReuse = 1 << 5, StrictLimit = 1 << 6,
            AutoShrink = 1 << 7, SampledOptimal = 1 << 8,
            ReSampling = 1 << 9, AxisChanging = 1 << 10,
            ReSamplingOnly = 1 << 11, AxisChangingOnly = 1 << 12, ReSampleAxisChangeBoth = 1 << 13,
            PartialConstJ = 1 << 14,
        }
        private UInt32 mState = 0;

        public OptParam<T> OptParam { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptMD{T}"/> class with the specified optimization parameters.
        /// </summary>
        /// <remarks>This constructor initializes the optimization model based on the provided parameters.
        /// It validates the input dimensions and precomputes step sizes and other necessary data for
        /// optimization.</remarks>
        /// <param name="optParam">The optimization parameters used to configure the instance. This includes the objective function,
        /// tolerances, dimensionality, bounds, and mode.</param>
        /// <exception cref="ArgumentException">Thrown if the lengths of <paramref name="optParam.Ns"/>, <paramref name="optParam.MinX"/>, and <paramref
        /// name="optParam.MaxX"/> do not match the specified dimensionality (<paramref name="optParam.DimN"/>), or if
        /// the mode is invalid.</exception>
        public OptMD(OptParam<T> optParam)
        {
            this.OptParam = optParam;
            //J = x => optParam.J(x,optParam.DimN);
            J = x => {
                Nfe++;
                return optParam.J(x, optParam.DimN);
            }            ;
            var tmpTolX = optParam.TolX < 0.0 ? 0.00001 : optParam.TolX; // default value if tolX is negative
            this.TolX = T.CreateChecked(tmpTolX);
            var tmpTolJ = optParam.TolJ < 0.0 ? 0.00001 : optParam.TolJ; // default value if tolJ is negative
            this.TolJ = T.CreateChecked(tmpTolJ);
            this.dimN = optParam.DimN;
            this.Ns = ImmutableArray.Create(optParam.Ns);
            this.orginNs = ImmutableArray.Create(optParam.Ns);
            this.minX = ImmutableArray.Create(optParam.MinX);
            this.maxX = ImmutableArray.Create(optParam.MaxX);
            this.mode = optParam.Mode;
            if (Ns.Length != dimN || minX.Length != dimN || maxX.Length != dimN)
                throw new ArgumentException("The lengths of Ns, minX, and maxX must match the dimension dimN.");
            // Validate the mode to ensure it is either Min or Max, but not both
            // 'mode' is different from 'mState', which is used for internal state management.
            // 'mode' is dedicated to specifying the optimization goal (Min or Max) and should not be combined.
            if (mode == (StaMaskEnum.Max | StaMaskEnum.Min))
                this.mode = StaMaskEnum.Max;
            if (mode == StaMaskEnum.Max)
                SetSysFlag(StaMaskEnum.Max);
            else if (mode == StaMaskEnum.Min)
                SetSysFlag(StaMaskEnum.Min);

            inputX = new T[dimN];
            inputXIdx = new uint[dimN];
            lutConstant = ImmutableDictionary<uint, OfflineGet<T>>.Empty;

            var dx = new T[dimN];
            var dxhalf = new T[dimN];
            for (int i = 0; i < dimN; i++)
            {
                dx[i] = (maxX[i] - minX[i]) / T.CreateChecked((Ns[i] - 1));
                dxhalf[i] = dx[i] / T.CreateChecked(2);
            }
            dX = ImmutableArray.Create(dx);
            dXHalf = ImmutableArray.Create(dxhalf);

            /*var samplejtot = new List<T>();
            GetSampleRecur(0, ref samplejtot);

            sampleJtot = [.. samplejtot]; //samplejtot.ToImmutableList();
            spanJ = sampleJtot.Max() - sampleJtot.Min();
            spanX = CalSpanX();*/
            
            
            InitSample();

            IntvX = new double[2][]; // minX, maxX

            DisplayMsgQueue = new(); // new ConcurrentQueue<string>();

            ClrSysFlag(StaMaskEnum.Done);
            ClrSysFlag(StaMaskEnum.MThread);
            SetSysFlag(StaMaskEnum.SampleReuse);

            SetSysFlag(StaMaskEnum.ReSamplingOnly);
            ClrSysFlag(StaMaskEnum.AxisChangingOnly);
            ClrSysFlag(StaMaskEnum.ReSampleAxisChangeBoth);         
        }

        public void InitSample()
        {
            Nfe = 0;
            var dx = new T[dimN];
            var dxhalf = new T[dimN];
            for (int i = 0; i < dimN; i++)
            {
                dx[i] = (maxX[i] - minX[i]) / T.CreateChecked((Ns[i] - 1));
                dxhalf[i] = dx[i] / T.CreateChecked(2);
            }
            dX = ImmutableArray.Create(dx);
            dXHalf = ImmutableArray.Create(dxhalf);

            var samplejtot = new List<T>();
            GetSampleRecur(0, ref samplejtot);

            sampleJtot = [.. samplejtot]; //samplejtot.ToImmutableList();
            spanJ = sampleJtot.Max() - sampleJtot.Min();
            spanX = CalSpanX();
        }
        
        private StateSnapshot<T> oldDataBeforeShrink = new StateSnapshot<T>();
        public void ShrinkBy(T factorSelected, int optSelectedIdx) // ratio other than 0.5 makes the sample number(Ns) changed!!! 
        {
            var minx = new T[dimN];
            var maxx = new T[dimN];
            var dx = new T[dimN];
            var dxhalf = new T[dimN];
            var ns = new int[dimN];
            var inputIndex = new uint[dimN];

            int[] nUpsample = new int[dimN];
            int[] nPreSample = new int[dimN];

            T[] divIdx = new T[dimN];
            T[] minIdxNext = new T[dimN];
            T[] maxIdxNext = new T[dimN];

            bool isReusableSample = IsSetSysFlag(StaMaskEnum.SampleReuse); // flag to indicate whether the previous samples can be reused
            bool isAutoShrink = IsSetSysFlag(StaMaskEnum.AutoShrink); // flag to indicate whether the shrink ratio is automatically adjusted based on the optimization results

            //AdjustResult(optSelected);

            for (int idxDim = 0; idxDim < dimN; idxDim++)
            {
                var optCentroidIdx = 0;
                T lengthLimit = T.Zero;
                T centroidXValue = T.Zero;
                T ratio = T.One;

                if ( isAutoShrink && (optResultReal.Length > 1))  // more than two candidate exist
                {
                    optCentroidIdx = optResultReal.Length - 1;  // if AutoShrink is enabled, use the last optimal result as the center for shrinking
                                                              // , which is the weighted centroid of the previous optimization results,
                                                              // and is expected to be closer to the true optimal solution.
                    centroidXValue = optResultReal[optCentroidIdx].X[idxDim][0]; // the centroid is always point not interval.

                    lengthLimit = (T.One + T.One) * optResultReal
                                //.Select(e => (dynamic)Math.Abs((dynamic)e.X[idxDim][0] - (dynamic)centerValue))
                                .Select(e =>
                                {
                                    var v0 = T.Abs(e.X[idxDim][0] - centroidXValue);

                                    return (
                                                e.X[idxDim][0] == e.X[idxDim][1])
                                                ? T.Zero//v0
                                                : T.Max(v0, T.Abs(e.X[idxDim][1] - centroidXValue)
                                            ); // lengthLimit should all the intervals of OptResultReal
                                })
                                .Max();         
                }
                else  // Only one candidate.
                {
                    centroidXValue = (optResultReal[optSelectedIdx].X[idxDim][1] + optResultReal[optSelectedIdx].X[idxDim][0]) / (T.One + T.One);
                    lengthLimit = optResultReal[optSelectedIdx].X[idxDim][1] - optResultReal[optSelectedIdx].X[idxDim][0];
                }
                //lengthLimit += dX[idxDim]; // add margin.
                ratio = lengthLimit / (maxX[idxDim] - minX[idxDim]);
                ratio = T.Max(ratio, factorSelected);

                T rangeIdx = T.CreateChecked(Ns[idxDim] - 1); // range of the current dimension in terms of index
                T rangeIdxHalf = rangeIdx / (T.One + T.One); // T.CreateChecked(2);
                T rdxIdx = ratio * rangeIdx; // shrinked range of the current dimension in terms of index
                if (rdxIdx <= T.Zero)
                    throw new InvalidOperationException("Shrinking Ratio is too small, should be larger than 0.5 for now.");

                //T optidx = (optResultReal[optCentroidIdx].X[idxDim][0] - minX[idxDim]) / dX[idxDim]; // the centroid is always point not interval.
                T optidx = (centroidXValue - minX[idxDim]) / dX[idxDim]; // the centroid is always point not interval.
                T minIdx = optidx - rdxIdx / (T.One + T.One);
                T maxIdx = minIdx + rdxIdx;
                if (IsSetSysFlag(StaMaskEnum.StrictLimit))
                {
                    if (minIdx < T.Zero)
                        minIdx = T.Zero;
                    maxIdx = minIdx + rdxIdx;
                    if (maxIdx >= rangeIdx)
                    {
                        maxIdx = rangeIdx;
                        minIdx = maxIdx - rdxIdx;
                    }
                }                 

                int uMinIdx = (int)Math.Ceiling(double.CreateChecked(minIdx));
                int uMaxIdx = (int)Math.Floor(double.CreateChecked(maxIdx));
                if (
                        !IsSetSysFlag(StaMaskEnum.SampleReuse) ||  // if SampleReuse is not set, we cannot reuse the previous samples, so we need to resample freely
                        ((rdxIdx < T.One) && (uMinIdx > uMaxIdx))  // if the shrinked range is less than 1 and the upper index is less than the lower index, we cannot reuse the previous samples, so we need to resample freely  
                    )
                {
                    // possible to resample freely
                    minx[idxDim] = minX[idxDim] + minIdx * dX[idxDim];
                    maxx[idxDim] = minX[idxDim] + maxIdx * dX[idxDim];
                    ns[idxDim] = (int)Ns[idxDim];
                    dx[idxDim] = (maxx[idxDim] - minx[idxDim]) / T.CreateChecked(ns[idxDim] - 1);
                    dxhalf[idxDim] = dx[idxDim] / (T.One + T.One);
                    isReusableSample = false; // even 1 dimension is not reusable, the whole sample is not reusable, so set the flag to false
                }
                else
                {
                    divIdx[idxDim] = T.Zero;
                    minIdxNext[idxDim] = T.Zero;
                    maxIdxNext[idxDim] = T.Zero;

                    nUpsample[idxDim] = 2;
                    int tmpNs = 0;

                    var tmpLimit = Ns[idxDim] * double.CreateChecked(ratio); // orginNs[idxDim] * double.CreateChecked(ratio); //orginNs[idxDim]
                    var maxSampleLimit = tmpLimit >= 5 ? tmpLimit : 5;
                    while (tmpNs < maxSampleLimit) // cap the number of samples to avoid excessive upsampling
                    {
                        divIdx[idxDim] = T.CreateChecked(1.0 / (double)nUpsample[idxDim]);
                        minIdxNext[idxDim] = T.CreateChecked(uMinIdx);
                        int n = 0;
                        while (minIdxNext[idxDim] >= minIdx)
                        {
                            n++;
                            minIdxNext[idxDim] = T.CreateChecked(uMinIdx) - T.CreateChecked(n) * divIdx[idxDim];
                        }
                        n--;
                        minIdxNext[idxDim] = T.CreateChecked(uMinIdx) - T.CreateChecked(n) * divIdx[idxDim];

                        tmpNs = nPreSample[idxDim] = n; // number of samples in the current dimension after upsampling less than uMinIdx
                        if (uMaxIdx == uMinIdx)
                            tmpNs += 1; // for uMinIdx
                        else
                        {
                            tmpNs += (uMaxIdx - uMinIdx) * nUpsample[idxDim] - 1; // number of samples in the current dimension after upsampling between uMinIdx and uMaxIdx
                            tmpNs += 2; // for uMinIdx and uMaxIdx
                        }
                        maxIdxNext[idxDim] = T.CreateChecked(uMaxIdx);
                        n = 0;
                        //while (maxIdxNext < maxIdx + T.CreateChecked(0.000001))
                        while (maxIdxNext[idxDim] <= maxIdx)
                        {
                            n++;
                            maxIdxNext[idxDim] = T.CreateChecked(uMaxIdx) + T.CreateChecked(n) * divIdx[idxDim];
                        }
                        n--;
                        maxIdxNext[idxDim] = T.CreateChecked(uMaxIdx) + T.CreateChecked(n) * divIdx[idxDim];
                        tmpNs += n; // number of samples in the current dimension after upsampling greater than uMaxIdx
                        nUpsample[idxDim]++;
                    }
                    nUpsample[idxDim]--;
                    minx[idxDim] = minX[idxDim] + minIdxNext[idxDim] * dX[idxDim];
                    maxx[idxDim] = minX[idxDim] + maxIdxNext[idxDim] * dX[idxDim];
                    ns[idxDim] = tmpNs;
                    dx[idxDim] =  (maxx[idxDim] - minx[idxDim]) / T.CreateChecked(ns[idxDim] - 1);//dX[idxDim] * divIdx[idxDim];
                    dxhalf[idxDim] = dx[idxDim] / (T.One + T.One);
                }
                //inputIndex[idxDim] = (uint)minIdx;
            }


            oldDataBeforeShrink = SaveState(); // Save the current state before shrinking
            int[] oldNs = Ns.Select(x => (int)x).ToArray(); //int[] oldNs = [.. Ns]; // Ns.ToArray();
            Ns = ImmutableArray.CreateRange(ns.Select(x => (uint)x)); //Ns = ImmutableArray.Create(ns);
            minX = ImmutableArray.Create(minx);
            maxX = ImmutableArray.Create(maxx);
            dX = ImmutableArray.Create(dx);
            dXHalf = ImmutableArray.Create(dxhalf);

            var samplejtot = new List<T>();

            if (isReusableSample)
            {
                int[] initIdxOfOldSample = new int[dimN];
                for (int i = 0; i < dimN; i++)
                {
                    initIdxOfOldSample[i] = (int)Math.Ceiling(double.CreateChecked(minIdxNext[i]));
                }
                UpSampleRecur(0, initIdxOfOldSample, nUpsample, nPreSample, ref samplejtot, true, oldNs);
            }
            else
            {
                UpSampleRecur(0, ref samplejtot);
            }

            spanJ = samplejtot.Max() - samplejtot.Min();
            spanX = CalSpanX();
            //UpSample(0, ref samplejtot, true, inputIndex, oldNs);
            sampleJtot = [.. samplejtot]; //samplejtot.ToImmutableList();

            /*var adjTolX = TolX / ratio; // Adjust TolX based on the shrink ratio to maintain the same relative tolerance in the new, smaller search space
                                        // the size of clusters is determined by the product of TolX and the span of the search space.
                                        // the span of the search space is reduced by the shrink ratio, so we need to increase TolX by the same ratio to maintain the same cluster size.
                                        // the greater TolX, the larger the cluster size, and the fewer clusters will be formed, which may lead to a loss of optimal solutions.
            TolX = (adjTolX < T.One) ? adjTolX : T.One; */

            ClrSysFlag(StaMaskEnum.Done);
        }
        private T CalSpanX()
        {
            T spanx = T.Zero;
            for (int i = 0; i < dimN; i++)
            {
                spanx += (maxX[i] - minX[i]) * (maxX[i] - minX[i]);
            }
            spanx = T.Sqrt(spanx);

            return spanx;
        }
        public StateSnapshot<T> SaveState()
        {
            return new StateSnapshot<T>
            {
                Ns = [.. Ns.ToArray()],
                MinX = [.. minX.ToArray()],
                MaxX = [.. maxX.ToArray()],
                DX = [.. dX.ToArray()],
                DXHalf = [.. dXHalf.ToArray()],
                SampleJtot = [.. sampleJtot.ToList()],
                //TolX = TolX,
                OptResultReal = [.. optResultReal.ToArray()],
                Nfe = Nfe
            };
        }
        public void RestoreState()
        {
            if (
                !(oldDataBeforeShrink.Ns == null ||
                oldDataBeforeShrink.MinX == null ||
                oldDataBeforeShrink.MaxX == null ||
                oldDataBeforeShrink.DX == null ||
                oldDataBeforeShrink.DXHalf == null ||
                oldDataBeforeShrink.SampleJtot == null ||
                oldDataBeforeShrink.OptResultReal == null)
               )
            {
                Ns = [.. oldDataBeforeShrink.Ns];
                minX = [.. oldDataBeforeShrink.MinX];
                maxX = [.. oldDataBeforeShrink.MaxX];
                dX = [.. oldDataBeforeShrink.DX];
                dXHalf = [.. oldDataBeforeShrink.DXHalf];
                sampleJtot = [.. oldDataBeforeShrink.SampleJtot];
                //TolX = oldDataBeforeShrink.TolX;
                optResultReal = [.. oldDataBeforeShrink.OptResultReal];
                spanJ = sampleJtot.Max() - sampleJtot.Min();
                spanX = CalSpanX();
                Nfe = oldDataBeforeShrink.Nfe;
            }
        }

        /// <summary>
        /// Asynchronously optimizes a multi-dimensional dataset and updates the optimization results.
        /// </summary>
        /// <remarks>This method performs an asynchronous optimization process over a multi-dimensional
        /// dataset. The results of the optimization are stored in the <c>optResult</c> field. The method clears and
        /// sets system flags before and after the optimization process, respectively, and adds a message to the display
        /// upon completion.</remarks>
        public async Task OptimizeMD()
        {
            //var xOpt = new T[dimN];
            var Opt = (X: new T[dimN][], J: default(T));
            List<(T[][] X, T J)>? Optlist1 = null, Optlist2 = null;

            ClrSysFlag(StaMaskEnum.Done);
            bool doReSample = IsSetSysFlag(StaMaskEnum.ReSamplingOnly) || IsSetSysFlag(StaMaskEnum.ReSampleAxisChangeBoth);
            bool doAxisChange = IsSetSysFlag(StaMaskEnum.AxisChangingOnly) || IsSetSysFlag(StaMaskEnum.ReSampleAxisChangeBoth);

            if (IsSetSysFlag(StaMaskEnum.MThread)) // 병렬 실행 (Parallel/Async)
            {
                var t1 = doReSample ? ExecuteAsync(StaMaskEnum.ReSampling, StaMaskEnum.AxisChanging) : Task.FromResult<List<(T[][] X, T J)>>(null);
                var t2 = doAxisChange ? ExecuteAsync(StaMaskEnum.AxisChanging, StaMaskEnum.ReSampling) : Task.FromResult<List<(T[][] X, T J)>>(null);

                await Task.WhenAll(t1, t2);

                Optlist1 = await t1;
                Optlist2 = await t2;
            }
            else // 직렬 실행 (Serial/Sync)
            {
                if (doReSample) Optlist1 = ExecuteSync(StaMaskEnum.ReSampling, StaMaskEnum.AxisChanging);
                if (doAxisChange) Optlist2 = ExecuteSync(StaMaskEnum.AxisChanging, StaMaskEnum.ReSampling);
            }

            // 공통: 결과 합치기 (LINQ)
            optResult = (Optlist1 ?? Enumerable.Empty<(T[][] X, T J)>())
                .Concat(Optlist2 ?? Enumerable.Empty<(T[][] X, T J)>())
                .ToArray();

            // --- 헬퍼 로컬 함수 ---
            async Task<List<(T[][] X, T J)>> ExecuteAsync(StaMaskEnum setFlag, StaMaskEnum clrFlag)
            {
                SetSysFlag(setFlag);
                ClrSysFlag(clrFlag);
                return await OptimaizeMdRecurAsync(0, sampleJtot, Opt);
            }

            List<(T[][] X, T J)> ExecuteSync(StaMaskEnum setFlag, StaMaskEnum clrFlag)
            {
                SetSysFlag(setFlag);
                ClrSysFlag(clrFlag);
                return OptimaizeMdRecur(0, sampleJtot, Opt);
            }
            /*// for Parallel running ***************************************
            if (IsSetSysFlag(StaMaskEnum.MThread))
            {
                if (IsSetSysFlag(StaMaskEnum.ReSamplingOnly) || IsSetSysFlag(StaMaskEnum.ReSampleAxisChangeBoth))
                {
                    SetSysFlag(StaMaskEnum.ReSampling);
                    ClrSysFlag(StaMaskEnum.AxisChanging);                
                    await OptimaizeMdRecurAsync(0, sampleJtot, Opt).ContinueWith(
                        t =>
                        {
                            Optlist1 = t.Result;
                            optResult = [.. Optlist1]; //Optlist.ToArray();
                        }
                    );
                }
                if (IsSetSysFlag(StaMaskEnum.AxisChangingOnly) || IsSetSysFlag(StaMaskEnum.ReSampleAxisChangeBoth))
                {
                    SetSysFlag(StaMaskEnum.AxisChanging);
                    ClrSysFlag(StaMaskEnum.ReSampling);
                    await OptimaizeMdRecurAsync(0, sampleJtot, Opt).ContinueWith(
                        t =>
                        {
                            Optlist2 = t.Result;
                            optResult = [.. Optlist2]; //Optlist.ToArray();
                        }
                    );
                }
                // 1. 각 리스트의 개수 계산
                int count1 = Optlist1?.Count ?? 0;
                int count2 = Optlist2?.Count ?? 0;

                // 2. 최종 배열 바로 생성 및 CopyTo로 단순 복사
                optResult = new (T[][] X, T J)[count1 + count2];

                Optlist1?.CopyTo(optResult, 0);       // null이 아니면 0번 인덱스부터 복사
                Optlist2?.CopyTo(optResult, count1);  // null이 아니면 count1번 인덱스부터 연속 복사
            }
            else // for Serial running ***************************************
            {                                
                if (IsSetSysFlag(StaMaskEnum.ReSamplingOnly) || IsSetSysFlag(StaMaskEnum.ReSampleAxisChangeBoth))
                {
                    SetSysFlag(StaMaskEnum.ReSampling);
                    ClrSysFlag(StaMaskEnum.AxisChanging);
                    Optlist1 = OptimaizeMdRecur(0, sampleJtot, Opt);
                }
                if (IsSetSysFlag(StaMaskEnum.AxisChangingOnly) || IsSetSysFlag(StaMaskEnum.ReSampleAxisChangeBoth))
                {
                    SetSysFlag(StaMaskEnum.AxisChanging);
                    ClrSysFlag(StaMaskEnum.ReSampling);
                    Optlist2 = OptimaizeMdRecur(0, sampleJtot, Opt);
                }

                // 1. 각 리스트의 개수 계산
                int count1 = Optlist1?.Count ?? 0;
                int count2 = Optlist2?.Count ?? 0;

                // 2. 최종 배열 바로 생성 및 CopyTo로 단순 복사
                optResult = new (T[][] X, T J)[count1 + count2];

                Optlist1?.CopyTo(optResult, 0);       // null이 아니면 0번 인덱스부터 복사
                Optlist2?.CopyTo(optResult, count1);  // null이 아니면 count1번 인덱스부터 연속 복사
            }*/

            AdjustResult(); // optResultReal = adjustResult().ToArray();            
        }
        /// <summary>
        /// Adjusts the result by clustering items based on their distance and selecting representative items from each
        /// cluster according to the specified mode.
        /// </summary>
        /// <remarks>This method groups items into clusters using a distance-based algorithm and then
        /// selects a single representative item from each cluster. The selection is determined by the <see
        /// cref="StaMaskEnum"/> mode: <list type="bullet"> <item> <description>If the mode is <see
        /// cref="StaMaskEnum.Min"/>, the item with the minimum value of <c>J</c> in each cluster is
        /// selected.</description> </item> <item> <description>If the mode is <see cref="StaMaskEnum.Max"/>, the item
        /// with the maximum value of <c>J</c> in each cluster is selected.</description> </item> </list> Clusters with
        /// no items are excluded from the result.</remarks>
        /// <returns>An array of tuples, where each tuple contains: <list type="bullet"> <item> <description><c>X</c>: An array
        /// representing the clustered items.</description> </item> <item> <description><c>J</c>: The selected
        /// representative value from the cluster.</description> </item> </list></returns>
        public void AdjustResult()
        {
            CalResultReal();

            var clusters = new List<List<(T[][] X, T J)>>();
            if (TolX < T.One)
            {
                spanX = CalSpanX();

                //clusters = GeneralMath<T>.ClusterByDistance(optResultReal, TolX * spanX);
                clusters = GeneralMath<T>.ClusterByDistance(optResultReal, TolXReal);
            }
            else // If TolX is greater than or equal to 1, treat all results as a single cluster
            {
                clusters.Add(new List<(T[][] X, T J)>());
                clusters[0] = optResultReal.ToList();
            }


            (T[][] X, T J)[] newResult = []; //Array.Empty<(T[], T)>();
            T optj = T.Zero;

            if (mode == StaMaskEnum.Min) // If the mode is set to Min, select the item with the minimum J value from each cluster
                                         // and filter the results to include only those within TolJReal of the optimal J value
                                         // and order the results by J value in ascending order
            {
                /* newResult = [.. clusters
                                  .Where(cluster => cluster.Count > 0)
                                  .Select(cluster => cluster.MinBy(item => item.J))];
                 optj = newResult.Min(item => item.J);
                 newResult = [.. newResult.Where(item => T.Abs(item.J - optj) <= TolJReal)]; //.ToArray();
                 newResult = [.. newResult.OrderBy(item => item.J)]; //.ToArray();*/

                // make array of minimum items from each cluster
                var initialMinItems = clusters
                                            .Where(cluster => cluster.Count > 0)
                                            .Select(cluster => cluster.MinBy(item => item.J))
                                            /*.Select(cluster =>
                                            {
                                                // X[] 평균 (각 인덱스별 평균)
                                                int length = cluster[0].X.Length;
                                                var avgX = new T[length];
                                                for (int i = 0; i < length; i++)
                                                {
                                                    // 각 item.X[i]의 평균
                                                    double avg = cluster.Average(item => Convert.ToDouble(item.X[i]));
                                                    avgX[i] = (T)Convert.ChangeType(avg, typeof(T));
                                                }
                                                
                                                var avgJ = J(avgX);
                                                return (X: avgX, J: (T)Convert.ChangeType(avgJ, typeof(T)));
                                            })*/
                                            .ToList();

                if (initialMinItems.Count > 0)
                {
                    optj = initialMinItems.Min(item => item.J);

                    // Filter the results to include only those within TolJReal of the optimal J value and order the results by J value in ascending order
                    newResult = [.. initialMinItems
                                            .Where(item => T.Abs(item.J - optj) <= TolJReal)
                                            //.Select(item => (item.X, J:J(item.X)))
                                            .OrderBy(item => item.J)];
                }

            }
            else if (mode == StaMaskEnum.Max) // If the mode is set to Max, select the item with the maximum J value from each cluster
                                              // and filter the results to include only those within TolJReal of the optimal J value
                                              // and order the results by J value in descending order
            {
                var initialMaxItems = clusters
                                            .Where(cluster => cluster.Count > 0)
                                            .Select(cluster => cluster.MaxBy(item => item.J))
                                            /*.Select(cluster =>
                                            {
                                                // X[] 평균 (각 인덱스별 평균)
                                                int length = cluster[0].X.Length;
                                                var avgX = new T[length];
                                                for (int i = 0; i < length; i++)
                                                {
                                                    // 각 item.X[i]의 평균
                                                    double avg = cluster.Average(item => Convert.ToDouble(item.X[i]));
                                                    avgX[i] = (T)Convert.ChangeType(avg, typeof(T));
                                                }
                                                // J 평균
                                                var avgJ = cluster.Average(item => Convert.ToDouble(item.J));

                                                return (X: avgX, J: (T)Convert.ChangeType(avgJ, typeof(T)));
                                            })*/
                                            .ToList();
                if(initialMaxItems.Count > 0)
                {
                    optj = initialMaxItems.Max(item => item.J);
                    newResult = [.. initialMaxItems
                                            .Where(item => T.Abs(item.J - optj) <= TolJReal)
                                            //.Select(item => (item.X, J:J(item.X)))
                                            .OrderByDescending(item => item.J)];
                }
            }

            //AddWeightedCentroid(ref newResult, optj, newResult.Length - 1); // Add the weighted centroid of the clusters to the result
            //AddWeightedCentroid(ref newResult, optj, newResult.Length - 1); // Add the weighted centroid of the clusters to the result
            if (newResult.Length > 1)
            {
                var centroid = CalculateWeightedCentroid(ref newResult, optj, newResult.Length - 1);
                Array.Resize(ref newResult, newResult.Length + 1);
                newResult[^1] = centroid;
            }

            optResultReal = newResult;
            CalResultPrj();

            SetSysFlag(StaMaskEnum.Done);
            AddDispMsg(
                Environment.NewLine +
                ResultMsg(optResult.Length - 1),
                LogLevel.Info0
            );
        }
        /// <summary>
        /// Calculates the weighted centroid of a set of points based on their objective function values and a reference optimal value.
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="optj"></param>
        /// <param name="maxIdx"></param>
        /// <returns></returns>
        public (T[][] X, T J) CalculateWeightedCentroid(ref (T[][] X, T J)[] newResult, T? optjGiven, int maxIdx)
        {
            T optj = T.Zero;
            if (optjGiven.HasValue)
                optj = optjGiven.Value;
            else
                optj = mode == StaMaskEnum.Min
                    ? newResult.Min(item => item.J)
                    : newResult.Max(item => item.J);

            if (newResult.Length <= 1)
                return newResult[0];
            // 전체 가중치 합
            T totalWeight = T.Zero;
            T totalDiff = T.Zero;
            int dimension = newResult[0].X.Length;
            T[] weightedSum = new T[dimension];

            //T offset = T.Zero;
            //if (mode == StaMaskEnum.Min)
            //    offset = calculateOffsetWeightForCentroid(newResult,maxIdx,optj);

            T Jmax = newResult.Max(x => x.J);
            T Jmin = newResult.Min(x => x.J);
            T Beta = T.CreateChecked(1.0);
            for (int idx = 0; idx <= maxIdx && idx < newResult.Length; idx++)
            {
                var (X, J) = newResult[idx];

                T diff = T.Abs(optj - J);
                T weight = T.Zero;
                /*T weight = J + T.Abs(optj); // J + |optj| is always non-negative, and the larger J is, the larger the weight will be.
                if (mode == StaMaskEnum.Min)
                {
                    T offset = calculateOffsetWeightForCentroid(newResult, maxIdx, optj); // maxJ - (J + |optj|) is always non-negative, but minimum is always 0
                                                                                          // so we need to add some more offset(=avg/2) to make it larger than 0.
                                                                                          // If offset is too small, the weight will be too small, and
                                                                                          // the centroid will be biased towards the first item.
                    weight = offset - weight;
                }*/

                if (mode == StaMaskEnum.Min)
                {
                    weight = T.Exp(-Beta * (J - Jmin) / (Jmax - Jmin));
                    //weight = T.Exp(-Beta * (J - Jmin));
                }
                if (mode == StaMaskEnum.Max)
                {
                    weight = T.Exp(-Beta * (Jmax - J) / (Jmax - Jmin));
                }


                totalWeight += weight;
                totalDiff += diff;  

                for (int i = 0; i < dimension; i++)
                {
                    T avgX = (X[i][0] + X[i][1]) / (T.One + T.One); // If X[i] is interval, the center value is utilized for calculation
                    //weightedSum[i] += X[i][0] * weight;
                    weightedSum[i] += avgX * weight;
                }
            }

            /*foreach (var (X, J) in newResult)
            {
                T weight = T.Zero;
                if (mode == StaMaskEnum.Min) // For minimization, weight is inversely proportional to J and non-negative 
                {
                    weight = maxTmp - (J + T.Abs(optj));
                }                    
                else if (mode == StaMaskEnum.Max) // For maximization, weight is directly proportional to J and non-negative
                {
                    weight = J + T.Abs(optj);
                }

                totalWeight += weight;

                for (int i = 0; i < dimension; i++)
                {
                    weightedSum[i] += X[i] * weight;
                }
            }*/

            (T[][], T) newItem = new ();            

            if ( T.Abs(totalDiff) < T.CreateChecked(0.000001) )
            {
                AddDispMsg("Warning: All weights are identical, Possible multi Optima" + Environment.NewLine, LogLevel.Warning);
                AddDispMsg("Warning: AutoShrink is disabled, and First solution is selected for shrinkage.", LogLevel.Warning);
                // If all weights are identical, return a zero vector and a negative J value to indicate an issue                
                newItem = (
                            Enumerable.Range(0, (int)dimension)
                                                      .Select(_ => new T[] { T.Zero, T.Zero })
                                                      .ToArray(),
                            T.CreateChecked(-1)
                        );
            }
            else
            {
                // 무게 중심(가중 평균) 계산
                //T[][] centroid = new T[dimension][];
                T[][] centroid = (
                    Enumerable.Range(0, dimension)
                              .Select(_ => new T[] { T.Zero, T.Zero })
                              .ToArray()
                );
                for (int i = 0; i < dimension; i++)
                {
                    // the centroid is always point not interval.
                    centroid[i][0] = weightedSum[i] / totalWeight;
                    centroid[i][1] = weightedSum[i] / totalWeight;
                }
                // 결과를 newResult 마지막에 추가
                newItem = (centroid, totalWeight);
            }

            return newItem;
        }
        /// <summary>
        /// Calculates an offset weight for the centroid based on the provided results, maximum index, and optimal value.
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="maxIdx"></param>
        /// <param name="optj"></param>
        /// <returns></returns>
        private T calculateOffsetWeightForCentroid((T[][] X, T J)[] newResult, int maxIdx, T optj)
        {
            T sum = T.Zero;
            T max = newResult[0].J + T.Abs(optj);
            int count = 0;

            for (int i = 0; i <= maxIdx; i++)
            {
                T value = newResult[i].J + T.Abs(optj);
                if (value > max) max = value;
                sum += value;
                count++;
            }

            T avg = sum / T.CreateChecked(count);
            T offset = max + avg / T.CreateChecked(2); // max - (J + |optj|) is always non-negative, but minimum is always 0
                                  // so we need to add (avg/2) to make it larger than 0.
                                  // If offset is too small, the weight will be too small, and
                                  // the centroid will be biased towards the first item.
            return offset; 
        }

        /// <summary>
        /// Recursively optimizes a multi-dimensional function along each axis and returns a list of optimal solutions.
        /// </summary>
        /// <remarks>This method performs a recursive optimization process across multiple dimensions. For
        /// each axis, it identifies      the optimal values and refines the solution by sampling and optimizing along
        /// subsequent axes. The recursion      terminates when all dimensions have been processed, at which point the
        /// optimized solutions are collected and returned.          The method assumes that the input data and
        /// dimensions are valid and that the optimization process is well-defined      for the given function and
        /// constraints.</remarks>
        /// <param name="axisIdx">The index of the current axis being optimized. Must be within the range of valid dimensions.</param>
        /// <param name="samplejtot">An immutable list representing the current sample data for optimization. Cannot be null.</param>
        /// <param name="Opt">A tuple containing the current best solution vector <c>X</c> and its corresponding objective value <c>J</c>.</param>
        /// <returns>A list of tuples, where each tuple contains an optimized solution vector <c>X</c> and its corresponding
        /// objective value <c>J</c>.</returns>
        private List<(T[][] X, T J)> OptimaizeMdRecur(int axisIdx, ImmutableList<T> samplejtot, (T[][] X, T J) Opt)
        {
            AddDispMsg(string.Format("MdRecur({0,0:G3})" + Environment.NewLine, axisIdx), LogLevel.Info1);

            var Optlist = new List<(T[][] X, T J)>();

            //---
            /*(T[] X, T J)[] optlist1Axis;
            double[][] optintv;

            if (IsSetSysFlag(StaMaskEnum.ReSampling))
            {
                // Optimize the current axis and get the optimal values and intervals
                (optlist1Axis, optintv) = OptimizeMD1Axis(samplejtot, axisIdx);
            }
            else // IsSetSysFlag(StaMaskEnum.AxisChanging)
            {
                (optlist1Axis, optintv) = OptimizeMD1Axis(samplejtot, 0);
            }*/
            int targetAxis = IsSetSysFlag(StaMaskEnum.ReSampling) ? axisIdx : 0;
            var (optlist1Axis, optintv) = OptimizeMD1Axis(samplejtot, targetAxis);

            if (axisIdx == 0)
                IntvX = optintv;

            // Recursively optimize the next axis if not at the last dimension
            foreach (var opt1Axis in optlist1Axis)
            {
                // Deep Copy of Opt
                T[][] OptXCopy = new T[Opt.X.Length][];
                for (int i = 0; i < Opt.X.Length; i++)
                {
                    if (Opt.X[i] != null)
                    {
                        OptXCopy[i] = (T[])Opt.X[i].Clone(); // 내부 1차원 배열 복사
                    }
                }
                var optNew = (X: OptXCopy, J: Opt.J);

                optNew.X[axisIdx] = opt1Axis.X;
                optNew.J = opt1Axis.J;

                // If at the last dimension, clone the current solution and add it to the result list
                if (axisIdx == dimN - 1)
                {
                    //var opt = ((T[][])Opt.X.Clone(), Opt.J);  // Optlist.Add only clones the reference, not the value.

                    Optlist.Add(optNew);
                }
                // If not at the last dimension, resample and continue optimizing recursively
                else
                {
                    if ( T.Abs(opt1Axis.X[0] - opt1Axis.X[1]) < T.CreateChecked(0.0000001) )  // Not interval, point
                    {
                        List<T> newSample = []; // new List<T>()
                        var inputIdx = new uint[dimN];

                        List<(T[][] X, T J)> OptlistNew;
                        if (IsSetSysFlag(StaMaskEnum.ReSampling))
                        {
                            ReSampleRecur(opt1Axis.X[0], axisIdx + 1, axisIdx + 1, ref inputIdx, ref samplejtot, ref newSample);
                        }
                        else // IsSetSysFlag(StaMaskEnum.AxisChanging)
                        {
                            AxisChangeSampleRecur(axisIdx + 1, 0, ref inputIdx, ref samplejtot, ref newSample);
                        }

                        OptlistNew = OptimaizeMdRecur(axisIdx + 1, [.. newSample], optNew); // newSample.ToImmutableList();
                        Optlist.AddRange(OptlistNew);
                    }
                    else // In case of the interval, no further recurrence, but just return the same value of optimal intervals
                         // which were calculated when optimizing the axis 0.
                    { 
                        for( int i = axisIdx+1; i<dimN; i++)
                        {
                            optNew.X[i] = new T[2];
                            optNew.X[i][0] = T.CreateChecked(IntvX[0][i]);
                            optNew.X[i][1] = T.CreateChecked(IntvX[1][i]);
                        }

                        Optlist.Add(optNew);
                    }
                }
            }
            return Optlist;
        }
        private void AxisChangeSampleRecur(int axisIdx, int idxDim, ref uint[] inputIdx, ref ImmutableList<T> iSample, ref List<T> oSample)
        {
            int idxDimCur;

            if (idxDim == 0)
                idxDimCur = axisIdx;
            else if (idxDim == axisIdx)
                idxDimCur = 0;
            else
                idxDimCur = idxDim;

            for (uint n = 0; n < Ns[idxDimCur]; n++)
            {
                inputIdx[idxDimCur] = n;
                if (idxDim == dimN - 1)
                {
                    oSample.Add(iSample[(int)GetIdTot(inputIdx)]);
                }
                else
                    AxisChangeSampleRecur(axisIdx, idxDim + 1, ref inputIdx, ref iSample, ref oSample);
            }
        }
        /// <summary>
        /// Recursively optimizes a multi-dimensional function along specified axes and returns a list of optimal
        /// </summary>
        /// <param name="axisIdx"></param>
        /// <param name="samplejtot"></param>
        /// <param name="Opt"></param>
        /// <returns></returns>
        private async Task<List<(T[][] X, T J)>> OptimaizeMdRecurAsync(int axisIdx, ImmutableList<T> samplejtot, (T[][] X, T J) Opt)
        {
            AddDispMsg(string.Format("MdRecur({0,0:G3})" + Environment.NewLine, axisIdx), LogLevel.Info1);

            var Optlist = new List<(T[][] X, T J)>();

            // 1. 비동기 호출 간결화 및 인자 일치
            int targetAxis = IsSetSysFlag(StaMaskEnum.ReSampling) ? axisIdx : 0;
            var (optlist1Axis, optintv) = await OptimizeMD1AxisAsync(samplejtot, targetAxis);

            if (axisIdx == 0)
                IntvX = optintv;

            // 2. foreach 내부 재귀를 병렬(Task)로 처리하여 실제 속도 향상 도모
            var tasks = new List<Task<List<(T[][] X, T J)>>>();

            foreach (var opt1Axis in optlist1Axis)
            {
                // Deep Copy of Opt
                T[][] OptXCopy = new T[Opt.X.Length][];
                for (int i = 0; i < Opt.X.Length; i++)
                {
                    if (Opt.X[i] != null)
                        OptXCopy[i] = (T[])Opt.X[i].Clone();
                }
                var optNew = (X: OptXCopy, J: Opt.J);

                optNew.X[axisIdx] = opt1Axis.X;
                optNew.J = opt1Axis.J;

                if (axisIdx == dimN - 1)
                {
                    Optlist.Add(optNew);
                }
                else
                {
                    if (T.Abs(opt1Axis.X[0] - opt1Axis.X[1]) < T.CreateChecked(0.0000001))
                    {
                        List<T> newSample = [];
                        var inputIdx = new uint[dimN];

                        if (IsSetSysFlag(StaMaskEnum.ReSampling))
                            ReSampleRecur(opt1Axis.X[0], axisIdx + 1, axisIdx + 1, ref inputIdx, ref samplejtot, ref newSample);
                        else
                            AxisChangeSampleRecur(axisIdx + 1, 0, ref inputIdx, ref samplejtot, ref newSample);

                        // 3. 올바른 비동기 재귀 호출 (Task 생성 후 병렬 수집)
                        var immutableSample = newSample.ToImmutableList();
                        tasks.Add(Task.Run(() => OptimaizeMdRecurAsync(axisIdx + 1, immutableSample, optNew)));
                    }
                    else
                    {
                        for (int i = axisIdx + 1; i < dimN; i++)
                        {
                            optNew.X[i] = new T[2];
                            optNew.X[i][0] = T.CreateChecked(IntvX[0][i]);
                            optNew.X[i][1] = T.CreateChecked(IntvX[1][i]);
                        }
                        Optlist.Add(optNew);
                    }
                }
            }

            // 병렬로 실행된 재귀 Task들의 결과를 하나로 병합
            if (tasks.Count > 0)
            {
                var results = await Task.WhenAll(tasks);
                foreach (var res in results)
                {
                    Optlist.AddRange(res);
                }
            }

            return Optlist;
        }
        /*/// <summary>
        /// Recursively optimizes a multi-dimensional function along specified axes and returns a list of optimal
        /// solutions.
        /// </summary>
        /// <remarks>This method performs a recursive optimization process by iterating over each axis of
        /// a multi-dimensional space. For each axis, it computes optimal values and intervals, and then refines the
        /// optimization by sampling and recursively optimizing along subsequent axes. The process continues until all
        /// dimensions are optimized.  If the current axis is the last dimension, the method clones the current solution
        /// and adds it to the result list. Otherwise, it resamples the space and continues the optimization recursively
        /// for the next axis.  The method uses asynchronous operations to perform optimizations and handle recursive
        /// calls efficiently.</remarks>
        /// <param name="axisIdx">The index of the current axis being optimized. Must be within the range of valid axes.</param>
        /// <param name="samplejtot">An immutable list of samples representing the current state of the optimization process.</param>
        /// <param name="Opt">A tuple containing the current best solution vector <see langword="X"/> and its corresponding objective
        /// value <see langword="J"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of tuples, where each tuple
        /// contains: <list type="bullet"> <item><description>An array <see langword="X"/> representing a solution
        /// vector.</description></item> <item><description>The corresponding objective value <see langword="J"/> for
        /// the solution vector.</description></item> </list></returns>
        private async Task<List<(T[][] X, T J)>> OptimaizeMdRecurAsync(int axisIdx, ImmutableList<T> samplejtot, (T[][] X, T J) Opt)
        {
            AddDispMsg(string.Format("MdRecur({0,0:G3})" + Environment.NewLine, axisIdx), LogLevel.Info1);

            var Optlist = new List<(T[][] X, T J)>();

            (T[] X, T J)[] optlist1Axis = []; //Array.Empty<(T, T)>();
            double[][] optintv = []; //Array.Empty<double[]>();

            await OptimizeMD1AxisAsync(samplejtot, axisIdx).ContinueWith(
                    t =>
                    {
                        (optlist1Axis, optintv) = t.Result;
                    }
                );

            if (axisIdx == 0)
                IntvX = optintv;

            foreach (var opt1Axis in optlist1Axis)
            {
                Opt.X[axisIdx] = opt1Axis.X;
                Opt.J = opt1Axis.J;

                if (axisIdx == dimN - 1)
                {
                    var opt = ((T[][])Opt.X.Clone(), Opt.J);  // Optlist.Add only clones the reference, not the value.

                    Optlist.Add(opt);
                }
                else
                {
                    List<T> newSample = []; // new List<T>()
                    var inputIdx = new uint[dimN];
                    ReSampleRecur(opt1Axis.X[0], axisIdx + 1, axisIdx + 1, ref inputIdx, ref samplejtot, ref newSample);

                    await OptimaizeMdRecurAsync(axisIdx + 1, [.. newSample], Opt).ContinueWith( // newSample.ToImmutableList();
                        t =>
                        {
                            var OptlistNew = t.Result;
                            Optlist.AddRange(OptlistNew);
                        }
                    );
                }
            }
            return Optlist;
        }*/
        private async Task<((T[] X, T J)[] Optlist1Axis, double[][] optIntv)> OptimizeMD1AxisAsync(ImmutableList<T> samplejtot, int idxDim)
        {
            Opt1D<T> opt1D;
            var sampleJs = new T[Ns[idxDim]];
            var minXs = new double[Ns[idxDim]][];
            var maxXs = new double[Ns[idxDim]][];
            var dx = 1.0 / Ns[idxDim]; // caution!!! not (1.0 / (Ns[idxDim] - 1.0))
            double[][]? optIntv =
                [
                    new double[dimN], // minX
			        new double[dimN]  // maxX
                ];

            var axisIdx = idxDim;
            var tasks = new List<Task<((T[] X, T J)[] Optlist1Axis, double[][] optIntv)>>();
            // Recursively sample and optimize the current axis
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                var inputIndex = new uint[dimN];
                inputIndex[idxDim] = n;

                inputIndex[idxDim] = n; // Set the current index for the dimension being processed
                AddDispMsg(string.Format("        Md1Axis({0,0:G3},{1,0:G3},{2,0:G3})" + Environment.NewLine, axisIdx, idxDim, n), LogLevel.Info3);

                // If at the last dimension, directly sample the objective function value
                if (idxDim == dimN - 1)
                {
                    sampleJs[n] = samplejtot[(int)GetIdTot(inputIndex)];
                    // optmal value is definitive.
                    if (axisIdx == 0) // No effect for idxDim == dimN - 1, since the minXs and maxXs are not used in that case.
                    {
                        minXs[n] = new double[dimN];
                        maxXs[n] = new double[dimN];
                        minXs[n][idxDim] = dx * (n + 0.5);
                        maxXs[n][idxDim] = dx * (n + 0.5);
                    }
                }
                else
                {
                    AddDispMsg(string.Format("    task: Md1Axis({0,0:G3},{1,0:G3})" + Environment.NewLine, idxDim, n), LogLevel.Info2);

                    var task = pcQ.Enqueue<((T[] X, T J)[] Optlist1Axis, double[][] optIntv)>(
                        () => OptimizeMD1AxisRecur(samplejtot, axisIdx, idxDim + 1, inputIndex)
                    );
                    tasks.Add(task);

                }
            }
            if (tasks.Count > 0)
            {
                //var _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                await Task.WhenAll(tasks).ContinueWith(
                    t =>
                    {
                        if (mode == StaMaskEnum.Min)
                            sampleJs = [.. t.Result.Select(item => item.Optlist1Axis.Min(opt => opt.J))]; // t.Result.Select(item => item.Optlist1Axis.Min(opt => opt.J)).ToArray();
                        else if (mode == StaMaskEnum.Max)
                            sampleJs = [.. t.Result.Select(item => item.Optlist1Axis.Max(opt => opt.J))]; // t.Result.Select(item => item.Optlist1Axis.Max(opt => opt.J)).ToArray();
                        minXs = [.. t.Result.Select(x => x.optIntv[0])]; // t.Result.Select(x => x.optIntv[0]).ToArray();
                        maxXs = [.. t.Result.Select(x => x.optIntv[1])]; // t.Result.Select(x => x.optIntv[1]).ToArray();
                    }
                // , _uiScheduler
                );
            }

            OfflineGet<T> constVar;
            lock (_locker)
            {
                // Check if the constant variable for the current dimension already exists in the lookup table
                if (lutConstant.TryGetValue(Ns[idxDim], out OfflineGet<T>? value))
                    constVar = value;
                // If not, create a new constant variable and add it to the lookup table
                else
                {
                    constVar = new OfflineGet<T>(Ns[idxDim]);
                    lutConstant = lutConstant.Add(Ns[idxDim], constVar);
                }
            }

            // Create an instance of the Opt1D class for the current dimension, using the sampled values and other parameters
            if ((axisIdx != 0) && (idxDim == axisIdx))
            {
                var optIntV1D = new[] { IntvX[0][idxDim], IntvX[1][idxDim] };
                //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)mode, optIntV1D);
                //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)(mode | StaMaskEnum.PartialConstJ), null);
                opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mState, null);
            }
            // If not at the specified axis, create an instance of the Opt1D class without specifying intervals
            else
            {
                //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)(mode | StaMaskEnum.PartialConstJ), null);
                opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mState, null);
                // If at the first axis, calculate the minimum and maximum intervals for the remaining dimensions based on the optimized values
                if (axisIdx == 0)
                {
                    for (int i = idxDim + 1; i < dimN; i++)
                    {
                        var intvMin = double.MaxValue;
                        var intvMax = double.MinValue;

                        foreach (var opt in opt1D.Optlist)
                        {
                            var optidxStart = (int)(double.CreateChecked(opt.X[0]) / dx);
                            if (T.Abs(opt.X[0] - opt.X[1]) < T.CreateChecked(0.0000001)) // just point, not interval
                                CalculateIntvMinMax(i, idxDim, optidxStart, minXs, maxXs, ref intvMin, ref intvMax, false); // opt.X is not at sample point for lower axes
                            else // Interval, not just point
                            {
                                var optidxEnd = (int)(double.CreateChecked(opt.X[1]) / dx);
                                //if (optidxEnd >= minXs.Length)
                                //    optidxEnd = minXs.Length - 1; // for the case that the margin is added in Opt1D.GetSegOfConst()
                                for (int j = optidxStart; j <= optidxEnd; j++)
                                {
                                    CalculateIntvMinMax(i, idxDim, j, minXs, maxXs, ref intvMin, ref intvMax, true); // opt.X is at the exact sampling point for lower axes
                                }
                            }
                            /*// If the optimized index is at the last position, use the last intervals for the current dimension
                            // , because there is no (optidx + 1) at the last interval
                            if (optidx >= (Ns[idxDim] - 1))
                            {
                                var idxlast = Ns[idxDim] - 1;
                                intvMin = minXs[idxlast][i];
                                intvMax = maxXs[idxlast][i];
                            }
                            // Calculate the minimum and maximum intervals which are adjacent to the current optimized value
                            else
                            {
                                intvMin = Math.Min(
                                    intvMin,
                                    Math.Min(
                                        minXs[optidx][i],
                                        minXs[optidx + 1][i]
                                    )
                                );
                                intvMax = Math.Max(
                                    intvMax,
                                    Math.Max(
                                        maxXs[optidx][i],
                                        maxXs[optidx + 1][i]
                                    )
                                );
                            }*/

                        }
                        optIntv[0][i] = intvMin;
                        optIntv[1][i] = intvMax;
                    }

                    var marginDx = 0.0 * dx;

                    //optIntv[0][idxDim] = double.CreateChecked(opt1D.Optlist.Min(item => item.X)) - marginDx;
                    //if (optIntv[0][idxDim] < 0.0)
                    //{
                    //    optIntv[0][idxDim] = 0.0;
                    //    optIntv[1][idxDim] = 2 * marginDx;
                    //}

                    //optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X)) + marginDx;
                    //if (optIntv[1][idxDim] > 1.0)
                    //{
                    //    optIntv[0][idxDim] = 1.0 - 2 * marginDx;
                    //    optIntv[1][idxDim] = 1.0;
                    //}

                    // Adjust the intervals for the current dimension based on the optimized values and a margin
                    optIntv[0][idxDim] = double.CreateChecked(opt1D.Optlist.Min(item => item.X[0])) - marginDx;
                    //optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X[0])) + marginDx;
                    optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X[1])) + marginDx;

                    if (optIntv[0][idxDim] < 0.0)
                    {
                        optIntv[0][idxDim] = 0.0;
                    }
                    if (optIntv[1][idxDim] > 1.0)
                    {
                        optIntv[1][idxDim] = 1.0;
                    }
                }
            }
            return (opt1D.Optlist, optIntv);

        }
        /* /// <summary>
         /// Asynchronously optimizes a single axis of a multi-dimensional optimization problem.
         /// </summary>
         /// <remarks>This method performs a recursive optimization process for the specified axis of a
         /// multi-dimensional problem.  It uses parallel tasks to optimize sub-problems for higher dimensions and
         /// aggregates the results to compute the optimal values and intervals for the current axis.</remarks>
         /// <param name="samplejtot">An immutable list of sampled values representing the total objective function values across all dimensions.</param>
         /// <param name="idxDim">The index of the dimension (axis) to optimize.</param>
         /// <returns>A tuple containing: <list type="bullet"> <item> <description>An array of tuples, where each tuple contains
         /// the optimized value <typeparamref name="T"/> (<c>X</c>) and its corresponding objective function value
         /// (<c>J</c>) for the specified axis.</description> </item> <item> <description>A two-dimensional array
         /// representing the intervals of the optimized values for the specified axis. The first sub-array contains the
         /// minimum values, and the second sub-array contains the maximum values.</description> </item> </list></returns>
         private async Task<((T[] X, T J)[] Optlist1Axis, double[][] optIntv)> OptimizeMD1AxisAsync(ImmutableList<T> samplejtot, int idxDim)
         {
             Opt1D<T> opt1D;
             var sampleJs = new T[Ns[idxDim]];
             var minXs = new double[Ns[idxDim]][];
             var maxXs = new double[Ns[idxDim]][];
             var dx = 1.0 / Ns[idxDim]; // caution!!! not (1.0 / (Ns[idxDim] - 1.0))
             double[][]? optIntv =
                 [
                     new double[dimN], // minX
             new double[dimN]  // maxX
                 ];

             var axisIdx = idxDim;
             var tasks = new List<Task<((T[] X, T J)[] Optlist1Axis, double[][] optIntv)>>();

             for (uint n = 0; n < Ns[idxDim]; n++)
             {
                 var inputIndex = new uint[dimN];
                 inputIndex[idxDim] = n;

                 if (idxDim == dimN - 1)
                 {
                     sampleJs[n] = samplejtot[(int)GetIdTot(inputIndex)];
                     // optmal value is definitive.
                     if (axisIdx == 0) // No effect for idxDim == dimN - 1, since the minXs and maxXs are not used in that case.
                     {
                         minXs[n] = new double[dimN];
                         maxXs[n] = new double[dimN];
                         minXs[n][idxDim] = dx * (n + 0.5);
                         maxXs[n][idxDim] = dx * (n + 0.5);
                     }
                 }
                 else
                 {
                     AddDispMsg(string.Format("    task: Md1Axis({0,0:G3},{1,0:G3})" + Environment.NewLine, idxDim, n), LogLevel.Info2);

                     var task = pcQ.Enqueue<((T[] X, T J)[] Optlist1Axis, double[][] optIntv)>(
                         () => OptimizeMD1AxisRecur(samplejtot, axisIdx, idxDim + 1, inputIndex)
                     );
                     tasks.Add(task);

                 }
             }
             if (tasks.Count > 0)
             {
                 //var _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                 await Task.WhenAll(tasks).ContinueWith(
                     t =>
                     {
                         if (mode == StaMaskEnum.Min)
                             sampleJs = [.. t.Result.Select(item => item.Optlist1Axis.Min(opt => opt.J))]; // t.Result.Select(item => item.Optlist1Axis.Min(opt => opt.J)).ToArray();
                         else if (mode == StaMaskEnum.Max)
                             sampleJs = [.. t.Result.Select(item => item.Optlist1Axis.Max(opt => opt.J))]; // t.Result.Select(item => item.Optlist1Axis.Max(opt => opt.J)).ToArray();
                         minXs = [.. t.Result.Select(x => x.optIntv[0])]; // t.Result.Select(x => x.optIntv[0]).ToArray();
                         maxXs = [.. t.Result.Select(x => x.optIntv[1])]; // t.Result.Select(x => x.optIntv[1]).ToArray();
                     }
                 // , _uiScheduler
                 );
             }

             OfflineGet<T> constVar;
             lock (_locker)
             {
                 if (lutConstant.TryGetValue(Ns[idxDim], out OfflineGet<T>? value))
                     constVar = value;
                 else
                 {
                     constVar = new OfflineGet<T>(Ns[idxDim]);
                     lutConstant = lutConstant.Add(Ns[idxDim], constVar);
                 }
             }

             if ((axisIdx != 0) && (idxDim == axisIdx))
             {
                 var optIntV1D = new[] { IntvX[0][idxDim], IntvX[1][idxDim] };
                 opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mState, optIntV1D);
                 //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)(mode | StaMaskEnum.PartialConstJ), optIntV1D);
                 //opt1D = new Opt1D<T>(sampleJs, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mode, null);
             }
             else
             {
                 opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mState, null);
                 //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)(mode | StaMaskEnum.PartialConstJ), null);
                 if (axisIdx == 0)
                 {
                     for (int i = idxDim + 1; i < dimN; i++)
                     {
                         var intvMin = double.MaxValue;
                         var intvMax = double.MinValue;

                         foreach (var opt in opt1D.Optlist)
                         {
                             var optidx = (int)(double.CreateChecked(opt.X[0]) / dx);
                             if (optidx >= (Ns[idxDim] - 1))
                             {
                                 var idxlast = Ns[idxDim] - 1;
                                 intvMin = minXs[idxlast][i];
                                 intvMax = maxXs[idxlast][i];
                             }
                             else
                             {
                                 intvMin = Math.Min(
                                     intvMin,
                                     Math.Min(
                                         minXs[optidx][i],
                                         minXs[optidx + 1][i]
                                     )
                                 );
                                 intvMax = Math.Max(
                                     intvMax,
                                     Math.Max(
                                         maxXs[optidx][i],
                                         maxXs[optidx + 1][i]
                                     )
                                 );
                             }

                         }
                         optIntv[0][i] = intvMin;
                         optIntv[1][i] = intvMax;
                     }

                     var marginDx = 0.1 * dx;

                     //optIntv[0][idxDim] = double.CreateChecked(opt1D.Optlist.Min(item => item.X)) - marginDx;
                     //if (optIntv[0][idxDim] < 0.0)
                     //{
                     //    optIntv[0][idxDim] = 0.0;
                     //    optIntv[1][idxDim] = 2 * marginDx;
                     //}

                     //optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X)) + marginDx;
                     //if (optIntv[1][idxDim] > 1.0)
                     //{
                     //    optIntv[0][idxDim] = 1.0 - 2 * marginDx;
                     //    optIntv[1][idxDim] = 1.0;
                     //}

                     // Adjust the intervals for the current dimension based on the optimized values and a margin
                     optIntv[0][idxDim] = double.CreateChecked(opt1D.Optlist.Min(item => item.X[0])) - marginDx;
                     optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X[1])) + marginDx;

                     if (optIntv[0][idxDim] < 0.0)
                     {
                         optIntv[0][idxDim] = 0.0;
                     }
                     if (optIntv[1][idxDim] > 1.0)
                     {
                         optIntv[1][idxDim] = 1.0;
                     }
                 }
             }

             return (opt1D.Optlist, optIntv);

         }*/
        /// <summary>
        /// Optimizes a single axis of a multi-dimensional dataset and returns the results.
        /// </summary>
        /// <remarks>This method performs optimization on a single axis of a multi-dimensional dataset by
        /// invoking a recursive helper method.  The optimization results include both the optimized values and their
        /// associated intervals.</remarks>
        /// <param name="samplejtot">An immutable list of sample data points to be used in the optimization process.</param>
        /// <param name="idx">The index of the axis to optimize.</param>
        /// <returns>A tuple containing: <list type="bullet"> <item> <description>An array of tuples where each tuple contains
        /// the optimized value for the axis (<typeparamref name="T"/>) and its corresponding metric (<typeparamref
        /// name="T"/>).</description> </item> <item> <description>A jagged array of doubles representing the intervals
        /// associated with the optimization process.</description> </item> </list></returns>
        private ((T[] X, T J)[] Optlist1Axis, double[][] optIntv) OptimizeMD1Axis(ImmutableList<T> samplejtot, int idx)
        {
            var inputIndex = new uint[dimN];

            ((T[] X, T J)[] Optlist1Axis, double[][] optIntv) = OptimizeMD1AxisRecur(samplejtot, idx, idx, inputIndex);
            return (Optlist1Axis, optIntv);
        }
        /// <summary>
        /// Recursively optimizes a single axis of a multi-dimensional optimization problem.
        /// </summary>
        /// <remarks>This method performs a recursive optimization process for a single axis of a
        /// multi-dimensional problem. It evaluates the objective function values for the given axis and dimension, and
        /// computes the optimal values and intervals. <para> The recursion continues until the innermost dimension is
        /// reached, at which point the objective function values are directly sampled. For higher dimensions, the
        /// method aggregates results from deeper recursion levels. </para> <para> The method ensures thread safety when
        /// accessing or creating constant lookup tables for optimization calculations. </para></remarks>
        /// <param name="samplejtot">A collection of sample values representing the total objective function values for the optimization process.</param>
        /// <param name="axisIdx">The index of the axis being optimized in the current recursion step.</param>
        /// <param name="idxDim">The current dimension index being processed in the recursion.</param>
        /// <param name="inputIndex">An array representing the indices of the current input values for each dimension.</param>
        /// <returns>A tuple containing: <list type="bullet"> <item> <description> An array of tuples where each tuple contains
        /// the optimized value for the axis (<c>X</c>) and its corresponding objective function value (<c>J</c>).
        /// </description> </item> <item> <description> A two-dimensional array representing the intervals of the
        /// optimization results for each dimension. The first sub-array contains the minimum values, and the second
        /// sub-array contains the maximum values. </description> </item> </list></returns>
        private ((T[] X, T J)[] Optlist1Axis, double[][] optIntv) OptimizeMD1AxisRecur(ImmutableList<T> samplejtot, int axisIdx, int idxDim, uint[] inputIndex)
        {
            Opt1D<T> opt1D;
            var sampleJs = new T[Ns[idxDim]];
            var minXs = new double[Ns[idxDim]][];
            var maxXs = new double[Ns[idxDim]][];
            var dx = 1.0 / Ns[idxDim]; // caution!!! not (1.0 / (Ns[idxDim] - 1.0))
            double[][]? optIntv =
                [
                    new double[dimN], // minX
                    new double[dimN]  // maxX
                ];

            // Recursively sample and optimize the current axis
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputIndex[idxDim] = n; // Set the current index for the dimension being processed
                AddDispMsg(string.Format("        Md1Axis({0,0:G3},{1,0:G3},{2,0:G3})" + Environment.NewLine, axisIdx, idxDim, n), LogLevel.Info3);

                // If at the last dimension, directly sample the objective function value
                if (idxDim == dimN - 1)
                {
                    sampleJs[n] = samplejtot[(int)GetIdTot(inputIndex)];
                    // optmal value is definitive.
                    if (axisIdx == 0) // No effect for idxDim == dimN - 1, since the minXs and maxXs are not used in that case.
                    {
                        minXs[n] = new double[dimN];
                        maxXs[n] = new double[dimN];
                        minXs[n][idxDim] = dx * (n + 0.5);
                        maxXs[n][idxDim] = dx * (n + 0.5);
                    }
                }
                else
                {
                    // AddDispMsg(string.Format("        Md1Axis({0,0:G3},{1,0:G3},{2,0:G3})" + Environment.NewLine, axisIdx, idxDim, n));

                    (var optlist, optIntv) = OptimizeMD1AxisRecur(samplejtot, axisIdx, idxDim + 1, inputIndex);

                    if (mode == StaMaskEnum.Min)
                        sampleJs[n] = optlist.Min(item => item.J);
                    else if (mode == StaMaskEnum.Max)
                        sampleJs[n] = optlist.Max(item => item.J);

                    if (axisIdx == 0)
                    {
                        minXs[n] = optIntv[0];
                        maxXs[n] = optIntv[1];
                    }
                }
            }

            OfflineGet<T> constVar;
            lock (_locker)
            {
                // Check if the constant variable for the current dimension already exists in the lookup table
                if (lutConstant.TryGetValue(Ns[idxDim], out OfflineGet<T>? value))
                    constVar = value;
                // If not, create a new constant variable and add it to the lookup table
                else
                {
                    constVar = new OfflineGet<T>(Ns[idxDim]);
                    lutConstant = lutConstant.Add(Ns[idxDim], constVar);
                }
            }

            // Create an instance of the Opt1D class for the current dimension, using the sampled values and other parameters
            if ((axisIdx != 0) && (idxDim == axisIdx))
            {
                var optIntV1D = new[] { IntvX[0][idxDim], IntvX[1][idxDim] };
                //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)mode, optIntV1D);
                //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)(mode | StaMaskEnum.PartialConstJ), null);
                opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mState, null);
            }
            // If not at the specified axis, create an instance of the Opt1D class without specifying intervals
            else
            {
                //opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, (Opt1D<T>.StaMaskEnum)(mode | StaMaskEnum.PartialConstJ), null);
                opt1D = new Opt1D<T>(sampleJs, TolJReal, Ns[idxDim], minX[idxDim], maxX[idxDim], constVar, mState, null);
                // If at the first axis, calculate the minimum and maximum intervals for the remaining dimensions based on the optimized values
                if (axisIdx == 0)
                {
                    for (int i = idxDim + 1; i < dimN; i++)
                    {
                        var intvMin = double.MaxValue;
                        var intvMax = double.MinValue;

                        foreach (var opt in opt1D.Optlist)
                        {
                            var optidxStart = (int)(double.CreateChecked(opt.X[0]) / dx);
                            if (T.Abs(opt.X[0] - opt.X[1]) < T.CreateChecked(0.0000001)) // just point, not interval
                                CalculateIntvMinMax(i, idxDim, optidxStart, minXs, maxXs, ref intvMin, ref intvMax, false); // opt.X is not at sample point for lower axes
                            else // Interval, not just point
                            {
                                var optidxEnd = (int)(double.CreateChecked(opt.X[1]) / dx);
                                //if (optidxEnd >= minXs.Length)
                                //    optidxEnd = minXs.Length - 1; // for the case that the margin is added in Opt1D.GetSegOfConst()
                                for (int j = optidxStart; j <= optidxEnd; j++)
                                {
                                    CalculateIntvMinMax(i, idxDim, j, minXs, maxXs, ref intvMin, ref intvMax, true); // opt.X is at the exact sampling point for lower axes
                                }
                            }
                            /*// If the optimized index is at the last position, use the last intervals for the current dimension
                            // , because there is no (optidx + 1) at the last interval
                            if (optidx >= (Ns[idxDim] - 1))
                            {
                                var idxlast = Ns[idxDim] - 1;
                                intvMin = minXs[idxlast][i];
                                intvMax = maxXs[idxlast][i];
                            }
                            // Calculate the minimum and maximum intervals which are adjacent to the current optimized value
                            else
                            {
                                intvMin = Math.Min(
                                    intvMin,
                                    Math.Min(
                                        minXs[optidx][i],
                                        minXs[optidx + 1][i]
                                    )
                                );
                                intvMax = Math.Max(
                                    intvMax,
                                    Math.Max(
                                        maxXs[optidx][i],
                                        maxXs[optidx + 1][i]
                                    )
                                );
                            }*/

                        }
                        optIntv[0][i] = intvMin;
                        optIntv[1][i] = intvMax;
                    }

                    var marginDx = 0.0 * dx;

                    //optIntv[0][idxDim] = double.CreateChecked(opt1D.Optlist.Min(item => item.X)) - marginDx;
                    //if (optIntv[0][idxDim] < 0.0)
                    //{
                    //    optIntv[0][idxDim] = 0.0;
                    //    optIntv[1][idxDim] = 2 * marginDx;
                    //}

                    //optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X)) + marginDx;
                    //if (optIntv[1][idxDim] > 1.0)
                    //{
                    //    optIntv[0][idxDim] = 1.0 - 2 * marginDx;
                    //    optIntv[1][idxDim] = 1.0;
                    //}

                    // Adjust the intervals for the current dimension based on the optimized values and a margin
                    optIntv[0][idxDim] = double.CreateChecked(opt1D.Optlist.Min(item => item.X[0])) - marginDx;
                    //optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X[0])) + marginDx;
                    optIntv[1][idxDim] = double.CreateChecked(opt1D.Optlist.Max(item => item.X[1])) + marginDx;

                    if (optIntv[0][idxDim] < 0.0)
                    {
                        optIntv[0][idxDim] = 0.0;
                    }
                    if (optIntv[1][idxDim] > 1.0)
                    {
                        optIntv[1][idxDim] = 1.0;
                    }
                }
            }
            return (opt1D.Optlist, optIntv);

        }
        private void CalculateIntvMinMax(int iDim, int idxDim, int optidx, double[][] minXs, double[][] maxXs, ref double intvMin, ref double intvMax, bool isExactSample)
        {
            // If the optimized index is at the last position, use the last intervals for the current dimension
            // , because there is no (optidx + 1) at the last interval
            if (optidx >= (Ns[idxDim] - 1) && !isExactSample)
            {
                var idxlast = Ns[idxDim] - 1;
                /*intvMin = minXs[idxlast][iDim];
                intvMax = maxXs[idxlast][iDim];*/
                intvMin = Math.Min(intvMin, minXs[idxlast][iDim]);
                intvMax = Math.Max(intvMax, maxXs[idxlast][iDim]);
            }
            // Calculate the minimum and maximum intervals which are adjacent to the current optimized value
            else
            {
                if (!isExactSample)
                {
                    intvMin = Math.Min(
                        intvMin,
                        Math.Min(
                            minXs[optidx][iDim],
                            minXs[optidx + 1][iDim]
                        )
                    );
                    intvMax = Math.Max(
                        intvMax,
                        Math.Max(
                            maxXs[optidx][iDim],
                            maxXs[optidx + 1][iDim]
                        )
                    );
                }
                else
                {
                    intvMin = Math.Min(intvMin, minXs[optidx][iDim]);
                    intvMax = Math.Max(intvMax, maxXs[optidx][iDim]);
                }
            }
        }
        /// <summary>
        /// Recursively generates a sample of values based on the specified dimension index and adds them to the
        /// provided list.
        /// </summary>
        /// <remarks>This method traverses through the specified dimension and computes values based on
        /// the input range and step size. If the current dimension is the last one, the computed value is added to the
        /// list. Otherwise, the method recurses into the next dimension.</remarks>
        /// <param name="idxDim">The current dimension index being processed. Must be within the valid range of dimensions.</param>
        /// <param name="samplejtot">A reference to the list where the generated sample values will be added. Cannot be null.</param>
        public void GetSampleRecur(int idxDim, ref List<T> samplejtot)
        {
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX[idxDim];
                if (idxDim == dimN - 1)
                    samplejtot.Add(J(inputX));
                else
                    GetSampleRecur(idxDim + 1, ref samplejtot);
            }
        }
        private void UpSampleRecur(int idxDim, ref List<T> samplejtot)
        {
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX[idxDim];
                if (idxDim == dimN - 1)
                {
                    samplejtot.Add(J(inputX));
                }
                else
                    UpSampleRecur(idxDim + 1, ref samplejtot);
            }
        }
        private void UpSampleRecur(int idxDim, int[] initIdxOfOldSample,int[] nUpsample, int[] nPreSample, ref List<T> samplejtot, bool isReusable, int[] oldNs)
        {            
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                bool isReusableCurr = isReusable && TestReusable(n, nUpsample[idxDim], nPreSample[idxDim]);
                inputXIdx[idxDim] = (uint)(
                                            ((int)n - nPreSample[idxDim]) / nUpsample[idxDim] + initIdxOfOldSample[idxDim]
                                    );
                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX[idxDim];
                if (idxDim == dimN - 1)
                { 
                    if(IsSetSysFlag(StaMaskEnum.SampleReuse) && isReusableCurr)
                    {
                        samplejtot.Add(sampleJtot[(int)GetIdTot(inputXIdx, oldNs)]);
                        var test = sampleJtot[(int)GetIdTot(inputXIdx, oldNs)];
                        if ( Math.Abs(double.CreateChecked(test) - double.CreateChecked(J(inputX))) > double.CreateChecked(1e-12) )
                        {
                            AddDispMsg(string.Format("UpSampleRecur: sampleJtot[{0,0:G3}]={1,0:G3} != J(inputX)={2,0:G3}" + Environment.NewLine, GetIdTot(inputXIdx, oldNs), test, J(inputX)), LogLevel.Warning);
                        }
                    }                        
                    else
                        samplejtot.Add(J(inputX));
                }
                else
                    UpSampleRecur(idxDim + 1,initIdxOfOldSample, nUpsample, nPreSample, ref samplejtot, isReusableCurr, oldNs);
            }
        }
        private bool TestReusable(uint n, int nUpsample, int nPreSample)
        {
            if(IsSetSysFlag(StaMaskEnum.SampleReuse) == false)
                return false;            
            if ( ( ((int)n - nPreSample) % nUpsample) != 0)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Recursively resamples input data along a specified axis and generates an output sample.
        /// </summary>
        /// <remarks>This method performs a recursive traversal of the input dimensions to resample data
        /// along the specified axis. The resampling process involves applying a linear transformation and a weighted
        /// sum of cosine functions to generate the output sample values.</remarks>
        /// <param name="x">The input value used in the resampling process.</param>
        /// <param name="axisIdx">The index of the axis along which the resampling is performed.</param>
        /// <param name="idxDim">The current dimension index being processed in the recursion.</param>
        /// <param name="inputIdx">A reference to an array representing the current indices for each dimension during the resampling process.</param>
        /// <param name="iSample">A reference to the immutable list of input samples used for resampling.</param>
        /// <param name="oSample">A reference to the list where the resampled output values will be stored.</param>
        private void ReSampleRecur(T x, int axisIdx, int idxDim, ref uint[] inputIdx, ref ImmutableList<T> iSample, ref List<T> oSample)
        {
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                inputIdx[idxDim] = n;
                if (idxDim == dimN - 1)
                {
                    var samplej = new T[Ns[axisIdx - 1]];
                    for (uint i = 0; i < Ns[axisIdx - 1]; i++)
                    {
                        inputIdx[axisIdx - 1] = i;
                        samplej[i] = iSample[(int)GetIdTot(inputIdx)];
                    }
                    //OfflineGet<T> constVars = lutConstant[Ns[idxDim-1]];
                    OfflineGet<T> constVars = lutConstant[Ns[axisIdx - 1]];
                    var w = GeneralMath<T>.LinearTransform(constVars.P.Data, samplej);

                    T sum = T.Zero;
                    for (uint i = 0; i < w.Length; i++)
                        sum += w[i] * T.Cos( T.CreateChecked(i) * T.Pi * x);
                    oSample.Add(sum);
                }
                else
                    ReSampleRecur(x, axisIdx, idxDim + 1, ref inputIdx, ref iSample, ref oSample);
            }
        }
        /*/// <summary>
        /// Recursively generates upsampled data points for a multidimensional grid.
        /// </summary>
        /// <remarks>This method processes each dimension of a multidimensional grid recursively,
        /// generating upsampled data points based on the provided parameters.  For the final dimension, it either adds
        /// precomputed values or computes new values based on the input.  The method ensures that the state of
        /// <paramref name="inputIndex"/> is restored after processing each dimension.</remarks>
        /// <param name="idxDim">The current dimension index being processed.</param>
        /// <param name="samplejtot">A reference to the list where the upsampled data points will be stored.</param>
        /// <param name="isEven">A boolean value indicating whether the current level of recursion corresponds to an even index.</param>
        /// <param name="inputIndex">An array of indices representing the current position in the multidimensional grid. This parameter is passed
        /// by reference to track updates during recursion.</param>
        private void UpSample(int idxDim, ref List<T> samplejtot, bool isEven, in uint[] inputIndex, in uint[] oldNs)
        {
            uint tmpInputIdx = inputIndex[idxDim];
            for (uint n = 0; n < Ns[idxDim]; n++)
            {
                bool iseven = isEven && (n % 2 == 0);

                inputX[idxDim] = minX[idxDim] + T.CreateChecked(n) * dX[idxDim];
                if (idxDim == dimN - 1)
                {
                    if (iseven)
                    {
                        samplejtot.Add(sampleJtot[(int)GetIdTot(inputIndex, oldNs)]);
                        inputIndex[idxDim]++;
                        //samplejtot.Add(J(inputX));
                    }
                    else
                        samplejtot.Add(J(inputX));
                }
                else
                {
                    UpSample(idxDim + 1, ref samplejtot, iseven, inputIndex, oldNs);
                    if (iseven)
                        inputIndex[idxDim]++;
                }

            }
            inputIndex[idxDim] = tmpInputIdx;
        }*/
        /// <summary>
        /// get an index for the sampleJtot array from the dimN dimensional inputIndex array.
        /// resample() makes the dimension to decrease by 1 and reduces the size of sampleJtot array,
        /// but, the index of the reduced demension, which is assumed to be zero, does no effect on the calculation.
        /// then, use the same getIdTot() method without any modification even after resample().
        /// </summary>
        /// <param name="inputIndex"></param>
        /// <returns></returns>
        private uint GetIdTot(in uint[] inputIndex)
        {
            /*uint idTot = inputIndex[0];
            for (uint i = 1; i < dimN; i++)
            {
                uint accumNs = 1;
                for (uint j = 0; j < i; j++)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }*/
            uint idTot = inputIndex[dimN - 1];
            for (int i = (int)dimN - 2; i >= 0; i--)
            {
                uint accumNs = 1;
                for (int j = (int)dimN - 1; j > i; j--)
                    accumNs *= Ns[j];
                idTot += inputIndex[i] * accumNs;
            }

            return idTot;
        }
        private uint GetIdTot(in uint[] inputIndex, in int[] ns)
        {
            uint idTot = inputIndex[dimN - 1];
            for (int i = (int)dimN - 2; i >= 0; i--)
            {
                uint accumNs = 1;
                for (int j = (int)dimN - 1; j > i; j--)
                    accumNs *= (uint)ns[j];
                idTot += inputIndex[i] * accumNs;
            }

            return idTot;
        }
        private uint GetIdTot(in uint[] inputIndex, in uint[] ns)
        {
            uint idTot = inputIndex[dimN - 1];
            for (int i = (int)dimN - 2; i >= 0; i--)
            {
                uint accumNs = 1;
                for (int j = (int)dimN - 1; j > i; j--)
                    accumNs *= ns[j];
                idTot += inputIndex[i] * accumNs;
            }

            return idTot;
        }
        /// <summary>
        /// Adjusts the values in the <see cref="optResult"/> collection by converting them to their real-world
        /// equivalents.
        /// </summary>
        /// <remarks>This method iterates through each element in the <see cref="optResult"/> collection
        /// and updates its values using the <see cref="CalcRealX"/> method. The conversion is performed for each
        /// dimension specified by <see cref="dimN"/>.</remarks>
        private void CalResultReal()
        {
            optResultReal = new (T[][] X, T J)[optResult.Length];
            for (int i = 0; i < optResult.Length; i++) // Iterate over the array of tuples
            {
                //optResultReal[i] = (new T[dimN][], optResult[i].J);
                optResultReal[i] = (
                                        Enumerable.Range(0, (int)dimN)
                                                      .Select(_ => new T[] { T.Zero, T.Zero })
                                                      .ToArray(),
                                        optResult[i].J
                                    );

                for (int j = 0; j < dimN; j++) // Iterate over the dimensions
                {
                    optResultReal[i].X[j][0] = CalcRealX(optResult[i].X[j][0], j); // Access the tuple's X array
                    optResultReal[i].X[j][1] = CalcRealX(optResult[i].X[j][1], j); // Access the tuple's X array
                }
            }
        }
        /// <summary>
        /// Projects the optimization results onto a new coordinate system.
        /// </summary>
        /// <remarks>This method transforms the optimization results stored in <see cref="optResultReal"/>
        /// by applying a projection function to each dimension of the result vectors.  The transformed results are
        /// stored in the <see cref="optResult"/> array.</remarks>
        private void CalResultPrj()
        {
            if(optResultReal.Length >= 3) // optResultReal contains the centroid as the last element, so we exclude it from the projection
                optResult = new (T[][] X, T J)[optResultReal.Length - 1];
            else
                optResult = new (T[][] X, T J)[optResultReal.Length];
            for (int i = 0; i < optResult.Length; i++) // Iterate over the array of tuples
            {
                //optResult[i] = (new T[dimN][], optResultReal[i].J);
                optResult[i] = (
                                    Enumerable.Range(0, (int)dimN)
                                                      .Select(_ => new T[] { T.Zero, T.Zero })
                                                      .ToArray(),
                                    optResultReal[i].J
                                );
                for (int j = 0; j < dimN; j++) // Iterate over the dimensions
                {
                    optResult[i].X[j][0] = CalcProjX(optResultReal[i].X[j][0], j); // Access the tuple's X array
                    optResult[i].X[j][1] = CalcProjX(optResultReal[i].X[j][1], j); // Access the tuple's X array
                }
            }
        }
        /// <summary>
        /// Calculates the real-world coordinate along a specified dimension based on a projected coordinate.
        /// </summary>
        /// <param name="projX">The projected coordinate value, typically normalized or scaled.</param>
        /// <param name="idxDim">The index of the dimension for which the real-world coordinate is calculated.</param>
        /// <returns>The real-world coordinate corresponding to the given projected coordinate along the specified dimension.</returns>
        public T CalcRealX(in T projX, in int idxDim) => (maxX[idxDim] - minX[idxDim] + dX[idxDim]) * projX + minX[idxDim] - dXHalf[idxDim];
        /// <summary>
        /// Calculates the projected value of a given coordinate along a specified dimension.
        /// </summary>
        /// <remarks>The projection is calculated as a normalized value based on the minimum and maximum
        /// bounds of the specified dimension.</remarks>
        /// <param name="realX">The real-world coordinate value to be projected.</param>
        /// <param name="idxDim">The index of the dimension along which the projection is calculated.</param>
        /// <returns>The projected value of <paramref name="realX"/> as a normalized ratio within the range of the specified
        /// dimension.</returns>
        public T CalcProjX(in T realX, in int idxDim) => (realX - minX[idxDim] + dXHalf[idxDim]) / (maxX[idxDim] - minX[idxDim] + dX[idxDim]);
        
        /// <summary>
        /// Sets the specified system flag in the current state.
        /// </summary>
        /// <remarks>This method modifies the internal state by enabling the specified flag. The operation
        /// is performed in-place.</remarks>
        /// <param name="mask">The flag to set, represented as a value from the <see cref="StaMaskEnum"/> enumeration.</param>
        public void SetSysFlag(in StaMaskEnum mask)
        {
            mState = GeneralMath<T>.SetFlag(ref mState, (UInt32)mask);

        }
        /// <summary>
        /// Clears the specified system flag in the current state.
        /// </summary>
        /// <remarks>This method modifies the internal state by clearing the bit corresponding to the
        /// specified flag.</remarks>
        /// <param name="mask">The flag to clear, represented as a value from the <see cref="StaMaskEnum"/> enumeration.</param>
        public void ClrSysFlag(in StaMaskEnum mask)
        {
            mState = GeneralMath<T>.ClrFlag(ref mState, (UInt32)mask);
        }
        /// <summary>
        /// Determines whether the specified system flag is set in the current state.
        /// </summary>
        /// <param name="mask">The system flag to check, represented as a <see cref="StaMaskEnum"/> value.</param>
        /// <returns><see langword="true"/> if the specified system flag is set; otherwise, <see langword="false"/>.</returns>
        public bool IsSetSysFlag(in StaMaskEnum mask)
        {
            return GeneralMath<T>.IsSetFlag(mState, (UInt32)mask);
        }

        public enum LogLevel
        {
            None,
            Error,
            Warning,
            Info0,
            Info1,
            Info2,
            Info3,
            Debug,
            Verbose
        }
        private LogLevel currentLevel = LogLevel.Info0; // 기본값

        public LogLevel CurrentLevel
        {
            get { return currentLevel; }
            set
            {
                // 필요하다면 유효성 검사나 이벤트 트리거 가능
                currentLevel = value;
            }
        }
        /// <summary>
        /// Adds a message to the display message queue if the verbose system flag is set.
        /// </summary>
        /// <remarks>The message is added to the queue only if the verbose system flag is enabled, as
        /// determined by  <see cref="isSetSysFlag(StaMaskEnum)"/>. If <paramref name="msg"/> is null or empty, the
        /// method does nothing.</remarks>
        /// <param name="msg">The message to enqueue. Must not be null or empty.</param>
        public void AddDispMsg(string msg, LogLevel level)
        {
            if (level <= CurrentLevel)
            {
                AddDispMsg(msg);
            }
        }
        public void AddDispMsg(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                DisplayMsgQueue.Enqueue(msg);
            }

        }
        /// <summary>
        /// Retrieves a message from the display message queue if the verbose system flag is set.
        /// </summary>
        /// <remarks>This method checks whether the verbose system flag is enabled before attempting to
        /// dequeue a message.  If no message is available or the verbose flag is not set, it returns an empty
        /// string.</remarks>
        /// <returns>The next message in the display message queue if the verbose system flag is set and a message is available; 
        /// otherwise, an empty string.</returns>
        public string GetDispMsg()
        {
            //if (IsSetSysFlag(StaMaskEnum.Verbose) && DisplayMsgQueue.TryDequeue(out string? msg))
            if (DisplayMsgQueue.TryDequeue(out string? msg))

                return msg;
            else
                return string.Empty;
        }
        /// <summary>
        /// Generates a formatted message summarizing the optimum results.
        /// </summary>
        /// <remarks>The message includes the optimum value and its corresponding parameters for each
        /// result in the collection. Each result is formatted as a line containing the optimum value followed by the
        /// parameter values.</remarks>
        /// <returns>A string containing the formatted message. Each line represents an optimum result,  with the optimum value
        /// and its associated parameters separated by spaces.</returns>
        public string ResultMsg(int maxIdx)
        {
            string msgOptX = "";
            string msgOptX1 = string.Empty;
            
            var numItem = 0;

            if(optResultReal.Length > 1)
                numItem = optResultReal.Length - 1; // Exclude the last element, which is the centroid
            else
                numItem = optResultReal.Length;

            /*foreach (var opt in optResultReal)
            {
                if( (j==0) || (j!= 0 && j != optResultReal.Length - 1))
                {
                    msgOptX1 = "Optimum(";
                    msgOptX1 += string.Format("{0,1:G3}) ", j);
                }
                else
                {
                    msgOptX1 = "Centroid(";
                    msgOptX1 += string.Format("{0,1:G3})  ", maxIdx);
                }

                if (!IsSetSysFlag(StaMaskEnum.SampledOptimal))
                {
                    msgOptX1 += string.Format("{0,8:G5}    at ", opt.J);
                }
                else
                {
                    msgOptX1 += string.Format("{0,8:G5} ({1,8:G5})    at ", opt.J, J(opt.X));
                }

                for (int i = 0; i < dimN; i++)
                {
                    msgOptX1 += string.Format("{0,8:G5} ", opt.X[i]);
                }
                msgOptX += msgOptX1 + Environment.NewLine;
                j++;
            }*/
            for (int j = 0; j < numItem; j++)
            {
                var opt = optResultReal[j];
                
                msgOptX1 = "Optimum(";
                msgOptX1 += string.Format("{0,1:G3}) ", j);
                

                if (!IsSetSysFlag(StaMaskEnum.SampledOptimal))
                {
                    msgOptX1 += string.Format("J:{0,8:G7} Err:{1,8:E2}   at ",
                                                    opt.J, CalcErr( OptParam.Optima[0].J, opt.J)
                                                );
                }
                else
                {
                    T[] tmpx = [.. opt.X.Select(row => row[0])];
                    var trueJ = J(tmpx); Nfe--;
                    msgOptX1 += string.Format("J:{0,8:G7} Err:{1,8:E2} (realJ: {2,8:G7} realErr:{3,8:E2})    at ",
                                                    opt.J, CalcErr(OptParam.Optima[0].J, opt.J),
                                                    trueJ, CalcErr(OptParam.Optima[0].J, trueJ)
                                                );
                    
                }

                for (int i = 0; i < dimN; i++)
                {
                    //if (!IsSetSysFlag(StaMaskEnum.PartialConstJ))
                    if (opt.X[i][0] == opt.X[i][1])
                    {
                        msgOptX1 += string.Format("{0,8:G7} ", opt.X[i][0]);
                    }
                    else
                    {                        
                        msgOptX1 += string.Format("({0,8:G7} {1,8:G7}) ", opt.X[i][0], opt.X[i][1]);
                    }
                }
                msgOptX += msgOptX1 + Environment.NewLine;
            }

            //DisplayMsgFunc(msgOptX);
            return msgOptX;
        }

        private T CalcErr(T TargetJ, T J)
        {
            return (T.Abs(TargetJ - J));
        }
    }
}
