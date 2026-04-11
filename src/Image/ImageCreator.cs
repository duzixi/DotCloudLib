//=====================================================================
// 模块名称：图像生成器 ImageCreator
// 功能简介：生成图像文件
// 使用条件：Winform平台（控制台不好使）
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 更新履历：2020.07.24 杜子兮 创建
//          2020.08.04 杜子兮 追加png图像信息头；高程着色生成器
//          2020.08.05 杜子兮 法线贴图生成器
//          2025.03.04 杜子兮 注释修改  
//======================================================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using CCTColor;
using DotCloudLib;
using Mathd;

namespace CreateImage
{
    /// <summary>
    /// 高程颜色选择
    /// </summary>
    public enum HeatPecker
    {
        /// <summary>
        /// 自动
        /// </summary>
        AUTO = 0,
        /// <summary>
        /// 手动设置，此时使用ImageCreator.MaxZ和ImageCreator.MinZ
        /// </summary>
        MANUAL = 1
    }

    /// <summary>
    /// 图片生成器
    /// </summary>
    public static class ImageCreator
    {
        /// <summary>
        /// 设定值：高程图最高高度（默认25米）
        /// </summary>
        public static double MaxZ = 0d;

        public static double MinZ = -15;

        public static HeatPecker heatPecker = HeatPecker.AUTO;

        /// <summary>
        /// 设定值：最高值对应的HSV颜色 H值
        /// </summary>
        public static int MaxH = 260;


        /// <summary>
        /// 生成料条等高线图片
        /// </summary>
        /// <param name="pileLine"></param>
        /// <param name="_filePath"></param>
        /// <param name="_addInfo"></param>
        public static void Contour(DotPileLine pileLine, string _filePath, bool _addInfo = false)
        {
            //pileLine.Points.Sort();
            pileLine.Points.Gridding(pileLine.Points.Unit);
            ZXPointSet fullPoints = new ZXPointSet();
            pileLine.Points.TurnToFull(ref fullPoints, float.NaN);

            // 一分米一个点
            int imgL = (int)Math.Round(pileLine.Bound.L * 10) + 1;
            int imgW = (int)Math.Round(pileLine.Bound.W * 10) + 1;

            Bitmap img = new Bitmap(imgL, imgW, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // X方向一米遍历
            for (int i = 0; i < imgL; i++)
            {
                // Y方向一米遍历
                for (int j = 0; j < imgW; j++)
                {
                    // STEP 0: 获取采样点index
                    int index = i * imgW + j;
                    ZXPoint p = fullPoints[index];

                    if ((p.Z - pileLine.Bound.MinZ) < 0.2f)
                    {
                        img.SetPixel(i, imgW - j - 1, Color.FromArgb(0, 239, 239, 239));
                    } else

                    // STEP 1: 根据高度计算颜色
                    if ((p.Z - pileLine.Bound.MinZ) * 10 % 10 <= 2.01f  )
                    {
                        // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                        int colorValue;
                        if (heatPecker == HeatPecker.AUTO)
                        {
                            // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                            colorValue = MaxH - (int)Math.Round(MaxH / (pileLine.Bound.MaxZ - pileLine.Bound.MinZ) * (p.Z - pileLine.Bound.MinZ));
                        }
                        else
                        {
                            // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                            colorValue = MaxH - (int)Math.Round(MaxH / (MaxZ - MinZ) * (p.Z - MinZ));
                        }

                        // STEP 2: 高程值映射HSV
                        CCTHSV hsvColor = new CCTHSV(colorValue, 1, 1);
                        Color c = hsvColor.ToRGB().ToColor();

                        // STEP 3: 上色
                        img.SetPixel(i, imgW - j - 1, c);
                    } else
                    {
                        img.SetPixel(i, imgW - j - 1, Color.FromArgb(239, 239, 239));
                        // img.SetPixel(i, imgW - j - 1, Color.White);
                        // img.SetPixel(i, imgW - j - 1, Color.FromArgb(12, 16, 26));
                    }
                }
            }

            img.Save(_filePath.Split('.')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            if (_addInfo)
            {
                AddPileLineInfoToPng(_filePath.Split('.')[0] + ".png", pileLine, ImageCreator.MaxZ, "Contour");
            }
        }
        
        /// <summary>
        /// 等高线
        /// </summary>
        /// <param name="pile">料型</param>
        /// <param name="_filePath">点云文件路径</param>
        /// <param name="_addInfo">图片附加信息</param>
        public static void Contour(DotPile pile, string _filePath, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(pile);
            Contour(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 等高线
        /// </summary>
        /// <param name="_filePath">原始格网化点云文件路径</param>
        /// <param name="_format">点格式</param>
        /// <param name="_addInfo">图片附加信息</param>
        public static void Contour(string _filePath, PointFormat _format, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(_filePath, _format);
            Contour(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 生成低精度灰度压缩图（料条DotPileLine）
        /// </summary>
        /// <param name="pileLine">料条</param>
        /// <param name="_filePath">原始格网化点云文件路径</param>
        /// <param name="_addInfo">图片附加信息</param>
        public static void GrayColor(DotPileLine pileLine, string _filePath, bool _addInfo = false)
        {
            pileLine.Points.Sort();
            int imgL = (int)Math.Round(pileLine.Bound.L * 10) + 1;
            int imgW = (int)Math.Round(pileLine.Bound.W * 10) + 1;
            Bitmap img = new Bitmap(imgL, imgW, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // X方向一米遍历
            for (int i = 0; i < imgL; i++)
            {
                // Y方向一米遍历
                for (int j = imgW - 1; j >= 0; j--)
                {
                    // STEP 0: 获取采样点index
                    int index = i * imgW + j;
                    ZXPoint p = pileLine.Points[index];

                    // STEP 1: 高程值映射 p.z -> 0 ~ 256d (0米 ~25米)
                    int colorValue = (int)Math.Round(256d / (MaxZ - MinZ) * (p.Z - MinZ));

                    // STEP 2: 高程值映射RGB
                    int r = colorValue;
                    int g = colorValue;
                    int b = colorValue;

                    // STEP 3: 上色
                    img.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }

            img.Save(_filePath + "_Gray.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        /// <summary>
        /// 生成低精度灰度压缩图（料堆DotPile）
        /// </summary>
        /// <param name="pile">料堆</param>
        /// <param name="_filePath">原始格网化点云文件路径</param>
        /// <param name="_addInfo">图片附加信息</param>
        public static void GrayColor(DotPile pile, string _filePath, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(pile);
            GrayColor(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 生成低精度灰度压缩图（点云xyz文本文件）
        /// </summary>
        /// <param name="_filePath">原始格网化点云文件路径</param>
        /// <param name="_format">点格式</param>
        /// <param name="_addInfo">图片附加信息</param>
        public static void GrayColor(string _filePath, PointFormat _format, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(_filePath, _format);
            GrayColor(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 任意点云生成高程图  2024.04.11
        /// </summary> 
        /// <param name="ps">任意点云</param>
        /// <returns>高程图</returns>
        public static Bitmap HeatColor(ZXPointSet ps)
        {
            ps.Gridding(ps.Unit);
            ZXPointSet fullPoints = new ZXPointSet();
            ps.TurnToFull(ref fullPoints, float.NaN);

            ZXBoundary bImg = ps.Boundary;

            int imgL = (int)Math.Round(fullPoints.Bound.L / ps.Unit) + 1;
            int imgW = (int)Math.Round(fullPoints.Bound.W / ps.Unit) + 1;

            Bitmap img = new Bitmap(imgL, imgW, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // X方向遍历
            for (int i = 0; i < imgL; i++)
            {
                // Y方向遍历
                for (int j = 0; j < imgW; j++)
                {
                    int index = i * imgW + j;

                    if (index < 0 || index >= fullPoints.Count)
                        continue;


                    ZXPoint p = fullPoints[index];

                    if (float.IsNaN(p.Z))
                    {
                        img.SetPixel(i, imgW - j - 1, Color.Transparent);
                    }
                    else
                    {
                        // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                        int colorValue;
                        if (heatPecker == HeatPecker.AUTO)
                        {
                            // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                            colorValue = MaxH - (int)Math.Round(MaxH / (bImg.MaxZ - bImg.MinZ) * (p.Z - bImg.MinZ));
                        }
                        else
                        {
                            // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                            colorValue = MaxH - (int)Math.Round(MaxH / (MaxZ - MinZ) * (p.Z - MinZ));
                        }

                        // STEP 2: 高程值映射HSV
                        CCTHSV hsvColor = new CCTHSV(colorValue, 1, 1);
                        Color c = hsvColor.ToRGB().ToColor();

                        // STEP 3: 上色
                        img.SetPixel(i, imgW - j - 1, c);
                    }
                }
            }

            return img;
        }

        /// <summary>
        /// 中心对齐的高程图
        /// </summary>
        /// <param name="ps">点云</param>
        /// <param name="center">点云中需要与图像中心对齐的点坐标。若center为null，则默认将电源中心与图像中心对齐</param>
        /// <param name="imgW">目标图像高度 - 对应点云y方向</param>
        /// <param name="imgH">目标图像宽度 - 对应点云x方向</param>
        /// <param name="scale">每scale米一个像素点</param>
        /// <returns></returns>
        public static Bitmap HeatColor_Center(ZXPointSet ps, ZXPoint center = null, int imgW = 360, int imgH = 360, float scale = 0.1f)
        {
            ps.Gridding(ps.Unit);
            ZXPointSet fullPoints = new ZXPointSet();
            ps.TurnToFull(ref fullPoints, float.NaN);

            ZXBoundary bImg = ps.Boundary;

            Bitmap img = new Bitmap(imgH, imgW, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int rangeX = (int)Math.Round(fullPoints.Bound.L / ps.Unit) + 1;
            int rangeY = (int)Math.Round(fullPoints.Bound.W / ps.Unit) + 1;

            if (center == null)
            {
                center = bImg.Center;
            }
            int center_x = fullPoints.GetIndexX(center.X);
            int center_y = fullPoints.GetIndexY(center.Y);

            int diameter = (int)(scale / ps.Unit);
            int xOffset = (imgH / 2 - center_x / diameter);
            int yOffset = (imgW / 2 - center_y / diameter);

            // X方向遍历
            for (int i = 0; i < rangeX; i += diameter)
            {
                // Y方向遍历
                for (int j = 0; j < rangeY; j += diameter)
                {
                    int index = i * rangeY + j;

                    if (index < 0 || index >= fullPoints.Count)
                        continue;

                    ZXPoint p = fullPoints[index];

                    int pixel_x = i / diameter + xOffset;
                    int pixel_y = imgW - (j / diameter + yOffset);

                    if (pixel_x < 0 || pixel_y < 0 || pixel_x > imgH - 1 || pixel_y > imgW - 1) continue;

                    float sumZ = 0;
                    int finityCount = 0;
                    for(int ii = i;ii < i + diameter && ii < rangeX; ii++)
                    {
                        for(int jj = j;jj < j + diameter && jj < rangeY; jj++)
                        {
                            ZXPoint pt = fullPoints[ii * rangeY + jj];
                            if (!float.IsNaN(pt.Z))
                            {
                                sumZ += pt.Z;
                                finityCount++;
                            }
                        }
                    }

                    if (finityCount <= 0)
                    {
                        img.SetPixel(pixel_x, pixel_y, Color.Transparent);
                    }
                    else
                    {
                        float avgZ = sumZ / diameter / diameter;
                        int colorValue;
                        if (heatPecker == HeatPecker.AUTO)
                        {
                            // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                            colorValue = MaxH - (int)Math.Round(MaxH / (bImg.MaxZ - bImg.MinZ) * (avgZ - bImg.MinZ));
                        }
                        else
                        {
                            // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                            colorValue = MaxH - (int)Math.Round(MaxH / (MaxZ - MinZ) * (avgZ - MinZ));
                        }

                        // STEP 2: 高程值映射HSV
                        CCTHSV hsvColor = new CCTHSV(colorValue, 1, 1);
                        Color c = hsvColor.ToRGB().ToColor();

                        // STEP 3: 上色
                        img.SetPixel(pixel_x, pixel_y, c);
                    }
                }
            }

            return img;
        }

        /// <summary>
        /// 生成热力图高程着色(料条 DotPileLine)
        /// </summary>
        /// <param name="pileLine">料条</param>
        /// <param name="_filePath">点云文件路径</param>
        /// <param name="_addInfo">图片是否添加辅助信息</param>
        public static void HeatColor(DotPileLine pileLine, string _filePath, bool _addInfo = false)
        {
            pileLine.Points.Sort();
            // 一分米一个点
            int imgL = (int)Math.Round(pileLine.Bound.L * 10) + 1;
            int imgW = (int)Math.Round(pileLine.Bound.W * 10) + 1;

            Bitmap img = new Bitmap(imgL, imgW, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // X方向一米遍历
            for (int i = 0; i < imgL; i++)
            {
                // Y方向一米遍历
                for (int j = 0; j < imgW; j++)
                {
                    // STEP 0: 获取采样点index
                    int index = i * imgW + j;
                    ZXPoint p = pileLine.Points[index];

                    // STEP 1: 高程值映射 p.z -> 0 ~ 300 (0米 ~25米)
                    int colorValue = MaxH - (int)Math.Round(MaxH / MaxZ * p.Z);

                    // STEP 2: 高程值映射HSV
                    CCTHSV hsvColor = new CCTHSV(colorValue, 1, 1);
                    Color c = hsvColor.ToRGB().ToColor();

                    // STEP 3: 上色
                    img.SetPixel(i, imgW - j - 1, c);
                }
            }

            img.Save(_filePath.Split('.')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            if (_addInfo)
            {
                AddPileLineInfoToPng(_filePath.Split('.')[0] + ".png", pileLine, ImageCreator.MaxZ, "HeatColor");
            }
        }

        /// <summary>
        /// 生成热力图高程着色(料堆DotPile)
        /// </summary>
        /// <param name="pile">料堆</param>
        /// <param name="_filePath">点云文件路径</param>
        /// <param name="_addInfo">图片是否添加辅助信息</param>
        public static void HeatColor(DotPile pile, string _filePath, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(pile);
            HeatColor(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 生成热力图高程着色(点云文本文件)
        /// </summary>
        /// <param name="_filePath"></param>
        /// <param name="_format"></param>
        /// <param name="_addInfo">图片是否添加辅助信息</param>
        public static void HeatColor(string _filePath, PointFormat _format, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(_filePath, _format);
            HeatColor(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 生成高精度真彩色压缩图(料条 DotPileLine)
        /// </summary>
        /// <param name="pileLine">料条</param>
        /// <param name="_filePath">点云文件路径</param>
        /// <param name="_addInfo">图片是否添加辅助信息</param>
        public static void TrueColor(DotPileLine pileLine, string _filePath, bool _addInfo = false)
        {
            pileLine.Points.Sort();

            // 一分米一个点
            int imgL = (int)Math.Round(pileLine.Bound.L * 10) + 1;
            int imgW = (int)Math.Round(pileLine.Bound.W * 10) + 1;

            Bitmap img = new Bitmap(imgL, imgW, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // X方向一米遍历
            for (int i = 0; i < imgL; i++)
            {
                // Y方向一米遍历
                for (int j = imgW - 1; j >= 0; j--)
                {
                    // STEP 0: 获取采样点index
                    int index = i * imgW + j;
                    ZXPoint p = pileLine.Points[index];

                    // STEP 1: 高程值映射 p.z -> 0 ~ 256d * 256d * 256d (0米 ~25米)
                    int colorValue = (int)Math.Round(256d * 256d * 256d / MaxZ * p.Z);

                    // STEP 2: 高程值映射RGB
                    int r = colorValue / (256 * 256);
                    int g = colorValue / 256 % 256;
                    int b = colorValue % 256;

                    // STEP 3: 上色
                    img.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }

            img.Save(_filePath + "_True.png", System.Drawing.Imaging.ImageFormat.Png);
            if (_addInfo)
            {
                AddPileLineInfoToPng(_filePath + "_True.png", pileLine, ImageCreator.MaxZ, "TrueColor");
            }
        }

        /// <summary>
        /// 生成高精度真彩色压缩图（料堆 DotPile）
        /// </summary>
        /// <param name="pile">料堆</param>
        /// <param name="_filePath">点云文件路径</param>
        /// <param name="_addInfo">图片是否添加辅助信息</param>
        public static void TrueColor(DotPile pile, string _filePath, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(pile);
            TrueColor(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 生成高精度真彩色压缩图（格网料堆点云文件）
        /// </summary>
        /// <param name="_filePath"></param>
        /// <param name="_format"></param>
        /// <param name="_addInfo">图片是否添加辅助信息</param>
        public static void TrueColor(string _filePath, PointFormat _format, bool _addInfo = false)
        {
            DotPileLine pileLine = new DotPileLine(_filePath, _format);
            TrueColor(pileLine, _filePath, _addInfo);
        }

        /// <summary>
        /// 生成法线贴图*.jpg（料条 DotPileLine）
        /// </summary>
        /// <param name="_filePath"></param>
        public static void NormalMap(DotPileLine pileLine, string _filePath)
        {
            pileLine.Points.Sort();

            int imgL = (int)Math.Round(pileLine.Bound.L * 10) + 1;
            int imgW = (int)Math.Round(pileLine.Bound.W * 10) + 1;

            Bitmap img = new Bitmap(imgL, imgW, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // X方向
            for (int i = 0; i < imgL; i++)
            {
                // Y方向
                for (int j = 0; j < imgW; j++)
                {
                    // STEP 1: 获取当前点
                    uint index = (uint)(i * imgW + j);
                    ZXPoint p = pileLine.Points[(int)index];
                    Vector3d v0 = new Vector3d(p.X, p.Y, p.Z);
                    Vector3d vec = Vector3d.zero;

                    // STEP 2: 获取上下左右点
                    ZXPoint p1 = new ZXPoint(0, 0, 0);
                    ZXPoint p2 = new ZXPoint(0, 0, 0);
                    ZXPoint p3 = new ZXPoint(0, 0, 0);
                    ZXPoint p4 = new ZXPoint(0, 0, 0);
                    pileLine.GetPoint(index, out p1, Dir.Left);
                    pileLine.GetPoint(index, out p2, Dir.Down);
                    pileLine.GetPoint(index, out p3, Dir.Right);
                    pileLine.GetPoint(index, out p4, Dir.Up);

                    Vector3d vLeft = Mathd.Vector3d.left;
                    Vector3d vDown = Mathd.Vector3d.down;
                    Vector3d vRight = Mathd.Vector3d.right;
                    Vector3d vUp = Mathd.Vector3d.up;

                    if (p1 != null)
                        vLeft = new Vector3d(p1.X, p1.Y, p1.Z);

                    if (p2 != null)
                        vDown = new Vector3d(p2.X, p2.Y, p2.Z);

                    if (p3 != null)
                        vRight = new Vector3d(p3.X, p3.Y, p3.Z);

                    if (p4 != null)
                        vUp = new Vector3d(p4.X, p4.Y, p4.Z);

                    // STEP 3: 累加四个方向叉乘
                    if (i > 0 && j > 0)
                        vec += Vector3d.Cross(vLeft - v0, vDown - v0);
                    if (j > 0 && i < imgL - 1)
                        vec += Vector3d.Cross(vDown - v0, vRight - v0);
                    if (i < imgL - 1 && j < imgW - 1)
                        vec += Vector3d.Cross(vRight - v0, vUp - v0);
                    if (j < imgW - 1 && i > 0)
                        vec += Vector3d.Cross(vUp - v0, vLeft - v0);

                    // STEP 4: 根据叉乘求法线向量
                    vec.Normalize();
                    vec = (Vector3d.one + vec) / 2.0f; // 注意，这里要除以2
                    

                    // STEP 5: 上色！
                    img.SetPixel(i, imgW - j - 1,
                        Color.FromArgb((int)(vec.x * 255), (int)(vec.y * 255), (int)(vec.z * 255)));
                }
            }

            // STEP 6: 保存图片
            string fileName = _filePath.Split('.')[0] + "_n.jpg";
            LibTool.Debug(fileName);
            img.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        /// <summary>
        /// 生成法线贴图*.jpg（料堆 DotPile）
        /// </summary>
        /// <param name="_filePath"></param>
        /// <param name="_format"></param>
        public static void NormalMap(DotPile pile, string _filePath)
        {
            DotPileLine pileLine = new DotPileLine(pile);
            NormalMap(pileLine, _filePath);
        }

        /// <summary>
        /// 生成法线贴图*.jpg（料堆格网点云文本文件）
        /// </summary>
        /// <param name="_filePath"></param>
        /// <param name="_format"></param>
        public static void NormalMap(string _filePath, PointFormat _format)
        {
            DotPileLine pileLine = new DotPileLine(_filePath, _format);
            NormalMap(pileLine, _filePath);
        }

        /// <summary>
        /// 给指定料条图片添加信息头
        /// </summary>
        /// <param name="_filePath"></param>
        /// <param name="_pileLine"></param>
        /// <param name="_maxZ"></param>
        /// <param name="_colorStyle"></param>
        public static void AddPileLineInfoToPng(string _filePath, DotPileLine _pileLine, double _maxZ, string _colorStyle)
        {
            // STEP 1: 读入文件二进制流
            List<byte> bytes = File.ReadAllBytes(_filePath).ToList();

            int index = 8;                      //png前8个字节是固定的
            int start = 0;                      //数据开始的下标

            while (index < bytes.Count)
            {
                int count = BytesToint(bytes, index);           //获取数据块长度
                string name = Bytes4ToString(bytes, index + 4); //获取数据块名称

                if (name == "iCCP") //如果原图片中有iCCP，删除原来的部分写入隐藏的数据
                {
                    bytes.RemoveRange(index, count + 4 + 4 + 4);
                    start = index;
                    break;
                }
                if (name == "IDAT") //读到IDAT前没有iCCP，则写到IDAT之前
                {
                    start = index;
                    break;
                }
                index += count + 4 + 4 + 4; //4个长度 4个名称 4个CRC
            }

            // STEP 2: 生成添加信息
            // 【未完待续】去第三方库
            /*
            JObject json = new JObject();
            json["id"] = "WKA";  // 临时
            json["oX"] = _pileLine.Bound.MinX;
            json["oY"] = _pileLine.Bound.MinY;
            json["oZ"] = _pileLine.Bound.MinZ;
            json["maxZ"] = _maxZ;
            json["colorStyle"] = _colorStyle;
            json["length"] = (_pileLine.LengthN - 1) * _pileLine.Unit;
            json["width"] = (_pileLine.WidthN - 1) * _pileLine.Unit;  // 米数
            json["ver"] = MetaData.VERSION;
            */

            // string jsonString = JsonConvert.SerializeObject(json,Formatting.None); // 埋入得信息

            string jsonString = "";

            // STEP 3: 添加数据
            List<byte> databyte = Encoding.Default.GetBytes(jsonString).ToList();
            List<byte> bytesLength = intToBytes(databyte.Count).ToList();

            //这4个字节是iCCP
            bytesLength.Add(0x69);
            bytesLength.Add(0x43);
            bytesLength.Add(0x43);
            bytesLength.Add(0x50);

            //写入数据（可加密）
            bytesLength.AddRange(databyte);

            //这4个字节是CRC
            bytesLength.Add(0x25);
            bytesLength.Add(0xD2);
            bytesLength.Add(0x9F);
            bytesLength.Add(0x33);

            //插入到原PNG中
            bytes.InsertRange(start, bytesLength);

            //重新写入文件
            File.WriteAllBytes(_filePath, bytes.ToArray());

        }
        
        private static byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);//高8位
            src[3] = (byte)(value & 0xFF);//低位
            return src;
        }

        private static int BytesToint(List<byte> value, int index = 0)
        {
            int i0 = value[index + 3] & 0xFF;
            int i1 = (value[index + 2] & 0xFF) << 8;
            int i2 = (value[index + 1] & 0xFF) << 16;
            int i3 = (value[index + 0] & 0xFF) << 24;
            return i0 | i1 | i2 | i3;
        }

        private static string Bytes4ToString(List<byte> value, int index = 0)
        {
            return Encoding.ASCII.GetString(new byte[] { value[index + 0], value[index + 1], value[index + 2], value[index + 3] });
        }

    }
}
