//=====================================================================
// 模块名称：点云插入工具类 DotInsert
// 功能简介：进行插入点操作
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.09.03 杜子兮 创建，移自PCLPerformance
//                     刘轩名 追加 指定精度插值
//          2022.06.30 杜子兮 追加 安息角插值、X方向插值
//          2022.07.04 杜子兮 移植 二次补全算法 from DotPile by Sanngoku 
//          2022.07.05 Sanngoku 更新DotInsert.ByX() - 优化插值逻辑，消除随预设距离增加，插值运算耗时随着增加的问题
//                     Sanngoku 新增方法DotInsert.InsertLine() - 以指定两点为线段端点，按照指定精度在线段上等间距插点
//          2022.09.02 杜子兮    剔除 CompleteInventory 中 间接使用静态变量的源码
//          2022.10.13 杜子兮    二次补全剔除多余部分
//          2022.10.21 杜子兮    调整截取范围 bOrg
//          2022.11.01 杜子兮    TurnToFull判断是否已满
//          2025.07.15 杜子兮    修改 DotInsert.InsertLine()
//=====================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

namespace DotCloudLib
{
    /// <summary>
    /// 点云插入工具类
    /// </summary>
    public static class DotInsert
    {
        /// <summary>
        /// 条形料堆 盘库扫描二次补全
        /// 安息角插值算法 [专利号：2018052952.3]
        /// <param name="orgPointSet">原始缓存点云->补全后点云</param>
        /// <param name="p_minY">二次补全后 起始Y坐标</param>
        /// <param name="p_maxY">二次补全后 终止Y坐标</param>
        /// <param name="p_minZ">地面基准高度 默认为0</param>
        /// </summary>
        public static ZXPointSet CompleteInventory(ZXPointSet orgPointSet, float p_minY, float p_maxY, float p_minZ = 0)
        {
            Console.WriteLine(MetaData.LIB_NAME + "  " + MetaData.VERSION);

            const float CUT_OFF_X = 1f;
            const float LEN = 15;
            const float MIN_X_SPAN = 1;
            const float MIN_Y_SPAN = 1;
            const float BORDER_W = 2f;

            // STEP 1: 预处理   ROI 提取，如果偏斜，两侧截掉一点
            Console.WriteLine("STEP 1: 预处理");

            ZXBoundary bOrg = orgPointSet.Boundary;

            bOrg.MinY = p_minY;
            bOrg.MaxY = p_maxY;
            orgPointSet.Intercept(bOrg);                              // O(n)
            orgPointSet.Gridding(0.1f);

            ZXBoundary bLeft = bOrg; bLeft.MaxX = bLeft.MinX + CUT_OFF_X;
            ZXBoundary bRight = bOrg; bRight.MinX = bRight.MaxX - CUT_OFF_X;

            // 0.9.5 对应 2022.11.29

            // 是否剔除左侧
            if (orgPointSet.Intercept(bLeft, false).Bound.W < LEN) 
                bOrg.MinX += CUT_OFF_X;

            // 是否剔除右侧
            if (orgPointSet.Intercept(bRight, false).Bound.W < LEN) 
                bOrg.MaxX -= CUT_OFF_X;

            // Y边界按安息角剔除轨道两侧杂点 2023.07.02

            float oY = (p_maxY - p_minY) * 0.5f + p_minY;

            ZXPointSet cutPointSet = new ZXPointSet();
            for (int i = 0; i < orgPointSet.Count; i++)
            {
                if (orgPointSet[i].Y <= oY)
                {
                    // case 1: 下侧
                    if (orgPointSet[i].Z < (orgPointSet[i].Y - p_minY) * Math.Tan(40 * Math.PI / 180f))
                    {
                        cutPointSet.Add(orgPointSet[i]);
                    }
                }
                else
                {
                    // case 2: 上侧
                    if (orgPointSet[i].Z < (p_maxY - orgPointSet[i].Y) * Math.Tan(40 * Math.PI / 180f))
                    {
                        cutPointSet.Add(orgPointSet[i]);
                    }
                }
            }

            orgPointSet = cutPointSet;

            // 旋转扫描对应  2023.05.22
            if (bOrg.L > 50)
            {
                orgPointSet.Intercept(bOrg);
                ZXPointSet ps = new ZXPointSet();

                do
                {
                    ps = orgPointSet.Intercept(bLeft, false);

                    if (ps.Boundary.W > LEN + 2.5)
                        break;

                    orgPointSet.CutOff(bLeft);

                    bLeft.MinX += CUT_OFF_X;
                    bLeft.MaxX += CUT_OFF_X;
                    

                } while (ps.Bound.W <= LEN + 2.5);
            }

            bOrg = orgPointSet.Boundary;
            bOrg.MinY = p_minY;
            bOrg.MaxY = p_maxY;

            // 如果剔除后，补全点跨度过小，直接返回空点
            if (bOrg.MaxX - bOrg.MinX < MIN_X_SPAN)
            {
                Console.WriteLine("DotInsert_L053: 有效X范围不足" + MIN_X_SPAN + "米，返回空点集");
                return new ZXPointSet();
            }

            if (bOrg.MaxY - bOrg.MinY < MIN_Y_SPAN)
            {
                Console.WriteLine("DotInsert_L072: 有效Y范围不足" + MIN_Y_SPAN + "米，返回空点集");
                return new ZXPointSet();
            }

            // STEP 2: 剔除大机
            Console.WriteLine("STEP 2: 剔除大机");
            ZXBoundary bObj = bOrg;
            bObj.MinZ = 2;

            if (p_minY < 0)
            {
                // CASE 1: 下方料场
                bObj.MaxY = -37;
            }
            else
            {
                // CASE 2: 上方料场
                bObj.MinY = 46; 
            }
            orgPointSet.CutOff(bObj);  

            bOrg.MinZ = p_minZ - 0.01f;         // 2022.11.24
            bOrg.MaxZ = 20;                     // 2022.10.21

            ZXPointSet outputPointSet = new ZXPointSet();
            float UNIT = outputPointSet.Unit;

#if EGANG


            // Sanngoku 20230301 切割隔壁大机 - 鄂钢
            {
                ZXPointSet afterCutCloud = new ZXPointSet();
                // 将点云投影到YOZ平面上，尝试分离“浮空”的部分
                // 每xStep执行一次，避免可能存在的 料堆尖峰 对 “浮空” 判定的干扰 - no，可能一米内全是浮空，没有地面可供“浮”
                float zThreshold = 0.3f;    // z方向判定为“浮空”的阈值
                float xStep = 1f;           // x方向步长

                ZXBoundary bound = orgPointSet.Bound;                
                float startX = bound.MinX;
                float endX = bound.MaxX;
                //for (; startX <= endX - xStep; startX += xStep)
                {
                    //ZXBoundary cutBound = new ZXBoundary(startX, startX + xStep, bound.MinY, bound.MaxY, bound.MinZ, bound.MaxZ);
                    //ZXPointSet stepCloud = orgPointSet.Intercept(cutBound, false);
                    ZXPointSet stepCloud = orgPointSet;
                    if (stepCloud.Count <= 0)
                    {
                        //continue;
                    }

                    ZXBoundary stepBound = stepCloud.Bound;
                    for (float y = stepBound.MinY; y < stepBound.MaxY; y+=0.1f)
                    {
                        List<float> zList = new List<float>();
                        //for (int j = 0; j < stepCloud.LengthN; j++)
                        //{
                        //    ZXPoint pt = stepCloud.Get(j, i);
                        //    if (pt.Z > -100)
                        //    {
                        //        zList.Add(pt.Z);
                        //    }
                        //}

                        ZXPointSet dyCloud = stepCloud.Intercept(new ZXBoundary(stepBound.MinX, stepBound.MaxX, y - 0.05f, y + 0.05f, stepBound.MinZ, stepBound.MaxZ), false);
                        for (int index = 0; index < dyCloud.Count; index++)
                        {
                            ZXPoint pt = dyCloud[index];
                            zList.Add(pt.Z);
                        }

                        zList.Sort();

                        float markZ = -1000;
                        for(int index = 0; index < zList.Count - 1; index++)
                        {
                            float dif = zList[index + 1] - zList[index];
                            if(dif > zThreshold)
                            {
                                markZ = zList[index];
                                break;
                            }
                        }

                        if (markZ > -1000)
                        {
                            //for (int j = 0; j < stepCloud.LengthN; j++)
                            //{
                            //    ZXPoint pt = stepCloud.Get(j, i);
                            //    if(pt.Z <= markZ)
                            //    {
                            //        afterCutCloud.Add(pt);
                            //    }
                            //}
                            for (int index = 0; index < dyCloud.Count; index++)
                            {
                                ZXPoint pt = dyCloud[index];
                                if(pt.Z <= markZ)
                                {
                                    afterCutCloud.Add(pt);
                                }
                            }
                        }
                        else
                        {
                            afterCutCloud.Merge(dyCloud);
                        }
                    }
                }

                //afterCutCloud.SaveAsXYZ(@"D:\PCL\PointDatas\鄂钢二次补全剔除大机\afterCutCloud.xyz");
                orgPointSet = afterCutCloud;
            }
#endif

            orgPointSet.TurnToFull(ref outputPointSet);

            // STEP 3.1: X方向横内插
            Console.WriteLine("STEP 3.1: X & Y方向交替内插");

            // ByX(ref outputPointSet, 5);    // O(n * n)
            ByX(ref outputPointSet, 20);
            ByY(ref outputPointSet, 20);
            ByX(ref outputPointSet, 30);
            ByY(ref outputPointSet, 40);

            // STEP 3.2: 一次中值迭代插值
            Console.WriteLine("STEP 3.2: 一次中值迭代插值");
            for (int i = 0; i < 2; i++)  
            {
                Dictionary<int, float> insertPoints = MidValue(ref outputPointSet, 3);

                if (insertPoints.Keys.Count == 0)
                    break;
            }

            // STEP 4: 提取边缘点集   （满秩阵） 
            Console.WriteLine("STEP 4: 提取边缘点集");   // O(n * 5)

            ZXPointSet borderLeft = new ZXPointSet();    // 左边缘点集
            ZXPointSet borderRight = new ZXPointSet();   // 右边缘点集
            ZXPointSet borderUp = new ZXPointSet();     // 上边缘点集
            ZXPointSet borderDown = new ZXPointSet();  // 下边缘点集 

            for (int i = 0; i < outputPointSet.Count; i++)                       
            {
                if (outputPointSet[i].Z < -1000)
                    continue; // 空点直接跳过

                int index = outputPointSet.GetIndex(i, Dir.Left);
                if (index >= 0 && index < outputPointSet.Count && outputPointSet[index].Z < -1000)
                    borderLeft.Add(outputPointSet[i]);

                index = outputPointSet.GetIndex(i, Dir.Right);
                if (index >= 0 && index < outputPointSet.Count && outputPointSet[index].Z < -1000)
                    borderRight.Add(outputPointSet[i]);

                index = outputPointSet.GetIndex(i, Dir.Up);
                if (index >= 0 && index < outputPointSet.Count && outputPointSet[index].Z < -1000)
                    borderUp.Add(outputPointSet[i]);

                index = outputPointSet.GetIndex(i, Dir.Down);
                if (index >= 0 && index < outputPointSet.Count && outputPointSet[index].Z < -1000)
                    borderDown.Add(outputPointSet[i]);
            }

            // 此时 outputPointSet 为满秩阵

            // STEP 5: 两侧X方向外插，补齐（前提条件：满秩阵）
            Console.WriteLine("STEP 5: 两侧X方向外插，斜边补齐");

            ZXBoundary bBorderLeft = borderLeft.Boundary; 
            bBorderLeft.MaxX = bOrg.MinX + BORDER_W * 4;
            bBorderLeft.MaxY = p_maxY - BORDER_W;
            bBorderLeft.MinY = p_minY + BORDER_W;
            borderLeft.Intercept(bBorderLeft);

            if (borderLeft.Bound.L < 3.5)  
            {
                for (int i = 0; i < borderLeft.Count; i++)
                {
                    for (float x = borderLeft[i].X - UNIT; x >= bOrg.MinX - UNIT; x -= UNIT)  // >   ->  >=
                    {
                        int index = outputPointSet.GetIndex(x, borderLeft[i].Y);
                        if (index >= 0 && index < outputPointSet.Count && borderLeft[i].Z > 4) // 2023.06.26   6 -> 4
                            outputPointSet[index].Z = borderLeft[i].Z;
                    }
                }
            }

            ZXBoundary bBorderRight = borderRight.Boundary;
            bBorderRight.MinX = bOrg.MaxX - BORDER_W * 4;
            // bBorderRight.MaxY = p_maxY - BORDER_W;
            // bBorderRight.MinY = p_minY + BORDER_W;
            bBorderRight.MinZ = 3.5f; // 为什么是3.5？
            borderRight.Intercept(bBorderRight);

            if (borderRight.Bound.L >= 3.5)
            {
                bBorderRight.MaxY = bBorderRight.MaxY - BORDER_W;
                borderRight.Intercept(bBorderRight);
                bBorderRight = borderRight.Boundary;

                if (borderRight.Bound.L >= 3.5)
                {
                    bBorderRight.MinY = bBorderRight.MinY + BORDER_W;
                    borderRight.Intercept(bBorderRight);
                    bBorderRight = borderRight.Boundary;
                }
            }

            if (borderRight.Bound.L < 3.5) // 2023.06.26  .H -> .L 
            {
                for (int i = 0; i < borderRight.Count; i++)
                {
                    if (borderRight[i].X < bOrg.OX + BORDER_W && bOrg.L > 5) // 2023.06.26 + && bOrg.L > 5
                        continue;

                    for (float x = borderRight[i].X + UNIT; x <= bOrg.MaxX + UNIT; x += UNIT) // <   ->  <=
                    {
                        int index = outputPointSet.GetIndex(x, borderRight[i].Y);
                        if (index >= 0 && index < outputPointSet.Count && borderRight[i].Z > 4) // 2023.06.26   6 -> 4
                            outputPointSet[index].Z = borderRight[i].Z;
                    }
                }
            }

            // STEP 6: 边缘安息角插值
            Console.WriteLine("STEP 6: 边缘安息角插值");

            ZXPointSet newPoints = new ZXPointSet();
            ZXBoundary bROI = outputPointSet.Boundary;
            bROI.MinZ = p_minZ + 0.2f; //      2022.11.27 0.6 -> 2023.04.20 0.2

            // STEP 6.1 右边缘安息角插值
            borderRight.Clear();

            for (int i = 0; i < outputPointSet.Count; i++)
            {
                if (outputPointSet[i].Z < -1000)
                    continue; // 空点直接跳过

                int index = outputPointSet.GetIndex(i, Dir.Right);
                if (index >= 0 && index < outputPointSet.Count && outputPointSet[index].Z < -1000)
                    borderRight.Add(outputPointSet[i]);
            }

            bBorderRight = borderRight.Boundary; 
            bBorderRight.MinY += 1.5f;    // 切掉右边的下边缘(俯视图)
            bBorderRight.MinZ = 3.5f;     // 切掉右边缘的底边
            borderRight.Intercept(bBorderRight);
            bBorderRight = borderRight.Bound;

            if (bBorderRight.H > 4 && bBorderRight.L < bBorderRight.W && bBorderRight.MinX > bOrg.OX) 
            {
                bBorderRight.MinX = bOrg.MaxX - BORDER_W * 7;
                bBorderRight.MinZ = 8;
                borderRight.Intercept(bBorderRight);

                if (borderRight.Bound.MaxX < bOrg.MaxX - BORDER_W * 2)
                {
                    borderRight.Sort();

                    for (int i = 0; i < borderRight.Count; i += 5)
                    {
                        newPoints.Merge(ByStopAngleX(borderRight[i], UNIT, bROI.MinZ));
                    }
                }
            }

            // 此时 outputPointSet 为满秩阵
            outputPointSet.RemoveNull();

            ZXBoundary b = outputPointSet.Boundary;
            LibTool.Debug(b);

            float OZ = outputPointSet.Boundary.MinZ + 0.2f; // 2024.04.16 + 

            outputPointSet.Merge(newPoints);

            // STEP 6.2: 上下边缘安息角插值

            float MaxX = bROI.MaxX;  // 记录滑动结束点
            bROI.MaxX = bROI.MinX + 0.5f;
            bROI.MinZ = p_minZ + 1.3f;     // Ver 1.2.16 1.3  
            float SPAN = 0.2f;
            float OFFSET = UNIT * 5;  // 滑动跨度
            float MIN_Z = p_minZ;

            newPoints.Clear(); // 2024.03.21

            do
            {
                ZXPointSet psROI = outputPointSet.Intercept(bROI, false);    // O(n)

                // STEP 6.2.1: 上边缘安息角插值
                if (psROI.Bound.MaxY > (p_minY + p_maxY) / 2 - 3)  // 2024.04.16 -3
                {
                    ZXBoundary bROITop = psROI.Bound;
                    bROITop.MinY = bROITop.MaxY - UNIT;
                    ZXPointSet psROITop = psROI.Intercept(bROITop, false);       // O(m)
                    bROITop = psROITop.Boundary;

                    if (bROITop.MinZ > MIN_Z && bROITop.MaxY + SPAN < p_maxY)
                    {
                        ZXPoint keyPoint = new ZXPoint(bROITop.MinX, bROITop.MaxY + SPAN, bROITop.OZ);
                        newPoints.Merge(ByStopAngle(keyPoint, UNIT, OZ));  // 2024.04.16 bROI.MinZ -> OZ
                    }
                }

                // STEP 6.2.2: 下边缘安息角插值
                if (psROI.Bound.MinY < (p_minY + p_maxY) / 2 + 3) // Ver 1.0.3  
                {
                    ZXBoundary bROIBottom = psROI.Bound;
                    bROIBottom.MaxY = bROIBottom.MinY + UNIT;
                    ZXPointSet psROIBottom = psROI.Intercept(bROIBottom, false);   // O(m)
                    bROIBottom = psROIBottom.Boundary;

                    if (bROIBottom.MinZ > MIN_Z && bROIBottom.MinY - SPAN > p_minY)
                    {
                        ZXPoint keyPoint = new ZXPoint(bROIBottom.MinX, bROIBottom.MinY - SPAN, bROIBottom.OZ);
                        newPoints.Merge(ByStopAngle(keyPoint, -UNIT, OZ));   // 2024.04.16 bROI.MinZ -> OZ
                    }
                }

                bROI.MinX += OFFSET;
                bROI.MaxX = bROI.MinX + OFFSET;

            } while (bROI.MinX < MaxX);

            outputPointSet.RemoveNull();

            outputPointSet.Merge(newPoints);

            newPoints.Clear();

            TurnToFull(ref outputPointSet);

            // STEP 7: 填充上下两边
            Console.WriteLine("STEP 7: 填充上下两边");

            bROI = outputPointSet.Boundary;

            // 下边
            if (bROI.MinY > p_minY)
            {
                DotPile pile = new DotPile(bROI.MinX, bROI.MaxX, p_minY, bROI.MinY, 0, 15, UNIT); // 2022.11.26 删掉 UNIT
                if (p_minZ != 0)
                    pile.Init(p_minZ);
                
                outputPointSet.Merge(pile.Points);
            }

            // 上边
            if (bROI.MaxY < p_maxY)
            {
                DotPile pile = new DotPile(bROI.MinX, bROI.MaxX, bROI.MaxY, p_maxY, 0, 15, UNIT); // 2022.11.26 删掉 UNIT
                if (p_minZ != 0)
                    pile.Init(p_minZ);
               
                outputPointSet.Merge(pile.Points);
            }

            TurnToFull(ref outputPointSet);  // 转为满秩阵

            // 上下两边锁边
            for (int i = 0; i < outputPointSet.Count; i++)
            {
                if (outputPointSet[i].Y <= bROI.MinY + UNIT || outputPointSet[i].Y >= bROI.MaxY - UNIT)
                {
                    outputPointSet[i].Z = p_minZ;
                }
            }

            ByX(ref outputPointSet, 7);

            // STEP 8: 中值插值 递增插值法
            List<int> r = new List<int> { 3, 3, 5, 7, 9, 11, 13, 15};

            for (int i = 0; i < r.Count; i++)
            {
                Dictionary<int, float> insertPoints = MidValue(ref outputPointSet, r[i]); // 中值插值

                if (insertPoints.Keys.Count == 0)
                    break;
            }

            outputPointSet.SetMinZ(p_minZ);
            outputPointSet.Intercept(bOrg);
            outputPointSet.Sort();

            // 20230208 鄂钢 临时修改 四舍五入一下，不然在处理时可能出问题
            for(int i = 0;i < outputPointSet.Count; i++)
            {
                outputPointSet[i].X = (float)Math.Round(outputPointSet[i].X, 1);
                outputPointSet[i].Y = (float)Math.Round(outputPointSet[i].Y, 1);
            }

            return outputPointSet;
        }

        /// <summary>
        /// 条形料堆 取料扫描二次补全
        /// </summary>
        /// <param name="p_pile">目标区域原有料型</param>
        /// <param name="zXPoints">更新点云</param>
        public static void CompleteReclaim(ref DotPile p_pile, ZXPointSet zXPoints)
        {
            //STEP 0: 去除地面
            ZXBoundary boundary = zXPoints.Bound;
            ZXBoundary minZBoundary = new ZXBoundary();
            p_pile.Bound = p_pile.Points.Boundary;

            //按原有点云与待插点云包围盒重新切割
            minZBoundary.MaxX = Math.Min(p_pile.Bound.MaxX, boundary.MaxX);
            minZBoundary.MaxY = Math.Min(p_pile.Bound.MaxY, boundary.MaxY);
            minZBoundary.MaxZ = Math.Min(p_pile.Bound.MaxZ, boundary.MaxZ);
            minZBoundary.MinX = Math.Min(p_pile.Bound.MinX, boundary.MinX);
            minZBoundary.MinY = Math.Min(p_pile.Bound.MinY, boundary.MinY);
            minZBoundary.MinZ = Math.Min(p_pile.Bound.MinZ, boundary.MinZ + 1);

            zXPoints.Intercept(minZBoundary);//去除地面后点云

            ZXPointSet resultPoints = new ZXPointSet();//存储结果
            float unit = p_pile.Unit;//点云精度
            resultPoints.Unit = unit;

            //STEP 1:自顶向下截取等高点云线
            float Z = minZBoundary.MaxZ;
            float minZ = minZBoundary.MinZ;
            float dif = unit;//向下截取步长

            while (Z > minZ)
            {
                ZXBoundary heightBound = new ZXBoundary();
                heightBound.MinX = boundary.MinX;
                heightBound.MaxX = boundary.MaxX;
                heightBound.MinY = boundary.MinY;
                heightBound.MaxY = boundary.MaxY;
                heightBound.MinZ = Z - dif;
                heightBound.MaxZ = Z;
                Z -= dif;
                ZXPointSet heightPoints = zXPoints.Intercept(heightBound, false);//等高点集
                if (heightPoints.Count < 10)
                {
                    continue;
                }
                heightPoints.Sort();
                ZXPointSet tempPoints = new ZXPointSet();

                //STEP 2: 沿等高点集，相邻点之间插入等高点
                for (int i = 0; i < heightPoints.Count - 1; i++)
                {
                    ZXPoint p0 = heightPoints[i];
                    ZXPoint p1 = heightPoints[i + 1];

                    //保证单向
                    if (p1.X - p0.X <= 0 || p1.Y - p0.Y <= 0)
                    {
                        continue;
                    }
                    tempPoints.Add(p0);
                    tempPoints.Add(p1);
                    tempPoints.AddDotSegment(p0, p1, unit);
                }

                resultPoints.Merge(tempPoints);
            }

            ZXPointSet points = new ZXPointSet();
            resultPoints.Sort();
            resultPoints.TurnToFull(ref points);
            DotInsert.Expansion(ref points, 5);

            p_pile.Points.Update(resultPoints);
            p_pile.Bound = p_pile.Points.Boundary;
        }

        /// <summary>
        /// 条形料堆 堆料扫描二次补全(一)
        /// </summary>
        /// <param name="p_pile">目标区域原有料型，取自数据库，多余区域点将被截掉</param>
        /// <param name="p_newPoints">更新点云，注：Y坐标按料条偏移量偏移</param>
        /// <returns></returns>
        public static bool CompleteStack(ref DotPile p_pile, ZXPointSet p_newPoints)
        {
            if (p_newPoints == null)
            {
                ArgumentNullException ex = new ArgumentNullException();
                LibTool.Error(ex, "DotInsert@616", "p_newPoints shouldn't be null.");
            }

            // STEP 1: 截取多余区域点
            ZXBoundary bPile = p_pile.Bound;
            bPile.MinZ = float.MinValue;
            bPile.MaxZ = float.MaxValue;
            p_newPoints.Intercept(bPile);

            // STEP 2: 新点覆盖旧点 
            for (int i = 0; i < p_newPoints.Count; i++)
            {
                int index = p_pile.GetIndex(p_newPoints[i].X, p_newPoints[i].Y);
                p_pile.Points[index].Z = (p_newPoints[i].Z < 0) ? 0 : p_newPoints[i].Z;
            }

            return true;
        }

        /// <summary>
        /// 条形料堆 堆料扫描二次补全(二)
        /// </summary>
        /// <param name="p_pile">目标区域原有料型</param>
        /// <param name="zXPoints">更新点云</param>
        /// <param name="stackPoint">堆料点</param>
        /// <returns></returns>
        public static bool CompleteStack(ref DotPile p_pile, ZXPointSet zXPoints, ZXPoint stackPoint)
        {
            if (stackPoint == null || zXPoints == null)
            {
                return false;
            }

            // STEP 0: 以堆料点，截取一条点云
            float x = stackPoint.X;
            float y = stackPoint.Y;
            ZXBoundary pileBound = p_pile.Bound;
            ZXBoundary apexBound = new ZXBoundary()
            {
                MaxX = pileBound.MaxX,
                MinX = pileBound.MinX,
                MaxY = y - p_pile.Unit,
                MinY = y + p_pile.Unit,
                MinZ = pileBound.MinZ,
                MaxZ = pileBound.MaxZ
            };
            ZXPointSet apexPoints = zXPoints.Intercept(apexBound);
            if (apexPoints.Count > 0)
            {
                // STEP 1: 线性拟合，求料堆安息角、高度、底面半径
                List<double> xList = new List<double>();
                List<double> zList = new List<double>();
                for (int i = 0; i < apexPoints.Count; i++)
                {
                    ZXPoint pt = apexPoints[i];
                    xList.Add(pt.X);
                    zList.Add(pt.Z);
                }

                CurveFit fit = new CurveFit();
                fit.polyfit(xList, zList, xList.Count, 1);
                if (fit.factor == null || fit.factor.Count != 2)
                {
                    return false;
                }

                double k = fit.factor[0];
                double b = fit.factor[1];
                double height = k * x + b; // 料堆高度
                double r = height / k; // 圆锥半径
                //double restAngleRedius = Math.Atan(k); // 安息角弧度

                // STEP 2: 补全料堆
                for (double i = x - r; i < x + r; i += p_pile.Unit)
                {
                    for (double j = y - r; j < y + r; j += p_pile.Unit)
                    {
                        double d2O = Math.Sqrt((i - x) * (i - x) + (j - y) * (j - y));// 到圆心距离
                        double d2E = r - d2O;// 到圆上距离
                        double z = d2E / r * height;// 当前点高度

                        int index = p_pile.Points.GetIndex((float)i, (float)j);
                        if (index == -1)
                        {
                            p_pile.Points.Add((float)i, (float)j, (float)z);
                        }
                        else
                        {
                            ZXPoint orgPoint = p_pile.Points[index];
                            if (orgPoint.Z < z)
                            {
                                orgPoint.Z = (float)z;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 中值插值算法
        /// 使用条件：满点阵
        /// </summary>
        /// <param name="orgPointSet">空点高程为极小值的满点阵</param>
        /// <param name="p_r">半径参数</param>
        /// <param name="p_n">周边点数阈值，周围点数过少，认为没有参考价值</param>
        /// <returns></returns>
        public static Dictionary<int, float> MidValue(ref ZXPointSet orgPointSet, int p_r, int p_n = 3)
        {
            // 新插入的点
            Dictionary<int, float> newPoints = new Dictionary<int, float>();

            // 校验，是否为满点阵
            if (!orgPointSet.IsFull())
            {
                LibTool.Error("DotInsert35: 不是满点阵，无法使用中值插值算法 MidValue()");
            }

            for (int i = 0; i < orgPointSet.LengthN; i++) // X方向遍历
            {
                for (int j = 0; j < orgPointSet.WidthN; j++) // Y方向遍历
                {
                    int index = orgPointSet.GetIndex(i, j);

                    // 直接跳过条件：不是空点
                    if (orgPointSet[index].Z > float.MinValue + 100)
                        continue;

                    // 记录要添加的点
                    float z = orgPointSet.GetMidValue(i, j, p_r, p_n); // 按半径参数r计算周边高度中间值
                    if (z > float.MinValue + 100)
                    {
                        newPoints.Add(index, z);
                    }
                }
            }

            // 【套路】将新加入的点更新到orgPointSet中
            foreach (int index in newPoints.Keys)
            {
                orgPointSet[index].Z = newPoints[index];  // 更新原始点
            }

            if (LibTool.DebugMode)
            {
                LibTool.Debug("    中值插值 新插入点数：" + newPoints.Keys.Count);
            }
            return newPoints;
        }

        /// <summary>
        /// 十字内插法：以当前点为中心，以半径p_r为界限寻找上下左右点，若横纵都有则取线性插值的平均值，否则取一个插值
        /// </summary>
        /// <param name="orgPointSet">原始点云</param>
        /// <param name="p_r">搜索范围</param>
        /// <returns>是否正确</returns>
        public static Dictionary<int, float> Cross(ref ZXPointSet orgPointSet, int p_r)
        {
            // 新插入的点
            Dictionary<int, float> newPoints = new Dictionary<int, float>();

            // 校验，是否为满点阵
            if (!orgPointSet.IsFull())
            {
                Console.WriteLine("Error: 不是满点阵");
                return newPoints;
            }

            orgPointSet.Bound = orgPointSet.Boundary;

            for (int i = 0; i < orgPointSet.LengthN; i++)
            {
                for (int j = 0; j < orgPointSet.WidthN; j++)
                {
                    int index = orgPointSet.GetIndex(i, j);

                    ZXPoint p = orgPointSet.GetPoint(index);

                    if (!p.IsEmpty(-99999))
                        continue;  // 不是空点,直接跳过

                    // STEP 2: 搜索上下左右四个点 PX1 PX2 PY1 PY2
                    ZXPoint pX1 = orgPointSet.GetNearestPoint(i, j, p_r, Dir.Right);
                    ZXPoint pX2 = orgPointSet.GetNearestPoint(i, j, p_r, Dir.Left);
                    ZXPoint pY1 = orgPointSet.GetNearestPoint(i, j, p_r, Dir.Up);
                    ZXPoint pY2 = orgPointSet.GetNearestPoint(i, j, p_r, Dir.Down);

                    // STEP 3: 根据四个点计算高点
                    if (!pX1.IsEmpty(-99999) && !pX2.IsEmpty(-99999) &&
                        !pY1.IsEmpty(-99999) && !pY2.IsEmpty(-99999))
                    {
                        // CASE 1: 上下左右都有点
                        float zX = (p.X - pX1.X) / (pX2.X - pX1.X) * (pX2.Z - pX1.Z) + pX1.Z;
                        float zY = (p.Y - pY1.Y) / (pY2.Y - pY1.Y) * (pY2.Z - pY1.Z) + pY1.Z;
                        newPoints.Add(index, (zX + zY) * 0.5f);
                        // Console.WriteLine("CASE 1: 上下左右都有点 " + ((zX + zY) * 0.5f));
                    }
                    else if (!pX1.IsEmpty(-99999) && !pX2.IsEmpty(-99999))
                    {
                        // CASE 2: 左右有点
                        float zX = (p.X - pX1.X) / (pX2.X - pX1.X) * (pX2.Z - pX1.Z) + pX1.Z;
                        newPoints.Add(index, zX);
                        // Console.WriteLine("CASE 2: 左右有点 " + zX);
                    }
                    else if (!pY1.IsEmpty(-99999) && !pY2.IsEmpty(-99999))
                    {
                        // CASE 3: 上下有点
                        float zY = (p.Y - pY1.Y) / (pY2.Y - pY1.Y) * (pY2.Z - pY1.Z) + pY1.Z;
                        newPoints.Add(index, zY);
                        // Console.WriteLine("CASE 3: 上下有点 " + zY);
                    }
                }
            }

            // LibTool.Debug("    十字插值 新插入点数：" + newPoints.Keys.Count);


            // 【套路】将新加入的点更新到orgPointSet中
            foreach (int index in newPoints.Keys)
            {
                // Console.WriteLine(newPoints[index]);
                orgPointSet[index].Z = newPoints[index];  // 更新原始点
            }

            return newPoints;
        }

        /// <summary>
        /// 指定精度插值  by { 刘轩名 }
        /// </summary>
        /// <param name="orgPointSet">原始点云</param>
        /// <param name="_unit">目标精度</param>
        /// <param name="_minX">最小x范围（精度：与原始输入点云精度相同）</param>
        /// <param name="_maxX">最大x范围（精度：与原始输入点云精度相同）</param>
        /// <returns></returns>
        public static ZXPointSet ByUnit(ZXPointSet orgPointSet, float _unit, float _minX = float.MinValue, float _maxX = float.MaxValue)
        {

            // STEP 0: 参数校验
            if (_unit >= orgPointSet.Unit)
            {
                // DotCloudExceptionSupport.WarnAction?.Invoke("插值算法警告：输入原始点云大于或等于目标精度，将不做处理，只返回原始点云的一个副本");
                LibTool.Error("DotInsert L00989: 插值算法警告：输入原始点云大于或等于目标精度");
                return new ZXPointSet(orgPointSet);
            }


            ZXPointSet result = new ZXPointSet();

            // STEP 1: 非均匀精度参数调整

            //两相邻点之间需要插入点数量
            int insertPointsCount = (int)Math.Round(orgPointSet.Unit / _unit, 0);
            result.Unit = orgPointSet.Unit / insertPointsCount;
            result.Bound = orgPointSet.Bound;
            float pointIncrement = 1 * result.Unit; //相邻点x、y增量s


            // STEP 2: 以调整后精度均匀插值
            // 【核心算法设计】

            float pointSeparate = insertPointsCount;

            /*
             *          a1     b121     b122     a2
             *          b131   b1234    b        b
             *          b132   b        b        b
             *          a3     b        b        a4
             * 
             *          b1234.z = 2 / 3 * 2 / 3 * a1.z
             *                  + 1 / 3 * 2 / 3 * a2.z
             *                  + 2 / 3 * 1 / 3 * a3.z
             *                  + 1 / 3 * 1 / 3 * a4.z
             */


            //输入插值范围无效
            if (_maxX <= _minX)
            {
                ArgumentException ex = new ArgumentException();
                LibTool.Error(ex, "DotInsert@1018", "输入插值范围参数错误 maxX:" + _maxX + " <= minX:" + _minX);
            }

            var bounder = orgPointSet.Bound;
            //长度方向插值起始序号
            int startLengthIndex;
            if(_minX <= bounder.MinX)
            {
                startLengthIndex = 0;
            }
            else
            {
                startLengthIndex = (int)((_minX - bounder.MinX) * orgPointSet.Unit);
            }

            //长度方向插值终止序号
            int endLengthIndex;
            if(_maxX >= bounder.MaxX)
            {
                endLengthIndex = orgPointSet.LengthN;
            }
            else
            {
                endLengthIndex = (int)(orgPointSet.LengthN - (bounder.MaxX - _maxX) * orgPointSet.Unit);
            }

            #region 另一种点排列方式，不适用
            ////y序号
            //for (int i = 0; i < orgPointSet.WidthN; i++)
            //{
            //    for (int ii = 0; ii < pointSeparate; ii++)
            //    {
            //        //x序号
            //        for (int j = 0; j < orgPointSet.LengthN; j++)
            //        {
            //            for (int jj = 0; jj < pointSeparate; jj++)
            //            {
            //                ZXPoint a1 = orgPointSet.GetPoint(orgPointSet.GetIndex(j, i));
            //                float x = a1.X + jj * pointIncrement;
            //                float y = a1.Y + ii * pointIncrement;
            //                float z;

            //                if (i + 1 >= orgPointSet.WidthN && j + 1 < orgPointSet.LengthN)
            //                {
            //                    ZXPoint a2 = orgPointSet.GetPoint(orgPointSet.GetIndex(j + 1, i));

            //                    float z1 = (1 - jj / pointSeparate) * a1.Z;
            //                    float z2 = (jj / pointSeparate) * a2.Z;
            //                    z = z1 + z2;

            //                    ZXPoint point = new ZXPoint(x, y, z);
            //                    result.Add(point);
            //                }
            //                else if (i + 1 >= orgPointSet.WidthN && j + 1 >= orgPointSet.LengthN)
            //                {
            //                    z = a1.Z;

            //                    ZXPoint point = new ZXPoint(x, y, z);
            //                    result.Add(point);
            //                }
            //                else if (i + 1 < orgPointSet.WidthN && j + 1 < orgPointSet.LengthN)
            //                {
            //                    ZXPoint a2 = orgPointSet.GetPoint(orgPointSet.GetIndex(j + 1, i));
            //                    ZXPoint a3 = orgPointSet.GetPoint(orgPointSet.GetIndex(j, i + 1));
            //                    ZXPoint a4 = orgPointSet.GetPoint(orgPointSet.GetIndex(j + 1, i + 1));

            //                    float z1 = (1 - jj / pointSeparate) * (1 - ii / pointSeparate) * a1.Z;
            //                    float z2 = (jj / pointSeparate) * (1 - ii / pointSeparate) * a2.Z;
            //                    float z3 = (1 - jj / pointSeparate) * (ii / pointSeparate) * a3.Z;
            //                    float z4 = (jj / pointSeparate) * (ii / pointSeparate) * a4.Z;
            //                    z = z1 + z3 + z2 + z4;

            //                    ZXPoint point = new ZXPoint(x, y, z);
            //                    result.Add(point);
            //                }
            //                else //i + 1 < orgPointSet.WidthN && j + 1 >= orgPointSet.LengthN
            //                {
            //                    ZXPoint a3 = orgPointSet.GetPoint(orgPointSet.GetIndex(j, i + 1));

            //                    float z1 = (1 - ii / pointSeparate) * a1.Z;
            //                    float z3 = (ii / pointSeparate) * a3.Z;
            //                    z = z1 + z3;

            //                    ZXPoint point = new ZXPoint(x, y, z);
            //                    result.Add(point);
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion


            //x序号
            for (int i = startLengthIndex; i < endLengthIndex; i++)
            {
                for (int ii = 0; ii < pointSeparate; ii++)
                {
                    //y序号
                    for (int j = 0; j < orgPointSet.WidthN; j++)
                    {
                        for (int jj = 0; jj < pointSeparate; jj++)
                        {
                            //以a1为原点进行计算
                            ZXPoint a1 = orgPointSet.GetPoint(orgPointSet.GetIndex(i, j));
                            float x = a1.X + ii * pointIncrement;
                            float y = a1.Y + jj * pointIncrement;
                            float z;

                            //单侧越界，z = a1 && a2
                            if (j + 1 >= orgPointSet.WidthN && i + 1 < orgPointSet.LengthN)
                            {
                                ZXPoint a2 = orgPointSet.GetPoint(orgPointSet.GetIndex(i + 1, j));

                                float z1 = (1 - ii / pointSeparate) * a1.Z;
                                float z2 = (ii / pointSeparate) * a2.Z;
                                z = z1 + z2;

                                ZXPoint point = new ZXPoint(x, y, z);
                                result.Add(point);

                                jj = (int)pointSeparate;
                            }
                            //两侧越界，则当前点z = a1.z
                            else if (j + 1 >= orgPointSet.WidthN && i + 1 >= orgPointSet.LengthN)
                            {
                                z = a1.Z;

                                ZXPoint point = new ZXPoint(x, y, z);
                                result.Add(point);


                                jj = (int)pointSeparate;
                                ii = (int)pointSeparate;
                            }
                            //两侧都在范围内，z = a1.z && a2.z && a3.z && a4.z
                            else if (j + 1 < orgPointSet.WidthN && i + 1 < orgPointSet.LengthN)
                            {
                                ZXPoint a2 = orgPointSet.GetPoint(orgPointSet.GetIndex(i + 1, j));
                                ZXPoint a3 = orgPointSet.GetPoint(orgPointSet.GetIndex(i, j + 1));
                                ZXPoint a4 = orgPointSet.GetPoint(orgPointSet.GetIndex(i + 1, j + 1));

                                float z1 = (1 - ii / pointSeparate) * (1 - jj / pointSeparate) * a1.Z;
                                float z2 = (ii / pointSeparate) * (1 - jj / pointSeparate) * a2.Z;
                                float z3 = (1 - ii / pointSeparate) * (jj / pointSeparate) * a3.Z;
                                float z4 = (ii / pointSeparate) * (jj / pointSeparate) * a4.Z;
                                z = z1 + z3 + z2 + z4;

                                ZXPoint point = new ZXPoint(x, y, z);
                                result.Add(point);
                            }
                            //单侧越界，z = a1 && a3
                            else //j + 1 < orgPointSet.WidthN && i + 1 >= orgPointSet.LengthN
                            {
                                ZXPoint a3 = orgPointSet.GetPoint(orgPointSet.GetIndex(i, j + 1));

                                float z1 = (1 - jj / pointSeparate) * a1.Z;
                                float z3 = (jj / pointSeparate) * a3.Z;
                                z = z1 + z3;

                                ZXPoint point = new ZXPoint(x, y, z);
                                result.Add(point);
                            }
                        }
                    }
                }
            }


            return result;
        }

        /// <summary>
        /// 二维图像膨胀插值算法
        /// 内部配合邻接高点的估算方法使用（点方位，内插、外插）
        /// </summary>
        /// <returns></returns>
        public static bool Expansion(ref ZXPointSet orgPointSet, int p_r, int p_n = 30)
        {
            // STEP 0：校验，是否为满点阵
            if (!orgPointSet.IsFull())
            {
                return false;
            }

            // STEP 1：细节逐步膨胀
            for (int i = 0; i < p_n; i++)
            {
                Dictionary<int, float> points = DotInsert.Cross(ref orgPointSet, 5);
                if (points.Keys.Count == 0)
                {
                    break;
                }
            }

            // STEP 2：快速膨胀
            for (int i = 2; i < 10; i++)
            {
                DotInsert.Cross(ref orgPointSet, i * i - 1);
            }

            orgPointSet.RemoveNull();

            return true;
        }


        /// <summary>
        /// 安息角插点（俯视图）
        /// </summary>
        /// <param name="keyPoint">始点</param>
        /// <param name="p_offsetY">Y方向插值偏移量 正：向上插入  负：向下插入</param>
        /// <param name="p_minZ">地面高度：默认为0</param>
        /// <returns>插入点集</returns>
        public static ZXPointSet ByStopAngle(ZXPoint keyPoint, float p_offsetY, float p_minZ = 0)
        {
            ZXPointSet newPoints = new ZXPointSet();

            while (keyPoint.Z > p_minZ)
            {
                newPoints.Add(new ZXPoint(keyPoint.X, keyPoint.Y, keyPoint.Z));
                keyPoint.Y = keyPoint.Y + p_offsetY;
                keyPoint.Z = keyPoint.Z - (float)(Math.Abs(p_offsetY) * Math.Tan(40 * Math.PI / 180.0));
            }
            return newPoints;
        }



        /// <summary>
        /// 点云上下边缘安息角插值 2025.09.29
        /// </summary>
        /// <param name="ps">待插点云</param>
        /// <param name="p_minZ">最低点</param>
        /// <returns></returns>
        public static ZXPointSet ByStopAngleY(ZXPointSet ps, float p_minZ)
        {
            // STEP 1: 上边缘
            ZXBoundary bROI = ps.Boundary;
            bROI.MinY = bROI.MaxY - ps.Unit * 0.5f;
            ZXPointSet psUp = ps.Intercept(bROI, false);

            ZXPointSet newPoints = new ZXPointSet();

            for (int i = 0; i < psUp.Count; i++)
            {
                newPoints.Merge(DotInsert.ByStopAngle(psUp[i], ps.Unit, p_minZ));
            }

            // STEP 2: 下边缘
            bROI = ps.Boundary;
            bROI.MaxY = bROI.MinY + ps.Unit * 0.5f;
            ZXPointSet psDown = ps.Intercept(bROI, false);

            for (int i = 0; i < psDown.Count; i++)
            {
                newPoints.Merge(DotInsert.ByStopAngle(psDown[i], -ps.Unit, p_minZ));
            }

            return newPoints;
        }

        /// <summary>
        /// 点云左右边缘安息角插值 2025.10.27
        /// </summary>
        /// <param name="ps">待插点云</param>
        /// <param name="p_minZ">最低点</param>
        /// <returns></returns>
        public static ZXPointSet ByStopAngleX(ZXPointSet ps, float p_minZ)
        {
            // STEP 1: 左边缘
            ZXBoundary bROI = ps.Boundary;
            bROI.MaxX = bROI.MinX + ps.Unit * 0.5f;
            ZXPointSet psLeft = ps.Intercept(bROI, false);

            ZXPointSet newPoints = new ZXPointSet();

            for (int i = 0; i < psLeft.Count; i++)
            {
                newPoints.Merge(DotInsert.ByStopAngleX(psLeft[i], ps.Unit, p_minZ));
            }

            // STEP 2: 右边缘
            bROI = ps.Boundary;
            bROI.MaxY = bROI.MinY + ps.Unit * 0.5f;
            ZXPointSet psDown = ps.Intercept(bROI, false);

            for (int i = 0; i < psDown.Count; i++)
            {
                newPoints.Merge(DotInsert.ByStopAngleX(psDown[i], -ps.Unit, p_minZ));
            }

            return newPoints;
        }


        /// <summary>
        /// 安息角插点 ———— X方向（俯视图）
        /// </summary>
        /// <param name="keyPoint">始点</param>
        /// <param name="p_offsetX">Y方向插值偏移量 正：向右插入  负：向左插入</param>
        /// <param name="p_minZ">地面高度：默认为0</param>
        /// <returns>插入点集</returns>
        public static ZXPointSet ByStopAngleX(ZXPoint keyPoint, float p_offsetX, float p_minZ = 0)
        {
            ZXPointSet newPoints = new ZXPointSet();

            while (keyPoint.Z > p_minZ)
            {
                newPoints.Add(new ZXPoint(keyPoint.X, keyPoint.Y, keyPoint.Z));
                keyPoint.X = keyPoint.X + p_offsetX;
                keyPoint.Z = keyPoint.Z - (float)(Math.Abs(p_offsetX) * Math.Tan(40 * Math.PI / 180.0));
            }
            return newPoints;
        }

        /// <summary>
        /// 原点云替换为满秩阵
        /// </summary>
        /// <param name="orgPointSet"></param>
        public static void TurnToFull(ref ZXPointSet orgPointSet)
        {
            if (orgPointSet.IsFull())
            {
                return;
            }

            ZXPointSet psFull = new ZXPointSet();
            orgPointSet.TurnToFull(ref psFull);
            orgPointSet.Clear();
            orgPointSet = psFull;
        }

        /// <summary>
        /// X轴内插法 - Sanngoku 20220705
        /// </summary>
        /// <param name="orgPointSet">原始点</param>
        /// <param name="p_r">最大相邻点搜索半径</param>
        /// <returns></returns>
        public static ZXPointSet ByX(ref ZXPointSet orgPointSet, int p_r)
        {
            ZXPointSet newPoints = new ZXPointSet();

            if (!orgPointSet.IsFull())
            {
                LibTool.Debug("ByX 不是满秩阵");
                TurnToFull(ref orgPointSet);
            }

            orgPointSet.Bound = orgPointSet.Boundary;
            float unit = orgPointSet.Unit;

            for (int y = 0; y < orgPointSet.WidthN; y++)
            {
                int startFlagX = -1;
                for (int x = 0; x < orgPointSet.LengthN;)
                {
                    int index = orgPointSet.GetIndex(x, y);

                    ZXPoint p = orgPointSet.GetPoint(index);

                    // 找到非空点，迭代记录
                    if (!float.IsNaN(p.Z) && !float.IsInfinity(p.Z) && p.Z > float.MinValue + 100)
                    {
                        startFlagX = x;
                        x++;
                    }

                    // 找到空点
                    else
                    {
                        // 当存在已记录点时，向x增大方向搜索
                        if (startFlagX != -1)
                        {
                            int r = 1;
                            bool isFound = false;
                            for (; r <= p_r; r++)
                            {
                                if (x + r >= orgPointSet.LengthN)
                                {
                                    break;
                                }
                                int p_index = orgPointSet.GetIndex(x + r, y);
                                ZXPoint p_pt = orgPointSet.GetPoint(p_index);
                                if (!float.IsNaN(p_pt.Z) && !float.IsInfinity(p_pt.Z) && p_pt.Z > float.MinValue + 100)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            // 搜索成功，则在两非空点间添加线段点
                            if (isFound)
                            {
                                ZXPoint startPoint = orgPointSet.Get(startFlagX, y);
                                ZXPoint endPoint = orgPointSet.Get(x + r, y);
                                var lineSet = DotInsert.InsertLine(startPoint, endPoint, unit);
                                foreach (var pt in lineSet)
                                {
                                    newPoints.Add(pt);
                                }
                                x += r ;
                                startFlagX = -1;
                            }
                            // 搜索失败 - 在指定距离内不存在另一端点，重置缓存，继续
                            else
                            {
                                x += p_r ;
                                startFlagX = -1;
                            }
                        }
                        // 不存在起始端点，继续搜索
                        else
                        {
                            x++;
                        }
                    }
                }
            }

            // orgPointSet.Merge(newPoints);

            // 在原满秩阵中，添加新点，保持满秩阵特征不变

            for (int i = 0; i < newPoints.Count; i++)
            {
                int index = orgPointSet.GetIndex(newPoints[i].X, newPoints[i].Y);
                orgPointSet[index].Z = newPoints[i].Z;  // 直接更新满秩阵高度值
            }

            Console.WriteLine("X轴内插法 新插入点数：" + newPoints.Count);

            return newPoints; 
        }

        /// <summary>
        /// X轴内插法 - Sanngoku 20220705
        /// </summary>
        /// <param name="orgPointSet">原始点</param>
        /// <param name="p_r">最大相邻点搜索半径</param>
        /// <param name="_unit">指定插值精度</param>
        /// <returns></returns>
        public static ZXPointSet ByX(ref ZXPointSet orgPointSet, int p_r, float _unit)
        {
            ZXPointSet newPoints = new ZXPointSet();

            if (!orgPointSet.IsFull())
            {
                TurnToFull(ref orgPointSet);
            }

            orgPointSet.Bound = orgPointSet.Boundary;

            for (int y = 0; y < orgPointSet.WidthN; y++)
            {
                int startFlagX = -1;
                for (int x = 0; x < orgPointSet.LengthN;)
                {
                    int index = orgPointSet.GetIndex(x, y);

                    ZXPoint p = orgPointSet.GetPoint(index);

                    // 找到非空点，迭代记录
                    if (!float.IsNaN(p.Z) && !float.IsInfinity(p.Z) && p.Z > float.MinValue + 100)
                    {
                        startFlagX = x;
                        x++;
                    }

                    // 找到空点
                    else
                    {
                        // 当存在已记录点时，向x增大方向搜索
                        if (startFlagX != -1)
                        {
                            int r = 1;
                            bool isFound = false;
                            for (; r <= p_r; r++)
                            {
                                if (x + r >= orgPointSet.LengthN)
                                {
                                    break;
                                }
                                int p_index = orgPointSet.GetIndex(x + r, y);
                                ZXPoint p_pt = orgPointSet.GetPoint(p_index);
                                if (!float.IsNaN(p_pt.Z) && !float.IsInfinity(p_pt.Z) && p_pt.Z > float.MinValue + 100)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            // 搜索成功，则在两非空点间添加线段点
                            if (isFound)
                            {
                                ZXPoint startPoint = orgPointSet.Get(startFlagX, y);
                                ZXPoint endPoint = orgPointSet.Get(x + r, y);
                                var lineSet = DotInsert.InsertLine(startPoint, endPoint, _unit);
                                foreach (var pt in lineSet)
                                {
                                    newPoints.Add(pt);
                                }
                                x += r;
                                startFlagX = -1;
                            }
                            // 搜索失败 - 在指定距离内不存在另一端点，重置缓存，继续
                            else
                            {
                                x += p_r;
                                startFlagX = -1;
                            }
                        }
                        // 不存在起始端点，继续搜索
                        else
                        {
                            x++;
                        }
                    }
                }
            }

            Console.WriteLine("X轴内插法 新插入点数：" + newPoints.Count);
            //orgPointSet.Merge(newPoints);
            foreach(var pt in newPoints)
            {
                orgPointSet.Add(pt);
            }

            return newPoints;
        }

        /// <summary>
        /// 按Y轴方向插值 - duzixi Ver.1.0.2 2023.04.19 
        /// </summary>
        /// <param name="orgPointSet"></param>
        /// <param name="p_r">最大相邻点搜索半径</param>
        /// <returns></returns>
        public static ZXPointSet ByY(ref ZXPointSet orgPointSet, int p_r)
        {
            ZXPointSet newPoints = new ZXPointSet();

            if (!orgPointSet.IsFull())
            {
                LibTool.Debug("ByX 不是满秩阵");
                TurnToFull(ref orgPointSet);
            }

            orgPointSet.Bound = orgPointSet.Boundary;
            float unit = orgPointSet.Unit;

            for (int x = 0; x < orgPointSet.LengthN; x++)
            {
                int startFlagY = -1;
                for (int y = 0; y < orgPointSet.WidthN;)
                {
                    int index = orgPointSet.GetIndex(x, y);

                    ZXPoint p = orgPointSet.GetPoint(index);

                    if (!float.IsNaN(p.Z) && !float.IsInfinity(p.Z) && p.Z > float.MinValue + 100)
                    {
                        // CASE 1: 找到非空点，迭代记录
                        startFlagY = y;
                        y++;
                    }
                    else
                    {
                        // CASE 2: 找到空点
                        // 当存在已记录点时，向x增大方向搜索
                        if (startFlagY != -1)
                        {
                            int r = 1;
                            bool isFound = false;
                            for (; r <= p_r; r++)
                            {
                                if (y + r >= orgPointSet.WidthN)  // 2023.04.19
                                    break;
                                
                                int p_index = orgPointSet.GetIndex(x, y + r);
                                ZXPoint p_pt = orgPointSet.GetPoint(p_index);
                                if (!float.IsNaN(p_pt.Z) && !float.IsInfinity(p_pt.Z) && p_pt.Z > float.MinValue + 100)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            // 搜索成功，则在两非空点间添加线段点
                            if (isFound)
                            {
                                ZXPoint startPoint = orgPointSet.Get(x, startFlagY);

                                ZXPoint endPoint = orgPointSet.Get(x, y + r);
                                var lineSet = DotInsert.InsertLine(startPoint, endPoint, unit);
                                foreach (var pt in lineSet)
                                {
                                    newPoints.Add(pt);
                                }
                                y += r;
                                startFlagY = -1;
                            }
                            // 搜索失败 - 在指定距离内不存在另一端点，重置缓存，继续
                            else
                            {
                                y += p_r;
                                startFlagY = -1;
                            }
                        }
                        // 不存在起始端点，继续搜索
                        else
                        {
                            y++;
                        }
                    }
                }
            }

            // 在原满秩阵中，添加新点，保持满秩阵特征不变
            for (int i = 0; i < newPoints.Count; i++)
            {
                int index = orgPointSet.GetIndex(newPoints[i].X, newPoints[i].Y);
                orgPointSet[index].Z = newPoints[i].Z;  // 直接更新满秩阵高度值
            }

            Console.WriteLine("Y轴内插法 新插入点数：" + newPoints.Count);

            return newPoints;
        }

        /// <summary>
        /// 两点间按照指定精度添加线段点集 - Sanngoku 20220705  duzixi 2025.07.15
        /// </summary>
        /// <param name="_pt0">端点0</param>
        /// <param name="_pt1">端点1</param>
        /// <param name="_unit">精度</param>
        /// <returns>两点间所添加点集</returns>
        public static ZXPointSet InsertLine(ZXPoint _pt0, ZXPoint _pt1, float _unit)
        {
            float dx = _pt1.X - _pt0.X;
            float dy = _pt1.Y - _pt0.Y;
            float dz = _pt1.Z - _pt0.Z;
            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            int count = (int)Math.Round(distance / _unit);
            dz = dz / count;
            dx = dx / count;
            dy = dy / count;

            ZXPointSet result = new ZXPointSet();
            for (int i = 1;i < count; i++)
            {
                ZXPoint pt = new ZXPoint(_pt0.X + dx * i, _pt0.Y + dy * i, _pt0.Z + dz * i);
                result.Add(pt);
            }

            return result;
        }
    
        /// <summary>
        /// 插入圆
        /// </summary>
        /// <param name="_p0">圆心</param>
        /// <param name="_r">半径</param>
        /// <param name="_type">朝向</param>
        /// <param name="_angleUnit">角分辨率</param>
        /// <returns></returns>
        public static ZXPointSet InsertCircle(ZXPoint _p0, float _r, PlaneType _type, float _angleUnit)
        {
            ZXPointSet ps = new ZXPointSet();

            // 按角分辨率遍历算点

            for (float a = 0; a < 360; a += _angleUnit)
            {
                float x = (float)(_r * Math.Cos(a / 180.0 * Math.PI));
                float y = (float)(_r * Math.Sin(a / 180.0 * Math.PI));
                float z = _p0.Z;

                switch (_type)
                {
                    case PlaneType.XOY:
                        ps.Add(new ZXPoint(_p0.X + x, _p0.Y + y, z));
                        break;
                    case PlaneType.YOZ:
                        ps.Add(new ZXPoint(z, _p0.Y + x, _p0.Z + y));
                        break;
                    case PlaneType.XOZ:
                        ps.Add(new ZXPoint(_p0.X + x, z, _p0.Z + y));
                        break;
                    default:
                        break;
                }
            }

            return ps;
        }

        /// <summary>
        /// 两点之间线性插点
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="_num"></param>
        /// <returns></returns>
        public static ZXPointSet InsertPoint(ZXPoint start, ZXPoint end, int _num)
        {
            ZXPointSet points = new ZXPointSet();

            if (_num <= 0)
            {
                return points;
            }

            // 计算每个间隔的步长
            double step = 1.0 / (_num + 1);

            // 生成插入点
            for (int i = 1; i <= _num; i++)
            {
                double t = step * i;
                points.Add(start.Lerp(end, t));
            }

            return points;
        }
    }
}
