//=====================================================================
// 模块名称：料条点云 DotPileLine
// 功能简介：一段料条的属性与行为
//           1. 从料条中识别料堆起止位置
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 当前版本：2020.7.23
// 更新履历：2020.7.21 杜子兮 创建
//          2021.1.22 杜子兮 添加构造方法
//============================================

using System;
using System.Collections;
using System.Collections.Generic;

namespace DotCloudLib
{
    /* 坐标系约定：算法坐标系
     * Y    
     * |   ___________________________
     * |  |       料条（俯视图）      |
     * |   ___________________________
     * 0 -------------------------------------> X
     */

    /// <summary>
    /// 料条
    /// </summary>
    public class DotPileLine : IList<DotPile>
    {
        /// <summary>
        /// 包含的料堆
        /// </summary>
        private List<DotPile> m_piles;

        /// <summary>
        /// 料条点集（未分割的）
        /// </summary>
        public ZXPointSet Points;

        /// <summary>
        /// 格网精度
        /// </summary>
        public float Unit = 1f;

        /// <summary>
        /// 料条边界
        /// </summary>
        public ZXBoundary Bound;

        /// <summary>
        /// X方向点数
        /// </summary>
        public uint LengthN
        {
            get { return (uint)Math.Round((Bound.MaxX - Bound.MinX) / Unit + 1); }
        }

        /// <summary>
        /// Y方向点数
        /// </summary>
        public uint WidthN
        {
            get { return (uint)Math.Round((Bound.MaxY - Bound.MinY) / Unit + 1); }
        }

        /// <summary>
        /// 根据index下标和方位获取点
        /// </summary>
        /// <param name="p_index">下标</param>
        /// <param name="p_point">保存最终获取的点</param>
        /// <param name="p_dir">方向（不做越界校验，依次延续。最下边点的下边会取左边的最上边）</param>
        /// <returns></returns>
        public bool GetPoint(uint p_index, out ZXPoint p_point, Dir p_dir = Dir.Current)
        {
            int index = -1;
            switch (p_dir)
            {
                case Dir.Current:
                    index = (int)p_index; break;
                case Dir.Left:
                    index = (int)(p_index - WidthN); break;
                case Dir.Up:
                    index = (int)p_index + 1; break;
                case Dir.Right:
                    index = (int)(p_index + WidthN); break;
                case Dir.Down:
                    index = (int)(p_index - 1); break;
                case Dir.LeftDown:
                    index = (int)(p_index - WidthN - 1); break;
                case Dir.LeftUp:
                    index = (int)(p_index - WidthN + 1); break;
                case Dir.RightUp:
                    index = (int)(p_index + WidthN - 1); break;
                case Dir.RightDown:
                    index = (int)(p_index + WidthN + 1); break;
            }

            // index校验
            if (index >= 0 && index < Points.Count)
            {
                p_point = Points[index];
                return true;
            }
            else
            {
                p_point = null;
                return false;  // 异常返回false
            }
        }


        /// <summary>
        /// 构造方法
        /// 已知：处理后的料条点云
        /// 求：对应的料条对象
        /// </summary>
        /// <param name="p_points"></param>
        public DotPileLine(ZXPointSet p_points)
        {
            Points = p_points;
            Bound = Points.Boundary;  // 重新计算边界
            m_piles = new List<DotPile>();
        }

        /// <summary>
        /// 构造方法
        /// 已知：一个料堆
        /// 求：料条
        /// </summary>
        /// <param name="p_pile">一个料堆对象</param>
        public DotPileLine(DotPile p_pile)
        {
            m_piles = new List<DotPile>();
            m_piles.Add(p_pile);
            Points = new ZXPointSet(p_pile.Points);

            this.Unit = p_pile.Unit;  // 2024.03.29 +
            Bound = Points.Boundary;  // 重新计算边界
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="points">点云数据</param>        
        public DotPileLine(List<string> points)
        {
            Points = new ZXPointSet();

            if (points != null && points.Count > 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                     string[] xyz = points[i].Split(',');
                     Points.Add(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2]));
                }
            }
           
            Unit = Points[1].Y - Points[0].Y;

            Bound = Points.Boundary;  // 重新计算边界
            m_piles = new List<DotPile>();
        }

        /// <summary>
        /// 构造方法
        /// 已知：本地文件名
        /// </summary>
        /// <param name="p_filePath">文件路径</param>
        /// <param name="p_format">点格式</param>
        public DotPileLine(string p_filePath, PointFormat p_format)
        {
            Points = new ZXPointSet();
            switch (p_format)
            {
                case PointFormat.XYZ:
                    Points.LoadFromXYZ(p_filePath);
                    break;
                case PointFormat.IXYZ:
                    Points.LoadFromIXYZ(p_filePath);
                    break;
                default:
                    break;
            }

            Unit = Points[1].Y - Points[0].Y;

            Bound = Points.Boundary;  // 重新计算边界
            m_piles = new List<DotPile>();
        }

        /// <summary>
        /// 格网化
        /// </summary>
        /// <param name="unit"></param>
        public void Gridding(float unit)
        {
            Points.Gridding(unit);
            Unit = unit;
        }

        /// <summary>
        /// 自动识别聊条中的堆料
        /// </summary>
        /// <param name="p_CutHight">等高线高度</param>
        /// <param name="p_tSpan">判定分割阈值</param>
        /// <returns></returns>
        public List<DotPileInfo> RecognizePiles(float p_CutHight = 3, float p_tSpan = 3)
        {
            List<DotPileInfo> infos = new List<DotPileInfo>();

            // STEP 1: 等高线截取
            ZXPointSet ps = this.Points.GetContour(this.Bound.MinZ + p_CutHight, 0.5f);

            // STEP 2: 等高线下采样
            // ps.DownSample(5);
            ps.Sort(); // 重新排序


            // STEP 3: 粗分 按X值粗分等高线
            List<ZXPointSet> pilesContour = new List<ZXPointSet>();
            ZXBoundary roiBound = ps.Boundary;

            for (int i = 0; i < ps.Count - 1; i++)
            {
                if (ps[i + 1].X - ps[i].X > p_tSpan)
                {
                    // 判定X方向不连续
                    float cutX = (ps[i + 1].X + ps[i].X) * 0.5f;

                    roiBound.MaxX = cutX;
                    pilesContour.Add(ps.Intercept(roiBound, false));
                    roiBound.MaxX += 0.1f;
                    roiBound.MinX = cutX;
                }
            }

            roiBound.MaxX = ps.Boundary.MaxX;
            pilesContour.Add(ps.Intercept(roiBound, false));  // 最后剩下的放进去

            for (int i = 0; i < pilesContour.Count; i++)
            {
                if (pilesContour[i].Bound.L < 5)
                    continue;

                // 根据料型重新计算料堆范围 0.9.5 对应 2022.11.29
                ZXBoundary bPile = pilesContour[i].Boundary + p_CutHight;
                bPile.MinZ = float.MinValue;
                bPile.MaxZ = float.MaxValue;
                ZXPointSet psPile = this.Points.Intercept(bPile, false);
                bPile = psPile.Boundary;

                DotPileInfo info = new DotPileInfo(bPile);
                infos.Add(info);
            }

            // --------以下【未完待续】------------------

            // STEP 4: 精分 以连通性为依准精分
            // 特殊料型的分割（两个料型包围盒在X轴方向有交集）
            // 先验前提：相对闭合曲线的空间连续性

            /*
            const float n = 2; // 窗口大小 n x n 米

            // 遍历所有的粗分等高线
            for (int i = 0; i < pilesContour.Count; i++)
            {
                Console.WriteLine("点数：" + pilesContour[i].Count);

                // 算法：
                ZXBoundary bTotal = pilesContour[i].Boundary;
                ZXPointSet psTotal = pilesContour[i];

                // 标记数组定义（米单位格网）
                int w = (int)Math.Round(pilesContour[i].Boundary.L) + 1;
                int l = (int)Math.Round(pilesContour[i].Boundary.W) + 1;

                // 0: 初始化;   1: 已搜索; 有   8: 已搜索没有
                int[,] flag = new int[w + 1, l + 1];

                for (int j = 0; j <= w; j++)
                {
                    for (int k = 0; k <= l; k++)
                    {
                        ZXBoundary win = bTotal;     
                        win.MinX += j * n;
                        win.MaxX = win.MinX + n;
                        win.MinY += k * n;
                        win.MaxY += win.MinY + n;

                        ZXPointSet roi = psTotal.CutOff(win);

                        if (roi.Count > 0)
                        {
                            flag[j, k] = 1;
                            
                        } else
                        {
                            flag[j, k] = 8;
                        }
                    }
                }

                for (int j = 0; j <= w; j++)
                {
                    for (int k = 0; k <= l; k++)
                    {
                        Console.Write(flag[j,k]);
                    }
                    Console.WriteLine();
                }

                // STEP 4.1: 按第一个点为搜索始点
                ZXPoint p0 = pilesContour[i][0];

                // STEP 4.2: 对周边8个方位进行窗口采样
                ZXBoundary win = bTotal;

                for (int iX = -1; iX <= 1; iX++)
                {
                    for (int iY = -1; iY <= 1; iY++)
                    {
                        if (iX == 0 && iY == 0)
                            continue; // 中间格子不关注

                        win.MinX = p0.X - n * 0.5f + iX;
                        win.MaxX = p0.X + n + 0.5f + iX;
                        win.MinY = p0.Y - n * 0.5f + iY;
                        win.MaxY = p0.Y + n * 0.5f + iY;
                    }
                }
 

                // STEP 4.4: 采用中有点，纳入当前集合


                // STEP 4.5: 窗口边缘，广度优先搜索


                // STEP 4.6: 循环遍历，直至所有窗口边缘不再有点


                // STEP 4.7: 对与剩下的点重复上述 1 ~ 6 直至原点集不再


            }
            */

            return infos;  // 返回信息
        }




        #region IList 接口成员实现

        /// <summary>
        /// 料条中包含料堆数量
        /// </summary>
        public int Count
        {
            get
            {
                return m_piles.Count;
            }
        }

        /// <summary>
        /// 是否为只读
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 料条中索引形式访问料堆
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DotPile this[int index]
        {
            get
            {
                return m_piles[index];
            }

            set
            {
                m_piles[index] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(DotPile item)
        {
            return m_piles.IndexOf(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, DotPile item)
        {
            m_piles.Insert(index, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            m_piles.RemoveAt(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(DotPile item)
        {
            m_piles.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            m_piles.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(DotPile item)
        {
            return m_piles.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(DotPile[] array, int arrayIndex)
        {
            m_piles.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(DotPile item)
        {
            return m_piles.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<DotPile> GetEnumerator()
        {
            return m_piles.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_piles.GetEnumerator();
        }


        #endregion
    }
}
