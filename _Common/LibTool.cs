//=====================================================================
// 模块名称：算法库工具类 LibTool
// 功能简介：控制整个库的LOG输出
// 版权声明：2021 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2021.11.   杜子兮 创建
//          2025.09.13 杜子兮 + DebugMode 调试模式控制变量
//============================================

using System;

namespace DotCloudLib
{
    /// <summary>
    /// 库管理器
    /// </summary>
    public class LibTool
    {
        /// <summary>
        /// 如果当前为调试模式，则输出点集，否则不输出   2025.09.13 +
        /// 【重要】每次发布前，要改成false
        /// </summary>
        public static bool DebugMode = true;

        /// <summary>
        /// 当前计算的调试对象
        /// </summary>
        private static DotDebug debug;

        /// <summary>
        /// 是否输出控制台
        /// </summary>
        public static bool WriteConsole = true;

        /// <summary>
        /// 开始输出Log
        /// </summary>
        /// <param name="p_path"></param>
        public static void StartLog(string p_path)
        {
            debug = new DotDebug(p_path);
        }

        /// <summary>
        /// 结束输出Log
        /// </summary>
        public static void StopLog()
        {
            if (debug != null)
            {
                debug.Debug("结束输出Log -> " + debug.LogPath);
                debug.ClearLogPath();
            }
        }

        /// <summary>
        /// Log文件输出
        /// </summary>
        /// <param name="content"></param>
        public static void Log(object content)
        {
            if (debug != null)
            {
                debug.Log(content.ToString());
            }
        }

        /// <summary>
        /// 控制台与文件输出
        /// </summary>
        /// <param name="content"></param>
        public static void Debug(object content)
        {
            if (debug != null)
            {
                debug.Debug(content.ToString());
            }
            
        }

        /// <summary>
        /// 报错
        /// </summary>
        /// <param name="content"></param>
        public static void Error(object content)
        {
            if (debug != null)
            {
                debug.Error(content.ToString());
            }
        }

        /// <summary>
        /// 错误处理
        /// </summary>
        /// <param name="ex">错误对象</param>
        /// <param name="source">代码源</param>
        /// <param name="helpLink">辅助信息</param>
        public static void Error(Exception ex, string source, string helpLink = "")
        {
            ex.Source = "DotCloudLib " + MetaData.VERSION + " " + source;
            ex.HelpLink = helpLink;
            string log = ex.Source + "\n" + ex.ToString() + "\n" + ex.HelpLink;
            // Console.WriteLine(log);
            if (debug != null)
            {
                debug.Error(log);
            }
            throw ex;
            
        }

        /// <summary>
        /// 显示当前库信息
        /// </summary>
        public static void ShowLibInfo()
        {
            if (debug == null)
            {
                debug = new DotDebug();
            }

            debug.Debug("===================================================================");
            debug.Debug("*    " + MetaData.LIB_NAME + "  " + MetaData.SLOGAN + " " + MetaData.VERSION);
            debug.Debug("-------------------------------------------------------------------");
            // debug.Debug("*         (" + MetaData.COMMENT + "）");
            debug.Debug("*  " + MetaData.COPY_RIGHT);
            debug.Debug("===================================================================");
        }
    }

    /// <summary>
    /// 浮点数精度工具类
    /// </summary>
    public static class Precisionf
    {
        public const float Epsilon = 1e-4f;

        public static bool Equals(float a, float b)
        {
            return Math.Abs(a - b) < Epsilon;
        }

        public static bool GreaterThan(float a, float b)
        {
            return a - b > Epsilon;
        }

        public static bool LessThan(float a, float b)
        {
            return b - a > Epsilon;
        }

        public static bool GreaterEqualThan(float a, float b)
        {
            return a - b >= Epsilon;
        }

        public static bool LessEqualThan(float a, float b)
        {
            return b - a >= Epsilon;
        }

        public static bool IsPointEqual(ZXPoint a, ZXPoint b)
        {
            return Math.Abs(a.X - b.X) < Epsilon && Math.Abs(a.Y - b.Y) < Epsilon && Math.Abs(a.Z - b.Z) < Epsilon;
        }
    }
}
