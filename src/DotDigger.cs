//=====================================================================
// 模块名称：取料机构 DotDigger
// 功能简介：各种取料头的基类
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.8.19 杜子兮 创建
//============================================

using GeoLib;

namespace DotCloudLib
{
    /// <summary>
    /// 取料机构点云
    /// </summary>
    public class DotDigger
    {
        /// <summary>
        /// 类型枚举
        /// </summary>
        public enum DiggerType
        {
            /// <summary>
            /// 自定义任意形状
            /// </summary>
            Custom,
            /// <summary>
            /// 斗轮
            /// </summary>
            Wheel,
            /// <summary>
            /// 链斗
            /// </summary>
            Chain,
            /// <summary>
            /// 抓斗
            /// </summary>
            Grab,
            /// <summary>
            /// 长方形
            /// </summary>
            Box
        }

        private DiggerType m_type; // 类型

        private ZXPointSet m_points; // 存储取料机构基准位姿形状的点云（每种都约定好）

        /// <summary>
        /// 取料机构点云模型
        /// </summary>
        public ZXPointSet Points
        {
            get => m_points;
        }

        private ZXPoint m_position;

        /// <summary>
        /// 中心点（基准点位置）
        /// </summary>
        public ZXPoint Position { get => m_position; set => m_position = value; }
        
        private float m_alfa;
        private float m_beta;
        private float m_gama;
        /// <summary>
        /// 第一旋转角（单位：角度。与设备类型相关）
        /// </summary>
        public float Alfa { get => m_alfa; set => m_alfa = value; }
        /// <summary>
        /// 第二旋转角（单位：角度。与设备类型相关）
        /// </summary>
        public float Beta { get => m_beta; set => m_beta = value; }
        /// <summary>
        /// 第三旋转角（单位：角度。与设备类型相关）
        /// </summary>
        public float Gama { get => m_gama; set => m_gama = value; }

        // 注意：以上三个旋转角不是按欧拉角定义，而是按机械旋转机构从属关系定义

        /// <summary>
        /// 设置斗轮位姿
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="alfa"></param>
        public void SetPose(float x, float y, float z, float alfa)
        {
            Position = new ZXPoint(x, y, z);
            Alfa = -alfa; // 【未完待续】 根据不同料堆
        }

        /// <summary>
        /// 获取全局点坐标
        /// </summary>
        /// <returns></returns>
        public ZXPointSet GetGlobalPoints()
        {
            ZXPointSet ps = new ZXPointSet();
            for (int i = 0; i < m_points.Count; i++)
            {
                ps.Add(m_points[i].X, m_points[i].Y, m_points[i].Z);
            }

            ps.Rotate(AxisType.Z, Alfa); // 未完待续
            ps.Translate(Position.X, Position.Y, Position.Z);
            return ps;
        }

        /// <summary>
        /// 构造方法——用点云定义任意形态的挖掘机构
        /// </summary>
        /// <param name="p_pointSet">点云定义的任意形态</param>
        public DotDigger(ZXPointSet p_pointSet)
        {
            m_points = p_pointSet;
            m_type = DiggerType.Custom;

            Position = new ZXPoint(0, 0, 0);
            Alfa = 0;
            Beta = 0;
            Gama = 0;
        }

        /// <summary>
        /// 构造方法——根据本地点云文件创建取料机构对象模型
        /// </summary>
        /// <param name="p_filePath"></param>
        public DotDigger(string p_filePath)
        {
            m_type = DiggerType.Custom;

            ZXPointSet psGrab = new ZXPointSet();
            psGrab.LoadFromXYZ(p_filePath);
            m_points = psGrab;

            Position = new ZXPoint(0, 0, 0);
            Alfa = 0;
            Beta = 0;
            Gama = 0;
        }

        /// <summary>
        /// 构造方法——根据类型定义取料机构
        /// </summary>
        /// <param name="type">取料机构类型</param>
        /// <param name="p_unit">格网精度</param>
        /// <param name="p_params">参数列表 斗轮[半径，宽度]</param>
        public DotDigger(DiggerType type, float p_unit, params float[] p_params )
        {
            m_type = type;
            switch (m_type)
            {
                case DiggerType.Wheel:
                    {
                        if (p_params.Length != 2)
                        {
                            LibTool.Error("DotDigger_108: 斗轮取料机构参数个数必须为2个 斗轮半径、斗轮宽度");
                            return;
                        }
                        float r = p_params[0]; // 斗轮半径 
                        float w = p_params[1]; // 斗轮宽度
                        CylinderGeometry cylinder = new CylinderGeometry(
                            new PointModel(0, 0, 0),
                            new AngleModel(0, 0, 0), 
                            new CylinderSize( r,  w ));
                        cylinder.CreateCylinderKeyPoint(p_unit);
                        m_points = Parser.GetKeyPoints(cylinder);
                        m_points.Rotate(AxisType.X, 90);

                        ZXPointSet ps = new ZXPointSet();

                        // 去掉立面的点
                        foreach (ZXPoint p in m_points)
                        {
                            if (p.X * p.X + p.Z * p.Z >= (r * r - p_unit ))
                            {
                                ps.Add(p);
                            }
                        }

                        m_points = ps;
                    }
                    break;
                case DiggerType.Box:
                    {
                        if (p_params.Length != 3) return;
                        CuboidGeometry cuboid = new CuboidGeometry(                            
                            new PointModel(0, 0, 0),
                            new AngleModel(0,0,0),  
                            new CuboidSize(p_params[0], p_params[1], p_params[2]));
                        cuboid.CreateCuboidKeyPoint(p_unit); 
                        m_points = Parser.GetKeyPoints(cuboid);
                    }
                    break;
                
                case DiggerType.Chain:
                    {
                        if (p_params.Length != 4)
                        {
                            LibTool.Error("DotDigger_202: 链斗参数个数必须为4个：伸缩长度、端部余量、根部余量、取料头宽度");
                            return;
                        }
                        float headOut = p_params[0]; // 伸缩长度
                        float d1 = p_params[1]; // 端部余量
                        float d2 = p_params[2]; // 根部余量
                        float w = p_params[3]; // 取料头宽度
                        LibTool.Debug("    伸缩长度：" + headOut + " 端部余量：" + d1 + " 根部余量：" + d2 + " 取料头宽度："+ w);

                        float maxD = (d2 > d1) ? d2 : d1;

                        // STEP 1: 根部
                        CylinderGeometry cylinder1 = new CylinderGeometry(
                            new PointModel(0, 0, 0), new AngleModel(0, 0, 0), new CylinderSize(d2, w));
                        cylinder1.CreateCylinderKeyPoint(p_unit * 0.5f);
                        ZXPointSet ps1 = Parser.GetKeyPoints(cylinder1);
                        ps1.Rotate(AxisType.X, 90);
                        ps1.Translate(0, 0, d2);  // 只要左下角
                        ZXBoundary b1 = ps1.Boundary;
                        b1.MaxX = b1.MaxX - d2;
                        b1.MaxZ = d2;
                        ps1.Intercept(b1);

                        // STEP 2: 端部
                        CylinderGeometry cylinder2 = new CylinderGeometry(
                            new PointModel(0, 0, 0), new AngleModel(0, 0, 0), new CylinderSize(d1, w));
                        cylinder2.CreateCylinderKeyPoint(p_unit * 0.5f);
                        ZXPointSet ps2 = Parser.GetKeyPoints(cylinder2);
                        ps2.Rotate(AxisType.X, 90);
                        ps2.Translate(headOut, 0, d1);  // 只要右下角
                        ZXBoundary b2 = ps2.Boundary;
                        b2.MinX = b2.MinX + d1;
                        b2.MaxZ = d1;
                        ps2.Intercept(b2);

                        // STEP 3: 连接部分
                        CuboidGeometry cuboid = new CuboidGeometry(
                            new PointModel(0, 0, 0), new AngleModel(0, 0, 0), new CuboidSize(headOut, w, maxD));
                        cuboid.CreateCuboidKeyPoint(p_unit * 0.5f);
                        ZXPointSet ps3 = Parser.GetKeyPoints(cuboid);
                        ps3.Translate(headOut * 0.5f, 0, maxD * 0.5f);
                        ZXBoundary b3 = ps3.Boundary;
                        b3.MaxZ = b3.MinZ + 0.05f;
                        ps3.Intercept(b3);

                        m_points = ps1;
                        m_points.Merge(ps2);
                        m_points.Merge(ps3);

                        // STEP 4: 切除
                        ZXBoundary bROI = m_points.Boundary;
                        bROI.MaxZ = maxD;
                        bROI.MinY += 0.1f;
                        bROI.MaxY -= 0.1f;
                        m_points.Intercept(bROI);
                        // m_points.Gridding(0.1f); // 原始点要密集一些
                    }
                    break;
                case DiggerType.Grab:
                    break;
                default:
                    break;
            }
        }


    }
}
