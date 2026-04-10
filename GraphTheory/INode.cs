using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCloudLib.GraphTheory
{

    /// <summary>
    /// 节点接口
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// 相邻
        /// </summary>
        /// <param name="p_node">比较节点</param>
        /// <param name="p_e">临近阈值</param>
        /// <returns>是否相邻</returns>
        bool NearTo(INode p_node, float p_e = 0.001f);

        /// <summary>
        /// 是拐点，即节点中心点不在一条直线上
        /// </summary>
        /// <param name="p_preNode">上一个节点</param>
        /// <param name="p_nextNode">下一个节点</param>
        /// <param name="p_e">临近阈值</param>
        /// <returns>是否是拐点</returns>
        bool IsFlex(INode p_preNode, INode p_nextNode, float p_e = 0.001f);
    }
}
