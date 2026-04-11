//=====================================================================
// 模块名称：进度条 UIProcessBar
// 应用环境：控制台 Console
// 功能简介：控制台进度条
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2021.1.22 杜子兮 创建
//============================================

using System;

namespace DotCloudLib
{
    /// <summary>
    /// 进度条
    /// </summary>
    public class UIProcessBar
    {
        /// <summary>
        /// 光标的列位置。将从 0 开始从左到右对列进行编号。
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// 光标的行位置。从上到下，从 0 开始为行编号。
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// 进度条宽度。
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 进度条当前值。
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 进度条背景色
        /// </summary>
        static public ConsoleColor s_backgroundColor = ConsoleColor.DarkMagenta;

        /// <summary>
        /// 进度条前景色
        /// </summary>
        static public ConsoleColor s_foregroundColor = ConsoleColor.DarkCyan;

        /// <summary>
        /// 进度条文字颜色
        /// </summary>
        static public ConsoleColor s_fontColor = ConsoleColor.DarkCyan;
       
        private ConsoleColor colorBack;
        private ConsoleColor colorFore;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public UIProcessBar() : this(Console.CursorLeft, Console.CursorTop)
        {

        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="p_left"></param>
        /// <param name="p_top"></param>
        /// <param name="p_width"></param>
        public UIProcessBar(int p_left, int p_top, int p_width = 50)
        {
            this.Left = p_left;
            this.Top = p_top;
            this.Width = p_width;

            // 清空显示区域
            Console.SetCursorPosition(Left, Top);
            for (int i = p_left; ++i < Console.WindowWidth;) { Console.Write(" "); }

            // 绘制进度条背景
            colorBack = Console.BackgroundColor;  // 记录背景色
            Console.SetCursorPosition(Left, Top);
            Console.BackgroundColor = s_backgroundColor; // 赋予新的颜色
            for (int i = 0; ++i <= p_width;) { Console.Write(" "); }
            Console.BackgroundColor = colorBack;  // 还原背景色

        }

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Display(int value)
        {
            return Display(value, null);
        }

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="value"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int Display(int value, string msg)
        {
            if (this.Value != value)
            {
                this.Value = value;

                // 保存背景色与前景色；
                colorBack = Console.BackgroundColor;
                colorFore = Console.ForegroundColor;
                // 绘制进度条进度                
                Console.BackgroundColor = s_foregroundColor;
                Console.SetCursorPosition(this.Left, this.Top);
                Console.Write(new string(' ', (int)Math.Round(this.Value / (100.0 / this.Width))));
                Console.BackgroundColor = colorBack;

                // 更新进度百分比   
                Console.ForegroundColor = s_fontColor;
                Console.SetCursorPosition(this.Left + this.Width + 1, this.Top);
                if (string.IsNullOrEmpty(msg)) { Console.Write("{0}%", this.Value); } else { Console.Write(msg); }
                Console.ForegroundColor = colorFore;
            }
            return value;
        }
     
        /// <summary>
        /// 结束
        /// </summary>
        public void Complete()
        {
            Display(100);
            Console.WriteLine();
        }
    }
}
