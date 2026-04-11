//=====================================================================
// 模块名称：堆料信息 DotPileInfo
// 功能简介：接口堆料信息
// 版权声明：2020 锐创理工科技有限公司  All Rights Reserved.
// 当前版本：2020.7.23
// 更新履历：2020.7.23 杜子兮 创建
//============================================

namespace DotCloudLib
{
    /// <summary>
    /// 堆料信息
    /// </summary>
    public struct DotPileInfo
    {
        /// <summary>
        /// 堆料边界
        /// </summary>
        public ZXBoundary Bound;

        /// <summary>
        /// 构造体
        /// </summary>
        public DotPileInfo(ZXBoundary p_Bound) 
        {
            Bound = p_Bound;
        }

    }
}
