//=====================================================================
// 模块名称：点云滤波工具类 DotFilter
// 功能简介：进行插入点操作
// 版权声明：2021 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2021.12.13 杜子兮 创建
//============================================

using System;
using System.Security.Principal;

namespace DotCloudLib
{
    /// <summary>
    /// 点云滤波工具类
    /// </summary>
    public static class DotFilter
    {
        /// <summary>
        /// 提取边缘
        /// </summary>
        /// <param name="orgPointSet">满秩阵</param>
        /// <returns>边缘点集</returns>
        public static ZXPointSet Edge(ZXPointSet orgPointSet)
        {
            ZXPointSet ps = new ZXPointSet();

            // 校验，是否为满点阵
            if (!orgPointSet.IsFull())
            {
                LibTool.Error("DotFilter27: 不是满点阵，无法使用提取边缘功能 Edge()");
            }

            for (int i = 0; i < orgPointSet.Count; i++)
            {
                if (orgPointSet[i].IsEmpty())
                {
                    continue;
                }

                ZXPoint pLeft = orgPointSet.GetPoint(i, Dir.Left);
                ZXPoint pRight = orgPointSet.GetPoint(i, Dir.Right);
                ZXPoint pUp = orgPointSet.GetPoint(i, Dir.Up);
                ZXPoint pDown = orgPointSet.GetPoint(i, Dir.Down);

                if (pLeft.IsEmpty() || pRight.IsEmpty() || pUp.IsEmpty() || pDown.IsEmpty())
                {
                    ps.Add(orgPointSet[i]); 
                }
            }

            return ps;
        }

        /// <summary>
        /// 高斯滤波 Sanngoku 20251027
        /// </summary>
        /// <param name="orgPointSet"></param>
        /// <param name="radius">滤波半径</param>
        /// <returns></returns>
        public static ZXPointSet Gaussian(ZXPointSet orgPointSet, int radius)
        {
            int kernelSize = 2 * radius + 1;
            double sigma = radius;  // 标准差与半径关联（可根据需求调整）
            float[,] kernel = GenerateGaussianKernel(kernelSize, sigma);

            ZXPointSet resultPoints = new ZXPointSet();
            resultPoints.Unit = orgPointSet.Unit;

            for (int i = 0; i < orgPointSet.LengthN; i++)
            {
                for(int j = 0;j < orgPointSet.WidthN; j++)
                {
                    float sumZ = 0;
                    float weightSum = 0;
                    for (int ii = 0;ii < kernelSize; ii++)
                    {
                        for(int jj = 0;jj < kernelSize; jj++)
                        {
                            int srcRow = i + ii - radius;
                            int srcCol = j + jj - radius;
                            if (srcRow >= 0 && srcRow < orgPointSet.LengthN && srcCol >= 0 && srcCol < orgPointSet.WidthN)
                            {
                                sumZ += orgPointSet.Get(srcRow, srcCol).Z * kernel[ii, jj];
                                weightSum += kernel[ii, jj];
                            }
                        }
                    }

                    // 计算平滑后的Z值（避免权重和为0的极端情况）
                    var pt = orgPointSet.Get(i, j);
                    float smoothedZ = weightSum > 0 ? sumZ / weightSum : pt.Z;
                    resultPoints.Add(new ZXPoint(pt.X, pt.Y, smoothedZ));
                }
            }

            return resultPoints;
        }

        /// <summary>
        /// 生成归一化的高斯核
        /// </summary>
        /// <param name="size">核大小（必须为奇数）</param>
        /// <param name="sigma">高斯函数标准差</param>
        /// <returns>归一化后的高斯核</returns>
        private static float[,] GenerateGaussianKernel(int size, double sigma)
        {
            float[,] kernel = new float[size, size];
            float sum = 0;
            int center = size / 2;  // 核中心坐标

            // 计算每个位置的高斯值
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    // 计算相对于中心的偏移
                    int dx = i - center;
                    int dy = j - center;

                    // 二维高斯函数：G(x,y) = (1/(2πσ²)) * exp(-(x²+y²)/(2σ²))
                    // 省略常数项（后续归一化会抵消）
                    kernel[i, j] = (float)Math.Exp(-(dx * dx + dy * dy) / (2 * sigma * sigma));
                    sum += kernel[i, j];
                }
            }

            // 归一化核（确保权重和为1）
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

            return kernel;
        }
    }
}
