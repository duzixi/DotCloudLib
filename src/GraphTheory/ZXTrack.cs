//=====================================================================
// 模块名称：轨迹 ZXTrack
// 功能简介：有序点集，顺序连接形成轨迹
// 版权声明：(C) 2021 ~ 2022 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2021.12.31 杜子兮 创建 
//          2023.05.24 王振宇 二维包围盒避让搜索
//============================================

using Mathd;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DotCloudLib.GraphTheory
{
    /// <summary>
    /// 点轨迹
    /// </summary>
    public class ZXTrack
    {
        /// <summary>
        /// 点集
        /// </summary>
        public List<ZXPoint> Points { get; set; } = new List<ZXPoint>();

        /// <summary>
        /// 点数
        /// </summary>
        public int Count
        {
            get
            {
                return this.Points.Count;
            }
        }

        /// <summary>
        /// 反馈给上层的消息提示
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 构造方法——根据点集构造轨迹
        /// </summary>
        /// <param name="p_points"></param>
        public ZXTrack(List<ZXPoint> p_points)
        {
            this.Points = p_points;
            this.Message = "";
        }

        /// <summary>
        /// 唯一化
        /// </summary>
        public void Uniquify(float p_e = 0.001f)
        {
            // STEP 1: 去掉相邻重复点
            for (int i = this.Count - 1; i >= 1; i--)
                if (this.Points[i].DistanceTo(this.Points[i - 1]) < p_e)
                    this.Points.RemoveAt(i);

            // STEP 2: 去掉一条直线上的点
            for (int i = this.Count - 2; i >= 1; i--)
            {
                if (!this.Points[i].IsFlex(this.Points[i + 1], this.Points[i - 1]))
                {
                    LibTool.Debug(this.Points[i].Id + this.Points[i].ToString() + "不是拐点，删除");
                    this.Points.RemoveAt(i);
                } else
                {
                    LibTool.Debug(this.Points[i].Id + this.Points[i].ToString() + "是拐点");
                }
            }
        }

        /// <summary>
        /// 唯一化（判定仅考虑二维，原始点三维信息仍保留）
        /// </summary>
        public void UniquifyXY(float p_e = 0.001f)
        {
            // STEP 1: 去掉相邻重复点
            for (int i = this.Count - 1; i >= 1; i--)
            {
                ZXPoint p0 = new ZXPoint(this.Points[i - 1].X, this.Points[i - 1].Y, 0);
                ZXPoint p1 = new ZXPoint(this.Points[i].X, this.Points[i].Y, 0);
                
                if (p0.DistanceTo(p1) < p_e)
                { 
                    this.Points.RemoveAt(i);
                }
            }

            // STEP 2: 去掉一条直线上的点
            for (int i = this.Count - 2; i >= 1; i--)
            {
                ZXPoint p0 = new ZXPoint(this.Points[i - 1].X, this.Points[i - 1].Y, 0); // 前一个点
                ZXPoint p1 = new ZXPoint(this.Points[i].X, this.Points[i].Y, 0);  // 当前点
                ZXPoint p2 = new ZXPoint(this.Points[i + 1].X, this.Points[i + 1].Y, 0); // 后一个点

                if (!p1.IsFlex(p2, p0))
                {
                    LibTool.Debug(this.Points[i].Id + this.Points[i].ToString() + "不是拐点，删除");
                    this.Points.RemoveAt(i);
                }
                else
                {
                    LibTool.Debug(this.Points[i].Id + this.Points[i].ToString() + "是拐点，保留");
                }
            }
        }

        /// <summary>
        /// 求二维包围盒外一点，到二维包围盒边的最短垂直路径
        /// </summary>
        /// <param name="p_point">包围盒外一点</param>
        /// <param name="p_bound">二维包围盒</param>
        /// <returns></returns>
        public static ZXTrack GetMinTrack(ZXPoint p_point, ZXBoundary p_bound)
        {
            // STEP 0: 初始化
            ZXTrack track = new ZXTrack(new List<ZXPoint> { });

            // STEP 1: 参数校验

            if (!p_bound.ContainX(p_point.X) && !p_bound.ContainY(p_point.Y))
            {
                LibTool.Debug("ZXTrack0059: 点 " + p_point.ToString() + " 到包围盒" + p_bound.ToString() + "无垂直最短距离"); // 2023.11.05 BUG
                return track;
            }

            if (p_bound.ContainXY(p_point))
            {
                LibTool.Debug("ZXTrack0065: 点 " + p_point.ToString() + " 在包围盒" + p_bound.ToString() + "内，无最短距离");
                return track;
            }

            // STEP 2: 求最短垂直线段

            track.Points.Add(p_point);

            
            if (p_bound.ContainX(p_point.X))
            {
                // CASE 1: 点在包围盒上或下
                if (p_point.Y > p_bound.Center.Y)
                {
                    // CASE 1.1: 点在包围盒上
                    track.Points.Add(new ZXPoint(p_point.X, p_bound.MaxY, p_bound.MaxZ));
                } 
                else
                {
                    // CASE 1.2: 点在包围盒下
                    track.Points.Add(new ZXPoint(p_point.X, p_bound.MinY, p_bound.MaxZ));
                }
            }
            else
            {
                // CASE 2: 点在包围盒左或右
                if (p_point.X < p_bound.Center.X)
                {
                    // CASE 2.1: 点在包围盒左
                    track.Points.Add(new ZXPoint(p_bound.MinX, p_point.Y, p_bound.MaxZ));
                } else
                {
                    // CASE 2.2: 点在包围盒右
                    track.Points.Add(new ZXPoint(p_bound.MaxX, p_point.Y, p_bound.MaxZ));
                }
            }

            return track;
        }

        /// <summary>
        /// 搜索最佳轨迹
        /// </summary>
        /// <param name="p_workArea">工作区域</param>
        /// <param name="p_safeAreas">安全区数组</param>
        /// <param name="p_start">始点</param>
        /// <param name="p_end">终点</param>
        /// <returns></returns>
        public static ZXTrack SearchBestTrack(ZXBoundary p_workArea, List<ZXBoundary> p_safeAreas, ZXPoint p_start, ZXPoint p_end)
        {
            // 调试用
            
            // new ZXPointSet().AddDotBoundary(p_workArea).SaveAsXYZ("D:/Data/workArea.xyz");
            // for (int i = 0; i < p_safeAreas.Count; i++)
            //    new ZXPointSet().AddDotBoundary(p_safeAreas[i]).SaveAsXYZ("D:/Data/p_safeAreas"+i+".xyz");
            // new ZXPointSet().Add(p_start)).SaveAsXYZ("D:/Data/p_start.xyz");

            // STEP 0: 初始化
            ZXTrack track = new ZXTrack(new List<ZXPoint> { });  // 无可行路径时返回空轨迹

            List<ZXNode<ZXBoundary>> nodes = new List<ZXNode<ZXBoundary>>();

            // STEP 1: 参数校验，判定无可行路径的情况 CASE 0
            if (!p_workArea.Contain(p_start))
                track.Message = "CASE 0.1: 作业区域不包含始点" + p_start.ToString() + "，无可行路径";
            else if (!p_workArea.Contain(p_end))
                track.Message = "CASE 0.2: 作业区域不包含终点" + p_end.ToString() + "，无可行路径";
            else
            {
                for (int i = 0; i < p_safeAreas.Count; i++)
                {
                    if (p_safeAreas[i].Contain(p_start))
                        track.Message = "CASE 0.3: 始点 " + p_start.ToString() + " 在安全区" + p_safeAreas[i].ToString() + "中，无可行路径";
                    else if (p_safeAreas[i].Contain(p_end))
                        track.Message = "CASE 0.4: 终点 " + p_end.ToString() + " 在安全区" + p_safeAreas[i].ToString() + "中，无可行路径";
                }
            }

            if (track.Message != "")
            {
                LibTool.Debug(track.Message);
                return track;
            }

            // CASE 1.0: 直连 —— 无安全区
            if (p_safeAreas.Count == 0)
            {
                track = new ZXTrack(new List<ZXPoint> { p_start, p_end });
                track.Message = "CASE 1.0: 无安全区，直连";
                LibTool.Debug(track.Message);
                return track;
            }

            // CASE 1.1: 直连 —— 安全区与始点连线不相交

            // 【未完待续】安全考虑，咱不实现


            // STEP 2: 构造搜索图
            List<ZXBoundary> bounds = p_workArea.SplitBy(p_safeAreas);

            // for (int i = 0; i < bounds.Count; i++)
            //    new ZXPointSet().AddDotBoundary(bounds[i]).SaveAsXYZ("D:/Data/bounds" + i + ".xyz");

            if (bounds.Count == 0)
            {
                track.Message = "CASE 0.5: 工作域被安全区切割后，无可行路径";
                LibTool.Debug(track.Message);
                return track;
            }

            track = new ZXTrack(new List<ZXPoint> { p_start, p_end });
            ZXNode<ZXBoundary> start = new ZXNode<ZXBoundary>(bounds[0]);
            ZXNode<ZXBoundary> end = new ZXNode<ZXBoundary>(bounds[0]);

            for (int i = 0; i < bounds.Count; i++)
            {
                if (bounds[i].ContainXY(p_start) && bounds[i].ContainXY(p_end))
                {
                    // CASE 1.2: 在同一个包围盒内，直连
                    track.Message = "CASE 1.2: 在同一个包围盒内，直连";
                    track = new ZXTrack(new List<ZXPoint> { p_start, p_end });  // 2023.11.03
                    LibTool.Debug(track.Message);
                    return track; 
                }
                else
                {
                    ZXNode<ZXBoundary> tempNode = new ZXNode<ZXBoundary>(bounds[i]);

                    if (bounds[i].ContainXY(p_start))
                    {
                        tempNode.Type = ZXNode<ZXBoundary>.NodeType.START;
                        start = tempNode;
                    }
                    else if (bounds[i].ContainXY(p_end))
                    {
                        tempNode.Type = ZXNode<ZXBoundary>.NodeType.END;
                        end = tempNode;
                    }
                    nodes.Add(tempNode);
                }
            }

            nodes.ForEach(P => { P.FindLinkNodes(nodes); });

            // STEP 3: 搜索最佳链路
            ZXLink<ZXBoundary> link = new ZXLink<ZXBoundary>(new List<ZXNode<ZXBoundary>>()); // 最终链路

            // 搜索过程中的链路
            List<ZXLink<ZXBoundary>> uncompleteLink = new List<ZXLink<ZXBoundary>>() { new ZXLink<ZXBoundary>(new List<ZXNode<ZXBoundary>> { start }) };

            while (uncompleteLink.Count != 0)
            {
                // 取出最后一条链路
                ZXLink<ZXBoundary> tempLink = uncompleteLink[uncompleteLink.Count - 1];

                // 如果最后一个点是终止点，保存起来，并从列表中删除，进行下一次检索
                //（因为之前判断了折点数量，所以这条链路折点数量一定是最小的）
                if (tempLink.EndNode == end)
                {
                    if (link.Nodes.Count != 0)
                    {
                        if (tempLink.FlexNum == link.FlexNum)
                            link = tempLink.Energy < link.Energy ? tempLink : link;
                        else if (tempLink.FlexNum < link.FlexNum)
                            link = tempLink;
                    }
                    else
                        link = tempLink;

                    uncompleteLink.Remove(tempLink);
                    continue;
                }

                // 遍历最后一条链路的 最后一个节点的 所有相邻节点
                foreach (var item in tempLink.EndNode.Nodes)
                {
                    if (!tempLink.Nodes.Contains(item))
                    {
                        // 获取最后一条链路的所有节点
                        List<ZXNode<ZXBoundary>> newNodes = new List<ZXNode<ZXBoundary>>(tempLink.Nodes);

                        // 计算最后一条链路的 最后一个节点 和 新的相邻节点的能量
                        ZXNode<ZXBoundary> endNode = newNodes[newNodes.Count - 1];
                        ZXNode<ZXBoundary> newNode = item;
                        ZXLink<ZXBoundary> newLink = new ZXLink<ZXBoundary>(newNodes);
                        newLink.FlexNum = tempLink.FlexNum;
                        newLink.Energy = tempLink.Energy + endNode.Content.Center.DistanceTo(newNode.Content.Center);  //赋值能量

                        //  最后一条链路的所有节点，添加本次遍历的节点
                        newNodes.Add(item);

                        if (newNodes.Count >= 3)
                        {
                            ZXNode<ZXBoundary> preNode = newNodes[newNodes.Count - 3];

                            bool hasFlex = endNode.Content.IsFlex(preNode.Content, newNode.Content);

                            //如果endNode是拐点
                            if (hasFlex)
                            {
                                newNodes[newNodes.Count - 2].isFlex = true;

                                newLink.FlexNum = tempLink.FlexNum + 1;  //检测折点数量
                                if (link.Nodes.Count != 0 && link.FlexNum < tempLink.FlexNum)
                                    continue;
                            }
                        }
                        uncompleteLink.Add(newLink); //  建立一条新的链路，存入链路池
                    }
                }
                uncompleteLink.Remove(tempLink); // 删除检索链路
            }

            // 重新判定拐点
            foreach (var node in link.Nodes)
            { 
                node.isFlex = false;
            }
            
            for (int i = 1; i < link.Nodes.Count - 1; i++)
            {
                ZXNode<ZXBoundary> preNode = link.Nodes[i - 1];
                ZXNode<ZXBoundary> currentNode = link.Nodes[i];
                ZXNode<ZXBoundary> nextNode = link.Nodes[i + 1];

                bool isFlex = currentNode.Content.IsFlex(preNode.Content, nextNode.Content);
                if (isFlex)
                {
                    currentNode.isFlex = true;
                }
            }

            LibTool.Debug("找到最佳链路： " + link.ToString());

            /*
            for (int i = 0; i < link.Count; i++)
            {
                ZXBoundary node = link.Nodes[i].Content;
                new ZXPointSet().AddDotBoundary(node).SaveAsXYZ("D:/Data/" + link.Nodes[i].Id + ".xyz");
            }
            */

            // STEP 4: 根据最佳链路，生成最佳轨迹

            if (link.FlexNum == 0)
            {
                // CASE 1: 无拐点，直连
                track.Message = "CASE 1.3: 无拐点，直连";
                track = new ZXTrack(new List<ZXPoint> { p_start, p_end });  // 2023.11.03
                LibTool.Debug(track.Message);
                return track;
            }

            // CASE 2: 有拐点
            int flexNum = 0;
            ZXPoint p = p_start;
            track.Points.Clear();

            for (int i = 0; i < link.Nodes.Count; i++)
            {
                if (link.Nodes[i].isFlex)
                {
                    // 如果是拐点
                    flexNum++;

                    ZXBoundary bNode = link.Nodes[i].Content;

                    ZXTrack minTrack = ZXTrack.GetMinTrack(p, bNode);

                    track.Points.AddRange(minTrack.Points);  // 求上一个点到拐点包围盒的垂直连线
                    p = track.Points[track.Points.Count - 1]; // 交点 p

                    if (flexNum == link.FlexNum)
                    {
                        // CASE 2.1: 当前最后一个拐点和终点
                        ZXTrack lastTrack = ZXTrack.GetMinTrack(p_end, link.Nodes[i].Content);
                        ZXPoint pLast = lastTrack.Points[1]; // 最后一个交点
                        ZXPoint pCenter = new ZXPoint(pLast.X, pLast.Y, pLast.Z);

                        if (Math.Abs(p.X - bNode.MinX) < 0.001 || Math.Abs(p.X - bNode.MaxX) < 0.001)
                        {
                            // CASE 1: 交点 p 在拐点包围盒左或右
                            pCenter.Y = p.Y;
                        }
                        else
                        {
                            // CASE 2: 交点 p 在拐点包围盒上或下
                            pCenter.X = p.X;
                        }
                        track.Points.Add(pCenter);
                        track.Points.Add(pLast);
                        track.Points.Add(p_end);
                    } 
                    else
                    {
                        // CASE 2.2: 不是最后一个拐点
                        ZXPoint pCenter = new ZXPoint(p.X, p.Y, p.Z);

                        if (Math.Abs(p.X - bNode.MinX) < 0.001 || Math.Abs(p.X - bNode.MaxX) < 0.001)
                        {
                            // CASE 1: 交点 p 在拐点包围盒左或右
                            pCenter.X = bNode.OX; // X取拐点包围盒中点，Y与交点 p 相同
                        }
                        else
                        {
                            // CASE 2: 交点 p 在拐点包围盒上或下
                            pCenter.Y = bNode.OY; // Y取拐点包围盒中点，X与交点 p 相同
                        }

                        track.Points.Add(pCenter);
                    }
                }
            }

            track.UniquifyXY();
            track.Message = "CASE 2: 有拐点，避让路径：" + track.ToString();

            LibTool.Debug(track.Message);

            return track;
        }



        /// <summary>
        /// 【调试用】输出轨迹中点序列
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";

            for (int i = 0; i < this.Count; i++)
            {
                str += this.Points[i].ToString();

                if (i != this.Count - 1)
                {
                    str += " -> ";
                }
            }

            return str;
        }

        /// <summary>
        /// 【调试用】可视化为轨迹点集
        /// </summary>
        /// <param name="p_unit"></param>
        /// <returns></returns>
        public ZXPointSet Visualize(float p_unit = 0.1f)
        {
            ZXPointSet ps = new ZXPointSet();
            for (int i = 0; i < Points.Count - 1; i++)
            {
                ps.AddDotSegment(Points[i], Points[i + 1], p_unit);
            }

            return ps;
        }

    }
}
