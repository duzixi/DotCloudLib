//=====================================================================
// 模块名称：线段 ZXSegment
// 功能简介：三维空间中的线段
// 版权声明：2023~2025 锐创理工科技有限公司  All Rights Reserved.
//           
// 更新履历：2023.11     杨波    创建 
//          2025.12.11  杜子兮  判定两条线段是否贡献
//============================================

using System;

namespace DotCloudLib
{
    /// <summary>
    /// 线段
    /// </summary>
    public class ZXSegment
    {
        /// <summary>
        /// 构造方法（三维线段）
        /// </summary>
        /// <param name="_startX"></param>
        /// <param name="_endX"></param>
        /// <param name="_startY"></param>
        /// <param name="_endY"></param>
        /// <param name="_startZ"></param>
        /// <param name="_endZ"></param>
        public ZXSegment(float _startX, float _endX, float _startY, float _endY, float _startZ, float _endZ)
        {
            l_startX = _startX;
            l_endX = _endX;
            l_startY = _startY;
            l_endY = _endY;
            l_startZ = _startZ;
            l_endZ = _endZ;

            l_line = new ZXPoint[2];
        }

        /// <summary>
        /// 构造方法（两点）
        /// </summary>
        /// <param name="_start"></param>
        /// <param name="_end"></param>
        public ZXSegment(ZXPoint _start, ZXPoint _end)
        {
            l_startX = _start.X;
            l_endX = _end.X;
            l_startY = _start.Y;
            l_endY = _end.Y;
            l_startZ = _start.Z;
            l_endZ = _end.Z;
        }

        /// <summary>
        /// 构造方法（二维线段）
        /// </summary>
        /// <param name="_startX"></param>
        /// <param name="_endX"></param>
        /// <param name="_startY"></param>
        /// <param name="_endY"></param>
        public ZXSegment(float _startX, float _endX, float _startY, float _endY)
        {
            l_startX = _startX;
            l_endX = _endX;
            l_startY = _startY;
            l_endY = _endY;
            l_startZ = 0;
            l_endZ = 0;

            l_line = new ZXPoint[2];
        }


        #region 属性

        private float l_startX;
        private float l_startY;
        private float l_startZ;
        private float l_endX;
        private float l_endY;
        private float l_endZ;
        /// <summary>
        /// 线段ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 线段长度
        /// </summary>
        public float Length{
            get
            {
                return (float)Math.Sqrt((l_endX - l_startX) * (l_endX - l_startX) + (l_endY - l_startY) * (l_endY - l_startY) + (l_endZ - l_startZ) * (l_endZ - l_startZ)); 
            }
        }
        private ZXPoint[] l_line;
        /// <summary>
        /// 线段坐标
        /// </summary>
        public ZXPoint[] L
        {
            get
            {
                if (l_line == null || l_line.Length < 2)
                {
                    l_line = new ZXPoint[2];
                }

                l_line[0] = new ZXPoint(l_startX, l_startY, l_startZ);
                l_line[1] = new ZXPoint(l_endX, l_endY, l_endZ);
               

                return l_line;
            }
        }

        /// <summary>
        /// 开始点
        /// </summary>
        public ZXPoint Start
        {
            get
            {
                return new ZXPoint(this.l_startX, this.l_startY, this.l_startZ);
            }

            set
            {
                this.l_startX = value.X;
                this.l_startY = value.Y;
                this.l_startZ = value.Z;
            }
        }

        /// <summary>
        /// 结束点
        /// </summary>
        public ZXPoint End
        {
            get
            {
                return new ZXPoint(this.l_endX, this.l_endY, this.l_endZ);
            }

            set
            {
                this.l_endX = value.X;
                this.l_endY = value.Y;
                this.l_endZ = value.Z;
            }

        }
        #endregion


        #region 基础判断

        /// <summary>
        /// 是否为空 
        /// </summary>
        /// <param name="p_e"></param>
        /// <returns></returns>
        public bool IsNull(float p_e = 0.001f)
        {
            return this.Length < p_e;
        }

        /// <summary>
        /// 判断两线段相交
        /// </summary>
        /// <param name="_zX">线段</param>
        /// <param name="_p">交点</param>
        /// <returns></returns>
        public bool IntersectXY(ZXSegment _zX, out ZXPoint _p)
        {
            _p = null;

            double px1 = Start.X;
            double py1 = Start.Y;
            double px2 = End.X;
            double py2 = End.Y;
            double px3 = _zX.Start.X;
            double py3 = _zX.Start.Y;
            double px4 = _zX.End.X;
            double py4 = _zX.End.Y;

            bool flag = false;
            double d = (px2 - px1) * (py4 - py3) - (py2 - py1) * (px4 - px3);

            if (Math.Abs(d) > 1e-6f) // 2025.09.04 改
            {
                double r = ((py1 - py3) * (px4 - px3) - (px1 - px3) * (py4 - py3)) / d;
                double s = ((py1 - py3) * (px2 - px1) - (px1 - px3) * (py2 - py1)) / d;
                if ((r >= 0) && (r <= 1) && (s >= 0) && (s <= 1))
                {
                    // 2025.09.04 +

                    // 计算交点坐标（使用第一条线段的参数方程）
                    double x = px1 + r * (px2 - px1);
                    double y = py1 + r * (py2 - py1);

                    // 对于Z坐标，可以取两线段对应点的平均值或根据实际需求处理
                    double z = (Start.Z + r * (End.Z - Start.Z) +
                               _zX.Start.Z + s * (_zX.End.Z - _zX.Start.Z)) / 2;
                    _p = new ZXPoint(x, y, z);
                    flag = true;
                }
            }
            return flag;
        }



        #endregion

        #region 调试用

        /// <summary>
        /// 字符串表示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[ l_startX: " + l_startX.ToString("0.00") + "  l_startY: " + l_startY.ToString("0.00") + "  l_startZ: " + l_startZ.ToString("0.00") 
                + "  -  l_endX: " + l_endX.ToString("0.00")
                + "  l_endY: " + l_endY.ToString("0.00")
                + "  l_endZ: " + l_endZ.ToString("0.00") + " ]";
        }
        /// <summary>
        /// 输出json字符串
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return "{ " +
                " \"length\": " + Length.ToString("0.000") + ", " +
                " \"l_startX\": " + l_startX.ToString("0.000") + ", " +
                " \"l_endX\": " + l_endX.ToString("0.000") + ", " +
                " \"l_startY\": " + l_startY.ToString("0.000") + ", " +
                " \"l_endY\": " + l_endY.ToString("0.000") + ", " +
                " \"l_startZ\": " + l_startZ.ToString("0.000") + ", " +
                " \"l_endZ\": " + l_endZ.ToString("0.000") + "} ";
        }

        /// <summary>
        /// 【调试用】可视化为点集
        /// </summary>
        /// <returns></returns>
        public ZXPointSet Visualize(float p_unit = 0.1f)
        {
            ZXPointSet ps = DotInsert.InsertLine(this.Start, this.End, p_unit);
            return ps;
        }

        #endregion
    }
}
