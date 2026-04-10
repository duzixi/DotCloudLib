/*
 * 模块名称：料堆基础类
 * 功能简介：料堆基础类 - 包含各种形态料场下料堆的通用操作方法
 * 版权声明：2022 锐创理工科技有限公司  All Rights Reserved.
 * 更新履历：2022.06.30  Sanngoku 创建
 *          2022.11.23  王振宇    添加圆形料场分区体积计算方法 GetVolume(float p_angle1, float p_angle2)
 */

using Mathd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace DotCloudLib
{
    /// <summary>
    /// 料堆基础类
    /// </summary>
    public abstract class DotPileBase
    {
        /// <summary>
        /// 根据有序点云构造
        /// </summary>
        /// <param name="ps">有序格网点云</param>
        public DotPileBase(ZXPointSet ps)
        {
            // STEP 0: 校验
            if (ps.Count < 3)
            {
                LibTool.Error("DotPileBase34: 点数过少，料堆构造失败");
            }

            // STEP 0.2: 校验有序性
            m_unit = ps.Unit;
            Points = ps;
            Bound = ps.Boundary; // 重新计算边界
            Points.Bound = Bound; // 2022.7.21
        }

        public DotPileBase() { }

        /// <summary>
        /// 存储点集
        /// </summary>
        public virtual ZXPointSet Points { get; set; }

        /// <summary>
        /// 料型边界
        /// </summary>
        protected ZXBoundary m_bound;

        /// <summary>
        /// 边界
        /// </summary>
        public ZXBoundary Bound;

        /// <summary>
        /// 料堆格网精度
        /// </summary>
        protected float m_unit;

        /// <summary>
        /// 格网精度
        /// </summary>
        public float Unit
        {
            get
            {
                return m_unit;
            }

            // 2023.09.25 加
            set 
            { 
                m_unit = value;
            }
        }

        /// <summary>
        /// 默认基准面高度
        /// </summary>
        public float Z0 = 0;

        /// <summary>
        /// X方向点数
        /// </summary>
        public virtual int LengthN
        {
            get { return this.Points.LengthN; }
        }

        /// <summary>
        /// Y方向点数
        /// </summary>
        public virtual int WidthN
        {
            get { return this.Points.WidthN; }
        }

        /// <summary>
        /// 料堆安息角(单位：度)
        /// </summary>
        public float Alfa = 40;

        /// <summary>
        /// 体积
        /// </summary>
        public virtual double Volume
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < this.Points.Count; i++)
                {
                    if (this.Points[i].Z > this.Z0)  // 2023.06.27
                    {
                        sum += (this.Points[i].Z - this.Z0) * (Unit * Unit);
                    }
                }
                return sum;
            }
        }

        /// <summary>
        /// 圆形料场根据指定角度计算体积  注：右侧X轴正向为0°, 上侧Y轴正向为0°  角度逆时针递增
        /// </summary>
        /// <param name="p_angle1">起始角度</param>
        /// <param name="p_angle2">终止角度</param>
        /// <returns></returns>
        public double GetVolume(float p_angle1, float p_angle2)
        {
            
            if (Math.Abs(p_angle1 - p_angle2) < 0.0001f)
            {
                return 0;
            }

            if (p_angle1 > p_angle2)
            {
                float temp = p_angle1;
                p_angle1 = p_angle2;
                p_angle2 = temp;
            }

            int n = (int)(Math.Abs(p_angle1) / 360) + 1;
            p_angle1 = (p_angle1 + n * 360) % 360;
            n = (int)(Math.Abs(p_angle2) / 360) + 1;
            p_angle2 = p_angle2 == 360 ? 360 : (p_angle2 + n * 360) % 360;


            if (p_angle1 > p_angle2)
            {
                return GetVolume(p_angle1, 360) + GetVolume(0, p_angle2);
            }

            Vector3d origin = new Vector3d(Bound.Center.X, Bound.Center.Y, 0);

            Func<Vector3d, bool> ComputeAngle = (v2) => {
                double angle = Vector3d.Angle(v2, Vector3d.right);
                if (Vector3d.Cross(Vector3d.right, v2).z < 0)
                    angle = 360 - angle;

                return angle > p_angle1 && angle < p_angle2;
            };

            double sum = 0;
            for (int i = 0; i < this.Points.Count; i++)
            {
                if (ComputeAngle(new Vector3d(Points[i].X, Points[i].Y, 0) - origin))
                    sum += (this.Points[i].Z - this.Z0) * (Unit * Unit);
            }
            return sum;
        }

        /// <summary>
        /// 初始化料堆，高度可指定，默认为0
        /// </summary>
        public void Init(float p_z = 0)
        {
            this.Points.Unit = this.Unit;
            this.Points.Clear();

            for (int i = 0; i < LengthN; i++)
            {
                for (int j = 0; j < WidthN; j++)
                {
                    float x = Bound.MinX + i * Unit;
                    float y = Bound.MinY + j * Unit;
                    Points.Add(x, y, p_z);
                }
            }

            this.Bound = Points.Boundary;  // 重新计算边界
        }

        /// <summary>
        /// 高程值数组初始化堆料
        /// </summary>
        /// <param name="p_heights">高程值数组</param>
        public bool Init(float[] p_heights)
        {
            if (LengthN * WidthN != p_heights.Length)
            {
                LibTool.Error("DotPile322: 高程数组长度与料格长宽不一致，初始化失败");
                return false;
            }

            Points.Clear();

            for (int i = 0; i < LengthN; i++)
            {
                for (int j = 0; j < WidthN; j++)
                {
                    int index = i * (int)WidthN + j;
                    float x = Bound.MinX + i * Unit;
                    float y = Bound.MinX + j * Unit;
                    Points.Add(x, y, p_heights[index]);
                }
            }

            this.Bound = Points.Boundary; // 重新计算边界
            return true;
        }

        #region 保存为文件格式

        /// <summary>
        /// 保存为XYZ格式
        /// </summary>
        /// <param name="p_filePath">保存路径</param>
        /// <param name="p_minZ">最低点Z值</param>
        public void SaveAsXYZ(string p_filePath, float p_minZ = -100)
        {
            this.Points.SaveAsXYZ(p_filePath, p_minZ);
        }

        /// <summary>
        /// 保存为IXYZ格式
        /// </summary>
        /// <param name="p_filePath"></param>
        public void SaveAsIXYZ(string p_filePath)
        {
            this.Points.SaveAsIXYZ(p_filePath);
        }

        /// <summary>
        /// 保存为JavaScript对象变量
        /// </summary>
        /// <param name="p_filePath"></param>
        public void SaveAsJSObject(string p_filePath)
        {
            this.Points.SaveAsJSObject(p_filePath);
        }

        #endregion

        #region 高级属性方法

        /// <summary>
        /// 获取高程一维数组float[]（对接Unity3D）
        /// </summary>
        /// <returns>如果大小不匹配，返回空数组</returns>
        public float[] GetHeights()
        {
            float[] heights = new float[LengthN * WidthN];

            if (LengthN * WidthN != Points.Count)
            {
                ArithmeticException ex = new ArithmeticException();
                LibTool.Error(ex, "DotPileBase@242",
                    "料堆点云不是满秩阵，不支持高程获取方法。LengthN " + LengthN + " * WidthN " + WidthN + " != Points.Count " + Points.Count);
            }

            for (int i = 0; i < heights.Length; i++)
            {
                heights[i] = Points[i].Z;
            }

            return heights;
        }

        /// <summary>
        /// 获取高程二维数组float[,]（对接Unity3D）
        /// </summary>
        /// <returns>如果大小不匹配，返回空数组</returns>
        public float[,] GetHeights2D()
        {
            float[,] heights = new float[LengthN, WidthN];

            if (LengthN * WidthN != Points.Count)
            {
                ArithmeticException ex = new ArithmeticException();
                LibTool.Error(ex, "DotPileBase@265",
                    "料堆点云不是满秩阵，不支持高程获取方法。LengthN " + LengthN + " * WidthN " + WidthN + " != Points.Count " + Points.Count);
            }

            for (int i = 0; i < LengthN; i++)
            {
                for (int j = 0; j < WidthN; j++)
                {
                    int index = GetIndex(i, j);
                    if (index == -1)
                    {
                        ArithmeticException ex = new ArithmeticException();
                        LibTool.Error(ex, "DotPileBase@242",
                            "下标越界：index = " + index + "  i = " + i + "  j = " + j);
                    }
                    heights[i, j] = Points[index].Z;
                }
            }

            return heights;
        }

        /// <summary>
        /// 获取高程字典 [index, height] (对接Unity3D)
        /// </summary>
        /// <param name="p_ROI">限定范围</param>
        /// <returns></returns>
        public Dictionary<int, float> GetHeightsDic(ZXBoundary p_ROI)
        {
            Dictionary<int, float> heights = new Dictionary<int, float>() { };

            ZXBoundary bPile = this.Bound;

            ZXPointSet ps = this.Points;
            for (int i = 0; i < ps.Count; i++)
            {
                if (p_ROI.ContainXY(ps[i]) && bPile.ContainXY(ps[i]))
                {
                    heights.Add(i, ps[i].Z);
                }
            }

            return heights;
        }

        /// <summary>
        /// 设置高程字典 [index, height]
        /// </summary>
        /// <param name="p_points"></param>
        public void SetHeightsDic(Dictionary<int, float> p_points)
        {
            foreach (int index in p_points.Keys)
            {
                if (index >= 0 && index < this.Points.Count)
                {
                    this.Points[index].Z = p_points[index];
                }

            }
        }

        /// <summary>
        /// 根据二维下标计算点下标 2021.1.25 by dzx
        /// </summary>
        /// <param name="p_i"></param>
        /// <param name="p_j"></param>
        /// <returns></returns>
        public int GetIndex(int p_i, int p_j)
        {
            // STEP 0: 参数校验
            if (p_i < 0 || p_i >= LengthN)
            {
                return -1; // 2023.05.12
            }

            if (p_j < 0 || p_j >= WidthN)
            {
                return -1;
            }

            // STEP 1: 返回计算结果
            return p_i * (int)WidthN + p_j;
        }

        /// <summary>
        /// 根据点XY坐标获取格网索引
        /// </summary>
        /// <param name="p_x"></param>
        /// <param name="p_y"></param>
        /// <returns>未找到返回-1</returns>
        public virtual int GetIndex(float p_x, float p_y)
        {
            int index = -1;

            // 越界校验
            if (p_x > Bound.MaxX || p_y > Bound.MaxY ||
                p_x < Bound.MinX || p_y < Bound.MinY)
            {
                return index;  // 非法越界
            }

            int iX = (int)Math.Round((p_x - Bound.MinX) / Unit);
            int iY = (int)Math.Round((p_y - Bound.MinY) / Unit);
            index = iX * (int)WidthN + iY;
            // index校验
            if (index >= 0 && index < Points.Count)
            {
                return index;
            } else
            {
                return -1;
            }
        }

        /// <summary>
        /// 根据点坐标计算料格中的下标
        /// </summary>
        /// <param name="p"></param>
        /// <returns>int[x,y] 如果越界，返回值为-1</returns>
        public int[] GetIndexXY(ZXPoint p)
        {
            int[] index = new int[2] { -1, -1 };

            // STEP 1: 判断X
            if (p.X >= Bound.MinX && p.X <= Bound.MaxX)
            {
                index[0] = (int)Math.Round((p.X - Bound.MinX) / Unit);
            }

            // STEP 2: 判断Y
            if (p.Y >= Bound.MinY && p.Y <= Bound.MaxY)
            {
                index[1] = (int)Math.Round((p.Y - Bound.MinY) / Unit);
            }
            return index;
        }

        /// <summary>
        /// 获取相邻点格网下标
        /// </summary>
        /// <param name="p_index"></param>
        /// <param name="p_dir"></param>
        /// <returns></returns>
        public int GetIndex(int p_index, Dir p_dir = Dir.Current)
        {
            int index = -1;

            // 越界校验【未完待续】
            switch (p_dir)
            {
                case Dir.Current:
                    {
                        if (p_index < 0 || p_index >= Points.Count)
                            return index; // -1
                        index = p_index;
                        break;
                    }
                case Dir.Left:
                    {
                        if (p_index < this.WidthN)
                            return index; // -1
                        index = p_index - (int)WidthN;
                        break;
                    }
                case Dir.Up:
                    {
                        if (p_index % this.WidthN == this.WidthN - 1)
                            return index; // -1
                        index = p_index + 1;
                        break;
                    }
                case Dir.Right:
                    {
                        if (p_index >= this.Points.Count - this.WidthN)
                            return index;  // -1
                        index = p_index + (int)WidthN;
                        break;
                    }
                case Dir.Down:
                    {
                        if (p_index % this.WidthN == 0)
                            return index; // -1
                        index = p_index - 1;
                        break;
                    }
                case Dir.LeftDown:
                    {
                        if (p_index < this.WidthN || p_index % this.WidthN == 0)
                            return index; // -1
                        index = p_index - (int)WidthN - 1;
                        break;
                    }
                case Dir.LeftUp:
                    {
                        if (p_index < this.WidthN || p_index % this.WidthN == this.WidthN - 1)
                            return index; // -1
                        index = p_index - (int)WidthN + 1;
                        break;
                    }
                case Dir.RightUp:
                    {
                        if (p_index >= this.Points.Count - this.WidthN || p_index % this.WidthN == this.WidthN - 1)
                            return index;  // -1
                        index = p_index + (int)WidthN - 1;
                        break;
                    }
                case Dir.RightDown:
                    {
                        if (p_index >= this.Points.Count - this.WidthN || p_index % this.WidthN == 0)
                            return index;  // -1
                        index = p_index + (int)WidthN + 1;
                        break;
                    }
            }

            // index校验
            if (index >= 0 && index < Points.Count)
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// 根据index下标和方位获取点
        /// </summary>
        /// <param name="p_index">下标</param>
        /// <param name="p_point">返回值</param>
        /// <param name="p_dir">方向（不做越界校验，依次延续。最下边点的下边会取左边的最上边）</param>
        /// <returns>是否越界</returns>
        public bool GetPoint(int p_index, ref ZXPoint p_point, Dir p_dir = Dir.Current)
        {
            int index = GetIndex(p_index, p_dir);

            // index校验
            if (index >= 0 && index < Points.Count)
            {
                p_point = Points[index];
                return true;
            }

            return false;  // 异常返回false
        }

        /// <summary>
        /// 根据index下标和方位给点赋值
        /// </summary>
        /// <param name="p_index">下标</param>
        /// <param name="p_point">点坐标</param>
        /// <param name="p_dir">方向</param>
        /// <returns></returns>
        public bool SetPoint(int p_index, ZXPoint p_point, Dir p_dir = Dir.Current)
        {
            int index = GetIndex(p_index, p_dir);

            // index校验
            if (index >= 0 && index < Points.Count)
            {
                Points[p_index] = p_point;
                return true;
            }

            return false;  // 异常返回false
        }

        /// <summary>
        /// 平整程度（顶部p_h米与底部p_h米的体积比）
        /// </summary>
        /// <param name="p_h"></param>
        /// <returns>比例值：0 ~ 1 越大约平整</returns>
        public double FlatRate(float p_h = 0.5f)
        {
            // STEP 1: 截取底部
            ZXPointSet psBottom = new ZXPointSet(this.Points);
            for (int i = 0; i < psBottom.Count; i++)
            {
                if (psBottom[i].Z > p_h)
                    psBottom[i].Z = p_h;
            }

            DotPile pileBottom = new DotPile(psBottom);

            // STEP 2: 截取顶部
            ZXPointSet psTop = new ZXPointSet(this.Points);
            ZXBoundary bROI = this.Points.Boundary;
            for (int i = 0; i < psTop.Count; i++)
            {
                psTop[i].Z -= bROI.H - p_h;

                if (psTop[i].Z < 0)
                    psTop[i].Z = 0;
            }
            DotPile pileTop = new DotPile(psTop);

            return pileTop.Volume / pileBottom.Volume;
        }

        /// <summary>
        /// 判断为平顶料堆
        /// </summary>
        /// <param name="p_h">判别高度</param>
        /// <param name="p_rate">阈值：顶部p_h米与底部p_h米的体积比</param>
        /// <returns></returns>
        public bool IsFlat(float p_h = 0.5f, float p_rate = 0.1f)
        {
            Console.WriteLine(this.FlatRate(p_h));

            return this.FlatRate(p_h) >= p_rate;
        }

        /// <summary>
        /// 获取指定点横截面
        /// 创建版本：0.7.6
        /// 使用前提：满秩阵
        /// 时间评估：O(sqrt(N))
        /// </summary>
        /// <param name="indexX">X点下标</param>
        /// <returns>横截面点集</returns>
        public ZXPointSet GetCrossSection(int indexX)
        {
            ZXPointSet ps = new ZXPointSet();
            for (int i = 0; i < this.WidthN; i++)
            {
                ps.Add(this.Points.Get(indexX, i));
            }
            return ps;
        }

        /// <summary>
        /// 获取指定点纵截面 
        /// 创建版本：0.7.6
        /// 使用前提：满秩阵
        /// 时间评估：O(sqrt(N))
        /// </summary>
        /// <param name="indexY">Y点下表</param>
        /// <returns>纵截面点集</returns>
        public ZXPointSet GetVerticalSection(int indexY)
        {
            ZXPointSet ps = new ZXPointSet();
            for (int i = 0; i < this.LengthN; i++)
            {
                ps.Add(this.Points.Get(i, indexY));
            }
            return ps;
        }

        /// <summary>
        /// 获取指定点横截面 
        /// 创建版本：0.7.6
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public ZXPointSet GetCrossSection(float x)
        {
            int indexX = this.Points.GetIndexX(x);

            if (indexX == -1)
                return null;

            return GetCrossSection(indexX);
        }

        /// <summary>
        /// 获取指定点纵截面 0.7.6
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public ZXPointSet GetVerticalSection(float y)
        {
            int indexY = this.Points.GetIndexY(y);

            if (indexY == -1)
                return null;

            return GetVerticalSection(indexY);
        }

        /// <summary>
        /// 获取中间的横截面 0.7.6
        /// </summary>
        /// <returns></returns>
        public ZXPointSet GetCrossSection()
        {
            float oX = this.Points.Bound.OX;
            return GetCrossSection(oX);
        }

        /// <summary>
        /// 获取中间的纵截面 0.7.6
        /// </summary>
        /// <returns></returns>
        public ZXPointSet GetVerticalSection()
        {
            float oY = this.Points.Bound.OY;
            return GetVerticalSection(oY);
        }

        #endregion

        /// <summary>
        /// 格网化
        /// </summary>
        /// <param name="p_unit">格网精度</param>
        public void Gridding(float p_unit)
        {
            m_unit = p_unit; // 重定义精度
            Points.Gridding(Unit); // 格网化
            Bound = Points.Boundary; // 重新计算边界
        }

        /// <summary>
        /// 重新计算边界
        /// </summary>
        public virtual void UpdateBound()
        {
            this.Bound = Points.Boundary;
        }


        /// <summary>
        /// 塌料仿真——全局法
        /// </summary>
        /// <param name="p_r">严格度  默认值：1</param>
        /// <returns></returns>
        public bool SimSlide(double p_r = 1)
        {
            // STEP 1: 监测全部将要发生塌方的点
            ZXPointSet psHigh = new ZXPointSet();
            ZXPointSet psLow = new ZXPointSet();

            for (int i = 0; i < this.Points.Count; i++)
            {
                // 遍历上下左右四个方向
                for (int j = 1; j <= 8; j++)
                {
                    int index = GetIndex(i, (Dir)(j));
                    if (index < 0 || index >= Points.Count) continue; // 越界算下一个

                    float H = Points[i].Z - Points[index].Z; // 计算邻接高度差

                    if (j <= 4)
                    {
                        // CASE 1: 上下左右
                        if (H > Unit * p_r)
                        {
                            psHigh.Add(Points[i]);
                            psLow.Add(Points[index]);
                        }
                    }
                    else
                    {
                        // CASE 2: 斜四个方向
                        if (H > Unit * 1.4f * p_r)
                        {
                            psHigh.Add(Points[i]);
                            psLow.Add(Points[index]);
                        }
                    }
                }
            }

            // CASE 1: 不会发生塌方
            if (psHigh.Count == 0 || psLow.Count == 0)
            {
                return false; // 
            }
            // CASE 2: 会发生塌方

            // STEP 2: 所有高点降低 delta H
            double deltaH = this.Unit * 0.2f;
            for (int i = 0; i < psHigh.Count; i++)
            {
                psHigh[i].Z -= (float)deltaH;
            }

            // STEP 3: 所有低点提升 detal H * n / m
            deltaH = deltaH * psHigh.Count / psLow.Count;
            for (int i = 0; i < psLow.Count; i++)
            {
                psLow[i].Z += (float)deltaH;
            }

            // STEP 4: 高低点更新原始点云
            this.Points.Update(psHigh);
            this.Points.Update(psLow);

            return true;
        }




        /// <summary>
        /// 塌料仿真——全局法（局部计算）
        /// </summary>
        /// <param name="p_ROI">目标计算区域</param>
        /// <param name="p_r"></param>
        /// <returns></returns>
        public bool SimSlide(ZXBoundary p_ROI, double p_r = 1)
        {
            // STEP 1: 监测全部将要发生塌方的点
            ZXPointSet psHigh = new ZXPointSet();
            ZXPointSet psLow = new ZXPointSet();

            for (int i = 0; i < this.Points.Count; i++)
            {
                if (!p_ROI.Contain(this.Points[i]))
                {
                    // 不在计算区域内的点不参与计算
                    continue;
                }

                // 遍历上下左右四个方向
                for (int j = 1; j <= 8; j++)
                {
                    int index = GetIndex(i, (Dir)(j));
                    if (index < 0 || index >= Points.Count) continue; // 越界算下一个

                    float H = Points[i].Z - Points[index].Z; // 计算邻接高度差

                    if (j <= 4)
                    {
                        // CASE 1: 上下左右
                        if (H > Unit * p_r)
                        {
                            psHigh.Add(Points[i]);
                            psLow.Add(Points[index]);
                        }
                    }
                    else
                    {
                        // CASE 2: 斜四个方向
                        if (H > Unit * 1.4f * p_r)
                        {
                            psHigh.Add(Points[i]);
                            psLow.Add(Points[index]);
                        }
                    }
                }
            }

            // CASE 1: 不会发生塌方
            if (psHigh.Count == 0 || psLow.Count == 0)
            {
                return false; // 
            }
            // CASE 2: 会发生塌方

            // STEP 2: 所有高点降低 delta H
            double deltaH = this.Unit * 0.2f;
            for (int i = 0; i < psHigh.Count; i++)
            {
                psHigh[i].Z -= (float)deltaH;
            }

            // STEP 3: 所有低点提升 detal H * n / m
            deltaH = deltaH * psHigh.Count / psLow.Count;
            for (int i = 0; i < psLow.Count; i++)
            {
                psLow[i].Z += (float)deltaH;
            }

            // STEP 4: 高低点更新原始点云
            this.Points.Update(psHigh);
            this.Points.Update(psLow);


            return true;
        }

        /// <summary>
        /// 料堆生成OBJ三维模型
        /// </summary>
        /// <param name="filePath"></param>
        public virtual void SaveAsObj(string filePath, float p_minZ = 0.01f)
        {
            FileStream fs = CommonLib.GetFileStream(filePath);

            // STEP 1: 头部
            byte[] data = System.Text.Encoding.Default.GetBytes("o Pile\n");
            fs.Write(data, 0, data.Length);

            // STEP 2: 点坐标
            for (int i = 0; i < this.Points.Count; i++)
            {
                string xyz = string.Format("v {0} {1} {2}\n", this.Points[i].X, this.Points[i].Y, this.Points[i].Z);
                data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }

            // STEP 2.5: UV
            for (int i = 0; i < this.Points.Count; i++)
            {
                string xyz = string.Format("vt {0} {1}\n", this.Points[i].X, this.Points[i].Y);
                data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }

            // STEP 3: 四边面
            for (int i = 0; i < this.LengthN - 1; i++)
            {
                for (int j = 0; j < this.WidthN - 1; j++)
                {
                    int index0 = this.GetIndex(i, j) + 1;
                    int index1 = this.GetIndex(i + 1, j) + 1;
                    int index2 = this.GetIndex(i + 1, j + 1) + 1;
                    int index3 = this.GetIndex(i, j + 1) + 1;

                    // 剔除地平面
                    if (this.Points[index0 -1].Z < p_minZ && this.Points[index1-1].Z < p_minZ && this.Points[index2-1].Z < p_minZ && this.Points[index3-1].Z < p_minZ)
                    {
                        continue;
                    }

                    string face = string.Format("f {0}/{0} {1}/{1} {2}/{2} {3}/{3}\n", index0, index1, index2, index3);
                    data = System.Text.Encoding.Default.GetBytes(face);
                    fs.Write(data, 0, data.Length);
                }
            }
            fs.Close();
        }

        private class ObjRectangle
        {
            private ZXPoint[] vertexs;

            public ObjRectangle(ZXPoint pt0,  ZXPoint pt1, ZXPoint pt2, ZXPoint pt3)
            {
                this.vertexs = new ZXPoint[4]
                {
                    pt0,
                    pt1,
                    pt2,
                    pt3
                };
            }

            private ZXPoint Center;
            private int[] beyondList;
            private float R2;

            /// <summary>
            /// 用圆心点、半径，更新在圆外的点数组s
            /// </summary>
            /// <param name="center"></param>
            /// <param name="R2"></param>
            /// <returns></returns>
            public int UpdateBeyond(ZXPoint center, float R2)
            {
                this.Center = center;
                this.R2 = R2;
                List<int> list = new List<int>();
                for (int ii = 0; ii < 4; ii++)
                {
                    ZXPoint pt = this.vertexs[ii];
                    float dis2 = (pt.X - center.X) * (pt.X - center.X) + (pt.Y - center.Y) * (pt.Y - center.Y);
                    if (dis2 > R2 + float.Epsilon)   // 圆的边缘线厚度
                    {
                        list.Add(ii);
                    }
                }

                this.beyondList = list.ToArray();
                return this.beyondList.Length;
            }

            /// <summary>
            /// 计算点插入 每个边只能有一个插点
            /// </summary>
            /// <param name="insertList">需要插入的点</param>
            /// <param name="tempIndexList">需要插入的点的临时ID</param>
            /// <returns>按序排列的ID列表，其中需要插入的点为-1占位</returns>
            public List<int> GetPointsOfObjStructure(int[] indexList, out List<ZXPoint> insertList, out List<long> tempIndexList)
            {
                switch(this.beyondList.Length)
                {
                    case 0:
                    case 4:
                        insertList = null;
                        tempIndexList = null;
                        return null;
                    // 只有一点在圆外
                    case 1:
                        {
                            int beyondIndex = this.beyondList[0];
                            int frontIndex = this.GetFrontIndex(beyondIndex);
                            int nextIndex = this.GetNextIndex(beyondIndex);
                            ZXPoint frontPt = this.GetEdgeInsert(frontIndex, beyondIndex);
                            ZXPoint nextPt = this.GetEdgeInsert(beyondIndex, nextIndex);

                            List<int> result = new List<int>();
                            for(int i = 0;i < 4;i++)
                            {
                                if(i != beyondIndex)
                                {
                                    result.Add(i);
                                }
                            }
                            insertList = new List<ZXPoint>();
                            tempIndexList = new List<long>();
                            if (frontPt != null)
                            {
                                result.Insert(beyondIndex, -1);
                                tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyondIndex]) << 32) + Math.Max(indexList[frontIndex], indexList[beyondIndex]));
                                insertList.Add(frontPt);

                                if (nextPt != null)
                                {
                                    result.Insert(beyondIndex + 1, -1);
                                    tempIndexList.Add(((long)Math.Min(indexList[nextIndex], indexList[beyondIndex]) << 32) + Math.Max(indexList[nextIndex], indexList[beyondIndex]));
                                    insertList.Add(nextPt);
                                }
                            }
                            else
                            {
                                if (nextPt != null)
                                {
                                    result.Insert(beyondIndex, -1);
                                    tempIndexList.Add(((long)Math.Min(indexList[nextIndex], indexList[beyondIndex]) << 32) + Math.Max(indexList[nextIndex], indexList[beyondIndex]));
                                    insertList.Add(nextPt);
                                }
                            }
                            return result;
                        }
                    case 2:
                        {
                            int beyoundIndex_min = beyondList.Min();
                            int beyoundIndex_max = beyondList.Max();
                            int frontIndex = this.GetFrontIndex(beyoundIndex_min);
                            int nextIndex = this.GetNextIndex(beyoundIndex_max);

                            if(frontIndex == beyoundIndex_max)
                            {
                                frontIndex = this.GetFrontIndex(beyoundIndex_max);
                                nextIndex = this.GetNextIndex(beyoundIndex_min);
                                int temp = beyoundIndex_max;
                                beyoundIndex_max = beyoundIndex_min;
                                beyoundIndex_min = temp;
                            }

                            ZXPoint frontPt = this.GetEdgeInsert(frontIndex, beyoundIndex_min);
                            ZXPoint nextPt = this.GetEdgeInsert(beyoundIndex_max, nextIndex);

                            List<int> result = new List<int>();
                            for (int i = 0; i < 4; i++)
                            {
                                if (i != beyoundIndex_min && i != beyoundIndex_max)
                                {
                                    result.Add(i);
                                }
                            }
                            insertList = new List<ZXPoint>();
                            tempIndexList = new List<long>();
                            if (beyoundIndex_min < beyoundIndex_max)
                            {
                                if (frontPt != null)
                                {
                                    result.Insert(beyoundIndex_min, -1);
                                    tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyoundIndex_min]) << 32)
                                        + Math.Max(indexList[frontIndex], indexList[beyoundIndex_min]));
                                    insertList.Add(frontPt);

                                    if (nextPt != null)
                                    {
                                        result.Insert(beyoundIndex_max, -1);
                                        tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyoundIndex_max]) << 32)
                                            + Math.Max(indexList[frontIndex], indexList[beyoundIndex_max]));
                                        insertList.Add(nextPt);
                                    }
                                }
                                else
                                {
                                    if (nextPt != null)
                                    {
                                        result.Insert(beyoundIndex_max - 1, -1);
                                        tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyoundIndex_max]) << 32)
                                            + Math.Max(indexList[frontIndex], indexList[beyoundIndex_max]));
                                        insertList.Add(nextPt);
                                    }
                                }
                            }
                            else
                            {
                                if (nextPt != null)
                                {
                                    result.Insert(beyoundIndex_max, -1);
                                    tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyoundIndex_max]) << 32)
                                        + Math.Max(indexList[frontIndex], indexList[beyoundIndex_max]));
                                    insertList.Add(nextPt);

                                    if (frontPt != null)
                                    {
                                        result.Insert(beyoundIndex_min, -1);
                                        tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyoundIndex_min]) << 32)
                                            + Math.Max(indexList[frontIndex], indexList[beyoundIndex_min]));
                                        insertList.Add(frontPt);
                                    }
                                }
                                else
                                {
                                    if (frontPt != null)
                                    {
                                        result.Insert(beyoundIndex_min - 1, -1);
                                        tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[beyoundIndex_min]) << 32)
                                            + Math.Max(indexList[frontIndex], indexList[beyoundIndex_min]));
                                        insertList.Add(frontPt);
                                    }
                                }
                            }
                            return result;
                        }
                    case 3:
                        {
                            bool[] flags = new bool[4];
                            for(int i = 0;i < beyondList.Length;i++)
                            {
                                flags[beyondList[i]] = true;
                            }

                            int insideIndex = -1;
                            for(int i = 0;i < 4;i++)
                            {
                                if (!flags[i])
                                {
                                    insideIndex = i;
                                    break;
                                }
                            }

                            int frontIndex = GetFrontIndex(insideIndex);
                            int nextIndex = GetNextIndex(insideIndex);
                            ZXPoint frontPt = this.GetEdgeInsert(frontIndex, insideIndex);
                            ZXPoint nextPt = this.GetEdgeInsert(insideIndex, nextIndex);
                            List<int> result = new List<int>()
                            {
                                -1, insideIndex, -1
                            };
                            tempIndexList = new List<long>();
                            insertList = new List<ZXPoint>()
                            { frontPt, nextPt };
                            tempIndexList.Add(((long)Math.Min(indexList[frontIndex], indexList[insideIndex]) << 32) + Math.Max(indexList[frontIndex], indexList[insideIndex]));
                            tempIndexList.Add(((long)Math.Min(indexList[nextIndex], indexList[insideIndex]) << 32) + Math.Max(indexList[nextIndex], indexList[insideIndex]));
                            return result;
                        }
                    default:
                        insertList = null;
                        tempIndexList = null;
                        return null;
                }
            }

            /// <summary>
            /// 获取逆时针顺序的下一个序号
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            private int GetNextIndex(int index)
            {
                int result = index + 1;
                if(result > 3)
                {
                    return 0;
                }
                else
                {
                    return result;
                }
            }

            /// <summary>
            /// 获取逆时针顺序的前一个序号
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            private int GetFrontIndex(int index)
            {
                int result = index - 1;
                if (result < 0)
                {
                    return 3;
                }
                else
                {
                    return result;
                }
            }

            private bool isEqualsFloat(float a, float b)
            {
                return Math.Abs(a - b) <= float.Epsilon;
            }

            /// <summary>
            /// 计算在边上插点
            /// </summary>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <returns></returns>
            private ZXPoint GetEdgeInsert(int startIndex, int endIndex)
            {
                float OX = (vertexs[startIndex].X + vertexs[endIndex].X) / 2;
                float OY = (vertexs[startIndex].Y + vertexs[endIndex].Y) / 2;

                float X, Y, Z;
                // 要么X相等，要么Y相等
                if (isEqualsFloat(vertexs[startIndex].X , vertexs[endIndex].X))
                {
                    X = vertexs[startIndex].X;

                    float tempY = (float)Math.Sqrt(Math.Round(R2 - (X - Center.X) * (X - Center.X), 5));   // tempY = ( Y - OY );

                    if(OY > Center.Y)
                    {
                        Y = tempY + Center.Y;
                    }
                    else
                    {
                        Y = -tempY + Center.Y;
                    }

                    if(isEqualsFloat(Y, vertexs[startIndex].Y) || isEqualsFloat(Y, vertexs[endIndex].Y))
                    {
                        return null;
                    }

                    float proportion = (Y - Math.Min(vertexs[startIndex].Y, vertexs[endIndex].Y)) / Math.Abs(vertexs[startIndex].Y - vertexs[endIndex].Y);
                    Z = proportion * Math.Abs(vertexs[startIndex].Z - vertexs[endIndex].Z) + Math.Min(vertexs[startIndex].Z, vertexs[endIndex].Z);
                }
                else
                {
                    Y = vertexs[startIndex].Y;

                    float tempX = (float)Math.Sqrt(Math.Round(R2 - (Y - Center.Y) * (Y - Center.Y), 5));   // tempX = ( X - OX );

                    if (OX > Center.X)
                    {
                        X = tempX + Center.X;
                    }
                    else
                    {
                        X = -tempX + Center.X;
                    }

                    if (isEqualsFloat(X, vertexs[startIndex].X) || isEqualsFloat(X, vertexs[endIndex].X))
                    {
                        return null;
                    }

                    float proportion = (X - Math.Min(vertexs[startIndex].X, vertexs[endIndex].X)) / Math.Abs(vertexs[startIndex].X - vertexs[endIndex].X);
                    Z = proportion * Math.Abs(vertexs[startIndex].Z - vertexs[endIndex].Z) + Math.Min(vertexs[startIndex].Z, vertexs[endIndex].Z);
                }


                return new ZXPoint(X, Y, Z);
            }
        }

        /// <summary>
        /// 料堆生成OBJ三维模型（圆形料场底面切割）
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAsObjCircle(string filePath)
        {
            FileStream fs = CommonLib.GetFileStream(filePath);

            // STEP 1: 头部
            byte[] data = System.Text.Encoding.Default.GetBytes("o Pile\n");
            fs.Write(data, 0, data.Length);

            // STEP 2: 点序列
            for (int i = 0; i < this.Points.Count; i++)
            {
                string xyz = string.Format("v {0} {1} {2}\n", this.Points[i].X, this.Points[i].Y, this.Points[i].Z);
                data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }

            // STEP 2.5: UV
            for (int i = 0; i < this.Points.Count; i++)
            {
                string xyz = string.Format("vt {0} {1}\n", this.Points[i].X, this.Points[i].Y);
                data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }

            float oX = this.Bound.OX;
            float oY = this.Bound.OY;
            float R = this.Bound.L * 0.5f;

            ZXPoint center = this.Bound.Center;
            center.Z = 0;

            FileStream tempFs = CommonLib.GetFileStream(filePath + ".temp");  // 创建临时文件，用以存储四边面
            Dictionary<long, int> insertMap = new Dictionary<long, int>();  // 需要插点的临时ID与在列表中序号的映射
            List<ZXPoint> insertPoints = new List<ZXPoint>();   // 存储插入点
            int index = this.Points.Count;
            float R2 = R * R;

            // STEP 3: 四边面
            for (int i = 0; i < this.LengthN - 1; i++)
            {
                for (int j = 0; j < this.WidthN - 1; j++)
                {
                    int index0 = this.GetIndex(i, j);
                    int index1 = this.GetIndex(i + 1, j);
                    int index2 = this.GetIndex(i + 1, j + 1);
                    int index3 = this.GetIndex(i, j + 1);

                    int[] indexList = new int[4]
                    {
                        this.GetIndex(i, j),
                        this.GetIndex(i + 1, j),
                        this.GetIndex(i + 1, j + 1),
                        this.GetIndex(i, j + 1)
                    };

                    string face;

                    ObjRectangle rectangle = new ObjRectangle(this.Points[index0], this.Points[index1], this.Points[index2], this.Points[index3]);
                    int beyondCount = rectangle.UpdateBeyond(center, R2);

                    if(beyondCount <= 0)
                    {
                        face = string.Format("f {0} {1} {2} {3}\n", index0 + 1, index1 + 1, index2 + 1, index3 + 1);
                    }
                    else if(beyondCount >= 4)
                    {
                        face = null;
                    }
                    else
                    {
                        List<ZXPoint> insertList;   // 需要插入的点列表
                        List<long> tempIndexList;   // 插入点的临时ID
                        List<int> indexOrderList = rectangle.GetPointsOfObjStructure(indexList, out insertList, out tempIndexList);

                        List<int> faceIndexList = new List<int>();  // 四边面真实序号列表 最多5个
                        for(int ii = 0, insertFlag = 0;ii < indexOrderList.Count;ii++)
                        {
                            if (indexOrderList[ii] == -1)
                            {
                                if (insertMap.ContainsKey(tempIndexList[insertFlag]))
                                {
                                }
                                else
                                {
                                    insertPoints.Add(insertList[insertFlag]);
                                    insertMap.Add(tempIndexList[insertFlag], index);
                                    index++;
                                }
                                faceIndexList.Add(insertMap[tempIndexList[insertFlag]] + 1);
                                insertFlag++;
                            }
                            else
                            {
                                faceIndexList.Add(indexList[indexOrderList[ii]] + 1);
                            }
                        }

                        if(faceIndexList.Count <= 4)
                        {
                            //face = string.Format("f {0} {1} {2} {3}\n", faceIndexList[0], faceIndexList[1], faceIndexList[2], faceIndexList[3]);
                            face = "f ";
                            for (int ii = 0;ii < faceIndexList.Count;ii++)
                            {
                                face += faceIndexList[ii] + " ";
                            }
                            face += "\n";
                        }
                        else
                        {
                            face = string.Format("f {0} {1} {2} {3}\n", faceIndexList[0], faceIndexList[1], faceIndexList[2], faceIndexList[3]);
                            face += string.Format("f {0} {1} {2}\n", faceIndexList[3], faceIndexList[4], faceIndexList[0]);
                        }
                    }

                    if (!string.IsNullOrEmpty(face))
                    {
                        data = System.Text.Encoding.Default.GetBytes(face);
                        tempFs.Write(data, 0, data.Length);
                    }
                }
            }
            tempFs.Flush();
            tempFs.Close();

            for (int i = 0;i < insertPoints.Count;i++)
            {
                string xyz = string.Format("v {0} {1} {2}\n", insertPoints[i].X, insertPoints[i].Y, insertPoints[i].Z);
                data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }

            tempFs = new FileStream(filePath + ".temp", FileMode.Open, FileAccess.Read);
            while (tempFs.Position <= tempFs.Length)
            {
                byte[] buffer = new byte[1024];
                var length = tempFs.Read(buffer, 0, 1024);
                if (length > 0)
                {
                    fs.Write(buffer, 0, length);
                    //tempFs.Position += length;
                }
                else
                {
                    break;
                }
            }
            tempFs.Close();

            fs.Close();
            File.Delete(filePath + ".temp");
        }




    }
}
