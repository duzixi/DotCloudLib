// ====================================================================
// 模块名称：RGB色彩模型 CCTRGB
// 功能简介：RGB色彩模型存储与处理
// 使用环境：Winform平台（控制台不好使）
// 版权声明：(C) 2019 九州创智  All Rights Reserved.
// 更新履历：2020.08.04 杜子兮 引入DotCloudLib库配套工程CreateImage
//======================================================================

using System;
using System.Drawing;

namespace CCTColor
{
    class CCTRGB
    {
        // 取值：0.0 ~ 1.0
        float R { get; set; } 
        float G { get; set; }
        float B { get; set; }


        public CCTRGB(float _R, float _G, float _B)
        {
            R = _R;
            G = _G;
            B = _B;
        }

        private bool IsEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.00001;
        }

        public CCTHSV ToHSV()
        {
            float max = R > G ? (R > B ? R : B) : (G > B ? G : B);
            float min = R < G ? (R < B ? R : B) : (G < B ? G : B);

            // STEP 1 计算色相H
            float h = 0; // 色相
            if (Math.Equals(max, min))
            {
                h = 0;
            }
            else if (IsEqual(max, R) && G >= B)
            {
                h = 60 * (G - B) / (max - min);
            }
            else if (IsEqual(max, R) && G < B)
            {
                h = 60 * (G - B) / (max - min) + 360;
            }
            else if (IsEqual(max, G))
            {
                h = 60 * (B - R) / (max - min) + 120;
            }
            else if (IsEqual(max, B))
            {
                h = 60 * (R - G) / (max - min) + 240;
            }
            h /= 360.0f;

            // STEP 2: 计算S
            float s = 0;
            if (!IsEqual(max, 0))
            {
                s = 1 - min / max;
            }

            // STEP 3: 计算V
            float v = max;

            return new CCTHSV(h, s, v);
        }

        public Color ToColor()
        {
            return Color.FromArgb(255, (int)(R * 255), (int)(G * 255),  (int)(B * 255));
        }

    }
}
