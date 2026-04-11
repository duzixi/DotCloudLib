//=====================================================================
// 模块名称：通用库 CommonLib
// 功能简介：保存类库的基本信息
// 版权声明：2023 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：
// 2023.04.01 添加欧式距离计算方法 - Sanngoku
//============================================

using System;
using System.IO;

namespace DotCloudLib
{
    /// <summary>
    /// 通用库
    /// </summary>
    public abstract class CommonLib
    {
        /// <summary>
        /// 欧氏距离
        /// </summary>
        /// <param name="p0">第1个点</param>
        /// <param name="p1">第2个点</param>
        /// <returns>两点距离</returns>
        public static double EuclideanDistance(ZXPoint p0,  ZXPoint p1)
        {
            return Math.Sqrt((p0.X - p1.X) * (p0.X - p1.X) + (p0.Y - p1.Y) * (p0.Y - p1.Y) + (p0.Z - p1.Z) * (p0.Z - p1.Z));
        }

        /// <summary>
        /// 欧式距离的平方
        /// </summary>
        /// <param name="p0">第1个点</param>
        /// <param name="p1">第2个点</param>
        /// <returns>两点距离平方</returns>
        public static double EuclideanDistanceP2(ZXPoint p0, ZXPoint p1)
        {
            return (p0.X - p1.X) * (p0.X - p1.X) + (p0.Y - p1.Y) * (p0.Y - p1.Y) + (p0.Z - p1.Z) * (p0.Z - p1.Z);
        }

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileStream GetFileStream(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return new FileStream(filePath, FileMode.Create);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="contents"></param>
        public static void WriteToFile(FileStream fs, string contents)
        {
            byte[] data = System.Text.Encoding.Default.GetBytes(contents);
            fs.Write(data, 0, data.Length);
        }
    }
}
