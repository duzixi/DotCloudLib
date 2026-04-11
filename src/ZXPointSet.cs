//=====================================================================
// 模块名称：点集 ZXPointSet
// 功能简介：点的集合
// 版权声明：2019 九州创智科技有限公司  All Rights Reserved.
//           2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2019.11 杜子兮 移自“知行”C++库
//          2022.07.05 Sanngoku 优化LengthN、WidthN的封装方式，提高运行效率
//          2022.12.14 点云数据中 nan 按 0 处理
//          2023.05.25 Add系列返回Add后的自身对象
//          2024.02.22 杨波 文件读写系列新增PLY格式
//============================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Mathd;

namespace DotCloudLib
{
    /// <summary>
    /// 坐标轴类型
    /// </summary>
    public enum AxisType
    {
        /// <summary>
        /// X坐标轴
        /// </summary>
        X,
        /// <summary>
        /// Y坐标轴
        /// </summary>
        Y,
        /// <summary>
        /// Z坐标轴
        /// </summary>
        Z
    }

    /// <summary>
    /// 平面类型
    /// </summary>
    public enum PlaneType
    {
        /// <summary>
        /// XOY 平面
        /// </summary>
        XOY,

        /// <summary>
        /// YOZ 平面
        /// </summary>
        YOZ,

        /// <summary>
        /// XOZ 平面
        /// </summary>
        XOZ
    }

    /// <summary>
    /// 点格式
    /// </summary>
    public enum PointFormat
    {
        /// <summary>
        /// 三个坐标，逗号分隔
        /// </summary>
        XYZ,
        /// <summary>
        /// 点ID + 三个坐标，逗号分隔
        /// </summary>
        IXYZ,
        /// <summary>
        /// 内部约定JSON格式，具体参见Pile3D接口文档
        /// </summary>
        JSON
    }

    /// <summary>
    /// 点集
    /// </summary>
    public class ZXPointSet : IList<ZXPoint>
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public ZXPointSet()
        {
            m_points = new List<ZXPoint>();
        }

        /// <summary>
        /// 构造方法——深拷贝
        /// 开辟新的内存空间，新点云内部点赋值不影响原有点云
        /// </summary>
        /// <param name="org"></param>
        public ZXPointSet(ZXPointSet org)
        {
            // this.m_points = new List<ZXPoint>(org.m_points.ToArray());
            m_points = new List<ZXPoint>();

            if (org == null)
            {
                return;
            }

            foreach (var point in org.m_points)
            {
                this.Add(new ZXPoint(point.X, point.Y, point.Z));
            }

            this.Unit = org.Unit;
            this.Bound = org.Bound;
        }


        /// <summary>
        /// 构造方法——默认载入.xyz格式点云数据构造，以前两个点的Y方向偏移为默认unit格网精度
        /// 创建版本：0.7.6
        /// </summary>
        /// <param name="filePath"></param>
        public ZXPointSet(string filePath)
        {
            m_points = new List<ZXPoint>();

            this.LoadFromXYZ(filePath);

            if (this.m_points.Count >= 2)
            {
                this.Unit = m_points[1].Y - m_points[0].Y;
            }
        }

        /// <summary>
        /// 构造方法——点List 
        /// </summary>
        /// <param name="points"></param>
        public ZXPointSet(List<ZXPoint> points)
        {
            m_points = new List<ZXPoint>(); // (2025.06.30 改 + 初始化)

            for (int i = 0; i < points.Count; i++)
            {
                this.Add(points[i].X, points[i].Y, points[i].Z);
            }
        }

        #region 属性

        /// <summary>
        /// 格网精度
        /// </summary>
        public float Unit = 0.1f;

        private int? lengthN;

        /// <summary>
        /// X方向格点数
        /// </summary>
        public int LengthN
        {
            get 
            {
                if(this.lengthN == null)
                {
                    this.Bound = this.Boundary;
                }
                return this.lengthN.Value;
            }
        }

        private int? widthN;

        /// <summary>
        /// Y方向各点数
        /// </summary>
        public int WidthN
        {
            get 
            {
                if(this.widthN == null)
                {
                    this.Bound = this.Boundary;
                }
                return this.widthN.Value;
            }
        }

        private ZXBoundary? m_bound;

        /// <summary>
        /// 上一边界（直接取值）
        /// </summary>
        public ZXBoundary Bound
        {
            get
            {
                if (this.m_bound.HasValue == false)
                {
                    return this.Boundary;
                }
                else
                {
                    return this.m_bound.Value;
                }
            }

            set
            {
                m_bound = value;
                if (m_bound != null) // 2022.7.20
                {
                    this.lengthN = (int)Math.Round((Bound.MaxX - Bound.MinX) / Unit) + 1;
                    this.widthN = (int)Math.Round((Bound.MaxY - Bound.MinY) / Unit) + 1;
                }
            }
        }

        private ZXBoundary m_boundary;

        /// <summary>
        /// 最新边界（重新计算）
        /// </summary>
        public ZXBoundary Boundary
        {
            // 每次取都会重新计算，如果不想重新计算，就取完保存
            get
            {
                if (this.m_points == null || this.m_points.Count <= 0)
                {
                    // 0.7.6 没有点时，包围盒为0
                    m_boundary = new ZXBoundary(0, 0, 0, 0, 0, 0);
                    m_bound = m_boundary;
                    this.lengthN = 0;  // 2022.7.20
                    this.widthN = 0;   // 2022.7.20
                    return m_boundary;
                }

                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float minZ = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;
                float maxZ = float.MinValue;

                foreach (var p in m_points)
                {

                    if (p.X >= maxX) { maxX = p.X; }
                    if (p.Y >= maxY) { maxY = p.Y; }
                    
                    if (p.X <= minX) { minX = p.X; }
                    if (p.Y <= minY) { minY = p.Y; }

                    if (!p.IsEmpty()) // 2022.7.1
                    {
                        if (p.Z >= maxZ) { maxZ = p.Z; }
                        if (p.Z <= minZ) { minZ = p.Z; }
                    }
                }
                m_boundary = new ZXBoundary(minX, maxX, minY, maxY, minZ, maxZ);
                m_bound = m_boundary;

                this.lengthN = (int)Math.Round((Bound.MaxX - Bound.MinX) / Unit) + 1;
                this.widthN = (int)Math.Round((Bound.MaxY - Bound.MinY) / Unit) + 1;
                return m_boundary;
            }
        }
        
        /// <summary>
        /// 返回点坐标List 2025.09.03 +
        /// </summary>
        public List<ZXPoint> PointsList { get { return m_points; } }
        
        #endregion



        #region 文件读写

        /// <summary>
        /// 载入XYZ格式数据文件
        /// </summary>
        /// <param name="filePath"></param>
        public bool LoadFromXYZ(string filePath)
        {
            // STEP 0: 清空内存
            m_points.Clear();

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            // STEP 1: 打开文件流
            string s = "";
            
            try
            {
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))  // 使用默认编码
                {
                    // STEP 2: 循环逐行读入
                    while ((s = sr.ReadLine()) != null)//判断是否读完文件，EndOfStream表示是否是流文件的结尾
                    {
                        if (string.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        char[] ch = new char[s.Length];
                        ch = s.ToCharArray();
                        if (ch[0] != '-' && ch[0] < 48 || ch[0] > 57)
                            continue;
                        string[] xyz = s.Split(',');
                        if (xyz.Length >= 3)
                        {
                            float x = 0;
                            float y = 0;
                            float z = 0;

                            if (xyz[0] != "nan")  x = float.Parse(xyz[0]);
                            if (xyz[1] != "nan")  y = float.Parse(xyz[1]);
                            if (xyz[2] != "nan")  z = float.Parse(xyz[2]);
                            
                            this.Add(x, y, z);
                            if (x >= maxX) { maxX = x; }
                            if (y >= maxY) { maxY = y; }
                            if (z >= maxZ) { maxZ = z; }
                            if (x <= minX) { minX = x; }
                            if (y <= minY) { minY = y; }
                            if (z <= minZ) { minZ = z; }
                        }
                    }

                }

                m_boundary = new ZXBoundary(minX, maxX, minY, maxY, minZ, maxZ);
                m_bound = m_boundary;
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@303", s);
            }

            return true;
        }

        /// <summary>
        /// 载入IXYZ格式点云数据文件
        /// </summary>
        /// <param name="filePath"></param>
        public bool LoadFromIXYZ(string filePath)
        {
            // STEP 0: 清空内存
            m_points.Clear();

            // STEP 1: 打开文件流
            try
            {
                string s = "";
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))  // 使用默认编码
                {
                    // STEP 2: 循环逐行读入
                    while ((s = sr.ReadLine()) != null)//判断是否读完文件，EndOfStream表示是否是流文件的结尾
                    {
                        string[] xyz = s.Split(',');
                        if (xyz != null && xyz.Length >= 3)
                        {
                            ZXPoint p = new ZXPoint(float.Parse(xyz[1]), float.Parse(xyz[2]), float.Parse(xyz[3]));
                            p.Id = xyz[0]; 
                            this.Add(p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@337");
            }

            return true;
        }

        /// <summary>
        /// 载入XYZ格式数据文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="offset">读值下标偏移</param>
        public bool LoadFromXYZABG(string filePath, uint offset = 0)
        {
            // STEP 0: 清空内存
            m_points.Clear();

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            // STEP 1: 打开文件流
            string s = "";
            try
            {
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))  // 使用默认编码
                {
                    // STEP 2: 循环逐行读入
                    while ((s = sr.ReadLine()) != null)//判断是否读完文件，EndOfStream表示是否是流文件的结尾
                    {
                        char[] ch = new char[s.Length];
                        ch = s.ToCharArray();
                        if (ch[0] != '-' && ch[0] < 48 || ch[0] > 57)
                            continue;
                        string[] xyz = s.Split(',');
                        if (xyz.Length >= 6 + offset)
                        {
                            float x = float.Parse(xyz[0 + offset]);
                            float y = float.Parse(xyz[1 + offset]);
                            float z = float.Parse(xyz[2 + offset]);
                            float alfa = float.Parse(xyz[3 + offset]);
                            float beta = float.Parse(xyz[4 + offset]);
                            float gama = float.Parse(xyz[5 + offset]);

                            this.Add(new ZXPoint(x, y, z, alfa, beta, gama));
                            if (x >= maxX) { maxX = x; }
                            if (y >= maxY) { maxY = y; }
                            if (z >= maxZ) { maxZ = z; }
                            if (x <= minX) { minX = x; }
                            if (y <= minY) { minY = y; }
                            if (z <= minZ) { minZ = z; }
                        }
                    }

                }

                m_boundary = new ZXBoundary(minX, maxX, minY, maxY, minZ, maxZ);
                m_bound = m_boundary;
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@401");
            }
            return true;
        }

        /// <summary>
        /// 载入IXYZABG格式数据文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool LoadFromIXYZABG(string filePath)
        {
            // STEP 0: 清空内存
            m_points.Clear();

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            // STEP 1: 打开文件流
            string s = "";
            try
            {
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))  // 使用默认编码
                {
                    // STEP 2: 循环逐行读入
                    while ((s = sr.ReadLine()) != null)//判断是否读完文件，EndOfStream表示是否是流文件的结尾
                    {
                        char[] ch = new char[s.Length];
                        ch = s.ToCharArray();
                        if (ch[0] != '-' && ch[0] < 48 || ch[0] > 57)
                            continue;
                        string[] xyz = s.Split(',');
                        if (xyz.Length >= 6 )
                        {
                            string id = xyz[0];
                            float x = float.Parse(xyz[1]);
                            float y = float.Parse(xyz[2]);
                            float z = float.Parse(xyz[3]);
                            float alfa = float.Parse(xyz[4]);
                            float beta = float.Parse(xyz[5]);
                            float gama = float.Parse(xyz[6]);
                            ZXPoint p = new ZXPoint(x, y, z, alfa, beta, gama);
                            p.Id = id;

                            this.Add(p);
                            if (x >= maxX) { maxX = x; }
                            if (y >= maxY) { maxY = y; }
                            if (z >= maxZ) { maxZ = z; }
                            if (x <= minX) { minX = x; }
                            if (y <= minY) { minY = y; }
                            if (z <= minZ) { minZ = z; }
                        }
                    }

                }

                m_boundary = new ZXBoundary(minX, maxX, minY, maxY, minZ, maxZ);
                m_bound = m_boundary;
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@401");
            }
            return true;
        }

        /// <summary>
        /// 载入PLC时序数据——上海宝钢链斗卸船机
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool LoadFromPLC(string filePath)
        {
            m_points.Clear();

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            int counter = 0;

            // STEP 1: 打开文件流
            try
            {
                string s = "";
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.Default))  // 使用默认编码
                {
                    // STEP 2: 循环逐行读入
                    while ((s = sr.ReadLine()) != null)//判断是否读完文件，EndOfStream表示是否是流文件的结尾
                    {
                        if (counter++ == 0)
                        {
                            continue;  // 第一行标题剔除
                        }

                        string[] xyz = s.Split(',');
                        if (xyz != null && xyz.Length >= 6)
                        {
                            string id = xyz[0];
                            float x = float.Parse(xyz[11]);
                            float y = float.Parse(xyz[12]);
                            float z = float.Parse(xyz[13]);
                            float alfa = float.Parse(xyz[4]); // pos_reclaim_rotate_encoder 取料头回转角度
                            float beta = float.Parse(xyz[5]); // pos_reclaim_out 取料头伸缩
                            float gama = 0;

                            ZXPoint zXPoint  = new ZXPoint(x, y, z, alfa, beta, gama);
                            zXPoint.Id = id;
                            this.Add(zXPoint);
                            if (x >= maxX) { maxX = x; }
                            if (y >= maxY) { maxY = y; }
                            if (z >= maxZ) { maxZ = z; }
                            if (x <= minX) { minX = x; }
                            if (y <= minY) { minY = y; }
                            if (z <= minZ) { minZ = z; }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@457");
            }

            return true;
        }

        /// <summary>
        /// 载入PLY格式数据文件  by:杨波
        /// </summary>
        /// <param name="filePath"></param>
        public bool LoadFromPLY(string filePath)
        {
            // STEP 0: 清空内存
            m_points.Clear();


            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;


            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    // 读取PLY文件头部信息
                    string line;
                    bool headerFinished = false;
                    while (!headerFinished && (line = ReadAsciiLine(reader)) != null)
                    {
                        if (line.Trim().ToLower() == "end_header")
                        {
                            headerFinished = true;
                        }
                    }

                    // 读取点云数据并写入ZXPointSet对象

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();
                            
                        if (!float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z))
                        {

                            this.Add(x, y, z);
                            if (x >= maxX) { maxX = x; }
                            if (y >= maxY) { maxY = y; }
                            if (z >= maxZ) { maxZ = z; }
                            if (x <= minX) { minX = x; }
                            if (y <= minY) { minY = y; }
                            if (z <= minZ) { minZ = z; }
                        }
                    }                  
                }
                m_boundary = new ZXBoundary(minX, maxX, minY, maxY, minZ, maxZ);
                m_bound = m_boundary;
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@466");
            }

            return true;
        }

        /// <summary>
        /// 读取bin文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool LoadFromBin(string filePath)
        {
            // STEP 0: 清空内存
            m_points.Clear();


            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    byte binaryVersion = reader.ReadByte();
                    Int32 pointCount = reader.ReadInt32();
                    byte userRGB = reader.ReadByte();

                    // 读取点云数据并写入ZXPointSet对象
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();
                        if (!float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z))
                        {
                            if (userRGB != 0)
                            {
                                float r = reader.ReadSingle();
                                float g = reader.ReadSingle();
                                float b = reader.ReadSingle();
                                this.Add(new ZXPoint(x, y, z, r, g, b));
                            }
                            else
                            {
                                this.Add(new ZXPoint(x, y, z));
                            }

                            if (x >= maxX) { maxX = x; }
                            if (y >= maxY) { maxY = y; }
                            if (z >= maxZ) { maxZ = z; }
                            if (x <= minX) { minX = x; }
                            if (y <= minY) { minY = y; }
                            if (z <= minZ) { minZ = z; }
                        }
                    }
                    if (this.m_points.Count != pointCount)
                    {
                        return false;
                    }
                }
                m_boundary = new ZXBoundary(minX, maxX, minY, maxY, minZ, maxZ);
                m_bound = m_boundary;
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@686");
                return false;
            }

            return true;
        }


        /// <summary>
        /// 自定义的辅助方法，用于从二进制文件中读取 ASCII 字符串并忽略末尾的换行符  by:杨波
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        static string ReadAsciiLine(BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            while ((c = reader.ReadChar()) != '\n')
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 载入PLY格式数据文件  by:杨波
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAsPLY(string filePath)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    // 写入PLY文件头部信息
                    WriteAsciiLine(writer, "ply");
                    WriteAsciiLine(writer, "format binary_little_endian 1.0");
                    WriteAsciiLine(writer, "comment Created by DotCloudLib");
                    WriteAsciiLine(writer, "comment (C)2024 IRT All Rights Reserved.");
                    WriteAsciiLine(writer, "comment Created " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
                    WriteAsciiLine(writer, "obj_info Generated by DotCloudLib of IRT!");
                    WriteAsciiLine(writer, "element vertex " + m_points.Count);
                    WriteAsciiLine(writer, "property float x");
                    WriteAsciiLine(writer, "property float y");
                    WriteAsciiLine(writer, "property float z");
                    WriteAsciiLine(writer, "end_header");

                    // 写入点云数据
                    foreach (var point in m_points)
                    {
                        writer.Write(point.X);
                        writer.Write(point.Y);
                        writer.Write(point.Z);
                    }
                }
            }
            catch (Exception ex)
            {
                LibTool.Error(ex, "ZXPointSet@516");
            }

        }
        /// <summary>
        /// 自定义的辅助方法，用于向二进制文件中写入 ASCII 字符串并在末尾添加换行符 by:杨波
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="line"></param>
        static void WriteAsciiLine(BinaryWriter writer, string line)
        {
            writer.Write(line.ToCharArray());
            writer.Write((byte)'\n');
        }
        /// <summary>
        /// 输出*.ixyz格式点云数据
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAsIXYZ(string filePath)
        {
            FileStream fs = GetFileStream(filePath);

            for (int i = 0; i < m_points.Count; i++)
            {
                string xyz = string.Format("{0},{1},{2},{3},{4},{5},{6}\n",
                    m_points[i].Id, m_points[i].X, m_points[i].Y, m_points[i].Z, m_points[i].Alfa, m_points[i].Beta, m_points[i].Gama);
                byte[] data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 输出*.xyz格式点云数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="p_minZ">空点基准高度</param>
        public void SaveAsXYZ(string filePath, float p_minZ = -100000)
        {
            FileStream fs = GetFileStream(filePath);

            for (int i = 0; i < m_points.Count; i++)
            {
                if (m_points[i].Z <= p_minZ)
                {
                    continue;
                }

                string xyz = string.Format("{0},{1},{2}\n", m_points[i].X, m_points[i].Y, m_points[i].Z);
                byte[] data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();
            LibTool.Debug("    点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 输出*.xyz格式点云数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="p_minZ">空点基准高度</param>
        public void SaveAsXYZABG(string filePath, float p_minZ = -100000)
        {
            FileStream fs = GetFileStream(filePath);

            for (int i = 0; i < m_points.Count; i++)
            {
                if (m_points[i].Z <= p_minZ)
                {
                    continue;
                }

                string xyz = string.Format("{0},{1},{2},{3},{4},{5}\n", m_points[i].X, m_points[i].Y, m_points[i].Z, m_points[i].Alfa, m_points[i].Beta, m_points[i].Gama);
                byte[] data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 输出*.xyz格式点云数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="p_minZ">空点基准高度</param>
        public void SaveAsIXYZABG(string filePath, float p_minZ = -100000)
        {
            FileStream fs = GetFileStream(filePath);

            for (int i = 0; i < m_points.Count; i++)
            {
                if (m_points[i].Z <= p_minZ)
                {
                    continue;
                }

                // LibTool.Debug(m_points[i].Id);

                string xyz = string.Format("{0},{1},{2},{3},{4},{5},{6}\n", m_points[i].Id, m_points[i].X, m_points[i].Y, m_points[i].Z, m_points[i].Alfa, m_points[i].Beta, m_points[i].Gama);
                // LibTool.Debug(xyz);

                byte[] data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 输出点坐标及Id
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAsXYZId(string filePath)
        {
            FileStream fs = GetFileStream(filePath);

            for (int i = 0; i < m_points.Count; i++)
            {
                string xyz = string.Format("{0},{1},{2},{3}\n", m_points[i].X, m_points[i].Y, m_points[i].Z, m_points[i].Id);
                byte[] data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 输出*.js格式点云文件 var points = [[x,y,z], [x,y,z], .... [x,y,z]];
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="varName"></param>
        public void SaveAsJSArray(string filePath, string varName = "points")
        {
            FileStream fs = GetFileStream(filePath);

            string str = "var " + varName + " = [";
            byte[] data = System.Text.Encoding.Default.GetBytes(str);
            fs.Write(data, 0, data.Length);
            for (int i = 0; i < m_points.Count; i++)
            {
                string xyz = string.Format("[{0},{1},{2}],\n", m_points[i].X, m_points[i].Y, m_points[i].Z);
                data = System.Text.Encoding.Default.GetBytes(xyz);
                fs.Write(data, 0, data.Length);
            }
            str = "];";
            data = System.Text.Encoding.Default.GetBytes(str);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 输出 *.js格式点云文件 var points = {  };
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="varName"></param>
        public void SaveAsJSObject(string filePath, string varName = "points")
        {
            // STEP 0: 校验
            if (!IsFull())
            {
                LibTool.Error("ZXPointSet559: 不是满点阵");
            }

            // STEP 1: 文件头
            FileStream fs = GetFileStream(filePath);
            WriteTo(ref fs, "var " + varName + " = {");

            WriteTo(ref fs, "oX:" + this.Bound.MinX + ",");
            WriteTo(ref fs, "oY:" + this.Bound.MinY + ",");
            WriteTo(ref fs, "oZ:" + this.Bound.MinZ + ",");
            WriteTo(ref fs, "length:" + this.LengthN + ",");
            WriteTo(ref fs, "width:" + this.WidthN + ",");
            WriteTo(ref fs, "unit:" + this.Unit + ",\n");
            DotPile pile = new DotPile(this);

            WriteTo(ref fs, "volume:" + pile.Volume + ",\n");
            WriteTo(ref fs, "heights:[");

            // STEP 2: 点序列
            for (int i = 0; i < m_points.Count; i++)
            {
                string z = string.Format("{0},", m_points[i].Z);
                WriteTo(ref fs, z);
            }

            // STEP 3: 文件尾
            WriteTo(ref fs, "]};");

            // STEP 4: 关闭
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 使用条件：满点阵
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAsJson(string filePath)
        {
            // STEP 0: 校验
            if (!IsFull())
            {
                LibTool.Error("ZXPointSet559: 不是满点阵");
            }

            // STEP 1: 文件头
            FileStream fs = GetFileStream(filePath);
            WriteTo(ref fs, "{");
            WriteTo(ref fs, "\"id\":\"" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\",");
            WriteTo(ref fs, "\"oX\":" + this.Bound.MinX + ",");
            WriteTo(ref fs, "\"oY\":" + this.Bound.MinY + ",");
            WriteTo(ref fs, "\"oZ\":" + this.Bound.MinZ + ",");
            WriteTo(ref fs, "\"length\":" + this.LengthN + ",");
            WriteTo(ref fs, "\"width\":" + this.WidthN + ",");
            WriteTo(ref fs, "\"unit\":" + this.Unit + ",\n");
            DotPile pile = new DotPile(this);

            WriteTo(ref fs, "\"volume\":" + pile.Volume + ",\n");
            WriteTo(ref fs, "\"heights\":[");

            // STEP 2: 点序列
            for (int i = 0; i < m_points.Count - 1; i++)
            {
                string z = string.Format("{0},", m_points[i].Z);
                WriteTo(ref fs, z);
            }

            string z1 = string.Format("{0}", m_points[m_points.Count - 1].Z);
            WriteTo(ref fs, z1);

            // STEP 3: 文件尾
            WriteTo(ref fs, "]}");

            // STEP 4: 关闭
            fs.Flush();
            fs.Close();
            LibTool.Debug("点云数据保存为：" + filePath);
        }

        /// <summary>
        /// 保存成bin文件
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveAsBin(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var stream = new FileStream(filePath, FileMode.Create);
            stream.WriteByte(1);
            stream.Write(BitConverter.GetBytes(m_points.Count), 0, 4);
            stream.WriteByte(1);
            for (int i = 0; i < m_points.Count; i++) 
            {
                stream.Write(BitConverter.GetBytes(m_points[i].X), 0, 4);
                stream.Write(BitConverter.GetBytes(m_points[i].Y), 0, 4);
                stream.Write(BitConverter.GetBytes(m_points[i].Z), 0, 4);

                stream.Write(BitConverter.GetBytes(m_points[i].Alfa), 0, 4);
                stream.Write(BitConverter.GetBytes(m_points[i].Beta), 0, 4);
                stream.Write(BitConverter.GetBytes(m_points[i].Gama), 0, 4);
            }
            stream.Flush();
            stream.Close();
        }

        /// <summary>
        /// 写如文件流
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="content"></param>
        private void WriteTo(ref FileStream fs, string content)
        {
            byte[] data = System.Text.Encoding.Default.GetBytes(content);
            fs.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private FileStream GetFileStream(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return new FileStream(filePath, FileMode.Create);
        }

        #endregion

        #region 特征

        /// <summary>
        /// 判断是否为满点阵(简单判断方案)
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return WidthN * LengthN == m_points.Count;
        }

        /// <summary>
        /// 判断是否有空点
        /// </summary>
        /// <returns></returns>
        public bool HasNullPoint()
        {
            for (int i = 0; i < m_points.Count; i++)
            {
                if (m_points[i].IsEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 二维下标转一维下标
        /// </summary>
        /// <param name="p_i"></param>
        /// <param name="p_j"></param>
        /// <returns></returns>
        public int GetIndex(int p_i, int p_j)
        {
            if (p_i < 0 || p_j < 0 || p_i > this.LengthN || p_j > this.WidthN)
            {
                LibTool.Error("ZXPointSet_611: GetIndex(i,j) 参数越界");
            }

            return p_i * WidthN + p_j;
        }

        /// <summary>
        /// 根据点XY坐标获取格网索引
        /// </summary>
        /// <param name="p_x"></param>
        /// <param name="p_y"></param>
        /// <returns></returns>
        public int GetIndex(float p_x, float p_y)
        {
            int iX = GetIndexX(p_x);
            int iY = GetIndexY(p_y);

            // 越界校验
            if (iX == -1 || iY == -1)
                return -1;  // 非法越界

            int index = iX * WidthN + iY;

            if (index >= 0 && index < m_points.Count)
                return index;

            return -1;
        }

        /// <summary>
        /// 获取相邻点格网下标
        /// </summary>
        /// <param name="p_index"></param>
        /// <param name="p_dir"></param>
        /// <returns></returns>
        public int GetIndex(int p_index, Dir p_dir = Dir.Current)
        {
            int index = -1;
            switch (p_dir)
            {
                case Dir.Current:
                    index = p_index; break;
                case Dir.Left:
                    index = p_index - WidthN; break;
                case Dir.Up:
                    index = p_index + 1; break;
                case Dir.Right:
                    index = p_index + WidthN; break;
                case Dir.Down:
                    index = p_index - 1; break;
                case Dir.LeftDown:
                    index = p_index - WidthN - 1; break;
                case Dir.LeftUp:
                    index = p_index - WidthN + 1; break;
                case Dir.RightUp:
                    index = p_index + WidthN - 1; break;
                case Dir.RightDown:
                    index = p_index + WidthN + 1; break;
            }

            // index校验
            if (index >= 0 && index < m_points.Count)
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// 获取X下标
        /// </summary>
        /// <param name="p_x"></param>
        /// <returns></returns>
        public int GetIndexX(float p_x)
        {
            if (p_x > Bound.MaxX || p_x < Bound.MinX)
                return -1;  // 非法越界

            return (int)Math.Round((p_x - Bound.MinX) / Unit);
        }

        /// <summary>
        /// 获取Y下标
        /// </summary>
        /// <param name="p_y"></param>
        /// <returns></returns>
        public int GetIndexY(float p_y)
        {
            if (p_y > Bound.MaxY || p_y < Bound.MinY)
                return -1;  // 非法越界

            return (int)Math.Round((p_y - Bound.MinY) / Unit);
        }

        /// <summary>
        /// 根据二维下标获得点m
        /// </summary>
        /// <param name="p_i">X方向下标</param>
        /// <param name="p_j">Y方向下标</param>
        /// <returns>如果index越界，返回极小值点</returns>
        public ZXPoint Get(int p_i, int p_j)
        {
            if (p_i < 0 || p_j < 0 || p_i >= this.LengthN || p_j >= this.WidthN)
            {
                LibTool.Error("ZXPointSet_630: Get(i,j) 参数越界 i = " + p_i + " j = " + p_j + " LengthN = " + this.LengthN + " WidthN = " + this.WidthN);
            }

            int index = GetIndex(p_i, p_j);

            return this[index];
        }

        /// <summary>
        /// 获取相邻格网点
        /// </summary>
        /// <param name="p_index"></param>
        /// <param name="p_dir"></param>
        /// <returns></returns>
        public ZXPoint GetPoint(int p_index, Dir p_dir = Dir.Current)
        {
            int index = GetIndex(p_index, p_dir);

            // CASE 1: 返回空点(极小值)
            if (index < 0 || index >= m_points.Count)
            {
                return new ZXPoint(float.MinValue, float.MinValue, float.MinValue);
            }

            // CASE 2: 返回当前点
            return m_points[index];
        }

        /// <summary>
        /// 获取某一方向最近点
        /// </summary>
        /// <param name="p_i">X格网下标</param>
        /// <param name="p_j">Y格网下标</param>
        /// <param name="p_r">限制搜索半径</param>
        /// <param name="p_dir">方向</param>
        /// <returns></returns>
        public ZXPoint GetNearestPoint(int p_i, int p_j, int p_r, Dir p_dir)
        {
            ZXPoint p = new ZXPoint(float.MinValue, float.MinValue, float.MinValue);
            int index = GetIndex(p_i, p_j);
            if (index < 0 || index > this.Count) return p; // 下标越界返回空点

            int indexTarget = -1;
            bool found = false;

            switch (p_dir)
            {
                case Dir.Current:
                    return this[index];
                case Dir.Left:
                    for (int k = -1; k >= -p_r; k--)
                    {
                        if (p_i + k < 0)
                            return p; // 下标越界返回空点

                        indexTarget = GetIndex(p_i + k, p_j); // 获取当前点
                        if (indexTarget < 0 || indexTarget >= this.Count)
                            return p; // 下标越界返回空点

                        if (!this[indexTarget].IsEmpty(-99999))
                        {
                            found = true;
                            break; // 找到了点！！
                        }
                    }
                    break;
                case Dir.Up:
                    for (int k = 1; k < p_r; k++)
                    {
                        if (p_j + k > WidthN - 1)
                            return p; // 下标越界返回空点

                        indexTarget = GetIndex(p_i, p_j + k); // 获取当前点
                        if (indexTarget < 0 || indexTarget >= this.Count)
                            return p; // 下标越界返回空点

                        if (!this[indexTarget].IsEmpty(-99999))
                        {
                            found = true;
                            break; // 找到了点！！
                        }
                    }
                    break;
                case Dir.Right:
                    for (int k = 1; k <= p_r; k++)
                    {
                        if (p_i + k > LengthN - 1)
                            return p; // 下标越界返回空点

                        indexTarget = GetIndex(p_i + k, p_j); // 获取当前点
                        if (indexTarget < 0 || indexTarget >= this.Count)
                            return p; // 下标越界返回空点

                        if (!this[indexTarget].IsEmpty(-99999))
                        {
                            found = true;
                            break; // 找到了点！！
                        }
                    }
                    break;
                case Dir.Down:
                    for (int k = -1; k > -p_r; k--)
                    {
                        if (p_j + k < 0)
                            return p; // 下标越界返回空点

                        indexTarget = GetIndex(p_i, p_j + k); // 获取当前点
                        if (indexTarget < 0 || indexTarget >= this.Count)
                            return p; // 下标越界返回空点

                        if (!this[indexTarget].IsEmpty(-99999))
                        {
                            found = true;
                            break; // 找到了点！！
                        }
                    }
                    break;
                case Dir.LeftUp:
                    break;
                case Dir.RightUp:
                    break;
                case Dir.RightDown:
                    break;
                case Dir.LeftDown:
                    break;
                default:
                    break;
            }

            if (found && indexTarget >= 0 && indexTarget < this.Count)
            {
                return this[indexTarget];
            }

            return new ZXPoint(float.MinValue, float.MinValue, float.MinValue);
        }

        /// <summary>
        /// 求当前点集上与ps最相近的点
        /// </summary>
        /// <param name="ps">比较点集</param>
        /// <returns>最相近的点</returns>
        public ZXPoint GetNearestPoint(ZXPointSet ps)
        {
            float minDistance = float.MaxValue;
            ZXPoint point = new ZXPoint();
            for (int i = 0; i < ps.Count; i++)
            {
                for (int j = 0; j < this.m_points.Count; j++)
                {
                    float d = ps[i].DistanceTo(this[j]);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        point = this[j]; // 求交点
                    }
                }
            }
            return point;
        }

        /// <summary>
        /// 根据二维下标获得半径周围高程中值
        /// </summary>
        /// <param name="_xi"></param>
        /// <param name="_yj"></param>
        /// <param name="r"></param>
        /// <param name="p_n">周边点数阈值，周围点数过少，认为没有参考价值</param>
        /// <returns></returns>
        public float GetMidValue(int _xi, int _yj, int r, int p_n = 1)
        {
            // 条件检验
            if (!IsFull())
                return float.MinValue;

            int n = 0;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            for (int i = _xi - r; i <= _xi + r; i++)
            {
                // 如果越界，直接PASS
                if (i <= 0 || i >= LengthN)
                    continue;

                for (int j = _yj - r; j <= _yj + r; j++)
                {
                    if (j <= 0 || j >= WidthN)
                        continue;

                    // 再排除圆外点
                    int dx = i - _xi;
                    int dy = j - _yj;
                    if (dx * dx + dy * dy > r * r)
                        continue;

                    // 累加所有的非空点
                    float currentZ = Get(i, j).Z;
                    if (currentZ > float.MinValue)
                    {
                        n++;
                        if (currentZ < minZ)
                        {
                            minZ = currentZ;
                        }
                        if (currentZ > maxZ)
                        {
                            maxZ = currentZ;
                        }
                    }
                }
            }

            // 如果周围点过少
            if (n < p_n)
            {
                return float.MinValue;
            }

            return (minZ + maxZ) * 0.5f; // 取中间值
        }

        /// <summary>
        /// 返回料堆等高线
        /// </summary>
        /// <param name="p_h">高度</param>
        /// <param name="p_w"></param>
        /// <returns></returns>
        public ZXPointSet GetContour(float p_h, float p_w = 0.1f)
        {
            // STEP 1: 获取截取范围
            ZXBoundary bOrg = this.Boundary;
            ZXBoundary b = bOrg;
            b.MinZ = p_h - p_w / 2.0f;
            b.MaxZ = p_h + p_w / 2.0f;

            return this.Intercept(b, false);
        }

        /// <summary>
        /// 获取指定点横截面 
        /// 创建版本：0.7.6
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public ZXPointSet GetCrossSection(float x)
        {
            ZXBoundary bOrg = this.Boundary;
            ZXBoundary b = bOrg;
            b.MinX = x - this.Unit * 0.5f;
            b.MaxX = x + this.Unit * 0.5f;

            return this.Intercept(b, false);
        }

        /// <summary>
        /// 获取指定点纵截面
        /// 创建版本：0.7.6
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public ZXPointSet GetVerticalSection(float y)
        {
            ZXBoundary bOrg = this.Boundary;
            ZXBoundary b = bOrg;
            b.MinY = y - this.Unit * 0.5f;
            b.MaxY = y + this.Unit * 0.5f;

            return this.Intercept(b, false);
        }

        #endregion

        #region 增

        /// <summary>
        /// 添加一个点
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        public ZXPointSet Add(float _x, float _y, float _z)
        {
            m_points.Add(new ZXPoint(_x, _y, _z));
            return this;
        }

        /// <summary>
        /// 添加一个点 2029.09.03 + 
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public ZXPointSet Add(double _x, double _y, double _z)
        {
            return this.Add((float)_x, (float)_y, (float)_z);
        }

        /// <summary>
        /// 添加点阵线段
        /// </summary>
        /// <param name="_p0">始点</param>
        /// <param name="_p1">终点</param>
        /// <param name="_unit">点间距</param>
        public ZXPointSet AddDotSegment(ZXPoint _p0, ZXPoint _p1, float _unit)
        {
            float d = _p0.DistanceTo(_p1);
            int count = (int)(d / _unit);

            if (count <= 0)
            {
                return this;
            }

            float deltaX = (_p1.X - _p0.X) / count;
            float deltaY = (_p1.Y - _p0.Y) / count;
            float deltaZ = (_p1.Z - _p0.Z) / count;

            for (int i = 0; i <= count; i++)
            {
                float x = _p0.X + deltaX * i;
                float y = _p0.Y + deltaY * i;
                float z = _p0.Z + deltaZ * i;

                m_points.Add(new ZXPoint(x, y, z));
            }

            return this;
        }

        /// <summary>
        /// 在X轴上添加线
        /// </summary>
        /// <param name="_x0"></param>
        /// <param name="_x1"></param>
        /// <param name="_unit"></param>
        public ZXPointSet AddDotSegmentX(float _x0, float _x1, float _unit)
        {
            return this.AddDotSegment(new ZXPoint(_x0, 0, 0), new ZXPoint(_x1, 0, 0), _unit);
        }

        /// <summary>
        /// 在Y轴上添加线
        /// </summary>
        /// <param name="_y0"></param>
        /// <param name="_y1"></param>
        /// <param name="_unit"></param>
        public ZXPointSet AddDotSegmentY(float _y0, float _y1, float _unit)
        {
            return this.AddDotSegment(new ZXPoint(0, _y0, 0), new ZXPoint(0, _y1, 0), _unit);
        }

        /// <summary>
        /// 在Z轴上添加线
        /// </summary>
        /// <param name="_z0"></param>
        /// <param name="_z1"></param>
        /// <param name="_unit"></param>
        public ZXPointSet AddDotSegmentZ(float _z0, float _z1, float _unit)
        {
            return this.AddDotSegment(new ZXPoint(0, 0, _z0), new ZXPoint(0, 0, _z1), _unit);
        }

        /// <summary>
        /// 添加矩阵点阵
        /// </summary>
        /// <param name="_pLeftBottom">左下角坐标</param>
        /// <param name="_l">X方向长度</param>
        /// <param name="_w">Y方向长度</param>
        /// <param name="_unit">点间距</param>
        public ZXPointSet AddDotRectOXY(ZXPoint _pLeftBottom, float _l, float _w, float _unit)
        {
            // STEP 1：计算四个点
            ZXPoint p0 = new ZXPoint(_pLeftBottom.X, _pLeftBottom.Y, _pLeftBottom.Z);
            ZXPoint p1 = new ZXPoint(_pLeftBottom.X, _pLeftBottom.Y + _w, _pLeftBottom.Z);
            ZXPoint p2 = new ZXPoint(_pLeftBottom.X + _l, _pLeftBottom.Y + _w, _pLeftBottom.Z);
            ZXPoint p3 = new ZXPoint(_pLeftBottom.X + _l, _pLeftBottom.Y, _pLeftBottom.Z);

            // STEP 2：连接四个点
            AddDotSegment(p0, p1, _unit);
            AddDotSegment(p1, p2, _unit);
            AddDotSegment(p2, p3, _unit);
            AddDotSegment(p3, p0, _unit);

            return this;
        }

        /// <summary>
        /// 添加矩阵点阵
        /// </summary>
        /// <param name="_pCenter">中心点坐标</param>
        /// <param name="_l">X方向长度</param>
        /// <param name="_w">Y方向长度</param>
        /// <param name="_unit">点间距</param>
        public ZXPointSet AddDotRectCenter(ZXPoint _pCenter, float _l, float _w, float _unit)
        {
            // STEP 1：计算四个点
            ZXPoint p0 = new ZXPoint(_pCenter.X - _l * 0.5f, _pCenter.Y - _w * 0.5f, _pCenter.Z);
            ZXPoint p1 = new ZXPoint(_pCenter.X - _l * 0.5f, _pCenter.Y + _w * 0.5f, _pCenter.Z);
            ZXPoint p2 = new ZXPoint(_pCenter.X + _l * 0.5f, _pCenter.Y + _w * 0.5f, _pCenter.Z);
            ZXPoint p3 = new ZXPoint(_pCenter.X + _l * 0.5f, _pCenter.Y - _w * 0.5f, _pCenter.Z);

            // STEP 2：连接四个点
            AddDotSegment(p0, p1, _unit);
            AddDotSegment(p1, p2, _unit);
            AddDotSegment(p2, p3, _unit);
            AddDotSegment(p3, p0, _unit);

            return this;
        }

        /// <summary>
        /// 添加点阵包围盒
        /// </summary>
        /// <param name="_b">包围盒</param>
        /// <param name="_unit">点间距</param>
        public ZXPointSet AddDotBoundary(ZXBoundary _b, float _unit = 0.1f)
        {
            ZXPoint[] p = _b.V;

            AddDotSegment(p[0], p[1], _unit);
            AddDotSegment(p[1], p[2], _unit);
            AddDotSegment(p[2], p[3], _unit);
            AddDotSegment(p[3], p[0], _unit);

            AddDotSegment(p[4], p[5], _unit);
            AddDotSegment(p[5], p[6], _unit);
            AddDotSegment(p[6], p[7], _unit);
            AddDotSegment(p[7], p[4], _unit);

            AddDotSegment(p[0], p[4], _unit);
            AddDotSegment(p[1], p[5], _unit);
            AddDotSegment(p[2], p[6], _unit);
            AddDotSegment(p[3], p[7], _unit);

            return this;
        }

        /// <summary>
        /// 转换为满点阵（使用条件：格网化）
        /// </summary>
        /// <param name="psFull"></param>
        /// <param name="p_Z">默认为极小值</param>
        /// <returns></returns>
        public bool TurnToFull(ref ZXPointSet psFull, float p_Z = float.MinValue)
        {
            // LibTool.Debug("点数：" + m_points.Count);

            psFull.Unit = this.Unit;
            psFull.Bound = this.Boundary; // 2025.4.17 Bound -> Boundary
            Bound = Boundary; // 重新计算边界，保存到静态边界里
            int l = LengthN;
            int w = WidthN;

            if (l * w == m_points.Count)
            {
                // Console.WriteLine("Full");
                psFull = this;  // 2022.12.06 + 
                return false; // 不需要转换，已经是满点阵
            }

            float minX = Bound.MinX;
            float minY = Bound.MinY;

            psFull.Clear();

            // STEP 1: 满点阵初始化
            for (int i = 0; i < l; i++)
            {
                float x = (float)Math.Round( minX + i * Unit, 1);

                for (int j = 0; j < w; j++)
                {
                    float y =  (float)Math.Round(minY + j * Unit, 1);

                    psFull.Add(new ZXPoint(x, y, p_Z)); // 默认填充原始点云最低点
                }
            }

            psFull.Bound = psFull.Boundary;  // 重新计算边界

            // LibTool.Debug("点数：" + m_points.Count);


            //  STEP 2: 覆盖填充原始点云
            for (int i = 0; i < m_points.Count; i++)
            {
                int index = psFull.GetIndex(this[i].X, this[i].Y);

                // LibTool.Debug(index);

                if (index < psFull.Count && index >= 0)
                {
                    psFull[index].Z = this[i].Z;
                }
            }

            psFull.Bound = psFull.Boundary;  // 重新计算边界

            return true;
        }

        /// <summary>
        /// 空点填充 2022.11.24 Ver.0.9.2
        /// </summary>
        /// <param name="p_minZ"></param>
        public ZXPointSet SetMinZ(float p_minZ = 0)
        {
            for (int i = 0; i < m_points.Count; i++)
            {
                if (m_points[i].IsEmpty())
                {
                    m_points[i].Z = p_minZ;
                }
                else if (m_points[i].Z < p_minZ)
                {
                    m_points[i].Z = p_minZ;
                }
            }

            return this;
        }

        /// <summary>
        /// 所有X坐标归一
        /// </summary>
        /// <param name="p_x"></param>
        /// <returns></returns>
        public ZXPointSet SetX(float p_x)
        {
            for (int i = 0; i < m_points.Count; i++)
            {
                m_points[i].X = p_x;
            }

            return this;
        }

        /// <summary>
        /// 所有Y坐标归一
        /// </summary>
        /// <param name="p_y"></param>
        /// <returns></returns>
        public ZXPointSet SetY(float p_y)
        {
            for (int i = 0; i < m_points.Count; i++)
            {
                m_points[i].Y = p_y;
            }

            return this;
        }


        /// <summary>
        /// 统一高度
        /// </summary>
        /// <param name="p_Z">统一高度</param>
        /// <returns></returns>
        public ZXPointSet SetZ(float p_Z)
        {
            for (int i = 0; i < m_points.Count; i++)
            {
                m_points[i].Z = p_Z;
            }

            return this;
        }

       
        #endregion

        #region 删

        /// <summary>
        /// 下采样（N里挑一）
        /// </summary>
        /// <param name="_n">采样间隔</param>
        /// <param name="_changeOrg">是否改变原始点云</param>
        /// <returns></returns>
        public ZXPointSet DownSample(int _n, bool _changeOrg = true)
        {
            ZXPointSet outputPointSet = new ZXPointSet();

            // 按参数下采样
            for (int i = 0; i < this.Count; i += _n)
            {
                outputPointSet.Add(this[i]);
            }

            if (_changeOrg)
            {
                this.Clear();
                this.m_points = outputPointSet.m_points;
                return this;
            }
            else
            {
                return outputPointSet;
            }
        }

        /// <summary>
        /// 截取包围盒，默认改变原始点云
        /// </summary>
        /// <param name="_b">包围盒范围</param>
        /// <param name="_changeOrg"></param>
        /// <returns>截取后点云</returns>
        public ZXPointSet Intercept(ZXBoundary _b, bool _changeOrg = true)
        {
            ZXPointSet outputPointSet = new ZXPointSet();

            foreach (var p in m_points)
            {
                if (_b.Contain(p))
                {
                    outputPointSet.Add(p);
                }
            }

            if (_changeOrg)
            {
                this.Clear();
                this.m_points = outputPointSet.m_points;
                this.Bound = outputPointSet.Boundary;

                return this;
            } else
            {
                outputPointSet.Bound = outputPointSet.Boundary;
                outputPointSet.Unit = this.Unit;
                return outputPointSet;
            }
        }

        /// <summary>
        /// 切除指定区域，原点云减少
        /// </summary>
        /// <param name="_b"></param>
        /// <returns>返回被切除部分</returns>
        public ZXPointSet CutOff(ZXBoundary _b)
        {
            ZXPointSet cutPointSet = new ZXPointSet();
            ZXPointSet leftPointSet = new ZXPointSet();

            foreach (var p in m_points)
            {
                if (_b.Contain(p))
                {
                    cutPointSet.Add(p);
                } else
                {
                    leftPointSet.Add(p);
                }
            }

            this.Clear();
            this.m_points = leftPointSet.m_points;
            this.Bound = this.Boundary;
            return cutPointSet;
        }

        /// <summary>
        /// 去重：XY相同，保留Z最小点
        /// 使用前提：有序点
        /// </summary>
        /// <returns></returns>
        public ZXPointSet Removal()
        {
            // 记录上一个点坐标
            float lastX = float.MinValue;
            float lastY = float.MinValue;

            ZXPointSet outputPointSet = new ZXPointSet();

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].X.CompareTo(lastX) != 0 ||
                    this[i].Y.CompareTo(lastY) != 0)
                {
                    ZXPoint p = new ZXPoint(this[i].X, this[i].Y, this[i].Z);
                    outputPointSet.Add(p);
                    lastX = p.X;
                    lastY = p.Y;
                }
                else
                {
                    continue;
                }
            }
            this.Clear();
            this.m_points = outputPointSet.m_points;

            return outputPointSet;
        }

        /// <summary>
        /// 去重：XYZ相同
        /// 使用前提：有序点
        /// </summary>
        /// <returns></returns>
        public ZXPointSet RemovalXYZ()
        {
            // 记录上一个点坐标
            float lastX = float.MinValue;
            float lastY = float.MinValue;
            float lastZ = float.MinValue;

            ZXPointSet outputPointSet = new ZXPointSet();

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].X.CompareTo(lastX) == 0 &&
                    this[i].Y.CompareTo(lastY) == 0 &&
                    this[i].Z.CompareTo(lastZ) == 0)
                {
                    continue;
                }
                else
                {
                    ZXPoint p = new ZXPoint(this[i].X, this[i].Y, this[i].Z);
                    outputPointSet.Add(p);
                    lastX = p.X;
                    lastY = p.Y;
                    lastZ = p.Z;
                }
            }
            this.Clear();
            this.m_points = outputPointSet.m_points;
            return outputPointSet;
        }

        /// <summary>
        /// 异常点剔除
        /// 使用前提：格网下采样，非满秩阵
        /// </summary>
        /// <returns></returns>
        public ZXPointSet FilterOutlier(float p_unit)
        {
            Unit = p_unit;
            this.Bound = this.Boundary; // 重新计算边界

            // STEP 1: 转化为满秩阵，空点极小值部位
            ZXPointSet psFull = new ZXPointSet();
            TurnToFull(ref psFull);

            // STEP 2: 遍历所有点，标记删除
            for (int i = 0; i < psFull.Count; i++)
            {
                if (psFull[i].IsEmpty()) 
                {
                    continue; // 空点不考虑
                }

                float sum = 0;
                int n = 0;
                int index = -1;

                // STEP 2.1: 计算周围非空点高度
                // 遍历上下左右八个方向
                for (int j = 1; j <= 8; j++)
                {
                    index = GetIndex(i, (Dir)(j));
                    if (index < 0 || index >= psFull.Count) continue; // 越界算下一个

                    if (!psFull[index].IsEmpty())
                    {
                        sum += psFull[index].Z;
                        n++;
                    }
                }

                // STEP 2.2: 比较，如果差别大，剔除
                if (n > 3 && Math.Abs(sum / n - psFull[i].Z) > Unit && index >= 0)
                {
                    psFull[i].Z = float.MinValue;
                }
            }

            // STEP 3: 切实删除原点云
            this.m_points.Clear();

            for (int i = 0; i < psFull.Count; i++)
            {
                if (!psFull[i].IsEmpty())
                {
                    this.Add(psFull[i]);
                }
            }

            this.Bound = this.Boundary; // 重新计算边界
            return this;
        }

        /// <summary>
        /// 删除空节点
        /// </summary>
        public ZXPointSet RemoveNull()
        {
            ZXPointSet outputPointSet = new ZXPointSet();
            for (int i = 0; i < m_points.Count; i++)
            {
                if (!m_points[i].IsEmpty() && m_points[i].Z > -99999)  // 2024.04.16 dzx
                {
                    outputPointSet.Add(m_points[i]);
                }
            }
            this.Clear();
            this.m_points = outputPointSet.m_points;
            this.Bound = this.Boundary;
            return this;
        }

        /// <summary>
        /// 从原有点云中取出一段，并更新自身
        /// </summary>
        /// <param name="_startLocation">起始位置</param>
        /// <param name="_endLocation">终止位置</param>
        public ZXPointSet CutApart(float _startLocation, float _endLocation)
        {
            float minX = this.Bound.MinX;
            int startIndex = (int)((_startLocation - minX) * WidthN);
            int endIndex = (int)((_endLocation - minX) * WidthN);
            List<ZXPoint> newList = new List<ZXPoint>();
            for (int i = startIndex; i < endIndex; i++)
            {
                newList.Add(this.m_points[i]);
            }
            this.m_points = newList;
            this.Bound = this.Boundary;
            return this;
        }

        #endregion

        #region 改

        /// <summary>
        /// 根据XY坐标计算角度 → 0° ↑ 90° ← 180° ↓ 270°
        /// </summary>
        public void ComputeAlfa()
        {
            for (int i = 0; i < this.Count; i++)
            {
                float x = this[i].X;
                float y = this[i].Y;
                float angle = Vector3d.Angle(Vector3d.right, new Vector3d(x, y, 0));
                Vector3d cross = Vector3d.Cross(Vector3d.right, new Vector3d(x, y, 0)).normalized;
                angle = (angle * (float)cross.z + 360) % 360;
                this[i].Alfa = angle;

                if (x < 0 && Math.Abs(y) < 0.0001)
                {
                    this[i].Alfa = 180;
                    // LibTool.Debug("*********  X: " + x + " Y: " + y + "  " + this[i].Alfa);
                }
                
            }
        }

        /// <summary>
        /// 平移(会改变自身)
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public ZXPointSet Translate(float _x, float _y, float _z)
        {

            foreach (var p in m_points)
            {
                p.X += _x;
                p.Y += _y;
                p.Z += _z;
            }
            this.Bound = this.Boundary;
            return this;
        }

        /// <summary>
        /// 平移(会改变自身)
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        /// <returns></returns>
        public ZXPointSet Translate(double _x, double _y, double _z)
        {
            return this.Translate((float)_x, (float)_y, (float)_z);
        }

        /// <summary>
        /// 镜像(改变自身)
        /// </summary>
        /// <param name="_axis"></param>
        /// <returns></returns>
        public ZXPointSet Mirror(AxisType _axis)
        {
            foreach (var p in m_points)
            {
                switch (_axis)
                {
                    case AxisType.X:
                        p.X *= -1;
                        break;
                    case AxisType.Y:
                        p.Y *= -1;
                        break;
                    case AxisType.Z:
                        p.Z *= -1;
                        break;
                    default:
                        break;
                }
            }
            this.Bound = this.Boundary;
            return this;
        }

        /// <summary>
        /// 合并另一个点云
        /// </summary>
        /// <param name="_ps"></param>
        /// <returns></returns>
        public ZXPointSet Merge(ZXPointSet _ps)
        {
            foreach (var p in _ps)
            {
                this.Add(p.X, p.Y, p.Z); // 2015.8.22 改
            }
            this.Bound = this.Boundary;
            return this;
        }

        /// <summary>
        /// 围绕某一个轴旋转(会改变自身)
        /// </summary>
        /// <param name="_axis">旋转轴：左手螺旋定则</param>
        /// <param name="_alpha">旋转角度（单位：角度°）</param>
        /// <returns></returns>
        public ZXPointSet Rotate(AxisType _axis, float _alpha)
        {
            if (_alpha < 0.00001 && _alpha > -0.00001)
            {
                return this; // 旋转角度为0不参与计算
            }

            // STEP 1: 计算欧拉角
            Vector3d euler = new Vector3d(0, 0, 0);

            switch (_axis)
            {
                case AxisType.X:
                    euler[0] = -_alpha;
                    break;
                case AxisType.Y:
                    euler[1] = -_alpha;
                    break;
                case AxisType.Z:
                    euler[2] = -_alpha;
                    break;
                default:
                    break;
            }

            // STEP 2: 计算四元数
            Quaterniond q = new Quaterniond();
            q.eulerAngles = euler;

            // STEP 3: 用四元数乘每个点
            foreach (var p in m_points)
            {
                Vector3d v = new Vector3d(p.X, p.Y, p.Z);
                v = q * v;
                p.X = (float)v.x;
                p.Y = (float)v.y;
                p.Z = (float)v.z;
            }

            this.Bound = this.Boundary;

            return this;
        }

        /// <summary>
        /// 根据局部点云更新原来格网点云
        /// 使用条件：标准格网化
        /// </summary>
        /// <param name="_newPointSet">离散点云</param>
        /// <returns></returns>
        public ZXPointSet Update(ZXPointSet _newPointSet)
        {

            for (int i = 0; i < _newPointSet.Count; i++)
            {
                ZXPoint p = _newPointSet[i];

                // STEP 1: 根据X,Y坐标找到对应下标
                int index = GetIndex(p.X, p.Y);
                if (index < 0 || index >= this.Count)
                {
                    continue;
                }

                // STEP 2: 高程赋值
                this.m_points[index].Z = p.Z;
            }

            return this;
        }

        /// <summary>
        /// 整块化
        /// 使用前提：格网化有序点云
        /// </summary>
        /// <param name="_unit">整块化精度</param>
        /// <returns></returns>
        [Obsolete]
        public ZXPointSet Blocking(float _unit)
        {
            // STEP 0: 参数校验
            if (_unit <= this.Unit)
            {
                LibTool.Error("ZXPointSet_1438: 整块化精度参数不能小于当前点云精度 " + _unit + " <= " + this.Unit);
            }

            if (_unit / this.Unit * 10 % 10 != 0)
            {
                LibTool.Error("ZXPointSet_1444: 整块化精度参数" + _unit + "必须为当前点云精度的整数倍 " + this.Unit);
            }

            int n = (int)(_unit / this.Unit); // 倍率

            ZXPointSet newPointSet = new ZXPointSet();
            newPointSet.Unit = _unit;
            for (int i = 0; i < this.LengthN; i += n)
            {
                for (int j = 0; j < this.WidthN; j += n)
                {
                    // 对 n x n 个点高度进行求和
                    float sum = 0;

                    ZXPoint p = this.GetPoint(this.GetIndex(i, j));
                    for (int k = 0; k < n; k++)
                    {
                        for (int l = 0; l < n; l++)
                        {
                            int index = this.GetIndex(i, j) + k * n * this.WidthN + l;
                            if (index >= this.Count)
                            {
                                continue;
                            }

                            sum += this[index].Z;
                        }
                    }
                    //Sanngoku: 目的？
                    float avg = sum / (n * n);
                    newPointSet.Add(p.X, p.Y, avg);
                }
            }

            this.m_points.Clear();
            this.m_points = newPointSet.m_points;
            this.Unit = _unit; // 更新精度
            this.Bound = this.Boundary; // 重新计算边界

            return this;
        }

        /// <summary>
        /// 格网化
        /// </summary>
        /// <param name="_unit">精度</param>
        /// <param name="inverse">反向排序 默认为false </param>
        /// <returns>格网化后点</returns>
        public ZXPointSet Gridding(float _unit, bool inverse = false)
        {
            this.Unit = _unit;
            this.Bound = this.Boundary; // 重新计算边界

            foreach (var p in m_points)
            {
                int iX = (int)Math.Round(p.X / _unit);
                int iY = (int)Math.Round(p.Y / _unit);

                p.X = iX * _unit;
                p.Y = iY * _unit;
            }

            // 按默认方式排序
            this.Sort(inverse);

            // 去重
            // inverse = false  保留z最小值
            // inverse = true   保留z最大值
            this.Removal();

            this.Bound = this.Boundary; // 重新计算边界

            return this;
        }

        /// <summary>
        /// 三维格网化：上下重复点保留
        /// </summary>
        /// <param name="_unit">精度</param>
        /// <returns>格网化后点</returns>
        public ZXPointSet GriddingXYZ(float _unit)
        {
            foreach (var p in m_points)
            {
                int iX = (int)Math.Round(p.X / _unit);
                int iY = (int)Math.Round(p.Y / _unit);
                int iZ = (int)Math.Round(p.Z / _unit);

                p.X = iX * _unit;
                p.Y = iY * _unit;
                p.Z = iZ * _unit;
            }

            // 按默认方式排序
            this.Sort();

            // 去重，保留z最小值
            this.RemovalXYZ();

            return this;
        }

        /// <summary>
        /// 对点进行排序
        /// </summary>
        /// <returns></returns>
        public ZXPointSet Sort(bool inverse = false)
        {
            // 按默认方式排序
            if (!inverse)
            {
                m_points.Sort((p0, p1) => p0.CompareTo(p1));
            }
            else
            {
                m_points.Sort((p0, p1) => p1.CompareTo(p0));
            }


            return this;
        }

        /// <summary>
        /// 按ZXY顺序排序
        /// </summary>
        /// <returns></returns>
        public ZXPointSet SortByZXY(bool inverse = false)
        {
            if (inverse)
            {
                m_points.Sort((p0, p1) => p1.CompareToByZXY(p0));
            }
            else
            {
                m_points.Sort((p0, p1) => p0.CompareToByZXY(p1));
            }

            return this;
        }

        /// <summary>
        /// 按Alfa排序 202
        /// </summary>
        /// <returns></returns>
        public ZXPointSet SortByAlfa()
        {
            this.ComputeAlfa();

            m_points.Sort((p0, p1) => p0.CompareToByAlfa(p1));

            return this;
        }

        /// <summary>
        /// 排序(任意比较形式)
        /// </summary>
        /// <param name="comparison">比较方法</param>
        /// <returns>排序后的点云</returns>
        public ZXPointSet Sort(Comparison<ZXPoint> comparison)
        {
            m_points.Sort(comparison);
            return this;
        }

        /// <summary>
        /// 加密（使用条件：按Alfa排序）
        /// </summary>
        /// <param name="p_d"></param>
        /// <returns></returns>
        public ZXPointSet Dense(float p_d = 0.1f)
        {
            ZXPointSet psAdd = new ZXPointSet();
            for (int i = 0; i < this.Count - 1; i++)
            {
                if (this[i].DistanceTo(this[i + 1]) > p_d)
                {
                    ZXPoint p = new ZXPoint((this[i].X + this[i + 1].X) * 0.5f, (this[i].Y + this[i + 1].Y) * 0.5f, (this[i].Z + this[i + 1].Z) * 0.5f);
                    psAdd.Add(p);
                }
            }
            this.Merge(psAdd);
            this.SortByAlfa();

            return this;
        }

        /// <summary>
        /// 删除重复点（使用条件：按Alfa排序）
        /// </summary>
        /// <param name="p_d"></param>
        /// <returns></returns>
        public ZXPointSet RemoveDuplicate(float p_d = 0.1f)
        {
            // 2025.10.13 duzixi +
            if (this.Count == 0)
            {
                return this; 
            }

            ZXPointSet psSelected = new ZXPointSet();
            psSelected.Add(this[0]);
            int lastIndex = 0;
            for (int i = 0; i < this.Count - 1; i++)
            {
                if (this[i + 1].DistanceTo(this[lastIndex]) > p_d)
                {
                    psSelected.Add(this[i + 1]);
                    lastIndex = i + 1;
                }
            }

            this.m_points = psSelected.m_points;

            return this;
        }

        /// <summary>
        /// 缩放，以圆心为中心缩放点云 2024.04.09 +
        /// </summary>
        /// <param name="p_rateX"></param>
        /// <param name="p_rateY"></param>
        /// <param name="p_rateZ"></param>
        /// <returns></returns>
        public ZXPointSet Scale(float p_rateX, float p_rateY, float p_rateZ)
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].X *= p_rateX;
                this[i].Y *= p_rateY;
                this[i].Z *= p_rateZ;
            }

            return this;
        }


        #endregion

        #region 查（搜索类算法）

        /// <summary>
        /// 判断两个点集是否相交
        /// </summary>
        /// <param name="otherPS">另一个点集</param>
        /// <param name="pi">交点</param>
        /// <param name="pj">交点</param>
        /// <param name="p_d">点距小于该值，视为相交</param>
        /// <returns></returns>
        public bool Intersect(ZXPointSet otherPS, ref ZXPoint pi, ref ZXPoint pj, float p_d = 0.05f)
        {
            for (int i = 0; i < this.Count; i++)
            {
                for (int j = 0; j < otherPS.Count; j++)
                {
                    float d = this[i].DistanceTo(otherPS[j]);
                    if (d < p_d)
                    {
                        pi = this[i];
                        pj = otherPS[j];
                        return true;
                    }
                }
            }
            return false;

        }

        /// <summary>
        /// 判断两个点集是否相交
        /// </summary>
        /// <param name="otherPS">另一个点集</param>
        /// <param name="p_d">点距小于该值，视为相交</param>
        /// <returns></returns>
        public bool Intersect(ZXPointSet otherPS, float p_d = 0.05f)
        {
            ZXPoint p0 = new ZXPoint();
            ZXPoint p1 = new ZXPoint();
            return this.Intersect(otherPS, ref p0, ref p1, p_d);
        }

        #endregion

        #region IList

        private List<ZXPoint> m_points;

        /// <summary>
        /// IList：点数
        /// </summary>
        public int Count
        {
            get
            {
                return m_points.Count;
            }
        }

        /// <summary>
        /// IList：是否只读
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///  IList：索引下标取点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ZXPoint this[int index]
        {
            get
            {
                if (index >= m_points.Count)
                {
                    LibTool.Error("ZXPointSet_1551: this[index]下标越界 index = " + index + " m_points.Count = " + m_points.Count);
                }
                return m_points[index];
            }

            set
            {
                m_points[index] = value;
            }
        }

        /// <summary>
        ///  IList：返回点下标
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(ZXPoint item)
        {
            return m_points.IndexOf(item);
        }

        /// <summary>
        ///  IList：在指定下标处插入点
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, ZXPoint item)
        {
            m_points.Insert(index, item);
        }

        /// <summary>
        ///  IList：删除指定下标处点
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            m_points.RemoveAt(index);
        }

        /// <summary>
        ///  IList：在List最后添加点
        /// </summary>
        /// <param name="item"></param>
        public void Add(ZXPoint item)
        {
            m_points.Add(item);
        }

        /// <summary>
        ///  IList：清空List
        /// </summary>
        public void Clear()
        {
            m_points.Clear();
        }

        /// <summary>
        ///  IList：判断是否包含点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ZXPoint item)
        {
            return m_points.Contains(item);
        }

        /// <summary>
        ///  IList：拷贝？？？
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(ZXPoint[] array, int arrayIndex)
        {
            m_points.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///  IList：删除点
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(ZXPoint item)
        {
            return m_points.Remove(item);
        }

        /// <summary>
        ///  IList：获取枚举器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ZXPoint> GetEnumerator()
        {
            return m_points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_points.GetEnumerator();
        }

        #endregion

        #region 形态学操作

        /// <summary>
        /// 膨胀
        /// </summary>
        /// <param name="p_r">膨胀序号半径</param>
        /// <returns></returns>
        public ZXPointSet Expand(int p_r)
        {
            // STEP -1: 转换成image-structure
            ZXPointSet psFull = new ZXPointSet();
            this.TurnToFull(ref psFull, float.MinValue);

            // STEP 0: 构建膨胀后图像
            ZXPointSet result = new ZXPointSet();
            for (int i = 0; i < psFull.LengthN; i++)
            {
                for (int j = 0; j < psFull.WidthN; j++)
                {
                    float x = psFull.Bound.MinX + i * this.Unit;
                    float y = psFull.Bound.MinY + j * this.Unit;

                    // STEP 0.1: 搜索邻域Z值
                    List<float> nearZ = new List<float>();
                    for (int ii = -p_r; ii < p_r + 1; ii++)
                    {
                        for (int jj = -p_r; jj < p_r + 1; jj++)
                        {
                            int p_i = i + ii;
                            int p_j = j + jj;

                            if (p_i < 0 || p_j < 0 || p_i >= this.LengthN || p_j >= this.WidthN)
                            {
                                nearZ.Add(float.MinValue);
                            }
                            else
                            {
                                float z = psFull.Get(p_i, p_j).Z;
                                nearZ.Add(z);
                            }
                        }
                    }

                    // STEP 0.2: 查找邻域Z值中的最大值
                    float maxValue = float.MinValue;
                    for (int k = 0; k < nearZ.Count; k++)
                    {
                        if (nearZ[k] > maxValue)
                        {
                            maxValue = nearZ[k];
                        }
                    }
                    // STEP 0.3: 校验值合法性
                    if (maxValue > float.MinValue)
                    {
                        result.Add(new ZXPoint(x, y, maxValue));
                    }
                }
            }

            result.RemoveNull();
            return result;
        }

        /// <summary>
        /// 腐蚀
        /// </summary>
        /// <param name="p_r">腐蚀序号半径</param>
        /// <returns></returns>
        public ZXPointSet Corrosion(int p_r)
        {
            // STEP 0: 转换成image-structure
            ZXPointSet psFull = new ZXPointSet();
            this.TurnToFull(ref psFull, float.MinValue);

            // STEP 1: 构建腐蚀后图像
            ZXPointSet result = new ZXPointSet();
            for (int i = 0; i < psFull.LengthN; i++)
            {
                for (int j = 0; j < psFull.WidthN; j++)
                {
                    int index = i * psFull.WidthN + j;

                    if (psFull[index].IsEmpty(-99999))
                    {
                        continue;
                    }

                    float x = psFull.Bound.MinX + i * this.Unit;
                    float y = psFull.Bound.MinY + j * this.Unit;

                    // STEP 2.1: 横向检测
                    int counterX = 0;
                    for (int ii = -p_r; ii < p_r + 1; ii++)
                    {
                        int p_i = i + ii;
                        int p_j = j;
                        if (p_i < 0 || p_j < 0 || p_i >= this.LengthN || p_j >= this.WidthN)
                        {
                            // 越界忽略
                        }
                        else
                        {
                            if (psFull.Get(p_i, p_j).Z > -9000)
                            {
                                counterX++;
                            }
                        }
                    }

                    // STEP 2.2: 纵向检测
                    int counterY = 0;
                    for (int jj = -p_r; jj < p_r + 1; jj++)
                    {
                        int p_i = i;
                        int p_j = j + jj;

                        if (p_i < 0 || p_j < 0 || p_i >= this.LengthN || p_j >= this.WidthN)
                        {
                            // 越界忽略
                        }
                        else
                        {
                            if (psFull.Get(p_i, p_j).Z > -9000)
                            {
                                counterY++;
                            }
                        }
                    }

                    if (counterX != p_r && counterY != p_r || counterX + counterY >= 7)
                    {
                        result.Add(psFull[index]);
                    }
                }
            }
            this.m_points = result.m_points;
            return result;
        }

        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="condition">二值化条件</param>
        /// <param name="isNegativeRemain">是否保留0</param>
        /// <returns></returns>
        public ZXPointSet Binarization(Func<ZXPoint, bool> condition, bool isNegativeRemain)
        {
            ZXPointSet result = new ZXPointSet();
            for(int i = 0;i < this.Count;i++)
            {
                if(condition(this[i]))
                {
                    result.Add(this[i].X, this[i].Y, 1);
                }
                else
                {
                    if (isNegativeRemain)
                    {
                        result.Add(this[i].X, this[i].Y, 0);
                    }
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 显示点云信息
        /// </summary>
        /// <returns>点坐标字符串</returns>
        public override string ToString()
        {
            return "点集 " + Bound.ToString() + " 格网精度：" + Unit + " 点数：" + Count;
        }
    }
}
