// ============================================================
// 模块名称：类型转换器 Parser
// 功能简介：将基本几何体的关键点转换为通点云类型
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.8.18 杜子兮 创建
//============================================

using System.Collections.Generic;
using GeoLib;

namespace DotCloudLib
{
    /// <summary>
    /// 坐标系转换
    /// </summary>
    public static class Parser
    {
         /// <summary>
         /// 获取圆柱体关键点全局坐标
         /// </summary>
         /// <param name="geometry">圆柱体对象</param>
         /// <returns>点集</returns>
         public static ZXPointSet GetKeyPoints(CylinderGeometry geometry)
         {
             ZXPointSet outPoints = new ZXPointSet();

             List<PointGeometry> points = geometry.keypoints; // 获取关键点

             // RTMatrix m0 = new RTMatrix(geometry.rotation, geometry.position);  // 不考虑缩放的旋转平移矩阵

            for (int i = 0; i < points.Count; i++)
            {
                RTMatrix m1 = new RTMatrix(points[i].position, geometry.size); // 缩放变形矩阵
                // RTMatrix m2 = m0 * m1; // 矩阵变换

                float x = (float) m1[0];
                float y = (float) m1[1];
                float z = (float) m1[2];
                outPoints.Add(x, y, z);
             }
             
             return outPoints;
         }
         
         /// <summary>
         /// 获取长方体关键点全局坐标
         /// </summary>
         /// <param name="geometry"></param>
         /// <returns></returns>
         public static ZXPointSet GetKeyPoints(CuboidGeometry geometry)
         {
            ZXPointSet outPoints = new ZXPointSet();

            List<PointGeometry> points = geometry.keypoints; // 获取关键点

            for (int i = 0; i < points.Count; i++)
            {
                RTMatrix m1 = new RTMatrix(points[i].position, geometry.size); // 缩放变形矩阵
                // RTMatrix m2 = m0 * m1; // 矩阵变换

                float x = (float)m1[0];
                float y = (float)m1[1];
                float z = (float)m1[2];
                outPoints.Add(x, y, z);
            }

            return outPoints;
         }
    }
}
