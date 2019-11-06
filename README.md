# CVCapture

这个项目的目的主要是使用在软件流程自动化中，去解析应用的界面，分析出一个**控件布局**的**树形结构**。

以此为基础，可以基于比如xpath之类的技术去定位UI中的控件。

## 主要模块

OpenCVAppLayoutUnderstanding库通过图形学的方法对APP（桌面APP，手机APP）递归切分成不同的小块。以skype界面为例，它会先自动识别应用界面，根据最顶端的布局切分成左右两块（以蓝线标出）：
![image](https://rongbao.club/wp-content/uploads/2019/11/layout-hierarchy-level-1.png)
然后，在每个子级布局中，找出它们的子节点：
![image](https://rongbao.club/wp-content/uploads/2019/11/layout-hierarchy-level-2.png)
不断递归，直到找到最底层的控件：
![image](https://rongbao.club/wp-content/uploads/2019/11/layout-hierarchy-level-3.png)
最后把找到的这些区域的标注总和起来，就是下图：
![image](https://rongbao.club/wp-content/uploads/2019/11/combined-regions.png)

## 原材料

下载Nuget包 **OpenCvSharp4** 和 **OpenCvSharp4.runtime.win** version 4.1.1.20191026

## 基本用法

1. 加载灰度图
```cs
Mat grayScale = Cv2.ImRead("a12.png", ImreadModes.Grayscale);
```
2. 配置
```cs
var v = new Variables
{
    bg_color_top_n = 3,
    img_padding = 20,
    BGCalcIncludeColors = new List<BGCalcIncludeColor> {
        new BGCalcIncludeColor {
            Background = new Scalar(240, 244, 248),
            IncludeColor = new Scalar(199, 237, 252)
        }
    },
    segment_ignore_gap_pixels = 8,
    adaptive_gap_pixels = true,
    gap_pixels_minimum = 6,
    gap_pixels_decrement_rate = 2,
    layout_depth = 3
};
```

配置名 | 解释
--- | ---
bg_color_top_n | 其中一种顶部布局的分块的方法是通过背景色来区分，分析前bg_color_top_n种主要的颜色，判断它是不是背景色
img_padding | padding是为了消除边缘的影响，这个数值影响不大
BGCalcIncludeColors | 这个是背景色的特例处理。举个例子，在一个列表布局中，一般选中某个item都会高亮它，这个配置就是用来把这个高亮的颜色等同化为原先的背景色，这样在背景色分析中，就可以将一个列表panel连成一整块
BGCalcIncludeColors.Background | 布局组件的背景色
BGCalcIncludeColors.IncludeColor | 特例背景色
segment_ignore_gap_pixels | 对于类似文字段落的处理，文字之间有间距，同一个段落行间有间距，当**间距**<=segment_ignore_gap_pixels的时候不会将文字区分开来
adaptive_gap_pixels | 当它为true的时候，**segment_ignore_gap_pixels**不是固定值，而是作为一个初始值应用于顶层layout的区分，而每个下层的layout都会根据**gap_pixels_decrement_rate**和**gap_pixels_decrement_rate**这两个参数动态调节segment_ignore_gap_pixels的值
gap_pixels_minimum | **segment_ignore_gap_pixels**递归减小的最小值
gap_pixels_decrement_rate | layout层级每深入一级**segment_ignore_gap_pixels**减小的值
layout_depth | 最大探索的layout层级

3. 调用API获取所有抓取的区域
```cs
var finalRegions = OpenCVAppLayoutUnderstanding.API.FindLayoutDepthRegions(grayScale, v);
```

4. 用open cv的imshow api把region都画出来
```cs
static void DrawRegions(Mat grayScale, List<RegionInfo> regions, string title = "regions")
{
    using (var mat = new Mat())
    {
        Cv2.CvtColor(grayScale, mat, ColorConversionCodes.GRAY2RGB);
        foreach (var b in regions)
        {
            Cv2.Rectangle(mat, b.Region, new Scalar(255, (b.Level - 1) * 50, (b.Level - 1) * 50), 2);
        }
        Cv2.ImShow(title, mat);
        Cv2.WaitKey(0);
    }
}
```

## 运行项目

您可以直接跑TestCV工程，在里面有OpenCVAppLayoutUnderstanding库的基本用法，以及窗口展示上面给出的那些截图

## 做个广告

作者正在穷愁潦倒中，如果您有下面相关的工作或者项目可以施舍给作者的，不胜感激哦
1. 网站开发（前端/后端/公众号/小程序）
2. App开发
3. OpenCV相关的开发
4. RPA系统的开发和搭建

关于作者的背景以及之前做过的项目，有意向的话可以详谈哦

邮箱：zhao_sun@hotmail.com
