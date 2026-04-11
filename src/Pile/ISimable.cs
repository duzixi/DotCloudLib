//=====================================================================
// 模块名称：Simable 料堆仿真行为定义
// 功能简介：料堆仿真堆取变化后，可调用注册事件
// 版权声明： 2023 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2023.05  Sanngoku  创建     
//============================================

using System;
using System.Collections.Generic;


namespace DotCloudLib
{
    /// <summary>
    /// 序号特征点
    /// </summary>
    public struct IndicesPoint
    {
        /// <summary>
        /// 点在原始点集中的序号
        /// </summary>
        public int Index;
        /// <summary>
        /// 点在原始点集中的二维X坐标序号
        /// </summary>
        public int xIndex;
        /// <summary>
        /// 点在原始点集中的二维Y坐标序号
        /// </summary>
        public int yIndex;
        /// <summary>
        /// 点的更新后Z值
        /// </summary>
        public float Z;
        /// <summary>
        /// 点在更新前的Z值
        /// </summary>
        public float BeforeZ;
    }

    /// <summary>
    /// 仿真后事件参数
    /// </summary>
    public class SimulatedArgs : EventArgs
    {
        /// <summary>
        /// 仿真是否使得点集发生变化
        /// 若false，则list_points为空容器
        /// </summary>
        public bool isChanged;

        /// <summary>
        /// 仿真使得变化的点集
        /// </summary>
        public List<IndicesPoint> list_Points;

        /// <summary>
        /// 仿真使得体积变化量  大于 0 为增， 小于 0 为减
        /// </summary>
        public float Volumn;
    }

    /// <summary>
    /// 可仿真接口 - 特指料堆的可仿真变化行为
    /// </summary>
    internal interface ISimable
    {
        /// <summary>
        /// 仿真后的变化事件
        /// </summary>
        EventHandler<SimulatedArgs> SimulatedHandler { get; set; }
    }
}
