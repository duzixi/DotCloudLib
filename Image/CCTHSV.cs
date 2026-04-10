// ====================================================================
// 模块名称：HSV色彩模型 CCTHSV
// 功能简介：HSV色彩模型存储与处理
// 使用条件：CCTRGB
// 版权声明：(C) 2019 九州创智  All Rights Reserved.
// 更新履历：2020.08.04 杜子兮 引入DotCloudLib库配套工程CreateImage
//======================================================================

namespace CCTColor
{
    class CCTHSV
    {
        float H { get; set; } // Hue 色相 0 ~ 360
        float S { get; set; } // Saturation 饱和度 0 ~ 1
        float V { get; set; } // Value 亮度 0 ~ 1


        public CCTHSV (float _H, float _S, float _V)
        {
            H = _H > 359 ? 359 : _H < 0 ? 0 : _H;
            S = _S > 1 ? 1 : _S < 0 ? 0 : _S;
            V = _V > 1 ? 1 : _V < 0 ? 0 : _V;
        }

        public CCTRGB ToRGB()
        {
            // 运算量：每个颜色5行代码量
            int i = (int)(H / 60.0f) % 6;
            float f = (H / 60.0f) - i;
            float p = V * (1 - S);
            float q = V * (1 - f * S);
            float t = V * (1 - (1 - f) * S);

            switch (i)
            {
                case 0: return new CCTRGB(V, t, p);
                case 1: return new CCTRGB(q, V, p);
                case 2: return new CCTRGB(p, V, t);
                case 3: return new CCTRGB(p, q, V);
                case 4: return new CCTRGB(t, p, V);
                case 5: return new CCTRGB(V, p, q);
                default:
                    break;
            }

            return new CCTRGB(0,0,0);
        }
    }
}
