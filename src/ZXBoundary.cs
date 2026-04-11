//=====================================================================
// 模块名称：边界 ZXBoundary
// 功能简介：AABB包围盒相关计算
// 版权声明：2019 九州创智科技有限公司  All Rights Reserved.
//          2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2019.11 杜子兮 移自“知行”C++库
//          2020.11 杜子兮 合并iWMS空间逻辑计算代码
//===========================================================


using DotCloudLib.Search;
using System;
using System.Collections.Generic;

using DotCloudLib.GraphTheory;
using System.Linq;

namespace DotCloudLib
{
    /// <summary>
    /// 边界——AABB包围盒
    /// </summary>
    public struct ZXBoundary : INode
    {
        /// <summary>
        /// 构造方法（三维包围盒）
        /// </summary>
        /// <param name="_minX"></param>
        /// <param name="_maxX"></param>
        /// <param name="_minY"></param>
        /// <param name="_maxY"></param>
        /// <param name="_minZ"></param>
        /// <param name="_maxZ"></param>
        public ZXBoundary(float _minX, float _maxX, float _minY, float _maxY, float _minZ, float _maxZ)
        {
            m_id = "";
            m_minX = _minX;
            m_maxX = _maxX;
            m_minY = _minY;
            m_maxY = _maxY;
            m_minZ = _minZ;
            m_maxZ = _maxZ;

            float temp = 0;
            if (m_maxX < m_minX)
            {
                temp = m_maxX;
                m_maxX = m_minX;
                m_minX = temp;
            }

            if (m_maxY < m_minY)
            {
                temp = m_maxY;
                m_maxY = m_minY;
                m_minY = temp;
            }

            if (m_maxZ < m_minZ)
            {
                temp = m_maxZ;
                m_maxZ = m_minZ;
                m_minZ = temp;
            }

            m_oX = (m_minX + m_maxX) * 0.5f;
            m_oY = (m_minY + m_maxY) * 0.5f;
            m_oZ = (m_minZ + m_maxZ) * 0.5f;

            m_vertices = new ZXPoint[8];
        }

        /// <summary>
        /// 构造方法（二维包围盒）
        /// </summary>
        /// <param name="_minX"></param>
        /// <param name="_maxX"></param>
        /// <param name="_minY"></param>
        /// <param name="_maxY"></param>
        public ZXBoundary(float _minX, float _maxX, float _minY, float _maxY)
        {
            m_id = "";
            m_minX = _minX;
            m_maxX = _maxX;
            m_minY = _minY;
            m_maxY = _maxY;
            m_minZ = 0;
            m_maxZ = 0;

            float temp = 0;
            if (m_maxX < m_minX)
            {
                temp = m_maxX;
                m_maxX = m_minX;
                m_minX = temp;
            }

            if (m_maxY < m_minY)
            {
                temp = m_maxY;
                m_maxY = m_minY;
                m_minY = temp;
            }

            m_oX = (m_minX + m_maxX) * 0.5f;
            m_oY = (m_minY + m_maxY) * 0.5f;
            m_oZ = 0;

            m_vertices = new ZXPoint[8];
        }

        /// <summary>
        /// 构造函数——有编号的二维包围盒
        /// </summary>
        /// <param name="_id">编号</param>
        /// <param name="_minX">最小X</param>
        /// <param name="_maxX">最大X</param>
        /// <param name="_minY">最小Y</param>
        /// <param name="_maxY">最大Y</param>
        public ZXBoundary(string _id, float _minX, float _maxX, float _minY, float _maxY)
        {
            this = new ZXBoundary(_minX, _maxX, _minY, _maxY);

            this.Id = _id;
            
        }


        #region 基本属性

        private string m_id;

        /// <summary>
        /// 唯一编码
        /// </summary>
        public string Id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        private float m_minX;
        private float m_maxX;
        private float m_minY;
        private float m_maxY;
        private float m_minZ;
        private float m_maxZ;
        private float m_oX;
        private float m_oY;
        private float m_oZ;

        /// <summary>
        /// 最小X
        /// </summary>
        public float MinX
        {
            get
            {
                return m_minX;
            }

            set
            {
                m_minX = value;
            }
        }

        /// <summary>
        /// 最大X
        /// </summary>
        public float MaxX
        {
            get
            {
                return m_maxX;
            }

            set
            {
                m_maxX = value;
            }
        }

        /// <summary>
        /// 最小Y
        /// </summary>
        public float MinY
        {
            get
            {
                return m_minY;
            }

            set
            {
                m_minY = value;
            }
        }

        /// <summary>
        /// 最大Y
        /// </summary>
        public float MaxY
        {
            get
            {
                return m_maxY;
            }

            set
            {
                m_maxY = value;
            }
        }

        /// <summary>
        /// 最小Z
        /// </summary>
        public float MinZ
        {
            get
            {
                return m_minZ;
            }

            set
            {
                m_minZ = value;
            }
        }

        /// <summary>
        /// 最大Z
        /// </summary>
        public float MaxZ
        {
            get
            {
                return m_maxZ;
            }

            set
            {
                m_maxZ = value;
            }
        }

        /// <summary>
        /// 中心点X
        /// </summary>
        public float OX
        {
            get
            {
                return (MinX + MaxX) * 0.5f;
            }
        }

        /// <summary>
        /// 中心点Y
        /// </summary>
        public float OY
        {
            get
            {
                return (MinY + MaxY) * 0.5f;
            }
        }

        /// <summary>
        /// 中心点Z
        /// </summary>
        public float OZ
        {
            get
            {
                return (MinZ + MaxZ) * 0.5f;
            }
        }

        /// <summary>
        /// X方向长
        /// </summary>
        public float L
        {
            get
            {
                return (MaxX - MinX);
            }
        }

        /// <summary>
        /// Y方向宽
        /// </summary>
        public float W
        {
            get
            {
                return (MaxY - MinY);
            }
        }

        /// <summary>
        /// Z方向高
        /// </summary>
        public float H
        {
            get
            {
                return (MaxZ - MinZ);
            }
        }

        /// <summary>
        /// 平面面积
        /// </summary>
        public float Area
        {
            get
            {
                return this.L * this.W;
            }
        }

        #endregion


        #region 高级属性

        /// <summary>
        /// 返回中间点
        /// </summary>
        public ZXPoint Center
        {
            get
            {
                return new ZXPoint(this.OX, this.OY, this.OZ);
            }
        }

        /*
            p3   p2         p7   p6
            1               2
            p0   p1         p4   p5
	    */
        private ZXPoint[] m_vertices;
        // private string id = "";

        /// <summary>
        /// 八个角点
        /// ^ Y轴
        /// |
        /// 3------2  7------6
        /// |      |  |      |
        /// |      |  |      |
        /// 0------1  4------5
        ///   底部       顶部    -> X轴
        /// </summary>
        public ZXPoint[] V 
        {
            get
            {
                if (m_vertices == null || m_vertices.Length < 8)
                {
                    m_vertices = new ZXPoint[8];
                }

                m_vertices[0] = new ZXPoint(MinX, MinY, MinZ);
                m_vertices[1] = new ZXPoint(MaxX, MinY, MinZ);
                m_vertices[2] = new ZXPoint(MaxX, MaxY, MinZ);
                m_vertices[3] = new ZXPoint(MinX, MaxY, MinZ);

                m_vertices[4] = new ZXPoint(MinX, MinY, MaxZ);
                m_vertices[5] = new ZXPoint(MaxX, MinY, MaxZ);
                m_vertices[6] = new ZXPoint(MaxX, MaxY, MaxZ);
                m_vertices[7] = new ZXPoint(MinX, MaxY, MaxZ);

                return m_vertices;
            }
        }

        /// <summary>
        /// 最小点（底部左下角坐标）
        /// </summary>
        public ZXPoint Min
        {
            get
            {
                return new ZXPoint(this.MinX, this.MinY, this.MinZ);
            }
        }

        /// <summary>
        /// 最大点（顶部右上角坐标）
        /// </summary>
        public ZXPoint Max
        {
            get
            {
                return new ZXPoint(this.MaxX, this.MaxY, this.MaxZ);
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
            return this.L < p_e;
        }

        /// <summary>
        /// 是否为标准
        /// </summary>
        /// <returns></returns>
        public bool IsStandard()
        {
            return 
                m_minX < m_maxX &&
                m_minY < m_maxY &&
                m_minZ < m_maxZ;
        }

        /// <summary>
        /// 标准化
        /// </summary>
        public void Normalize()
        {
            float temp = 0;
            if (m_maxX < m_minX)
            {
                temp = m_maxX;
                m_maxX = m_minX;
                m_minX = temp;
            }

            if (m_maxY < m_minY)
            {
                temp = m_maxY;
                m_maxY = m_minY;
                m_minY = temp;
            }

            if (m_maxZ < m_minZ)
            {
                temp = m_maxZ;
                m_maxZ = m_minZ;
                m_minZ = temp;
            }
        }

        /// <summary>
        /// X坐标是否在边界内
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool ContainX(float _x, bool _open = false)
        {
            if (_open)
            {
                return _x > m_minX && _x < m_maxX;
            }
            return _x >= m_minX && _x <= m_maxX;
        }
        
        /// <summary>
        /// 判定包围盒X方向是否包含
        /// </summary>
        /// <param name="_b"></param>
        /// <param name="_open"></param>
        /// <returns></returns>
        public bool ContainX(ZXBoundary _b, bool _open = false)
        {
            return ContainX(_b.MinX, _open) &&
                   ContainX(_b.MaxX, _open);
        }

        /// <summary>
        /// Y坐标是否在边界内
        /// </summary>
        /// <param name="_y"></param>、
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool ContainY(float _y, bool _open = false)
        {
            if (_open)
            {
                return _y > m_minY && _y < m_maxY;
            }
            return _y >= m_minY && _y <= m_maxY;
        }

        /// <summary>
        /// 判定包围盒Y方向是否包含
        /// </summary>
        /// <param name="_b"></param>
        /// <param name="_open"></param>
        /// <returns></returns>
        public bool ContainY(ZXBoundary _b, bool _open = false)
        {
            return ContainY(_b.MinY, _open) &&
                   ContainY(_b.MaxY, _open);
        }

        /// <summary>
        /// Z坐标是否在边界内
        /// </summary>
        /// <param name="_z"></param>
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool ContainZ(float _z, bool _open = false)
        {
            if (_open)
            {
                return _z > m_minZ && _z < m_maxZ;
            }
            return _z >= m_minZ && _z <= m_maxZ;
        }

        /// <summary>
        /// 判定包围盒Z方向是否包含
        /// </summary>
        /// <param name="_b"></param>
        /// <param name="_open"></param>
        /// <returns></returns>
        public bool ContainZ(ZXBoundary _b, bool _open = false)
        {
            return ContainZ(_b.MinZ, _open) &&
                   ContainZ(_b.MaxZ, _open);
        }

        /// <summary>
        /// 点坐标是否在边间内（在边界上，视为在边界内）
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool Contain(float _x, float _y, float _z, bool _open = false)
        {
            return ContainX(_x, _open) &&
                   ContainY(_y, _open) &&
                   ContainZ(_z, _open);
        }

        /// <summary>
        /// 点坐标是否在边间内（在边界上，视为在边界内）
        /// </summary>
        /// <param name="_p"></param>
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool Contain(ZXPoint _p, bool _open = false)
        {
            return Contain(_p.X, _p.Y, _p.Z, _open);
        }

        /// <summary>
        /// 只考虑二维俯视图包含点坐标
        /// </summary>
        /// <param name="_p"></param>
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool ContainXY(ZXPoint _p, bool _open = false)
        {
            return ContainX(_p.X, _open) && ContainY(_p.Y, _open);
        }

        /// <summary>
        /// 判定二维包围盒包含
        /// </summary>
        /// <param name="_b"></param>
        /// <param name="_open"></param>
        /// <returns></returns>
        public bool ContainXY(ZXBoundary _b, bool _open = false)
        {
            return ContainXY(_b.V[0], _open) &&
                   ContainXY(_b.V[1], _open) && 
                   ContainXY(_b.V[2], _open) && 
                   ContainXY(_b.V[3], _open);

        }

        /// <summary>
        /// 只考虑二维俯视图包含二维线段    by: 杨波
        /// </summary>
        /// <param name="_l"></param>
        /// <param name="_open">开区间</param>
        /// <returns></returns>
        public bool ContainXY(ZXSegment _l, bool _open = false)
        {

            //return ContainX(_l.Start.X, _open) &&
            //        ContainY(_l.Start.Y, _open) &&
            //        ContainX(_l.End.X, _open) &&
            //        ContainY(_l.End.Y, _open);
            return ContainXY(_l.Start, _open) &&
                    ContainXY(_l.End, _open) ;
        }

        /// <summary>
        /// 判断两个包围盒是否有交集(只贴边不算有交集)
        /// </summary>
        /// <param name="_b"></param>
        /// <returns></returns>
        public bool Intersect(ZXBoundary _b)
        {
            return this.MaxX > _b.MinX && this.MinX < _b.MaxX
                 && this.MaxY > _b.MinY && this.MinY < _b.MaxY
                 && this.MaxZ > _b.MinZ && this.MinZ < _b.MaxZ;
        }

        /// <summary>
        /// 判断两个包围盒在XOY平面上是否有交集
        /// </summary>
        /// <param name="_b"></param>
        /// <returns></returns>
        public bool IntersectXY(ZXBoundary _b)
        {
            return this.MaxX > _b.MinX && this.MinX < _b.MaxX
                 && this.MaxY > _b.MinY && this.MinY < _b.MaxY;
        }

        /// <summary>
        /// 判断包围盒和二维线段在XOY平面上是否有交集  by: 杨波
        /// </summary>
        /// <param name="_l"></param>
        /// <returns></returns>
        public bool IntersectXY(ZXSegment _l)
        {
            ZXPoint p = null;

            // STEP 1: 包围盒4个角点4条边初始化
            for (int i = 0; i < 4; i ++)
            {
                ZXSegment zx = new ZXSegment(V[i].X, V[(i + 1) % 4].X, V[i].Y, V[(i + 1) % 4].Y);

                // STEP 2: 遍历四条边，判断是否相交
                if (zx.IntersectXY(_l, out p)) return true;
            }

            return false;
        }
        #endregion

        #region 空间逻辑计算

        /// <summary>
        /// 二维包围盒切割算法：被另一个包围盒切割成两个
        /// （仅适用于XOY方向的二维包围盒）
        /// 默认都是纵切
        /// </summary>
        /// <param name="_b">切割包围盒</param>
        /// <returns></returns>
        [Obsolete]
        public List<ZXBoundary> CutBy(ZXBoundary _b)
        {
            List<ZXBoundary> bounds = new List<ZXBoundary>();

            // CASE 0.1: 未被切到，返回自身
            if (!this.IntersectXY(_b))
            {
                return new List<ZXBoundary>() { this };// 返回自身，没有被切割
            }

            // CASE 0.2： 全切没，返回空数组
            if (_b.ContainXY(this, false))
            {
                return bounds;
            }

            // CASE 1: 切角
            if (ContainXY(_b.V[1], true) && _b.MinX <= this.MinX && _b.MaxY >= this.MaxY)
            {   // CASE 1.1: 左上角 b1
                bounds.Add(new ZXBoundary(this.MinX, _b.MaxX, this.MinY, _b.MinY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }
            else if (ContainXY(_b.V[2], true) && _b.MinX <= this.MinX && _b.MinY <= this.MinY)
            {   // CASE 1.2: 左下角 b2
                bounds.Add(new ZXBoundary(this.MinX, _b.MaxX, _b.MaxY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }
            else if (ContainXY(_b.V[3], true) && _b.MaxX >= this.MaxX && _b.MinY <= this.MinY)
            {   // CASE 1.3: 右下角 b3
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MinX, this.MaxX, _b.MaxY, this.MaxY));
            }
            else if (ContainXY(_b.V[0], true) && _b.MaxX >= this.MaxX && _b.MaxY >= this.MaxY)
            {   // CASE 1.4: 右上角 b4
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MinX, this.MaxX, this.MinY, _b.MinY));
            }

            // CASE 2: 切整边
            else if (_b.ContainXY(this.V[0], false) &&  _b.ContainXY(this.V[3], false) && 
                    !_b.ContainXY(this.V[1], false) && !_b.ContainXY(this.V[2], false))
            {   // CASE 2.1: 左边 b5
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }
            else if (_b.ContainXY(this.V[1], false) && _b.ContainXY(this.V[2], false) &&
                    !_b.ContainXY(this.V[0], false) && !_b.ContainXY(this.V[3], false))
            {   // CASE 2.2: 右边 b6
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
            }
            else if (_b.ContainXY(this.V[2], false) && _b.ContainXY(this.V[3], false) &&
                     !_b.ContainXY(this.V[0], false) && !_b.ContainXY(this.V[1], false))
            {   // CASE 2.3: 上边 b7
                bounds.Add(new ZXBoundary(this.MinX, this.MaxX, this.MinY, _b.MinY));
            }
            else if (_b.ContainXY(this.V[0], false) && _b.ContainXY(this.V[1], false) &&
                     !_b.ContainXY(this.V[2], false) && !_b.ContainXY(this.V[3], false))
            {   // CASE 2.4: 下边 b8
                bounds.Add(new ZXBoundary(this.MinX, this.MaxX, _b.MaxY, this.MaxY));
            }

            // CASE 3: 切中间整边（返回2个）
            else if (_b.MinX <= this.MinX && _b.MaxX >= this.MaxX && _b.MinY >= this.MinY && _b.MaxY <= this.MaxY)
            {   // CASE 3.1: 横切 b9
                bounds.Add(new ZXBoundary(this.MinX, this.MaxX, this.MinY, _b.MinY));
                bounds.Add(new ZXBoundary(this.MinX, this.MaxX, _b.MaxY, this.MaxY));
            }
            else if (_b.MinX >= this.MinX && _b.MaxX <= this.MaxX && _b.MinY <= this.MinY && _b.MaxY >= this.MaxY)
            {
                // CASE 3.2: 纵切 b10
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }

            // CASE 4: 切局部边 （返回3个）
            else if (ContainXY(_b.V[1], true) &&  ContainXY(_b.V[2], true) && 
                    !ContainXY(_b.V[0], false) && !ContainXY(_b.V[3], false)  )
            {
                // CASE 4.1: 左边 b11
                bounds.Add(new ZXBoundary(this.MinX, _b.MaxX, _b.MaxY, this.MaxY));
                bounds.Add(new ZXBoundary(this.MinX, _b.MaxX, this.MinY, _b.MinY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }
            else if (ContainXY(_b.V[3], true) && ContainXY(_b.V[2], true) &&
                    !ContainXY(_b.V[0], false) && !ContainXY(_b.V[1], false))
            {
                // CASE 4.2: 下边 b12
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MinX, _b.MaxX, _b.MaxY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }
            else if (ContainXY(_b.V[0], true) && ContainXY(_b.V[1], true) &&
                    !ContainXY(_b.V[3], false) && !ContainXY(_b.V[2], false))
            {
                // CASE 4.3: 上边 b13
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MinX, _b.MaxX, this.MinY, _b.MinY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }
            else if (ContainXY(_b.V[0], true) && ContainXY(_b.V[3], true) &&
                    !ContainXY(_b.V[1], false) && !ContainXY(_b.V[2], false))
            {
                // CASE 4.4: 右边 b14
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MinX, this.MaxX, this.MinY, _b.MinY));
                bounds.Add(new ZXBoundary(_b.MinX, this.MaxX, _b.MaxY, this.MaxY));
            }
            // CASE 5: 扣切中间 (返回4个) b15
            else if (ContainXY(_b, true))
            {
                bounds.Add(new ZXBoundary(this.MinX, _b.MinX, this.MinY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MinX, _b.MaxX, this.MinY, _b.MinY));
                bounds.Add(new ZXBoundary(_b.MinX, _b.MaxX, _b.MaxY, this.MaxY));
                bounds.Add(new ZXBoundary(_b.MaxX, this.MaxX, this.MinY, this.MaxY));
            }

            return bounds;
        }

        /// <summary>
        /// 依次被多个包围盒切割，内部迭代调用
        /// </summary>
        /// <param name="p_bounds">切割包围盒</param>
        /// <param name="p_filterY">Y方向大小小于该值时剔除</param>
        /// <returns></returns>
        [Obsolete]
        public List<ZXBoundary> CutBy(List<ZXBoundary> p_bounds, float p_filterY = 0)
        {
            List<ZXBoundary> boundsIn = new List<ZXBoundary>() { this }; // 切割前的包围盒
            List<ZXBoundary> boundsOut = new List<ZXBoundary>(); // 切割后的包围盒
            

            for (int i = 0; i < p_bounds.Count; i++)
            {
                // LibTool.Debug("i = " + i );

                List<ZXBoundary> boundsTemp = new List<ZXBoundary>();

                for (int j = 0; j < boundsIn.Count; j++)
                {
                    // LibTool.Debug("   j = " + j + " boundsIn.Count = " +boundsIn.Count);

                    boundsOut = boundsIn[j].CutBy(p_bounds[i]);

                    if (boundsOut.Count != 0)
                    {
                        boundsTemp.AddRange(boundsOut);
                    }

                }
                boundsIn = boundsTemp;
            }

            for (int i = boundsIn.Count - 1; i >= 0; i--)
            {
                if (boundsIn[i].W < p_filterY)
                {
                    boundsIn.RemoveAt(i);
                }
            }

            return boundsIn;
        }

        /// <summary>
        /// 被包围盒数组横纵切割
        /// </summary>
        /// <param name="p_bounds">包围盒数组</param>
        /// <param name="p_e">较小值</param>
        /// <returns>切割后的包围盒数组</returns>
        public List<ZXBoundary> SplitBy(List<ZXBoundary> p_bounds, float p_e = 0.001f)
        {
            List<ZXBoundary> splitedBounds = new List<ZXBoundary>() ; // 切割后的包围盒

            // STEP 1: 切割包围盒规整化：超出被切割包围盒部分剔除
            for (int i = p_bounds.Count - 1; i >= 0; i--)
            {
                ZXBoundary boundTemp = p_bounds[i];

                if (boundTemp.MinX < this.MinX)
                    boundTemp.MinX = this.MinX;

                if (boundTemp.MaxX > this.MaxX)
                    boundTemp.MaxX = this.MaxX;

                if (boundTemp.MinY < this.MinY)
                    boundTemp.MinY = this.MinY;

                if (boundTemp.MaxY > this.MaxY)
                    boundTemp.MaxY = this.MaxY;

                if (boundTemp.L <= p_e || boundTemp.W <= p_e)
                    p_bounds.Remove(p_bounds[i]);
                else
                    p_bounds[i] = boundTemp;
            }

            // STEP 2: 提取所有的X边界与Y边界
            // STEP 2.1: 初始化
            List<float> xi = new List<float>() { this.MinX, this.MaxX };
            List<float> yi = new List<float>() { this.MinY, this.MaxY };

            for (int i = 0; i < p_bounds.Count; i++)
            {
                xi.Add(p_bounds[i].MinX);
                xi.Add(p_bounds[i].MaxX);
                yi.Add(p_bounds[i].MinY);
                yi.Add(p_bounds[i].MaxY);
            }

            xi.Sort();
            yi.Sort();

            // STEP 2.2: 剔除相等边界
            for (int i = xi.Count - 1; i > 0 ; i--)
            {
                if (xi[i] - xi[i - 1] < p_e)
                    xi.RemoveAt(i);
            }

            for (int i = yi.Count - 1; i > 0; i--)
            {
                if (yi[i] - yi[i - 1] < p_e)
                    yi.RemoveAt(i);
            }

            if (xi.Count < 2 || yi.Count < 2)
            {
                LibTool.Error("ZXBoundary_L0803: 包围盒切割失败");
                return splitedBounds;
            }

            // STEP 3: 按X，Y边界生成包围盒
            for (int i = 0; i < xi.Count - 1; i++)
            {
                for (int j = 0; j < yi.Count - 1; j++)
                {
                    ZXBoundary b = new ZXBoundary(xi[i], xi[i + 1], yi[j], yi[j + 1], this.MinZ, this.MaxZ);
                    splitedBounds.Add(b);
                }
            }

            // STEP 4: 剔除切割包围盒内部的小包围盒
            for (int i = splitedBounds.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < p_bounds.Count; j++)
                {
                    if (p_bounds[j].ContainXY(splitedBounds[i], false))
                    {
                        splitedBounds.RemoveAt(i);
                        break;
                    }

                }
            }

            return splitedBounds;
        }

        /// <summary>
        /// 包围盒放大一圈
        /// </summary>
        /// <param name="_b">包围盒</param>
        /// <param name="_l">放大幅度(为正)</param>
        /// <returns>放大后包围盒</returns>
        public static ZXBoundary operator +(ZXBoundary _b, int _l)
        {
            return new ZXBoundary(
                _b.MinX - _l, _b.MaxX + _l,
                _b.MinY - _l, _b.MaxY + _l,
                _b.MinZ - _l, _b.MaxZ + _l);
        }

        /// <summary>
        /// 包围盒放大一圈
        /// </summary>
        /// <param name="_b">包围盒</param>
        /// <param name="_l">放大幅度(为正)</param>
        /// <returns>放大后包围盒</returns>
        public static ZXBoundary operator +(ZXBoundary _b, float _l)
        {
            return new ZXBoundary(
                _b.MinX - _l, _b.MaxX + _l,
                _b.MinY - _l, _b.MaxY + _l,
                _b.MinZ - _l, _b.MaxZ + _l);
        }

        /// <summary>
        /// 计算包围盒的空间偏移
        /// </summary>
        /// <param name="_b">包围盒</param>
        /// <param name="_p">偏移三维坐标</param>
        /// <returns>偏移后包围盒</returns>
        public static ZXBoundary operator +(ZXBoundary _b, ZXPoint _p)
        {
            return new ZXBoundary(
                _b.MinX + _p.X, _b.MaxX + _p.X,
                _b.MinY + _p.Y, _b.MaxY + _p.Y,
                _b.MinZ + _p.Z, _b.MaxZ + _p.Z);
        }

        /// <summary>
        /// 计算包围盒的空间偏移
        /// </summary>
        /// <param name="_b">包围盒</param>
        /// <param name="_p">偏移三维坐标</param>
        /// <returns>偏移后包围盒</returns>
        public static ZXBoundary operator -(ZXBoundary _b, ZXPoint _p)
        {
            return new ZXBoundary(
                _b.MinX - _p.X, _b.MaxX - _p.X,
                _b.MinY - _p.Y, _b.MaxY - _p.Y,
                _b.MinZ - _p.Z, _b.MaxZ - _p.Z);
        }

        /// <summary>
        /// 包围盒按向量抹出去后的整体
        /// </summary>
        /// <param name="_b0">包围盒</param>
        /// <param name="_dir">抹出去的向量</param>
        /// <returns></returns>
        public static ZXBoundary operator *(ZXBoundary _b0, ZXPoint _dir)
        {
            ZXBoundary b1 = _b0 + _dir;

            return new ZXBoundary(
                    Math.Min(_b0.MinX, b1.MinX),
                    Math.Max(_b0.MaxX, b1.MaxX),
                    Math.Min(_b0.MinY, b1.MaxY),
                    Math.Max(_b0.MaxY, b1.MaxY),
                    Math.Min(_b0.MinZ, b1.MaxZ),
                    Math.Max(_b0.MaxZ, b1.MaxZ)
                );
        }

        /// <summary>
        /// 两个包围盒做差
        /// </summary>
        /// <param name="_bB">大包围盒</param>
        /// <param name="_bS">小包围盒</param>
        /// <returns></returns>
        public static ZXBoundary operator -(ZXBoundary _bB, ZXBoundary _bS)
        {
            ZXBoundary bResult = _bB;

            if (_bS.MaxX < _bB.MaxX)
            {
                bResult.MinX = _bS.MaxX;
            }

            if (_bS.MinX > _bB.MinX)
            {
                bResult.MaxX = _bS.MinX;
            }

            if (_bS.MaxY < _bB.MaxY)
            {
                bResult.MinY = _bS.MaxY;
            }

            if (_bS.MinY > _bB.MinY)
            {
                bResult.MaxY = _bS.MinY;
            }

            if (_bS.MaxZ < _bB.MaxZ)
            {
                bResult.MinZ = _bS.MaxZ;
            }

            if (_bS.MinZ > _bB.MinZ)
            {
                bResult.MaxZ = _bS.MinZ;
            }

            return bResult;
        }

        /// <summary>
        /// 两个包围盒取并集（小中取小，大中取大）
        /// </summary>
        /// <param name="_b0"></param>
        /// <param name="_b1"></param>
        /// <returns></returns>
        public static ZXBoundary operator +(ZXBoundary _b0, ZXBoundary _b1)
        {
            return new ZXBoundary(
                Math.Min(_b0.MinX, _b1.MinX),
                Math.Max(_b0.MaxX, _b1.MaxX),
                Math.Min(_b0.MinY, _b1.MinY),
                Math.Max(_b0.MaxY, _b1.MaxY),
                Math.Min(_b0.MinZ, _b1.MinZ),
                Math.Max(_b0.MaxZ, _b1.MaxZ)
                );
        }

        /// <summary>
        /// 两个包围盒取并集（小中取小，大中取大）
        /// </summary>
        /// <param name="_b0"></param>
        /// <param name="_b1"></param>
        /// <returns></returns>
        public static ZXBoundary operator |(ZXBoundary _b0, ZXBoundary _b1)
        {
            return new ZXBoundary(
                Math.Min(_b0.MinX, _b1.MinX),
                Math.Max(_b0.MaxX, _b1.MaxX),
                Math.Min(_b0.MinY, _b1.MinY),
                Math.Max(_b0.MaxY, _b1.MaxY),
                Math.Min(_b0.MinZ, _b1.MinZ),
                Math.Max(_b0.MaxZ, _b1.MaxZ)
                );
        }

        /// <summary>
        /// 两个包围盒取交集（小中取大，大中取小）
        /// </summary>
        /// <param name="_b0"></param>
        /// <param name="_b1"></param>
        /// <returns></returns>
        public static ZXBoundary operator *(ZXBoundary _b0, ZXBoundary _b1)
        {
            return new ZXBoundary(
                Math.Max(_b0.MinX, _b1.MinX),
                Math.Min(_b0.MaxX, _b1.MaxX),
                Math.Max(_b0.MinY, _b1.MinY),
                Math.Min(_b0.MaxY, _b1.MaxY),
                Math.Max(_b0.MinZ, _b1.MinZ),
                Math.Min(_b0.MaxZ, _b1.MaxZ)
                );
        }

        /// <summary>
        /// 两个包围盒取交集（小中取大，大中取小）
        /// </summary>
        /// <param name="_b0"></param>
        /// <param name="_b1"></param>
        /// <returns></returns>
        public static ZXBoundary operator &(ZXBoundary _b0, ZXBoundary _b1)
        {
            return new ZXBoundary(
                Math.Max(_b0.MinX, _b1.MinX),
                Math.Min(_b0.MaxX, _b1.MaxX),
                Math.Max(_b0.MinY, _b1.MinY),
                Math.Min(_b0.MaxY, _b1.MaxY),
                Math.Max(_b0.MinZ, _b1.MinZ),
                Math.Min(_b0.MaxZ, _b1.MaxZ)
                );
        }

        /// <summary>
        /// 返回两个包围盒3个方向的大小比较
        /// </summary>
        /// <param name="_b0"></param>
        /// <param name="_b1"></param>
        /// <returns></returns>
        public static bool operator >(ZXBoundary _b0, ZXBoundary _b1)
        {
            return _b0.L > _b1.L && _b0.W > _b1.W && _b0.H > _b1.H;
        }

        /// <summary>
        /// 返回两个包围盒3个方向的大小比较
        /// </summary>
        /// <param name="_b0"></param>
        /// <param name="_b1"></param>
        /// <returns></returns>
        public static bool operator <(ZXBoundary _b0, ZXBoundary _b1)
        {
            return _b0.L < _b1.L && _b0.W < _b1.W && _b0.H < _b1.H;
        }

        #endregion

        #region INode

        /// <summary>
        /// 判断两个二维包围盒是否相连
        /// </summary>
        /// <param name="p_b"></param>
        /// <param name="p_e">比较阈值</param>
        /// <returns></returns>
        public bool NearTo(INode p_b, float p_e = 0.001f)
        {
            ZXBoundary b = this;
            b.MinX -= p_e;
            b.MaxX += p_e;

            if (b.IntersectXY((ZXBoundary)p_b))
            {
                // case 1: 横向相邻
                return true;
            } 

            // case 2: 纵向相邻
            b = this;
            b.MinY -= p_e;
            b.MaxY += p_e; 

            return b.IntersectXY((ZXBoundary)p_b);
        }

        /// <summary>
        /// 判断中间一个二维包围盒是否为拐点
        /// </summary>
        /// <param name="p_preNode">上一个包围盒</param>
        /// <param name="p_nextNode">下一个包围盒</param>
        /// <param name="p_e"></param>
        /// <returns></returns>
        public bool IsFlex(INode p_preNode, INode p_nextNode, float p_e = 0.001f)
        {
            bool hasFlex = true; // 是否是拐点

            ZXBoundary preNode = (ZXBoundary) p_preNode;
            ZXBoundary nextNode = (ZXBoundary) p_nextNode;

            if (Math.Abs(preNode.Center.X - this.Center.X) < p_e && Math.Abs(this.Center.X - nextNode.Center.X) < p_e)
            {
                hasFlex = false;
            }
            else if (Math.Abs(preNode.Center.Y - this.Center.Y) < p_e && Math.Abs(this.Center.Y - nextNode.Center.Y) < p_e)
            {
                hasFlex = false;
            }

            return hasFlex;
        }


        #endregion

        /// <summary>
        /// 【调试用】可视化为点集
        /// </summary>
        /// <returns></returns>
        public ZXPointSet Visualize(float p_unit = 0.1f)
        {
            ZXPointSet ps = new ZXPointSet();
            ps.AddDotBoundary(this);
            return ps;
        }

        /// <summary>
        /// 输出json字符串
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return "{ " +
                " \"L\": " + L.ToString("0.000") + ", " +
                " \"W\": " + W.ToString("0.000") + ", " +
                " \"H\": " + H.ToString("0.000") + ", " +
                " \"MinX\": " + MinX.ToString("0.000") + ", " +
                " \"MaxX\": " + MaxX.ToString("0.000") + ", " +
                " \"MinY\": " + MinY.ToString("0.000") + ", " +
                " \"MaxY\": " + MaxY.ToString("0.000") + ", " +
                " \"MinZ\": " + MinZ.ToString("0.000") + ", " +
                " \"MaxZ\": " + MaxZ.ToString("0.000") + "} ";
        }

        /// <summary>
        /// 按舱型数据格式*.cabin生成字符串
        /// </summary>
        /// <returns></returns>
        public string ToCabinString()
        {
            string str = "";
            str += MinZ+ ":";
            str += MinX.ToString("0.000") + "," + MinY.ToString("0.000") + ";";
            str += MaxX.ToString("0.000") + "," + MinY.ToString("0.000") + ";";
            str += MaxX.ToString("0.000") + "," + MaxY.ToString("0.000") + ";";
            str += MinX.ToString("0.000") + "," + MaxY.ToString("0.000");

            return str;
        }

        /// <summary>
        /// 返回边界字符串表示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[ X: " + MinX.ToString("0.00") + "-" + MaxX.ToString("0.00") 
                + "  Y: " + MinY.ToString("0.00") + "-" + MaxY.ToString("0.00") 
                + "  Z: " + MinZ.ToString("0.00") + "-" + MaxZ.ToString("0.00") 
                + "  L: " + L.ToString("0.00") 
                + "  W: " + W.ToString("0.00")
                + "  H: " + H.ToString("0.00")
                +" ]";
        }


    }
}
