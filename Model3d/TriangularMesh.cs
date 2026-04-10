using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCloudLib.Model3d
{
    /// <summary>
    /// 三角面片的几何信息
    /// </summary>
    public class TriangularMesh
    {
        public ZXPoint m_Point1 { get; protected set; }

        public ZXPoint m_Point2 { get; protected set; }

        public ZXPoint m_Point3 { get; protected set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TriangularMesh()
        { }

        /// <summary>
        /// ctor：逆时针顺序
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public TriangularMesh(ZXPoint p1, ZXPoint p2, ZXPoint p3)
        {
            this.m_Point1 = p1;
            this.m_Point2 = p2;
            this.m_Point3 = p3;
        }

        /// <summary>
        /// 获取法线向量字符串描述
        /// </summary>
        /// <returns></returns>
        public string GetNormalVectorString()
        {
            return "0 0 0";
        }

        /// <summary>
        /// STL ASC文本字符串
        /// </summary>
        /// <returns></returns>
        public string GetStlAscString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("facet normal " + this.GetNormalVectorString());
            sb.AppendLine("outer loop");
            sb.AppendLine("vertex " + this.m_Point1.X.ToString() + " " + this.m_Point1.Y.ToString() + " " + this.m_Point1.Z.ToString());
            sb.AppendLine("vertex " + this.m_Point2.X.ToString() + " " + this.m_Point2.Y.ToString() + " " + this.m_Point2.Z.ToString());
            sb.AppendLine("vertex " + this.m_Point3.X.ToString() + " " + this.m_Point3.Y.ToString() + " " + this.m_Point3.Z.ToString());
            sb.AppendLine("endloop");
            sb.AppendLine("endfacet");

            return sb.ToString();
        }

        public string GetStlAscString_XOY(float _boundZ)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("facet normal " + this.GetNormalVectorString());
            sb.AppendLine("outer loop");
            sb.AppendLine("vertex " + this.m_Point3.X.ToString() + " " + this.m_Point3.Y.ToString() + " " + _boundZ.ToString());
            sb.AppendLine("vertex " + this.m_Point2.X.ToString() + " " + this.m_Point2.Y.ToString() + " " + _boundZ.ToString());
            sb.AppendLine("vertex " + this.m_Point1.X.ToString() + " " + this.m_Point1.Y.ToString() + " " + _boundZ.ToString());
            sb.AppendLine("endloop");
            sb.AppendLine("endfacet");

            return sb.ToString();
        }

        public string GetStlAscString_YOZ(float _boundX)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("facet normal " + this.GetNormalVectorString());
            sb.AppendLine("outer loop");
            sb.AppendLine("vertex " + _boundX.ToString() + " " + this.m_Point1.Y.ToString() + " " + this.m_Point1.Z.ToString());
            sb.AppendLine("vertex " + _boundX.ToString() + " " + this.m_Point2.Y.ToString() + " " + this.m_Point2.Z.ToString());
            sb.AppendLine("vertex " + _boundX.ToString() + " " + this.m_Point3.Y.ToString() + " " + this.m_Point3.Z.ToString());
            sb.AppendLine("endloop");
            sb.AppendLine("endfacet");

            return sb.ToString();
        }

        public string GetStlAscString_XOZ(float _boundY)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("facet normal " + this.GetNormalVectorString());
            sb.AppendLine("outer loop");
            sb.AppendLine("vertex " + this.m_Point1.X.ToString() + " " + _boundY.ToString() + " " + this.m_Point1.Z.ToString());
            sb.AppendLine("vertex " + this.m_Point2.X.ToString() + " " + _boundY.ToString() + " " + this.m_Point2.Z.ToString());
            sb.AppendLine("vertex " + this.m_Point3.X.ToString() + " " + _boundY.ToString() + " " + this.m_Point3.Z.ToString());
            sb.AppendLine("endloop");
            sb.AppendLine("endfacet");

            return sb.ToString();
        }
    }
}
