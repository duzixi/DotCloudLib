//=====================================================================
// 模块名称：点 ZXPoint
// 功能简介：空间点
// 版权声明：2019 九州创智科技有限公司  Reserved.
//           2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2019.11  杜子兮    移自“知行”C++库  
//          2023.04  Sanngoku  添加下标索引访问X,Y,Z方法
//          2023.05  王振宇    继承INode接口，判定三点是否在一条直线上 
//============================================



using System;
using Mathd;
using DotCloudLib.GraphTheory;

namespace DotCloudLib
{
    /// <summary>
    /// 点坐标
    /// </summary>
    public class ZXPoint : IComparable<ZXPoint>, INode
    {
        #region 属性

        /// <summary>
        /// 点坐标的Id
        /// </summary>
        // public ulong Id { get; set; }

        public string Id { get; set; }


        private float m_x;
        /// <summary>
        /// X坐标
        /// </summary>
        public float X
        {
            get
            {
                return m_x;
            }

            set
            {
                m_x = value;
            }
        }

        private float m_y;
        /// <summary>
        /// Y坐标
        /// </summary>
        public float Y
        {
            get
            {
                return m_y;
            }

            set
            {
                m_y = value;
            }
        }

        private float m_z;
        /// <summary>
        /// Z坐标
        /// </summary>
        public float Z
        {
            get
            {
                return m_z;
            }

            set
            {
                m_z = value;
            }
        }

        /// <summary>
        /// 【备用】alfa值
        /// </summary>
        public float Alfa { get; set; }

        /// <summary>
        /// 【备用】beta值
        /// </summary>
        public float Beta { get; set; }

        /// <summary>
        /// 【备用】gama值
        /// </summary>
        public float Gama { get; set; }

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造原点
        /// </summary>
        public ZXPoint()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
            this.Alfa = 0;
            this.Beta = 0;
            this.Gama = 0;
        }

        /// <summary>
        /// 构造二维点
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        public ZXPoint(float _x, float _y)
        {
            X = _x;
            Y = _y;
            Z = 0;

            this.Alfa = 0;
            this.Beta = 0;
            this.Gama = 0;
        }

        /// <summary>
        /// 构造三维点
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        public ZXPoint(float _x, float _y, float _z)
        {
            X = _x;
            Y = _y;
            Z = _z;

            this.Alfa = 0;
            this.Beta = 0;
            this.Gama = 0;
        }

        /// <summary>
        /// 构造三维点  2024.05.11 +
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        public ZXPoint(double _x, double _y, double _z)
        {
            X = (float)_x;
            Y = (float)_y;
            Z = (float)_z;

            this.Alfa = 0;
            this.Beta = 0;
            this.Gama = 0;
        }

        /// <summary>
        /// 构造有空间角度的三维点
        /// </summary>
        /// <param name="_x">X坐标</param>
        /// <param name="_y">Y坐标</param>
        /// <param name="_z">Z坐标</param>
        /// <param name="_alfa">alfa值</param>
        /// <param name="_beta">beta值</param>
        /// <param name="_gama">gama值</param>
        public ZXPoint(float _x, float _y, float _z, float _alfa, float _beta, float _gama)
        {
            this.X = _x;
            this.Y = _y;
            this.Z = _z;
            this.Alfa = _alfa;
            this.Beta = _beta;
            this.Gama = _gama;
        }

        /// <summary>
        /// 有ID点
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        public ZXPoint(string _id, double _x, double _y, double _z)
        {
            this.Id = _id;
            X = (float)_x;
            Y = (float)_y;
            Z = (float)_z;

            this.Alfa = 0;
            this.Beta = 0;
            this.Gama = 0;
        }

        /// <summary>
        /// 构造方法 —— 深拷贝
        /// </summary>
        /// <param name="_p"></param>
        public ZXPoint(ZXPoint _p)
        {
            this.X = _p.X;
            this.Y = _p.Y;
            this.Z = _p.Z;

            this.Alfa = 0;
            this.Beta = 0;
            this.Gama = 0;
        }

        #endregion

        #region 高级特征

        /// <summary>
        /// 判别是否为空点 2025.6.30 改 （float.MinValue赋值时，因float小数点后有效位原因，导致空点比预想的大）
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty(float p_minZ = float.MinValue * 0.9f)
        {
            return this.Z < p_minZ;
        }

        /// <summary>
        /// 根据X,Y坐标，计算Alfa角  → 0° ↑ 90° ← 180° ↓ 270°
        /// </summary>
        /// <returns></returns>
        public float ComputeAlfa()
        {
            float x = this.X;
            float y = this.Y;
            float angle = Vector3d.Angle(Vector3d.right, new Vector3d(x, y, 0));
            Vector3d cross = Vector3d.Cross(Vector3d.right, new Vector3d(x, y, 0)).normalized;
            angle = (angle * (float)cross.z + 360) % 360;
            this.Alfa = angle;

            if (x < 0 && Math.Abs(y) < 0.0001)
            {
                this.Alfa = 180;
            }

            return this.Alfa;
        }

        /// <summary>
        /// 默认比较函数
        /// </summary>
        /// <param name="other">另一个点</param>
        /// <returns>大于 1； 等于 0； 小于 -1；</returns>
        public int CompareTo(ZXPoint other)
        {
            if (this.X.CompareTo(other.X) == 0 &&
                 this.Y.CompareTo(other.Y) == 0 &&
                 this.Z.CompareTo(other.Z) == 0
                )
            {
                return 0;
            }
            else if (
              this.X.CompareTo(other.X) == 0 &&
              this.Y.CompareTo(other.Y) == 0)
            {
                return this.Z.CompareTo(other.Z);
            }
            else if (
              this.X.CompareTo(other.X) == 0)
            {
                return this.Y.CompareTo(other.Y);
            }
            else
            {
                return this.X.CompareTo(other.X);
            }
        }

        /// <summary>
        /// 比较函数（ZXY顺序排列）
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareToByZXY(ZXPoint other)
        {
            if (this.X.CompareTo(other.X) == 0 &&
                this.Y.CompareTo(other.Y) == 0 &&
                this.Z.CompareTo(other.Z) == 0
            )
            {
                return 0;
            }
            else if (
              this.Z.CompareTo(other.Z) == 0 &&
              this.X.CompareTo(other.X) == 0)
            {
                return this.Y.CompareTo(other.Y);
            }
            else if (
              this.Z.CompareTo(other.Z) == 0)
            {
                return this.Y.CompareTo(other.Y);
            }
            else
            {
                return this.Z.CompareTo(other.Z);
            }
        }

        /// <summary>
        /// 比较两个点的 Alfa角 2022.8.5
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareToByAlfa(ZXPoint other)
        {
            return this.Alfa.CompareTo(other.Alfa);
        }

        /// <summary>
        /// 求两点距离
        /// </summary>
        /// <param name="_p">另一个点</param>
        /// <returns>两点距离</returns>
        public float DistanceTo(ZXPoint _p)
        {
            return (float)Math.Sqrt(this.SquareDistanceTo(_p));
        }

        /// <summary>
        /// 求两点距离的平方
        /// </summary>
        /// <param name="_p"></param>
        /// <returns></returns>
        public float SquareDistanceTo(ZXPoint _p)
        {
            return (X - _p.X) * (X - _p.X) + (Y - _p.Y) * (Y - _p.Y) + (Z - _p.Z) * (Z - _p.Z);
        }

        /// <summary>
        /// 点自身距离 2025.10.10 duzixi +
        /// </summary>
        /// <returns></returns>
        public float Distance()
        {
            return this.DistanceTo(new ZXPoint(0,0,0));
        }

        #endregion

        #region 操作符重载

        /// <summary>
        /// 索引访问
        /// </summary>
        /// <param name="index">0:X, 1:Y, 2:Z </param>
        /// <returns>点坐标分项值</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float this[int index]
        {
            get
            {
                if (index < 0 || index > 2)
                {
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("index");
                    LibTool.Error(ex, "ZXPoint@295", "ZXPoint object only have 3 dimention. index = " + index);
                }

                if (index == 0)
                {
                    return this.m_x;
                }
                else if (index == 1)
                {
                    return this.m_y;
                }
                else
                {
                    return this.m_z;
                }
            }
            set
            {
                if (index < 0 || index > 2)
                {
                    ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("index");
                    LibTool.Error(ex, "ZXPoint@295", "ZXPoint object only have 3 dimention. index = " + index);
                }

                if (index == 0)
                {
                    this.m_x = value;
                }
                else if (index == 1)
                {
                    this.m_y = value;
                }
                else
                {
                    this.m_z = value;
                }
            }
        }

        /// <summary>
        /// 两点加法重载
        /// </summary>
        /// <param name="a">第1个点</param>
        /// <param name="b">第2个点</param>
        /// <returns>两点坐标之和</returns>
        public static ZXPoint operator +(ZXPoint a, ZXPoint b)
        {
            return new ZXPoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// 按原点对称
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static ZXPoint operator -(ZXPoint a)
        {
            return new ZXPoint(-a.X, -a.X, -a.X);
        }

        /// <summary>
        /// 两点减法重载
        /// </summary>
        /// <param name="a">第一个点</param>
        /// <param name="b">第二个点</param>
        /// <returns>第一个点分别减去第二个点</returns>
        public static ZXPoint operator -(ZXPoint a, ZXPoint b)
        {
            return new ZXPoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// 按原点增倍
        /// </summary>
        /// <param name="d"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static ZXPoint operator *(float d, ZXPoint a)
        {
            return new ZXPoint(a.X * d, a.Y * d, a.Z * d);
        }
        /// <summary>
        /// 按原点增倍
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static ZXPoint operator *(ZXPoint a, float d)
        {
            return new ZXPoint(a.X * d, a.Y * d, a.Z * d);
        }
        /// <summary>
        /// 按原点缩短
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static ZXPoint operator /(ZXPoint a, float d)
        {
            return new ZXPoint(a.X / d, a.Y / d, a.Z / d);
        }

        /// <summary>
        /// 比较相等 edit by 刘轩名 2025.09
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(ZXPoint lhs, ZXPoint rhs)
        {
            Object p0 = lhs;
            Object p1 = rhs;

            if (p0 == null && p1 == null)
            {
                return true;
            }
            else if (p0 == null || p1 == null)
            {
                return false;
            }

            return Precisionf.Equals(lhs.X, rhs.X) && Precisionf.Equals(lhs.Y, rhs.Y) && Precisionf.Equals(lhs.Z, rhs.Z);
        }

        /// <summary>
        /// 判断不相等
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(ZXPoint lhs, ZXPoint rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// 比较相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this == (obj as ZXPoint);
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        /// <summary>
        /// 输出json字符串
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return "{ " +
                " \"X\": " + X.ToString("0.000") + ", " +
                " \"Y\": " + Y.ToString("0.000") + ", " +
                " \"Z\": " + Z.ToString("0.000") + "} ";
        }

        /// <summary>
        /// 显示点坐标
        /// </summary>
        /// <returns>点坐标字符串</returns>
        public override string ToString()
        {

            return X.ToString("0.00") + ", " + Y.ToString("0.00") + ", " + Z.ToString("0.00");

            // return "[" + Id + "](" + X.ToString("0.00") + ", " + Y.ToString("0.00") + ", " + Z.ToString("0.00") + ")" +
            //    "[" + Alfa.ToString("0.00") + "]" + "[" + Beta.ToString("0.00") + "]";
        }

        /// <summary>
        /// 按格式显示坐标
        /// </summary>
        /// <param name="p_format"></param>
        /// <returns></returns>
        public string ToString(string p_format)
        {
            return "(" + X.ToString(p_format) + ", " + Y.ToString(p_format) + ", " + Z.ToString(p_format) + ")";
        }

        /// <summary>
        /// 判断两个点是否相邻
        /// </summary>
        /// <param name="p_node">另一个点</param>
        /// <param name="p_e">相邻距离</param>
        /// <returns></returns>
        public bool NearTo(INode p_node, float p_e = 0.001F)
        {
            return this.DistanceTo((ZXPoint)p_node) < p_e;
        }

        /// <summary>
        /// 判断三点是否在一条直线上
        /// </summary>
        /// <param name="p_preNode"></param>
        /// <param name="p_nextNode"></param>
        /// <param name="p_e"></param>
        /// <returns></returns>
        public bool IsFlex(INode p_preNode, INode p_nextNode, float p_e = 0.001F)
        {
            ZXPoint preNode = (ZXPoint)p_preNode;
            ZXPoint nextNode = (ZXPoint)p_nextNode;

            Vector3d a = new Vector3d(preNode.X, preNode.Y, preNode.Z);
            Vector3d b = new Vector3d(this.X, this.Y, this.Z);
            Vector3d c = new Vector3d(nextNode.X, nextNode.Y, nextNode.Z);
            Vector3d v1 = (a - b).normalized;
            Vector3d v2 = (a - c).normalized;
            return v1 != v2 && v1 != -v2;
        }

        /// <summary>
        /// 计算两点之间的线性插值 2025.07.17+
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public ZXPoint Lerp(ZXPoint p1, double t)
        {
            return new ZXPoint(
                this.X + (p1.X - this.X) * t,
                this.Y + (p1.Y - this.Y) * t,
                this.Z + (p1.Z - this.Z) * t
            );
        }
    }
}
