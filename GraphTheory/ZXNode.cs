//=====================================================================
// 模块名称：节点 ZXNode
// 功能简介：用于图搜索的节点泛型，任何继承接口INode的类或结构体均可称为节点内的内容
// 版权声明：(C) 2021 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2021.12.24 杜子兮 创建
//          2021.12.28 王振宇 添加搜索邻接节点方法
//============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCloudLib.GraphTheory
{
    /// <summary>
    /// 节点类（泛型）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ZXNode<T> where T:INode
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public enum NodeType
        {
            /// <summary>
            /// 无
            /// </summary>
            NONE,
            /// <summary>
            /// 起始
            /// </summary>
            START,
            /// <summary>
            /// 终止
            /// </summary>
            END,
            /// <summary>
            /// 障碍
            /// </summary>
            OBSTACLE
        }


        private T m_content;
        private List<ZXNode<T>> m_nodes = new List<ZXNode<T>>();

        /// <summary>
        /// 计数器
        /// </summary>
        public static int counter = 0;

        /// <summary>
        /// 节点ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 是否被搜索
        /// </summary>
        public bool isDirty { get; set; }

        /// <summary>
        /// 是否是拐点
        /// </summary>
        public bool isFlex { get; set; }

        /// <summary>
        /// 搜索过程中节点类型
        /// </summary>
        public NodeType Type { get; set; }

        /// <summary>
        /// 节点中包含的元素
        /// </summary>
        public T Content { get => m_content; set => m_content = value; }

        /// <summary>
        /// 相邻节点
        /// </summary>
        public List<ZXNode<T>> Nodes { get => m_nodes; set => m_nodes = value; }

        /// <summary>
        /// 构造节点
        /// </summary>
        /// <param name="p_content"></param>
        public ZXNode(T p_content)
        {
            Id = "NODE_" + counter++; 
            m_content = p_content;
            isDirty = false;
            this.Type = NodeType.NONE;  
        }

        /// <summary>
        /// 搜索邻接节点
        /// </summary>
        /// <param name="p_nodes">被搜索节点列表</param>
        public void FindLinkNodes(List<ZXNode<T>> p_nodes) {
             
            // 遍历被搜索节点列表
            foreach (ZXNode<T> item in p_nodes)
            {
                // 要先判断是不是自身
                if (item == this)
                    continue;

                // 存入m_nodes
                if (item.m_content.NearTo(this.m_content) && !m_nodes.Contains(item)) {
                    m_nodes.Add(item);
                }

            }
        }

        /// <summary>
        /// 搜索当前节点和目标节点的所有链路
        /// </summary>
        /// <param name="p_node">目标节点</param>
        /// <param name="reverse">是否反向搜索，默认为false，正向搜索</param>
        /// <returns></returns>
        public ZXLink<T> FindLinks (ZXNode<T> p_node, bool reverse = false)
        {
            List<ZXNode<T>> link = new List<ZXNode<T>>();

            if (this.isDirty)
                return new ZXLink<T>(link);

            // CASE 0: 目标节点是自身, 返回只有一个节点的链表
            if (this.Id == p_node.Id)
            {
                link.Add(this);
                this.isDirty = true;
                return new ZXLink<T>(link);
            }

            // 循环正反向可控制
            int startI = reverse ? this.Nodes.Count - 1 : 0;
            int endI = reverse ? -1 : this.Nodes.Count;
            int offset = reverse ?  -1 : 1;

            for (int i = startI; i != endI; i+= offset)
            {
                if (this.Nodes[i].isDirty)
                    continue; // 搜过的不搜

                this.isDirty = true;
                link.Add(this);

                // CASE 1: 下一个节点就是目标节点
                if (this.Nodes[i].Id == p_node.Id)
                {
                    this.Nodes[i].isDirty = true;
                    link.Add(this.Nodes[i]);
                    return new ZXLink<T>(link);
                }

                // CASE 2: 下一个节点不是目标节点
                ZXLink<T> subLink = this.Nodes[i].FindLinks(p_node, reverse);
                if (subLink.Count != 0)
                {
                    link.AddRange(subLink.Nodes);
                    return new ZXLink<T>(link);
                }
            }

            return new ZXLink<T>(link);
        }

        /// <summary>
        /// 【调试用】输出
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            str += this.Id + " ";
            str += isDirty ? "[*]" : "[ ]";
            str += this.Content.ToString();

            return str;

        }
    }
}
