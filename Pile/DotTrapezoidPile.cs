//=====================================================================
// 模块名称：梯形料堆点云 DotTrapezoidPile
// 功能简介：继承DotPile，可生成梯形料堆
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.7.27 杜子兮 创建
//============================================
using System;
using System.Collections.Generic;
using System.Text;

namespace DotCloudLib
{
    /// <summary>
    /// 梯形料堆
    /// </summary>
    public class DotTrapezoidPile : DotPile
    {
        /// <summary>
        /// 梯形料堆最大堆料半径
        /// </summary>
        public double R
        {
            get { return Bound.H / Math.Tan((double)(Alfa / 180 * Math.PI)); }
        }

        /// <summary>
        /// 梯形料堆上表面宽度（Y方向差）
        /// </summary>
        public double TopW
        {
            get { return Bound.W - R * 2; }
        }

        /// <summary>
        /// 梯形料堆上表面长度（X方向差）
        /// </summary>
        public double TopL
        {
            get { return Bound.L - R * 2; }
        }

        /// <summary>
        /// 构造方法（理想料堆）
        /// </summary>
        /// <param name="p_minX">料条起始位</param>
        /// <param name="p_maxX">料条终止位</param>
        /// <param name="p_minY">近机点</param>
        /// <param name="p_maxY">远机点</param>
        /// <param name="p_minZ">地面高度</param>
        /// <param name="p_maxZ">最高高度</param>
        /// <param name="p_unit">格网精度</param>
        public DotTrapezoidPile(float p_minX, float p_maxX, float p_minY, float p_maxY, float p_minZ, float p_maxZ, float p_unit) 
            :base(p_minX, p_maxX, p_minY, p_maxY, p_minZ, p_maxZ, p_unit)
        {
            Bound = new ZXBoundary(p_minX, p_maxX, p_minY, p_maxY, p_minZ, p_maxZ);

            Points = new ZXPointSet();
            Points.Bound = Bound; // 2022.7.20
            Points.Unit = p_unit; // 2022.7.20
            base.m_unit = p_unit;

            CreatePileData();
        }

        /// <summary>
        /// 参数合法性校验
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {

            if (TopW < 0 || TopL < 0)
            {
                // 不能形成标准梯形
                MetaData.MESSAGE += "不能形成标准梯形";
                return false;
            }

            if (R < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生成标准梯形料堆格网点云数据
        /// </summary>
        /// <returns></returns>
        public bool CreatePileData()
        {
            // 计算关键分割点（梯形上表面四个角点）
            double topX0 = Bound.MinX + R;
            double topY0 = Bound.MinY + R;
            double topX1 = Bound.MaxX - R;
            double topY1 = Bound.MaxY - R;

            // 计算料堆左下角格网坐标
            double oX = Math.Round(Bound.MinX / Unit) * Unit;
            double oY = Math.Round(Bound.MinY / Unit) * Unit;

            // 按区逐点生成
            for (int i = 0; i < LengthN; i++)
            {
                for (int j = 0; j < WidthN; j++)
                {
                    // STEP 1: 计算每个格点对应的XY坐标
                    double x = oX + i * Unit;
                    double y = oY + j * Unit;
                    double z = 0; // 计算每个格点对应的高程值

                    // STEP 2: 判断所述区域
                    double d = 0;
                    if (x >= topX0 && x <= topX1 && y >= topY0 && y <= topY1)
                    {  // CASE 0: 顶部
                        z = Bound.MaxZ;
                    }
                    else
                    {
                        if (x < topX0 && y < topY0)
                        {   // CASE 1: 左下角
                            d = Math.Sqrt((x - topX0) * (x - topX0) + (y - topY0) * (y - topY0));
                        }
                        else if (x < topX0 && y > topY1)
                        {   // CASE 2: 左上角
                            d = Math.Sqrt((x - topX0) * (x - topX0) + (y - topY1) * (y - topY1));
                        }
                        else if (x > topX1 && y > topY1)
                        {   // CASE 3: 右上角
                            d = Math.Sqrt((x - topX1) * (x - topX1) + (y - topY1) * (y - topY1));
                        }
                        else if (x > topX1 && y < topY0)
                        {   // CASE 4: 右下角
                            d = Math.Sqrt((x - topX1) * (x - topX1) + (y - topY0) * (y - topY0));
                        }
                        else if (x < topX0)
                        {   // CASE 5: 左边
                            d = topX0 - x;
                        }
                        else if (x > topX1)
                        {   // CASE 6: 右边
                            d = x - topX1;
                        }
                        else if (y < topY0)
                        {   // CASE 7: 下边
                            d = topY0 - y;
                        }
                        else if (y > topY1)
                        {   // CASE 7: 上边
                            d = y - topY1;
                        }

                        z = Bound.MaxZ - d * Math.Tan(this.Alfa / 180 * Math.PI);
                        if (z < Bound.MinZ) z = Bound.MinZ;
                        if (z > Bound.MaxZ) z = Bound.MaxZ;
                    }
                    // 添加
                    Points.Add((float)x, (float)y, (float)z);
                }
            }

            return true;
        }

    }
}
