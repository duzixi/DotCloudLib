//=====================================================================
// 模块名称：数字点云可视化 DotNumber
// 功能简介：将整数转为可视点集
// 版权声明：2025 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2025.04.15 杜子兮 创建
//=====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCloudLib
{
    public class DotNumber
    {
        /// <summary>
        /// 0
        /// </summary>
        internal static int[,] Zero = new int[5, 3] { // 5行3列
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        /// <summary>
        /// 1
        /// </summary>
        internal static int[,] One = new int[5, 3]
        {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        /// <summary>
        /// 2
        /// </summary>
        internal static int[,] Two = new int[5, 3]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        internal static int[,] Three = new int[5, 3] {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        internal static int[,] Four = new int[5, 3] {
            {1, 0, 1},
            {1, 0, 1},
            {1, 1, 1},
            {0, 0, 1},
            {0, 0, 1}
        };

        internal static int[,] Five = new int[5, 3] {
            {1, 1, 1},
            {1, 0, 0},
            {1, 1, 1},
            {0, 0, 1},
            {1, 1, 1}
        };

        internal static int[,] Six = new int[5, 3]{
                {1, 1, 1},
                {1, 0, 0},
                {1, 1, 1},
                {1, 0, 1},
                {1, 1, 1}
        };

        internal static int[,] Seven = new int[5, 3]{
                {1, 1, 1},
                {0, 0, 1},
                {0, 0, 1},
                {0, 0, 1},
                {0, 0, 1}
        };

        internal static int[,] Eight = new int[5, 3] {
                {1, 1, 1},
                {1, 0, 1},
                {1, 1, 1},
                {1, 0, 1},
                {1, 1, 1}
        };

        internal static int[,] Nine = new int[5, 3] {
                {1, 1, 1},
                {1, 0, 1},
                {1, 1, 1},
                {0, 0, 1},
                {1, 1, 1}
        };

        static List<int[,]> NumberDot = new List<int[,]> {
            DotNumber.Zero, 
            DotNumber.One,
            DotNumber.Two,
            DotNumber.Three,
            DotNumber.Four,
            DotNumber.Five,
            DotNumber.Six,
            DotNumber.Seven,
            DotNumber.Eight,
            DotNumber.Nine
        };

        /// <summary>
        /// 当前数
        /// </summary>
        internal int Number { get; set; }  // 暂时只支持整数

        /// <summary>
        /// 每一位数字
        /// </summary>
        internal int[] Numbers { get; set; }

        /// <summary>
        /// 整数位数
        /// </summary>
        internal int DigitsCount { get; set; }

        internal float UnitWidth { get; set; }

        internal float UnitHeight { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="n"></param>
        /// <param name="unitWidth"></param>
        /// <param name="unitHeight"></param>
        public DotNumber(int n, float unitWidth, float unitHeight)
        {
            this.Number = n;
            this.UnitWidth = unitWidth; // 每个数字的宽度
            this.UnitHeight = unitHeight; // 每个数字的高度

            int number = Math.Abs(this.Number);
            this.DigitsCount = number.ToString().Length;


            int[] digits = new int[this.DigitsCount];

            for (int i = this.DigitsCount - 1; i >= 0; i--)
            {
                digits[i] = number % 10;
                number /= 10;
            }

            this.Numbers = digits;
        }

        /// <summary>
        /// 单一数字可视化
        /// </summary>
        /// <param name="n"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private ZXPointSet Visualize(int n, float unit = 0.1f)
        {
            ZXPointSet ps = new ZXPointSet();
            if (n >= 10) return ps;

            int[,] numberDot = DotNumber.NumberDot[n];

            float deltaX = this.UnitWidth / 3.0f;  // 每列宽度
            float deltaY = this.UnitHeight / 5.0f; // 每行宽度
            float startX = -this.UnitWidth * 0.5f;
            float endX = this.UnitWidth * 0.5f;
            float startY = -this.UnitHeight * 0.5f;
            float endY = this.UnitHeight * 0.5f;

            // STEP 1: 先铺满
            for (float iX = startX; iX <= endX; iX += unit)
            {
                for (float jY= startY; jY <= endY; jY += unit)
                {
                    ZXPoint p = new ZXPoint(iX, jY, 0);
                    // 行数 0, 1, 2, 3, 4 
                    p.Alfa = (int)Math.Floor((endY - jY) / deltaY);
                    // 列数 0, 1, 2
                    p.Beta = (int)Math.Floor((iX - startX) / deltaX);

                    // LibTool.Debug("列数：" + p.Beta);

                    if (p.Alfa < 0) p.Alfa = 0;
                    if (p.Alfa > 4) p.Alfa = 4;
                    if (p.Beta < 0) p.Beta = 0;
                    if (p.Beta > 2) p.Beta = 2;

                    // p.Alfa = Math.Clamp(p.Alfa, 0, 4);
                    // p.Beta = Math.Clamp(p.Beta, 0, 2);
                    // LibTool.Debug("行数: " + p.Alfa + " 列数: " + p.Beta);

                    ps.Add(p);
                }
            }

            // STEP 2: 再根据矩阵模板选点
            ZXPointSet psNumber = new ZXPointSet();
            for (int i = 0; i < ps.Count; i++)
            {
                int indexY = (int)ps[i].Alfa; // 行数
                int indexX = (int)ps[i].Beta; // 列数
                // LibTool.Debug("行数：" + indexY);
                
                if (numberDot[indexY, indexX] == 1)
                {
                    psNumber.Add(ps[i]);
                }
            }

            return psNumber;
        }

        /// <summary>
        /// 一串整数可视化
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="spanX"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public ZXPointSet Visualize(ZXPoint pos, float spanX = 0.2f, float unit = 0.1f) {
            
            ZXPointSet ps = new ZXPointSet();

            for (int i = 0; i < this.DigitsCount; i++) {

                ZXPointSet psUnit = this.Visualize(this.Numbers[i], unit);
                ps.Merge(psUnit);

                if (i != this.DigitsCount - 1)
                {
                    ps.Translate(-this.UnitWidth - spanX, 0, 0);
                }
            }
            ZXBoundary bNumber = ps.Boundary;
            ps.Translate(-bNumber.OX, -bNumber.OY, -bNumber.OZ);
            ps.Translate(pos.X, pos.Y, pos.Z);
            return ps;
        }
    }
}
