//=====================================================================
// 模块名称：链表 ZXLink
// 功能简介：节点ZXNode的有序连接
// 版权声明：(C) 2021 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2021.12.30 杜子兮 创建
//          2023.05.23 王振宇 添加：最后一个节点、能量、拐点数量
//============================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCloudLib.GraphTheory
{
    /// <summary>
    /// 链表
    /// </summary>
    public class ZXLink<T> where T:INode
    {

        private List<ZXNode<T>> m_nodes = new List<ZXNode<T>>();

        /// <summary>
        /// 链表中按顺序排列的节点 Node0 -> Node1 -> .... Node N
        /// </summary>
        public List<ZXNode<T>> Nodes { get => m_nodes; set => m_nodes = value; }

        /// <summary>
        /// 最后一个节点
        /// </summary>
        public ZXNode<T> EndNode { get { return Nodes[Nodes.Count - 1]; } }

        /// <summary>
        /// 返回链表中节点个数
        /// </summary>
        public int Count { get { return Nodes.Count; } }

        /// <summary>
        /// 能量
        /// </summary>
        public float Energy { get; set; }

        /// <summary>
        /// 折点数量
        /// </summary>
        public int FlexNum { get; set; }

        /// <summary>
        /// 构造方法-根据节点构建链表
        /// </summary>
        /// <param name="p_nodes"></param>
        public ZXLink(List<ZXNode<T>> p_nodes)
        {
            m_nodes = p_nodes;
        }

        /// <summary>
        /// 唯一化
        /// </summary>
        public void Uniquify()
        {
            for (int i = this.Count - 1; i > 0; i--)
            {
                if (this.Nodes[i].Id == this.Nodes[i - 1].Id)
                {
                    this.Nodes.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 清洗
        /// </summary>
        public void Clear()
        {
            foreach (ZXNode<T> node in this.Nodes)
            {
                node.isDirty = false;
            }
        }
        
        /// <summary>
        /// 比较两个链表是否相等
        /// </summary>
        /// <param name="p_link">另一个链表</param>
        /// <returns>是否相等</returns>
        public bool EqualTo(ZXLink<T> p_link)
        {
            if (this.Count != p_link.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Count; i++)
            {
                if (this.Nodes[i].Id != p_link.Nodes[i].Id)
                {
                    return false;
                }

            }

            return true;
        }


        /// <summary>
        /// 【调试用】输出链表中的节点ID
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";

            for (int i = 0; i < this.Count; i++)
            {
                str += this.Nodes[i].Id;
                str += this.Nodes[i].isFlex ? "[拐点]" : "";
                str += this.Nodes[i].Content.ToString();

                if (i != this.Count - 1)
                {
                   str += " -> ";
                }
            }

            return str;
        }
    }
}
