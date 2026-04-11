//=====================================================================
// 模块名称：料堆点云 DotPile
// 功能简介：单一料堆对象的属性与行为
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.7.21 杜子兮 创建
//          20220628 Sanngoku 添加 SetOffset()方法，用以偏移料堆相对设备行走轨道的距离
//============================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotCloudLib
{
    /* 坐标系约定：算法坐标系
     *                            （俯视图）
     * Y    
     * |   ________    ________   ________
     * |  | 料堆   |  | 料堆   | | 料堆    |
     * |   ________    ________   ________
     * 0 -----------------------------------> X
     * |   ________    _______________
     * |  | 料堆   |  |     长料堆      |
     * |   ________    ———————————————
     * |
     * 
     */

    /// <summary>
    /// 格网方位（按料堆俯视图）
    /// </summary>
    public enum Dir
    {
        /// <summary>
        /// 当前位置
        /// </summary>
        Current,
        /// <summary>
        /// 左边
        /// </summary>
        Left,
        /// <summary>
        /// 上边
        /// </summary>
        Up,
        /// <summary>
        /// 右边
        /// </summary>
        Right,
        /// <summary>
        /// 下边
        /// </summary>
        Down,
        /// <summary>
        /// 左上
        /// </summary>
        LeftUp,
        /// <summary>
        /// 右上
        /// </summary>
        RightUp,
        /// <summary>
        /// 右下
        /// </summary>
        RightDown,
        /// <summary>
        /// 左下
        /// </summary>
        LeftDown
    }

    /// <summary>
    /// 料堆
    /// </summary>
    public class DotPile : DotPileBase, ISimable
    {
        #region 成员变量与属性

        /// <summary>
        /// 唯一编码
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 向外传送消息
        /// </summary>
        public string Message { get; set; }


        /// <summary>
        /// 仿真后变化事件
        /// </summary>
        public EventHandler<SimulatedArgs> SimulatedHandler { get; set; }


        ///// <summary>
        ///// 料堆点集（格网、有序、补全后）
        ///// </summary>
        //public ZXPointSet Points { get; set; }       

        #endregion

        #region 构造类方法

        /// <summary>
        /// ctor.
        /// </summary>
        public DotPile()
        {
            this.Points = new ZXPointSet();
        }

        /// <summary>
        /// 7
        /// </summary>
        /// <param name="p_pile"></param>
        public DotPile(DotPile p_pile)
        {
            m_unit = p_pile.Unit;
            Points = new ZXPointSet(p_pile.Points);
            Points.Unit = m_unit;
            Bound = Points.Boundary;
        }

        /// <summary>
        /// 构造方法（空料堆）初始化每个点的高度为0
        /// </summary>
        /// <param name="p_minX">料条起始位</param>
        /// <param name="p_maxX">料条终止位</param>
        /// <param name="p_minY">近机点</param>
        /// <param name="p_maxY">远机点</param>
        /// <param name="p_minZ">地面高度</param>
        /// <param name="p_maxZ">最高高度</param>
        /// <param name="p_unit">格网精度</param>
        public DotPile(float p_minX, float p_maxX, float p_minY, float p_maxY, float p_minZ, float p_maxZ, float p_unit)
        {
            m_unit = p_unit;
            Bound = new ZXBoundary(p_minX, p_maxX, p_minY, p_maxY, p_minZ, p_maxZ);
            Points = new ZXPointSet();
            Points.Unit = p_unit; // 2022.7.21
            Points.Bound = Bound; // 2022.7.21
            Init(0);
        }

        /// <summary>
        /// 构造方法（空料堆）初始化每个点高度为0
        /// </summary>
        /// <param name="p_b">料堆包围盒</param>
        /// <param name="p_unit">格网精度</param>
        public DotPile(ZXBoundary p_b, float p_unit)
        {
            m_unit = p_unit;
            Bound = p_b;
            Points = new ZXPointSet();
            Points.Unit = p_unit;
            Points.Bound = Bound; // 2022.7.21
            Init(0);
        }

        /// <summary>
        /// 构造方法（空料堆）初始化每个点高度为 p_h  2022.07.30
        /// </summary>
        /// <param name="p_b">料堆包围盒</param>
        /// <param name="p_h">空料堆初始化高度</param>
        /// <param name="p_unit">格网精度</param>
        public DotPile(ZXBoundary p_b, float p_h, float p_unit)
        {
            m_unit = p_unit;
            Bound = p_b;
            Points = new ZXPointSet();
            Points.Unit = p_unit;
            Points.Bound = Bound; 
            Init(p_h);
        }

        public DotPile(ZXPointSet ps) :base(ps)
        {

        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="p_filePath">格网化后的料堆数据文件路径</param>
        /// <param name="p_format">点数据格式</param>
        /// <param name="p_unit"></param>
        public DotPile(string p_filePath, PointFormat p_format, float p_unit)
        {
            Points = new ZXPointSet();
            switch (p_format)
            {
                case PointFormat.XYZ:
                    Points.LoadFromXYZ(p_filePath);
                    m_unit = p_unit;
                    break;
                case PointFormat.IXYZ:
                    Points.LoadFromIXYZ(p_filePath);
                    m_unit = p_unit;
                    break;
                
                /*
                case PointFormat.JSON:
                    JObject json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(p_filePath));
                    float oX = float.Parse(json["oX"].ToString());
                    float oY = float.Parse(json["oY"].ToString());
                    float oZ = float.Parse(json["oZ"].ToString());
                    int length = int.Parse(json["length"].ToString());
                    int width = int.Parse(json["width"].ToString());
                    m_unit = float.Parse(json["unit"].ToString());

                    for (int i = 0; i < length; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            int index = i * width + j;
                            Points.Add(i * this.Unit, j * this.Unit, float.Parse(json["heights"][index].ToString()));
                        }
                    }
                    break;
                */
                default:
                    break;
            }

            Points.Gridding(Unit); // 格网化
            Bound = Points.Boundary; // 重新计算边界
        }

        /// <summary>
        /// 构造方法 (一维高程数组)
        /// 假设：l = 7, w = 5, 那么 heights.Length 一定为 35（否则报错）
        /// 料堆格网点 俯视图 下标如下：index = i(0 ~ 6) * w(5) + j (0 ~ 4)
        /// Y 
        /// 4 9 14 19 24 29 34
        /// 3 8 13 18 23 28 33
        /// 2 7 12 17 22 27 32
        /// 1 6 11 16 21 26 31
        /// 0 5 10 15 20 25 30  X方向  
        /// </summary>
        /// <param name="p_l">X方向点数</param>
        /// <param name="p_w">Y方向点数</param>
        /// <param name="p_unit">格网精度（左右、上下两点间距离）</param>
        /// <param name="heights">高程值数组</param>
        public DotPile(int p_l, int p_w, float p_unit, float[] heights)
        {
            // STEP 0: 参数校验
            if (p_l * p_w != heights.Length)
            {
                string errorMsg = "DotPile235: 构造DotPile失败，点云总数 " + heights.Length + " =/= " + p_l + " X " + p_w + " (" + p_l * p_w + ")";
                LibTool.Error(errorMsg);
            }

            // STEP 1: 赋值
            m_unit = p_unit;

            this.Points = new ZXPointSet();
            this.Points.Unit = this.m_unit;

            for (int i = 0; i < p_l; i++)
            {
                for (int j = 0; j < p_w; j++)
                {
                    int index = (int)(i * p_w + j);
                    float x = i * this.m_unit;
                    float y = j * this.m_unit;
                    this.Points.Add(x, y, heights[index]);
                }
            }

            // STEP 2: 重新计算边界
            this.Bound = this.Points.Boundary;
        }

        /// <summary>
        /// 构造方法 (一维高程数组)
        /// 假设：l = 7, w = 5, 那么 heights.Length 一定为 35（否则报错）
        /// 料堆格网点 俯视图 下标如下：index = i(0 ~ 6) * w(5) + j (0 ~ 4)
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="p_l"></param>
        /// <param name="p_w"></param>
        /// <param name="p_unit"></param>
        /// <param name="heights"></param>
        public DotPile(float minX, float minY, int p_l, int p_w, float p_unit, float[] heights)
        {
            if (p_l * p_w != heights.Length)
            {
                string errorMsg = "DotPile235: 构造DotPile失败，点云总数 " + heights.Length + " =/= " + p_l + " X " + p_w + " (" + p_l * p_w + ")";
                LibTool.Error(errorMsg);
            }

            // STEP 1: 赋值
            m_unit = p_unit;

            this.Points = new ZXPointSet();
            this.Points.Unit = this.m_unit;

            for (int i = 0; i < p_l; i++)
            {
                for (int j = 0; j < p_w; j++)
                {
                    int index = (int)(i * p_w + j);
                    float x = i * this.m_unit + minX;
                    float y = j * this.m_unit + minY;
                    this.Points.Add(x, y, heights[index]);
                }
            }

            // STEP 2: 重新计算边界
            this.Bound = this.Points.Boundary;

        }

        #endregion      

        #region 基础处理 

        /// <summary>
        /// 偏移料堆
        /// </summary>
        /// <param name="_axisType">偏移轴</param>
        /// <param name="_offset">偏移量</param>
        public void SetOffset(AxisType _axisType, float _offset)
        {
            switch(_axisType)
            {
                case AxisType.X:
                    {
                        this.Points.Translate(_offset, 0, 0);
                        this.Bound = Points.Boundary;
                        return;
                    }
                case AxisType.Y:
                    {
                        this.Points.Translate(0, _offset, 0);
                        this.Bound = Points.Boundary;
                        return;
                    }
                case AxisType.Z:
                    {
                        this.Points.Translate(0, 0, _offset);
                        this.Bound = Points.Boundary;
                        return;
                    }
            }
        }

        #endregion

        #region 最优点搜索

        /// <summary>
        /// 【废弃】寻找抓斗最佳抓料点（方案一）高精度，低性能
        /// </summary>
        /// <param name="digger">取料机构模型</param>
        /// <param name="p_xOffset">X方向搜索起止偏移量</param>
        /// <param name="p_yOffset">Y方向搜索起止偏移量</param>
        /// <returns></returns>
        [Obsolete]
        public ZXPoint SearchBestGrabPoint(DotDigger digger, float p_xOffset = 2, float p_yOffset = 3)
        {
            ZXPoint centerBest = new ZXPoint(0, 0, 0); // 目标返回值
            float maxV = 0;
            float volume = 0;
            float value = 0;

            // STEP 1: 大格网点遍历(1米间隔)
            LibTool.Log("STEP 1: 大格网点遍历");
            for (int i = 2; i < this.Bound.L - p_xOffset; i++)
            {
                for (int j = 3; j < this.Bound.W - p_yOffset; j++)
                {
                    /*
                    int index = this.GetIndex(i, j);
                    if (this.Points[index].Z < 0.3f)
                    {
                        continue;
                    }
                    */

                    ZXPoint center = new ZXPoint(i, j, 25);

                    volume = this.ComputeTakeVolume(digger, ref center); // 评估用仿真计算，只计算体积，不改变料型本身

                    if (volume <= 0) continue;

                    value = this.ComputeValue(volume, center); // 评分标准
                    if (value > maxV)
                    {
                        maxV = value;
                        centerBest = center;
                        LibTool.Debug("大格 抓取点：" + centerBest.ToString() + " 抓取量：" + volume + "立方米" + " 总评分：" + value);
                    }
                }
            }

            if (centerBest.DistanceTo(new ZXPoint(0, 0, 0)) < 0.001f)
            {
                LibTool.Debug("CASE 1: 未找到最佳抓料点");
                return centerBest;
            }
            LibTool.Debug("CASE 2: 找到最大大格抓取点 " + centerBest.ToString());

            // STEP 2: （一平方米内）小格网点遍历
            maxV = 0;
            float ox = centerBest.X;
            float oy = centerBest.Y;

            // this.Unit = 0.5f;
            int startI = -(int)(1.0f / this.Unit);
            int endI = -startI;
            int startJ = -(int)(1.0f / this.Unit);
            int endJ = -startI;

            for (int i = startI; i <= endI; i++)
            {
                for (int j = startJ; j <= endJ; j++)
                {

                    ZXPoint center = new ZXPoint(ox + (i * this.Unit), oy + (j * this.Unit), 25);
                    volume = this.ComputeTakeVolume(digger, ref center); // 评估用仿真计算，只计算体积，不改变料型本身
                    if (volume <= 0) continue;

                    value = this.ComputeValue(volume, center); // 评分标准
                    if (value > maxV)
                    {
                        maxV = value;
                        centerBest = center;
                        LibTool.Debug("小格 抓取点：" + centerBest.ToString() + " 抓取量：" + volume + "立方米" + " 总评分：" + value);
                    }
                }
            }

            return centerBest;
        }  

        #endregion

        #region 仿真堆取料

        // 前提：堆料边界无越界，若将允许越界，则先扩界再模拟堆料
        /// <summary>
        /// 堆料仿真（一）指定高度 
        /// 假设：p_stackPoint在格网上
        /// </summary>
        /// <param name="p_targetH">目标高度</param>
        /// <param name="p_stackPoint">卸料点</param>
        /// <param name="p_change">是否改变原始点</param>
        /// <param name="p_obsArea">障碍物范围</param>
        /// <returns>体积</returns>
        public double SimStackByHeight(double p_targetH, ZXPoint p_stackPoint, bool p_change = true, ZXBoundary p_obsArea = new ZXBoundary())
        {
            List<IndicesPoint> points = new List<IndicesPoint>();   // 仿真后事件
            float minZ = Bound.MinZ; // 底部基准面
            double sumV = 0; // 总堆积体积

            // STEP 1 求卸料点原有堆料的高度
            int indexX = (int)Math.Round((p_stackPoint.X - Bound.MinX) / Unit);  // 卸料点原点索引
            int indexY = (int)Math.Round((p_stackPoint.Y - Bound.MinY) / Unit);
            float unitArea = Unit * Unit; // 单位格网面积

            // STEP 2 自下向上遍历，直到计算体积大于目标体积
            double rReal = (p_targetH - minZ) / Math.Tan(Alfa / 180f * Math.PI); // 圆锥体底面实际半径
            int r = (int)Math.Round(rReal / Unit); // 圆锥体底面格网半径

            // STEP 2.1 遍历圆锥体表面每一个点
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    // 获取已有点下标
                    int iX = indexX + i;
                    int iY = indexY + j;
                    if (iX < 0 || iX >= LengthN || iY < 0 || iY >= WidthN) continue;  // 越界剔除
                    int iIndex = iX * (int)WidthN + iY;
                    if (iIndex < 0 || iIndex >= this.Points.Count)
                    {
                        // 2025.3.4
                        // LibTool.Error("DotPile603: 指定高度堆料时下标越界 " + iIndex + "  " + this.Points.Count);
                        continue;
                    }

                    if (Math.Pow(i, 2) + Math.Pow(j, 2) > Math.Pow(r, 2)) continue;  // 剔除非圆锥体表面点

                    // STEP 2.2 计算与已有堆料的差值
                    // 获取原始点
                    ZXPoint oP = this.Points[iIndex];

                    if (p_obsArea.ContainXY(oP)) continue; // 障碍物中不堆料 2025.3.4

                    /*
                    *   |\
                    *  h| \
                    *   |-- d （表面点距离中心轴距离）
                    *   |   \
                    *  z|  40\
                    *   ------
                    */

                    float d = (float)Math.Sqrt(i * i + j * j) * Unit;
                    float h = d * (float)Math.Tan(Alfa / 180f * Math.PI);
                    double z = p_targetH - h;  // 加高后的高度

                    // 如果高于堆，说明有散料堆积
                    if (z > oP.Z)
                    {
                        sumV += (z - oP.Z) * unitArea;
                        if (p_change)
                        {
                            // 20230523 Sanngoku : 添加仿真后料堆改变事件
                            IndicesPoint pt = new IndicesPoint()
                            {
                                BeforeZ = oP.Z,
                                Z = (float)z,
                                Index = iIndex,
                                xIndex = iX,
                                yIndex = iY,
                            };
                            points.Add(pt);

                            oP.Z = (float)z;
                            this.Points[iIndex] = oP;
                        }

                    }
                }
            }

            if(p_change)
            {
                SimulatedArgs args = new SimulatedArgs()
                {
                    isChanged = points.Count > 0 && sumV > 0,
                    list_Points = points,
                    Volumn = (float)sumV
                };
                this.SimulatedHandler?.Invoke(this, args);
            }
            //else
            //{
            //    SimulatedArgs args = new SimulatedArgs()
            //    {
            //        isChanged = false,
            //        list_Points = points,
            //        Volumn = (float)sumV
            //    };
            //    this.SimulatedHandler?.Invoke(this, args);
            //}

            return sumV; // 返回总堆积体积
        }

        // 前提：堆料边界无越界，若将允许越界，则先扩界再模拟堆料
        /// <summary>
        /// 堆料仿真（一）指定高度 - 圆顶
        /// 假设：p_stackPoint在格网上
        /// </summary>
        /// <param name="p_targetH">目标高度</param>
        /// <param name="p_stackPoint">卸料点</param>
        /// <param name="top_height">圆顶高度</param>
        /// <param name="p_change">是否改变原始点</param>
        /// <param name="p_obsArea">障碍物范围</param>
        /// <returns>体积</returns>
        public double SimStackByHeight(double p_targetH, ZXPoint p_stackPoint, double top_height, bool p_change = true, ZXBoundary p_obsArea = new ZXBoundary())
        {
            #region bezier
            List<IndicesPoint> points = new List<IndicesPoint>();   // 仿真后事件
            float minZ = Bound.MinZ; // 底部基准面
            double sumV = 0; // 总堆积体积

            p_targetH += top_height; // 原顶造成的高度损失值为top_height，为保证最终仿真料堆的高度等同于输入的目标高度，需在目标高度的基础上增加此损失值，以使得输入目标高度与料堆仿真结果高度近似相等。
            // STEP 1 求卸料点原有堆料的高度
            int indexX = (int)Math.Round((p_stackPoint.X - Bound.MinX) / Unit);  // 卸料点原点索引
            int indexY = (int)Math.Round((p_stackPoint.Y - Bound.MinY) / Unit);
            float unitArea = Unit * Unit; // 单位格网面积

            // STEP 2 自下向上遍历，直到计算体积大于目标体积
            double rReal = (p_targetH - minZ) / Math.Tan(Alfa / 180f * Math.PI); // 圆锥体底面实际半径
            int r = (int)Math.Round(rReal / Unit); // 圆锥体底面格网半径

            int top_r = (int)Math.Round(top_height * 2 / Math.Tan(Alfa / 180.0 * Math.PI) / Unit);
            double offset_z = p_targetH - top_r * Unit * (float)Math.Tan(Alfa / 180f * Math.PI);  // 加高后的高度
            int top_r_2 = top_r * top_r;

            //Func<ZXPoint, ZXPoint, ZXPoint, float, double> bezier = (p0, p1, p2, t) =>
            //{
            //    float mt = 1 - t;
            //    float mt2 = mt * mt;
            //    float t2 = t * t;
            //    float y = mt2 * p0.Y + 2 * mt * t * p1.Y + t2 * p2.Y;
            //    return y;
            //};
            double segement = top_r * 2;
            ZXPoint control = new ZXPoint(0, (float)(top_height * 2));
            ZXPoint pStart = new ZXPoint(-top_r * Unit, 0);
            ZXPoint pEnd = new ZXPoint(top_r * Unit, 0);

            // STEP 2.1 遍历圆锥体表面每一个点
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    // 获取已有点下标
                    int iX = indexX + i;
                    int iY = indexY + j;
                    if (iX < 0 || iX >= LengthN || iY < 0 || iY >= WidthN) continue;  // 越界剔除
                    int iIndex = iX * (int)WidthN + iY;
                    if (iIndex < 0 || iIndex >= this.Points.Count)
                    {
                        // 2025.3.4
                        // LibTool.Error("DotPile603: 指定高度堆料时下标越界 " + iIndex + "  " + this.Points.Count);
                        continue;
                    }

                    if (Math.Pow(i, 2) + Math.Pow(j, 2) > Math.Pow(r, 2)) continue;  // 剔除非圆锥体表面点

                    // STEP 2.2 计算与已有堆料的差值
                    // 获取原始点
                    ZXPoint oP = this.Points[iIndex];

                    if (p_obsArea.ContainXY(oP)) continue; // 障碍物中不堆料 2025.3.4


                    if (i * i + j * j < top_r_2)
                    {
                        double z = BezierTop(pStart, control, pEnd, (float)((top_r - Math.Sqrt(i * i + j * j)) / top_r / 2)) + offset_z;
                        // 如果高于堆，说明有散料堆积
                        if (z > oP.Z)
                        {
                            sumV += (z - oP.Z) * unitArea;
                            if (p_change)
                            {
                                // 20230523 Sanngoku : 添加仿真后料堆改变事件
                                IndicesPoint pt = new IndicesPoint()
                                {
                                    BeforeZ = oP.Z,
                                    Z = (float)z,
                                    Index = iIndex,
                                    xIndex = iX,
                                    yIndex = iY,
                                };
                                points.Add(pt);

                                oP.Z = (float)z;
                                this.Points[iIndex] = oP;
                            }

                        }
                    }
                    else
                    {
                        /*
                        *   |\
                        *  h| \
                        *   |-- d （表面点距离中心轴距离）
                        *   |   \
                        *  z|  40\
                        *   ------
                        */

                        float d = (float)Math.Sqrt(i * i + j * j) * Unit;
                        float h = d * (float)Math.Tan(Alfa / 180f * Math.PI);
                        double z = p_targetH - h;  // 加高后的高度

                        // 如果高于堆，说明有散料堆积
                        if (z > oP.Z)
                        {
                            sumV += (z - oP.Z) * unitArea;
                            if (p_change)
                            {
                                // 20230523 Sanngoku : 添加仿真后料堆改变事件
                                IndicesPoint pt = new IndicesPoint()
                                {
                                    BeforeZ = oP.Z,
                                    Z = (float)z,
                                    Index = iIndex,
                                    xIndex = iX,
                                    yIndex = iY,
                                };
                                points.Add(pt);

                                oP.Z = (float)z;
                                this.Points[iIndex] = oP;
                            }

                        }

                    }
                }
            }

            if (p_change)
            {
                SimulatedArgs args = new SimulatedArgs()
                {
                    isChanged = points.Count > 0 && sumV > 0,
                    list_Points = points,
                    Volumn = (float)sumV
                };
                this.SimulatedHandler?.Invoke(this, args);
            }
            return sumV; // 返回总堆积体积
            #endregion
        }

        private double BezierTop(ZXPoint p0, ZXPoint p1, ZXPoint p2,float t)
        {
            float mt = 1 - t;
            float mt2 = mt * mt;
            float t2 = t * t;
            float y = mt2 * p0.Y + 2 * mt * t * p1.Y + t2 * p2.Y;
            return y;
        }

        private int counter = 0; // 迭代次数

        /// <summary>
        /// 堆料仿真（二）指定体积
        /// </summary>
        /// <param name="p_targetV">指定体积</param>
        /// <param name="p_stackPoint">卸料点</param>
        /// <param name="p_obsArea">障碍物范围</param>
        /// <param name="e">精度：越小精度越高，计算越慢</param>
        /// <returns>堆积最高点高度</returns>
        public double SimStackByVolume(double p_targetV, ZXPoint p_stackPoint, ZXBoundary p_obsArea = new ZXBoundary(), double e = 0.0001)
        {

            LibTool.Debug("指定体积堆料仿真 SimStackByVolume() -------------------------------------");

            // 调取参数输出
            LibTool.Log("输入参数： 堆料体积: " + p_targetV + "(立方米) "
                + "落料点坐标：" + p_stackPoint.ToString() + " 体积精度：" + e + "(立方米)");

            this.Bound = this.Points.Boundary;

            LibTool.Debug("仿真堆取前堆料体积：" + this.Volume.ToString("0.000") + "(立方米) ");
            LibTool.Debug("仿真堆取前堆料范围：" + this.Bound.ToString());

            counter = 0;

            // STEP 0: 参数校验
            // STEP 0.1: 判定堆取体积
            if (p_targetV < e)
                LibTool.Error("DotPile659: 目标堆积体积 " + p_targetV + " < 体积精度 " + e);

            // STEP 0.2: 判定落料点
            if (p_stackPoint.X > Bound.MaxX || p_stackPoint.Y > Bound.MaxY ||
                p_stackPoint.X < Bound.MinX || p_stackPoint.Y < Bound.MinY)
            {
                LibTool.Error("DotPile551: 落料点超出料堆边界 " + Bound.ToString()
                    + " 落料点坐标：" + p_stackPoint.ToString());
            }

            int indexStackPoint = this.GetIndex(p_stackPoint.X, p_stackPoint.Y);
            LibTool.Log("落料点下标：" + indexStackPoint);
            if (indexStackPoint < 0 || indexStackPoint >= this.Points.Count)
            {
                LibTool.Error("DotPile665: 落料点下标越界 " + indexStackPoint + " " + this.Points.Count);
            }

            double minH = this.Points[indexStackPoint].Z;  // 堆料高度
            double maxH = p_stackPoint.Z; // 卸料点高度
            double deltaH = this.Unit * 0.4f; // 顶部调整量
            double deltaV = Math.Pow(this.Unit, 2) * deltaH; // 顶部调整体积偏量

            // STEP 0.3: 卸料点高度校验
            if (maxH - minH <= deltaH)
            {
                LibTool.Error("DotPile748: 卸料点必须高于料堆表面高度" + deltaH + "（米） 当前料面高度：" + minH + " 卸料点高度：" + maxH);
            }

            // STEP 0.4: 校验目标堆取体积和最大可堆体积
            double bestH = 0;
            double maxV = this.SimStackByHeight(maxH, p_stackPoint, false, p_obsArea);

            // STEP 1: 求得最佳高度
            if (p_targetV > maxV)
            {
                // CASE 1: 异常分支，目标堆取越界
                LibTool.Debug("Worning_DotPile701: 目标堆料体积 " + p_targetV + "（立方米） 超出最大可堆料体积 " + maxV + "（立方米） 按最大可堆体积仿真堆料");
                bestH = BinarySearchH(minH, maxH, p_stackPoint, maxV + deltaV, p_obsArea, e); // 二分法求解;
            }
            else
            {
                // CASE 2: 常规分支
                bestH = BinarySearchH(minH, maxH, p_stackPoint, p_targetV + deltaV, p_obsArea, e); // 二分法求解
            }

            // STEP 2: 改变料型
            this.SimStackByHeight(bestH, p_stackPoint, true, p_obsArea);
            //Points[indexStackPoint].Z -= (float)deltaH;
            // this.AdjustHightSimStack(indexStackPoint);

            // STEP 3: 更新边界
            this.Bound = Points.Boundary;
            LibTool.Debug("最佳堆取高度：" + bestH.ToString("0.000") + "(米)");
            LibTool.Debug("二分法搜索迭代次数：" + counter);
            LibTool.Debug("仿真堆取后堆料体积：" + this.Volume);
            LibTool.Debug("仿真堆取后堆料范围：" + this.Bound.ToString());

            return bestH; // 返回堆积高度
        }


        /// <summary>
        /// 堆料仿真（二）指定体积
        /// </summary>
        /// <param name="p_targetV">指定体积</param>
        /// <param name="p_stackPoint">卸料点</param>
        /// <param name="top_height"></param>
        /// <param name="p_obsArea">障碍物范围</param>
        /// <param name="e">精度：越小精度越高，计算越慢</param>
        /// <returns>堆积最高点高度</returns>
        public double SimStackByVolume(double p_targetV, ZXPoint p_stackPoint, double top_height, ZXBoundary p_obsArea = new ZXBoundary(), double e = 0.0001)
        {

            LibTool.Debug("指定体积堆料仿真 SimStackByVolume() -------------------------------------");

            // 调取参数输出
            LibTool.Log("输入参数： 堆料体积: " + p_targetV + "(立方米) "
                + "落料点坐标：" + p_stackPoint.ToString() + " 体积精度：" + e + "(立方米)");

            this.Bound = this.Points.Boundary;

            LibTool.Debug("仿真堆取前堆料体积：" + this.Volume.ToString("0.000") + "(立方米) ");
            LibTool.Debug("仿真堆取前堆料范围：" + this.Bound.ToString());

            counter = 0;

            // STEP 0: 参数校验
            // STEP 0.1: 判定堆取体积
            if (p_targetV < e)
                LibTool.Error("DotPile659: 目标堆积体积 " + p_targetV + " < 体积精度 " + e);

            // STEP 0.2: 判定落料点
            if (p_stackPoint.X > Bound.MaxX || p_stackPoint.Y > Bound.MaxY ||
                p_stackPoint.X < Bound.MinX || p_stackPoint.Y < Bound.MinY)
            {
                LibTool.Error("DotPile551: 落料点超出料堆边界 " + Bound.ToString()
                    + " 落料点坐标：" + p_stackPoint.ToString());
            }

            int indexStackPoint = this.GetIndex(p_stackPoint.X, p_stackPoint.Y);
            LibTool.Log("落料点下标：" + indexStackPoint);
            if (indexStackPoint < 0 || indexStackPoint >= this.Points.Count)
            {
                LibTool.Error("DotPile665: 落料点下标越界 " + indexStackPoint + " " + this.Points.Count);
            }

            double minH = this.Points[indexStackPoint].Z;  // 堆料高度
            double maxH = p_stackPoint.Z; // 卸料点高度
            double deltaH = this.Unit * 0.4f; // 顶部调整量
            double deltaV = Math.Pow(this.Unit, 2) * deltaH; // 顶部调整体积偏量

            // STEP 0.3: 卸料点高度校验
            if (maxH - minH <= deltaH)
            {
                LibTool.Error("DotPile748: 卸料点必须高于料堆表面高度" + deltaH + "（米） 当前料面高度：" + minH + " 卸料点高度：" + maxH);
            }

            // STEP 0.4: 校验目标堆取体积和最大可堆体积
            double bestH = 0;
            double maxV = this.SimStackByHeight(maxH, p_stackPoint, top_height, false, p_obsArea);

            // STEP 1: 求得最佳高度
            if (p_targetV > maxV)
            {
                // CASE 1: 异常分支，目标堆取越界
                LibTool.Debug("Worning_DotPile701: 目标堆料体积 " + p_targetV + "（立方米） 超出最大可堆料体积 " + maxV + "（立方米） 按最大可堆体积仿真堆料");
                bestH = BinarySearchH(minH, maxH, p_stackPoint, maxV + deltaV, top_height, p_obsArea, e); // 二分法求解;
            }
            else
            {
                // CASE 2: 常规分支
                bestH = BinarySearchH(minH, maxH, p_stackPoint, p_targetV + deltaV, top_height, p_obsArea, e); // 二分法求解
            }

            // STEP 2: 改变料型
            this.SimStackByHeight(bestH, p_stackPoint, top_height, true, p_obsArea);
            //Points[indexStackPoint].Z -= (float)deltaH;
            // this.AdjustHightSimStack(indexStackPoint);

            // STEP 3: 更新边界
            this.Bound = Points.Boundary;
            LibTool.Debug("最佳堆取高度：" + bestH.ToString("0.000") + "(米)");
            LibTool.Debug("二分法搜索迭代次数：" + counter);
            LibTool.Debug("仿真堆取后堆料体积：" + this.Volume);
            LibTool.Debug("仿真堆取后堆料范围：" + this.Bound.ToString());

            return bestH; // 返回堆积高度
        }

        /// <summary>
        /// 堆料仿真（三）指定包围盒 在原有料堆基础上，按标准梯形堆料
        /// 注：堆料边界不会超出原料堆XY边界，超出部分会自动剔除。返回体积为额外堆部分。
        /// </summary>
        /// <param name="p_minX">最小X</param>
        /// <param name="p_maxX">最大X</param>
        /// <param name="p_minY">最小Y</param>
        /// <param name="p_maxY">最大Y</param>
        /// <param name="p_minZ">最小Z</param>
        /// <param name="p_maxZ">最大Z</param>
        /// <returns></returns>
        public double SimStackByBoundary(float p_minX, float p_maxX, float p_minY, float p_maxY, float p_minZ, float p_maxZ)
        {
            List<IndicesPoint> points = new List<IndicesPoint>();   // 仿真后事件

            double v = 0; // 堆料体积

            // STEP 0: 参数校验
            if (p_minX >= p_maxX || p_minY >= p_maxY || p_minZ >= p_maxZ)
            {
                LibTool.Error("DotPile726: 边界包围盒参数错误，最小值 需 小于 最大值");
            }

            if (this.Unit <= 0)
            {
                LibTool.Error("DotPile731: 料堆格网精度必须大于0");
            }

            ZXBoundary bOrg = this.Bound; // 记录原始料堆边界

            // STEP 1: 按边界参数构造梯形料堆
            DotTrapezoidPile newPile = new DotTrapezoidPile(p_minX, p_maxX, p_minY, p_maxY, p_minZ, p_maxZ, this.Unit);

            // STEP 2: 遍历交叉区域内的所有点
            ZXBoundary bROI = bOrg * newPile.Bound;  // 交叉区域

            int xN = 0;
            for (float x = bROI.MinX; x <= bROI.MaxX; x += this.Unit)
            {
                x = bROI.MinX + xN++ * this.Unit;
                int yN = 0;

                for (float y = bROI.MinY; y <= bROI.MaxY; y += this.Unit)
                {
                    y = bROI.MinY + yN++ * this.Unit;

                    int indexOrg = this.GetIndex(x, y);
                    int indexNew = newPile.GetIndex(x, y);
                    if (indexOrg != -1 && indexNew != -1)
                    {
                        // STEP 3: 如果比原来高，替换
                        if (newPile.Points[indexNew].Z > this.Points[indexOrg].Z)
                        {
                            // 20230523 Sanngoku : 添加仿真后料堆改变事件
                            IndicesPoint pt = new IndicesPoint()
                            {
                                BeforeZ = this.Points[indexOrg].Z,
                                Z = newPile.Points[indexNew].Z,
                                Index = indexOrg,
                                xIndex = (int)Math.Round((x - Bound.MinX) / Unit),
                                yIndex = (int)Math.Round((y - Bound.MinX) / Unit),
                            };
                            points.Add(pt);

                            this.Points[indexOrg].Z = newPile.Points[indexNew].Z;

                            v += (newPile.Points[indexNew].Z - this.Points[indexOrg].Z) * this.Unit * this.Unit;
                        }
                    }
                }
            }

            SimulatedArgs args = new SimulatedArgs()
            {
                isChanged = points.Count > 0 && v > 0,
                list_Points = points,
                Volumn = (float)v
            };
            this.SimulatedHandler?.Invoke(this, args);

            return v;
        }


        // 二分法搜索堆取最佳高度
        private double BinarySearchH(double minH, double maxH, ZXPoint p_stackPoint, double targetV, ZXBoundary p_obsArea = new ZXBoundary(), double e = 0.0001 )
        {
            // STEP 0: 参数校验

            // STEP 0.1: 判定已经收敛
            double avgH = (minH + maxH) * 0.5;
            if (targetV < e)
            {
                LibTool.Error("DotPile659: 目标堆积体积 " + targetV + " < 体积精度 " + e);
            }

            // STEP 0.2: 参数合法性校验 高度最小值比最大值高
            if (minH > maxH)
            {
                LibTool.Error("DotPile697: 堆料最小高度" + minH + " > 堆料最大高度" + maxH + " 卸料点已埋入堆料中");
            }

            double minV = this.SimStackByHeight(minH, p_stackPoint, false, p_obsArea);
            double maxV = this.SimStackByHeight(maxH, p_stackPoint, false, p_obsArea);

            // STEP 0.3: 参数合法性校验 目标体积应介于最小最大之间
            if (minV > maxV)
            {
                LibTool.Error("DotPile707: 堆料仿真 最小体积" + minV + " > 最小体积" + maxV);
            }
            else if (targetV < minV)
            {
                LibTool.Error("DotPile711: 堆料仿真 目标体积" + targetV + " < 最小体积" + minV);
            }
            else if (targetV > maxV)
            {
                LibTool.Error("DotPile714: 堆料仿真 目标体积" + targetV + " > 最大体积" + maxV);
            }

            // STEP 1: 迭代终止条件 返回

            counter++;
            if (counter > 1000)
                return maxH;

            if (maxV - minV < e)
                return avgH;

            if (targetV - minV < e)
                return minH;
            else if (maxV - targetV < e)
                return maxH;

            // STEP 2: 判定继续二分法搜索范围
            if (this.SimStackByHeight(avgH, p_stackPoint, false, p_obsArea) > targetV)
                maxH = avgH;
            else
                minH = avgH;

            // STEP 3: 递归调用二分法搜索
            return BinarySearchH(minH, maxH, p_stackPoint, targetV, p_obsArea, e);
        }


        // 二分法搜索堆取最佳高度
        private double BinarySearchH(double minH, double maxH, ZXPoint p_stackPoint, double targetV, double top_height, ZXBoundary p_obsArea = new ZXBoundary(), double e = 0.0001)
        {
            // STEP 0: 参数校验

            // STEP 0.1: 判定已经收敛
            double avgH = (minH + maxH) * 0.5;
            if (targetV < e)
            {
                LibTool.Error("DotPile659: 目标堆积体积 " + targetV + " < 体积精度 " + e);
            }

            // STEP 0.2: 参数合法性校验 高度最小值比最大值高
            if (minH > maxH)
            {
                LibTool.Error("DotPile697: 堆料最小高度" + minH + " > 堆料最大高度" + maxH + " 卸料点已埋入堆料中");
            }

            double minV = this.SimStackByHeight(minH, p_stackPoint, top_height, false, p_obsArea);
            double maxV = this.SimStackByHeight(maxH, p_stackPoint, top_height, false, p_obsArea);

            //LibTool.Debug($"counter = {counter}, minH = {minH}, maxH = {maxH}, minV = {minV}, maxV = {maxV}, targetV = {targetV}");
            // STEP 0.3: 参数合法性校验 目标体积应介于最小最大之间
            if (minV > maxV)
            {
                LibTool.Error("DotPile707: 堆料仿真 最小体积" + minV + " > 最小体积" + maxV);
            }
            else if (targetV < minV)
            {
                LibTool.Error("DotPile711: 堆料仿真 目标体积" + targetV + " < 最小体积" + minV);
            }
            else if (targetV > maxV)
            {
                LibTool.Error("DotPile714: 堆料仿真 目标体积" + targetV + " > 最大体积" + maxV);
            }

            // STEP 1: 迭代终止条件 返回

            counter++;
            if (counter > 1000)
                return maxH;

            if (maxV - minV < e)
                return avgH;

            if (targetV - minV < e)
                return minH;
            else if (maxV - targetV < e)
                return maxH;

            // STEP 2: 判定继续二分法搜索范围
            if (this.SimStackByHeight(avgH, p_stackPoint, top_height, false, p_obsArea) > targetV)
                maxH = avgH;
            else
                minH = avgH;

            // STEP 3: 递归调用二分法搜索
            return BinarySearchH(minH, maxH, p_stackPoint, targetV, top_height, p_obsArea, e);
        }


        /// <summary>
        /// 抓斗卸料仿真（第二版）
        /// </summary>
        public bool SimStackByGrab(DotDigger grab, ZXPoint center, float p_targetV, ZXBoundary p_obsArea = new ZXBoundary())
        {
            this.Unit = 0.5f;  // 【临时】【未完待续】

            if (! this.Points.Boundary.ContainXY(center))
            {
                return false;
            }

            ZXPoint dropPoint = new ZXPoint(center.X, center.Y, 15);

            this.SimStackByVolume(p_targetV, dropPoint, p_obsArea);

            return true;
        }



        /// <summary>
        /// 堆料仿真（四）抓斗卸料仿真 (ver 0.6.0)
        /// </summary>
        /// <param name="grab">抓斗模型</param>
        /// <param name="center">卸料点（只取X和Y）</param>
        /// <param name="p_targetV">卸料体积（抓斗内的体积）</param>
        /// 
        /*
        public void SimStackByGrab(DotDigger grab, ZXPoint center, float p_targetV)
        {
            List<IndicesPoint> points = new List<IndicesPoint>();   // 仿真后事件

            this.Bound = this.Points.Boundary; // 2023.09.23 重新计算边界

            // STEP 0: 参数校验
            // 卸料中心点下标
            int iOX = this.GetIndexXY(center)[0];
            int iOY = this.GetIndexXY(center)[1];

            if (iOX < 0)
                LibTool.Error("DotPile_989：抓斗卸料点 x下标越界 " + center.X + "  " + this.Bound.ToString());

            if (iOY < 0)
                LibTool.Error("DotPIle_994: 抓斗卸料点 y下标越界 " + center.Y + "  " + this.Bound.ToString());

            // STEP 1: 初始化
            // 抓斗基准参数：(从grab中获取)
            // 横向 2m;  纵向 3m;
            // 精度 0.1f 从 pile中获取
            int startI = (int)(iOX - grab.Points.Bound.L / 2.0f / this.Unit);
            int endI = (int)(iOX + grab.Points.Bound.L / 2.0f / this.Unit);

            // STEP 2: 拆分成3次足够，纵向 幅度固定，横向取决于抓斗模型grab
            for (int k = 0; k < 3; k++)
            {
                // STEP 2.0: 判断剩余体积量
                if (p_targetV <= 0)
                    break;

                // STEP 2.1: 确定当次卸料体积
                float v = (p_targetV > 1 && k < 2) ? 1 : p_targetV;
                p_targetV -= v;

                // STEP 2.2: 计算Y方向坐标范围
                int startJ = (int)Math.Round((iOY - 1.0 * (k + 1) / 2.0f / this.Unit));
                int endJ = (int)Math.Round((iOY + 1.0 * (k + 1) / 2.0f / this.Unit));

                // Console.WriteLine("startJ:" + startJ + ", endJ:" + endJ);

                // STEP 2.3: 遍历、每个点均摊体积
                int num = (endI - startI + 1) * (endJ - startJ + 1);

                for (int i = startI; i <= endI; i++)
                {
                    for (int j = startJ; j <= endJ; j++)
                    {
                        int index = this.GetIndex(i, j);
                        if (index < 0)
                        {
                            // Console.WriteLine("越界:" + i + ", " + j + "  " + index);
                            continue;
                        }
                        // this.Points[index].Z += v / (grab.Points.Bound.L * 1.0f * (k+1));

                        float dv = v / (1.0f * num) / (this.Unit * this.Unit);

                        // 20230523 Sanngoku : 添加仿真后料堆改变事件

                        if (this.SimulatedHandler != null)
                        {
                            IndicesPoint pt = new IndicesPoint()
                            {
                                BeforeZ = this.Points[index].Z,
                                Z = this.Points[index].Z + dv,
                                Index = index,
                                xIndex = i,
                                yIndex = j,
                            };
                            points.Add(pt);
                        }

                        this.Points[index].Z += dv;
                    }
                }
            }

            ZXBoundary ROI = new ZXBoundary(
                center.X - grab.Points.Bound.L, center.X + grab.Points.Bound.L,
                center.Y - grab.Points.Bound.W, center.Y + grab.Points.Bound.W, 0, 15);

            // STEP 3: 模拟塌料
            
            int counter = 100; // 最多循环100次
            while (counter-- > 0)
            {
                bool slide = this.SimSlide(ROI);  // 可优化
                if (!slide)
                {
                    // 如果不会塌方，跳出
                    break;
                }
            }
            
            SimulatedArgs args = new SimulatedArgs()
            {
                isChanged = points.Count > 0 && p_targetV > 0,
                list_Points = points,
                Volumn = (float)p_targetV
            };
            this.SimulatedHandler?.Invoke(this, args);

        }
        */

        /// <summary>
        /// 取料仿真——任意取料头形状
        /// </summary>
        /// <param name="digger">取料头表面点云</param>
        /// <param name="p_changeOrg">改变原始点云</param>
        /// <returns>取料总体积数</returns>
        public float SimTake(ZXPointSet digger, bool p_changeOrg = true)
        {
            // List<IndicesPoint> points = new List<IndicesPoint>();   // 仿真后事件

            bool full = this.Points.IsFull();
            // LibTool.Debug("料型补满：" + full);
            if (!full) {

                return 0;
            }

            digger.Gridding(Unit);

            float sum = 0;

            // 遍历取料头的所有点
            for (int i = 0; i < digger.Count; i++)
            {
                // 根据X Y 值找到对应的下标
                int index = GetIndex(digger[i].X, digger[i].Y);
 
                if (index < 0 || index >= Points.Count)
                {
                    continue; // 越界跳过
                }

                if (digger[i].Z < Points[index].Z)
                {
                    // LibTool.Debug(index + ":" + digger[i].X + "," + digger[i].Y + "," + digger[i].Z + " Points.Z:" + Points[index].Z);
                    sum += Points[index].Z - digger[i].Z;
                    if (p_changeOrg)
                    {
                        // 20230523 Sanngoku : 添加仿真后料堆改变事件
                        /*
                        if (this.SimulatedHandler != null)
                        {
                            IndicesPoint pt = new IndicesPoint()
                            {
                                BeforeZ = this.Points[index].Z,
                                Z = digger[i].Z,
                                Index = index,
                                xIndex = (int)Math.Round((digger[i].X - Bound.MinX) / Unit),
                                yIndex = (int)Math.Round((digger[i].Y - Bound.MinX) / Unit),
                            };
                            points.Add(pt);
                        }
                        */

                        Points[index].Z = digger[i].Z;
                    }
                }
            }

            // LibTool.Debug("sum = " + sum);

            /*
            SimulatedArgs args = new SimulatedArgs()
            {
                isChanged = points.Count > 0 && sum > 0,
                list_Points = points,
                Volumn = -sum * Unit * Unit
            };
            this.SimulatedHandler?.Invoke(this, args);
            */
            return sum * Unit * Unit;  // 根据格网精度计算体积
        }

        /// <summary>
        /// 取料仿真
        /// </summary>
        /// <param name="digger">取料机构对象</param>
        /// <param name="p_changeOrg"></param>
        /// <returns></returns>
        public float SimTake(DotDigger digger, bool p_changeOrg = true)
        {
            return SimTake(digger.GetGlobalPoints(), p_changeOrg);
        }

        /// <summary>
        /// 取料仿真——抓斗
        /// </summary>
        /// <param name="digger">取料机构——抓斗模型</param>
        /// <param name="center">取料点（输入输出值）</param>
        /// <param name="p_in">抓斗进尺</param>
        /// <param name="p_changeOrg"></param>
        /// <returns>抓取体积</returns>
        public float SimTakeByGrab(DotDigger digger, ref ZXPoint center, float p_in, bool p_changeOrg = true)
        {
            LibTool.Debug("========= 抓斗取料仿真：SimTakeByGrab() ");

            float v = 0;

            this.Points.Gridding(this.Unit); // 2023.07.29 先格网化，调整位置

            this.Bound = this.Points.Boundary;

            LibTool.Debug("STEP 0: 料型范围 Bound:" + this.Bound.ToString()) ;
            LibTool.Debug("STEP 0: 抓取位置 center:" + center.ToString() + " 格网精度 Unit：" + this.Unit + " 抓斗进尺 p_in：" + p_in);

            // STEP 1: 设置取料机构位置
            digger.Position = center;

            // STEP 2: 获取边缘
            ZXPointSet ps = digger.GetGlobalPoints();
            ps.Gridding(this.Unit); // 0.5 米 格网化
            ZXBoundary b = ps.Boundary;
            LibTool.Debug("STEP 1: 取料机构边界：" + b.ToString());

            // STEP 3: 按抓斗边界(X, Y)取ROI
            b.MinZ = float.MinValue;
            b.MaxZ = float.MaxValue;

            ZXPointSet psROI = this.Points.Intercept(b, false); // 抓取区域

            // STEP 4: 以上下两边高度为基准，取最高点，即为抓取基准点
            LibTool.Debug("STEP 2: 抓取区域边界: " + psROI.Boundary);

            // psROI.Bound = psROI.Boundary; // 2023.07.11

            ZXPoint pMaxZ = new ZXPoint(0, 0, 0);
            // LibTool.Debug("    有效区域长度: " + (psROI.LengthN - 1));
            for (int i = 0; i < psROI.LengthN - 1; i++)
            {
                ZXPoint p0 = psROI.Get(i, 0);
                ZXPoint p1 = psROI.Get(i, psROI.WidthN - 1);
                if (p0.Z > pMaxZ.Z)
                {
                    pMaxZ = p0;
                }
                if (p1.Z > pMaxZ.Z)
                {
                    pMaxZ = p1;
                }
            }
            LibTool.Debug("STEP 3: 抓取边缘最高点: " + pMaxZ.ToString());
            // STEP 5: 按抓取基准点高度，重新移动抓斗

            center.Z = pMaxZ.Z - p_in;

            // LibTool.Debug("MinX: " + this.Bound.MinX);
            // LibTool.Debug("MinY: " + this.Bound.MinY);
            // center.X = center.X - this.Bound.MinX; // 2023.7.27 14:27
            // center.Y = center.Y - this.Bound.MinY; // 2023.7.27 14:27

            LibTool.Debug("STEP 4: 调整后中心点: " + center.ToString());

            digger.Position = center;

            // STEP 6: 执行仿真取料，返回取料量
            v = SimTake(digger, p_changeOrg);

            this.Points.Gridding(this.Unit); // 2023.07.29 格网化，调整位置

            return v;
        }

        /// <summary>
        /// 塌料仿真 —— 滑动窗口法 (使用条件：满秩阵)
        /// </summary>
        /// <param name="p_winR"></param>
        /// <returns></returns>
        public bool SimSlide(float p_winR, float p_rate)
        {
            // STEP 0: 初始化更新的点
            ZXPointSet psUpdate = new ZXPointSet();
            ZXBoundary bPile = this.Bound;

            LibTool.Debug("格网精度：" + this.Unit);

            for (float x = bPile.MinX + p_winR; x <= bPile.MaxX - p_winR; x += Unit)
            {
                x = (float)(Math.Round(x / this.Unit) * this.Unit);
                LibTool.Debug(x);
                for (float y = bPile.MinY + p_winR; y <= bPile.MaxY - p_winR; y += Unit)
                {
                    y = (float)(Math.Round(y / this.Unit) * this.Unit);

                    ZXBoundary bWin = new ZXBoundary(x - p_winR, x + p_winR, y - p_winR, y + p_winR, -99999, 99999);
                    ZXPointSet psWin = this.Points.Intercept(bWin, false);
                    bWin = psWin.Boundary;

                    if (bWin.H > p_winR * 2 * p_rate)
                    {
                        float sumZ = 0;
                        for (int i = 0; i < psWin.Count; i++)
                        {
                            sumZ += psWin[i].Z;
                        }
                        float avgZ = sumZ / psWin.Count;

                        psUpdate.Add(x, y, avgZ);
                    }
                }
            }

            this.Points.Update(psUpdate);

            return true;
        }


        /// <summary>
        /// 计算模拟抓取体积
        /// </summary>
        /// <param name="digger"></param>
        /// <param name="center">返回抓取高度</param>
        /// <returns>抓取体积</returns>
        public float ComputeTakeVolume(DotDigger digger, ref ZXPoint center)
        {
            // this.debug.Log("计算模拟抓取体积：X: " + center.X + "  Y: " + center.Y);

            int index = this.GetIndex(center.X, center.Y);

            // this.debug.Log("求得下标 index: " + index);

            // 如果抓取点高度低于阈值，返回评估值为0，不可抓取
            if (this.Points[index].Z <= 0.1f)
            {
                return 0;
            }

            // 评估用仿真计算，只计算体积，不改变料型本身
            float volume = this.SimTakeByGrab(digger, ref center, 0.25f, false);

            return volume;
        }

        /// <summary>
        /// 计算评估值
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public float ComputeValue(float volume, ZXPoint center)
        {
            const float w1 = 0.3f;
            const float w2 = 0.7f;

            // 评分标准【未完待续】
            float value = volume / 2 * w1 + center.Z / 7 * w2;

            return value;
        }

        #endregion
    }
}
