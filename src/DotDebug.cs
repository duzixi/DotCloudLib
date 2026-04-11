using System;
using System.IO;

namespace DotCloudLib
{
    /// <summary>
    /// 调试工具类
    /// </summary>
    public class DotDebug
    {
        private string m_logPath = "";

        /// <summary>
        /// Log路径
        /// </summary>
        public string LogPath { get { return m_logPath; } }
        
        /// <summary>
        /// 构造方法
        /// </summary>
        public DotDebug()
        {

            this.m_logPath = "";
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="p_logPath"></param>
        public DotDebug(string p_logPath)
        {
            this.m_logPath = p_logPath;
            this.Log("==================本次LOG记录开始=================");
        }

        /// <summary>
        /// 重新设置Log Path
        /// </summary>
        /// <param name="p_logPath"></param>
        public void SetLogPath(string p_logPath)
        {
            this.m_logPath = p_logPath;
            this.Log("=================================================");
            this.Log(MetaData.LIB_NAME);
            this.Log(MetaData.VERSION);
            this.Log("(" + MetaData.COMMENT + ")");
            this.Log(MetaData.COPY_RIGHT);
            this.Log("==================本次LOG记录开始=================");
        }

        /// <summary>
        /// 清除LOG文件路径（不再输出）
        /// </summary>
        public void ClearLogPath()
        {
            this.Log("-------------------本次LOG记录结束-----------------");
            this.m_logPath = "";
        }

        /// <summary>
        /// 判别是否是调试模式
        /// </summary>
        public bool DebugMode
        {
            get
            {
                return (m_logPath != "") ? true : false;
            }
        }

        /// <summary>
        /// 【记录模式】写入LOG文件
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool Log(string content)
        {
            if (DebugMode)
            {
                try
                {
                    // 2023.11.30
                    string path = Path.GetDirectoryName(m_logPath);

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(m_logPath, true))
                    {
                        file.WriteLine("[" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + "]【DotCloudLib】" + content);// 直接追加文件末尾，换行 
                        file.Close();
                    }
                }
                catch (Exception ex)
                {
                    LibTool.Error(ex, "DotDebug@90");
                }
            }
            return true;
        }

        /// <summary>
        /// 【调试模式】控制台打印，并写入LOG文件
        /// </summary>
        /// <param name="content"></param>
        public void Debug(string content)
        {
            if (LibTool.WriteConsole)
            {
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") +"] " + content);
            }
            
            this.Log(content);
        }

        /// <summary>
        /// 【错误模式】错误提示一条龙服务
        /// </summary>
        /// <param name="content"></param>
        public void Error(string content)
        {
            this.Debug("Error_" + content);
        }

    }
}
