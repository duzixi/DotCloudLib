//=====================================================================
// 模块名称：计时器 ZXTimer
// 功能简介：算法计时, 对Stopwatch 类进行封装
// 版权声明：2019 九州创智科技有限公司  All Rights Reserved.
//           2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.9 杜子兮 移自“知行”C++库
//============================================


using System;
using System.Diagnostics;

namespace DotCloudLib
{
    /// <summary>
    /// 计时器
    /// </summary>
    public class ZXTimer
    {
        private string m_name; // 计时器名

        /// <summary>
        /// 计时器名
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        private Stopwatch m_timer; // 计时器

        /// <summary>
        /// 构造方法——计时开始
        /// </summary>
        /// <param name="p_name"></param>
        public ZXTimer(string p_name)
        {
            m_name = p_name;
            LibTool.Debug("" + m_name + "进行中....");
            m_timer = new Stopwatch();
            m_timer.Start();
        }

        /// <summary>
        /// 结束计时
        /// </summary>
        /// <returns>总用时（毫秒）</returns>
        public double End()
        {
            m_timer.Stop();
            TimeSpan ts = m_timer.Elapsed;
            LibTool.Debug("" + m_name + "结束，总耗时：" 
                + Math.Floor(ts.TotalMilliseconds / 1000) + "秒" 
                + Math.Round(ts.TotalMilliseconds % 1000) + " 毫秒");
            return ts.TotalMilliseconds;
        }

        /// <summary>
        /// 显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            m_timer.Stop();
            TimeSpan ts = m_timer.Elapsed;

            str += "" + m_name + "结束，总耗时："
                + Math.Floor(ts.TotalMilliseconds / 1000) + "秒"
                + Math.Round(ts.TotalMilliseconds % 1000) + " 毫秒";

            return str;
        }

    }
}
