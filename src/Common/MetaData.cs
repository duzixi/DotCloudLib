//=====================================================================
// 模块名称：元数据类 MetaData
// 功能简介：保存类库的基本信息
// 版权声明：(C) 九州创智&锐创理工  All Rights Reserved.
// 更新履历：
// 2019.11.10 Ver.0.1.0  移自“知行”C++库 - 杜子兮 
// 2022.07.07 Ver.0.8.0  CurveFit拟合，提炼散料基础父类，添加点云高级搜索算法 - Sanngoku
// 2023.05.25 Ver.1.0.0  引入图论，节点，链路，图搜索 - 杜子兮 & 王振宇
//====================================================================

using System.Runtime.CompilerServices;

namespace DotCloudLib
{
    /// <summary>
    /// 元数据类
    /// </summary>
    public static class MetaData
    {
        /// <summary>
        /// 算法库名
        /// </summary>
        public static string LIB_NAME = "DotCloudLib";

        /// <summary>
        /// 广告语
        /// </summary>
        public static string SLOGAN = "点云后处理算法库";

        /// <summary>
        /// 当前版本 Ver.1.0.0 beta 2026.04.10
        /// dev 开发中(比如提供空接口)
        /// alfa 内部联调
        /// beta 外部联调(现场实施)
        /// </summary>
        public static string VERSION = "Ver.1.0.0 beta 2026.04.10";

        /// <summary> 
        /// 最新修改：删除业务类算法
        /// </summary>
        public static string COMMENT = "最新修改：Ver.1.0 for Github";

        // Ver.1.0.0 beta 2026.02.12 for Github

        /// <summary>
        /// 版权声明
        /// </summary>
        public static string COPY_RIGHT = "(C) 2020-2026 锐创理工科技(大连)有限公司  All Rights Reserved.";

        /// <summary>
        /// 获取库版本
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            return VERSION;
        }

        /// <summary>
        /// 算法库返回信息
        /// </summary>
        public static string MESSAGE = "";
    }
}
