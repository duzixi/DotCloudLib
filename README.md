点云后处理算法库

DotCloudLib是基于C#实现的点云后处理算法类库（对应点云前处理算法库PCLPerformance），主要用于在点云标准化预处理的基础上，从业务层面（料堆、料条等）进行三维空间分析和处理。
本库所有算法自主原生开发，仅引用基本的常规库，不依赖第三方算法库，可直接在.NET工程、Unity3D工程或原生C#程序使用。算法库名“Dot”援引.NET平台的“.”，中文双关“点”，意为点云库。

目前，公开的版本为1.0版，算法库源码参见src文件夹，在.NET平台下创建类库，逐一添加类脚本即可。
需要引用的dll参见dll文件夹。
后续算法库开源计划，以及算法库具体功能内容参见 doc 文件夹中的说明文档。

本算法库引用开源算法库dll开源源码如下：

2018.04.16 王振宇 https://github.com/Darkziyu/Mathd (C#版)

2024.08.13 杨波 https://github.com/yboooS/Mathd_cpp (C++版)

2024.11.21 崔艳龙 https://github.com/cuileo2018/Geometry-Algorithm (C# & C)

未来，本算法库将支持更多开发语言，如python、Java、SCL等版本。同时，也欢迎IT界大佬们自行拓展其它开发语言。

关于算法库相关问题，可以与作者（本人）联系。

杜子兮
duzixi@qq.com
