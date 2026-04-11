//=====================================================================
// 模块名称：料层 DotPileLayer
// 功能简介：单一料层的属性和行为
// 版权声明：2022 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2022.01.27 杜子兮 创建 对应版本 0.7.5
//          2024.03.01 杜子兮 添加料层类型
//============================================


using System.Collections.Generic;

namespace DotCloudLib
{
    /// <summary>
    /// 料层
    /// </summary>
    public class DotPileLayer
    {

        /// <summary>
        /// 层类型
        /// </summary>
        public enum LayerType
        {
            /// <summary>
            /// 未知
            /// </summary>
            UN_KNOWN = 0,

            /// <summary>
            /// 堆料层（料型中增加的部分）
            /// </summary>
            STACK = 1,

            /// <summary>
            /// 取料层（料型中减少的部分）
            /// </summary>
            TAKE = 2,

            /// <summary>
            /// 底层（去掉料层剩余部分）
            /// </summary>
            BASE = 3,
        }

        /// <summary>
        /// 默认情况下，空点赋值为-100
        /// </summary>
        public float p_nullZ = -100;

        /// <summary>
        /// 格网精度
        /// </summary>
        public float Unit { get; set; }

        /// <summary>
        /// 上表面(格网点云)
        /// </summary>
        public DotPile TopSurface { get; set; }

        /// <summary>
        /// 下表面(格网点云)
        /// </summary>
        public DotPile BottomSurface { get; set; }

        /// <summary>
        /// AABB包围盒
        /// </summary>
        public ZXBoundary Bound { get; set; }

        /// <summary>
        /// 料层类型
        /// </summary>
        public LayerType Type { get; set; } = LayerType.UN_KNOWN;

        /// <summary>
        /// 构造方法——空
        /// </summary>
        public DotPileLayer()
        { 

        }

        /// <summary>
        /// 构造方法——一个料堆就是一层
        /// </summary>
        /// <param name="pile">料堆</param>
        public DotPileLayer(DotPile pile)
        {
            this.Unit = pile.Unit;
            this.Bound = pile.Bound;
            this.TopSurface = new DotPile(pile);
            this.BottomSurface = new DotPile(this.Bound, this.Unit);
        }

        /// <summary>
        /// 构造方法——已知：格网精度、上下表面格网点云
        /// </summary>
        /// <param name="p_unit">格网精度</param>
        /// <param name="p_topSurface">上表面点云</param>
        /// <param name="p_bottomSurface">下表面点云</param>
        public DotPileLayer(float p_unit, ZXPointSet p_topSurface, ZXPointSet p_bottomSurface)
        {
            this.Unit = p_unit;
            this.TopSurface = new DotPile(p_topSurface);
            this.BottomSurface = new DotPile(p_bottomSurface);
            this.Bound = p_topSurface.Bound + p_bottomSurface.Bound;

            TurnToFull();
        }

        /// <summary>
        /// 构造方法—— 构造【堆料层】或【取料层】
        /// 【堆料层】为实体层；【取料层】为虚拟逻辑空间层
        /// 当构造【堆料层】时，第一个参数为堆料后的料型，第二个参数为堆料前的料型
        /// 当构造【取料层】时，第一个参数为取料前的料型，第二个参数为取料后的料型
        /// 创建版本：0.7.6
        /// </summary>
        /// <param name="p_topSurface">上表面料型</param>
        /// <param name="p_bottomSurface">下表面料型</param>
        /// <param name="p_deltaH">上下表面之间的最小高差（默认：0.1米）</param>
        /// <param name="p_turnToFull">默认转为满点阵，空点不填充需参数指定</param>
        public DotPileLayer(DotPile p_topSurface, DotPile p_bottomSurface, float p_deltaH = 0.1f, bool p_turnToFull = true)
        {
            // STEP 0: 参数初始化
            this.TopSurface = new DotPile(p_topSurface);       // 拷贝传参  2022.07.30  原：引用传参
            this.BottomSurface = new DotPile(p_bottomSurface); // 拷贝传参  2022.07.30  原：引用传参
            this.Unit = this.TopSurface.Unit;
            this.Bound = this.TopSurface.Bound + this.BottomSurface.Bound;

            int xN = 0;

            // STEP 1: 根据堆后料型，剪切堆前料型，获取真实上表面
            for (float x = this.Bound.MinX; x <= this.Bound.MaxX; x += this.Unit)
            {
                x = this.Bound.MinX + xN++ * this.Unit;
                int yN = 0;

                for (float y = this.Bound.MinY; y <= this.Bound.MaxY; y += this.Unit)
                {
                    y = this.Bound.MinY + yN++ * this.Unit;

                    int indexA = TopSurface.GetIndex(x, y);
                    int indexB = BottomSurface.GetIndex(x, y);
                    if (indexA != -1 && indexB != -1)
                    {
                        if (TopSurface.Points[indexA].Z < BottomSurface.Points[indexB].Z + p_deltaH) // 可调参数
                        {
                            TopSurface.Points[indexA].Z = float.MinValue;
                            BottomSurface.Points[indexB].Z = float.MinValue;
                        }
                    }
                }
            }

            TopSurface.Points.Intercept(this.Bound); // 切掉空点
            BottomSurface.Points.Intercept(this.Bound); // 切掉空点

            // STEP 2: 根据上表面，切割下表面
            ZXBoundary b = TopSurface.Points.Boundary;
            b.MinZ = float.MinValue;
            BottomSurface.Points.Intercept(b);

            this.Bound = TopSurface.Points.Boundary + BottomSurface.Points.Boundary;

            // 2022.2.31 上下表面边界根据计算结果更新，取并集
            TopSurface.Bound = this.Bound;
            BottomSurface.Bound = this.Bound;

            if (p_turnToFull)
            {
                TurnToFull(); // 2022.07.30 改可配置参数
            }
        }

        /// <summary>
        /// 根据一个指定高度，将一个料堆切割为上下两层
        /// </summary>
        /// <param name="p_pile">原始料堆</param>
        /// <param name="p_height">指定切割高度</param>
        /// <returns>上下两个料层；若指定高度不在料堆范围内，直接将料堆作为一个料层返回</returns>
        public static List<DotPileLayer> SplitPileByHeight(DotPile p_pile, float p_height)
        {
            List<DotPileLayer> layers = new List<DotPileLayer>();

            // STEP 1: 参数校验，剔除范围外切割高度
            ZXBoundary bROI = p_pile.Bound;

            if (p_height <= bROI.MinZ || p_height >= bROI.MaxZ)
            {
                layers.Add(new DotPileLayer(p_pile));
                return layers;  // 若指定高度不在料堆范围内，直接将料堆作为一个料层返回
            }

            // STEP 2: 构造料层下表面
            DotPile bottomSurface0 = new DotPile(bROI, bROI.MinZ, p_pile.Unit); // 下层
            DotPile bottomSurface1 = new DotPile(bROI, p_height, p_pile.Unit);  // 上层

            // STEP 3: 构造料层上表面
            DotPile topSurface0 = new DotPile(p_pile);
            DotPile topSurface1 = new DotPile(p_pile);

            for (int i = 0; i < p_pile.Points.Count; i++)
            {
                if (topSurface0.Points[i].Z > p_height)
                    topSurface0.Points[i].Z = p_height;

                if (topSurface1.Points[i].Z < p_height)
                    topSurface1.Points[i].Z = p_height;
            }

            // STEP 4: 根据上下表面构造料层
            layers.Add(new DotPileLayer(topSurface0, bottomSurface0, 0.01f));
            layers.Add(new DotPileLayer(topSurface1, bottomSurface1, 0.01f));

            return layers;
        }

        /// <summary>
        /// 根据若干指定高度，将一个料堆切割为多个料层
        /// 2022.07.29 追加
        /// </summary>
        /// <param name="p_pile">原始料堆</param>
        /// <param name="p_heights">指定切割高度</param>
        /// <returns></returns>
        public static List<DotPileLayer> SplitPileByHeights(DotPile p_pile, List<float> p_heights)
        {
            List<DotPileLayer> layers = new List<DotPileLayer>();

            // STEP 1: 参数校验，剔除范围外切割高度
            ZXBoundary bROI = p_pile.Points.Boundary;
            for (int i = p_heights.Count - 1; i >= 0; i--)
            {
                if (p_heights[i] <= bROI.MinZ || p_heights[i] >= bROI.MaxZ)
                {
                    p_heights.RemoveAt(i);
                }
            }

            // 无切割时，整个料堆作为一个料层返回
            if (p_heights.Count == 0)
            {
                layers.Add(new DotPileLayer(p_pile));
            }

            // STEP 1: 输入高度从大到小排序
            p_heights.Sort();

            DotPile leftPile = new DotPile(p_pile);  // 剩余切割料堆

            // STEP 2: 遍历切割高度
            for (int i = 0; i < p_heights.Count; i++)
            {
                List<DotPileLayer> cutLayers = SplitPileByHeight(leftPile, p_heights[i]);

                if (cutLayers.Count == 2)
                {
                    // 成功切割
                    layers.Add(cutLayers[0]);  // 添加下层
                    leftPile = cutLayers[1].TopSurface;  // 上层为 剩余切割料堆
                    // 存在空点，边界计算有问题
                    // leftPile.Bound = leftPile.Points.Boundary;  // 重新计算边界
                    leftPile.Bound.MinZ = p_heights[i]; // 每次切割，剩余切割料堆只有下边界在变

                    if (i == p_heights.Count - 1)
                    {
                        layers.Add(cutLayers[1]);
                    }
                }
            }

            return layers;
        }


        /// <summary>
        /// 计算料层体积
        /// </summary>
        public float GetVolume(float p_deltaH = 0.01f)
        {
            float v = 0;
            int xN = 0;

            // STEP 1: 格网遍历所有点
            for (float x = this.Bound.MinX; x <= this.Bound.MaxX; x += this.Unit)
            {
                x = this.Bound.MinX + xN++ * this.Unit;
                int yN = 0;

                for (float y = this.Bound.MinY; y <= this.Bound.MaxY; y += this.Unit)
                {
                    y = this.Bound.MinY + yN++ * this.Unit;

                    int indexA = TopSurface.GetIndex(x, y);
                    int indexB = BottomSurface.GetIndex(x, y);
                    if (indexA != -1 && indexB != -1)
                    {
                        // STEP 2: 计算高差
                        float h = TopSurface.Points[indexA].Z - BottomSurface.Points[indexB].Z;
                        LibTool.Debug(h);

                        // STEP 3: 累加体积
                        if (h > p_deltaH)
                        {
                            v += h * (this.Unit * this.Unit);
                        }
                    }
                }
            }

            return v;
        }


        /// <summary>
        /// 上下表面转化为满秩阵标准型
        /// </summary>
        private void TurnToFull()
        {
            ZXPointSet psFullT = new ZXPointSet();
            TopSurface.Points.TurnToFull(ref psFullT, p_nullZ);
            TopSurface.Points = psFullT;

            ZXPointSet psFullB = new ZXPointSet();
            BottomSurface.Points.TurnToFull(ref psFullB, p_nullZ);
            BottomSurface.Points = psFullB;
        }
    }
}
