# CVCapture

这个项目的目的主要是使用在软件流程自动化中，去解析应用的界面，分析出一个**控件布局**的**树形结构**。

以此为基础，可以基于比如xpath之类的技术去定位UI中的控件。

## 主要模块

OpenCVAppLayoutUnderstanding库通过图形学的方法对APP（桌面APP，手机APP）递归切分成不同的小块。以skype界面为例，它会先自动识别应用界面，根据最顶端的布局切分成左右两块（以蓝线标出）：
![image](https://github.com/wirfree/CVCapture/raw/master/screenshots/layout-hierarchy-level-1.png)
然后，在每个子级布局中，找出它们的子节点：
![image](https://github.com/wirfree/CVCapture/raw/master/screenshots/layout-hierarchy-level-2.png)
不断递归，直到找到最底层的控件：
![image](https://github.com/wirfree/CVCapture/raw/master/screenshots/layout-hierarchy-level-3.png)
最后把找到的这些区域的标注总和起来，就是下图：
![image](https://github.com/wirfree/CVCapture/raw/master/screenshots/combined-regions.png)

