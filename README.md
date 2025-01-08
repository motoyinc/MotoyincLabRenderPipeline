# Motoyinc Lab RenderPipeline

这是MotoyincLabRP对Unity的Scriptable Render Pipeline (SRP)特性的研究管线。MotoyincLabRP的结构大致与Unity的通用渲染管线(URP)类似，细节上稍有不同，Pass的组织方式也不会像URP那样复杂。

## 管线环境

- Unity版本：6000.0.x
- 关联包体：
  - com.unity.render-pipelines.core  **v17.0.3**
  - `关联包体在安装MotoyincLabRP时会自动安装`

 
## 安装管线
```URL
https://github.com/motoyinc/MotoyincLabRenderPipeline.git?path=/Packages/com.motoyinc.render-piplines.motoyinclab
```
- 将链接复制到**PackageManager -> install package from git URL** 后等待安装完成，有可能会出现网络原因的安装失败，多试几次就行
![image](https://github.com/user-attachments/assets/070b1156-4c7b-44dc-bbfe-e8a353808ca8)
![动画](https://github.com/user-attachments/assets/7c865d30-7948-48c0-9f9a-ac3b8a0f5dfe)
![image](https://github.com/user-attachments/assets/ff7ae794-f70f-4d33-b356-e28dcfea7068)



---

#### **开发日志 Development logs**

https://zhuanlan.zhihu.com/p/810720751

#### **参考文献 References**

[1] J. Flick, “tutorials for Unity” Catlike Coding, [Online]. Available: [https://catlikecoding.com/unity/tutorials/](https://catlikecoding.com/unity/tutorials/).

[2] Unity Technologies, “Universal Render Pipeline (URP) Source Code,” GitHub repository, 2020. [Online]. Available: [https://github.com/Unity-Technologies/Graphics](https://github.com/Unity-Technologies/Graphics). 

