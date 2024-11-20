# Motoyinc Lab RenderPipeline

这是MotoyincLabRP对Unity的Scriptable Render Pipeline (SRP)特性的研究管线。MotoyincLabRP的结构大致与Unity的通用渲染管线(URP)类似，细节上稍有不同，Pass的组织方式也不会像URP那样复杂。

## 管线环境

- Unity版本：6000.0.x
- 关联包体：
  - com.unity.render-pipelines.core  **v17.0.3**
  - `关联包体在安装MotoyincLabRP时会自动安装`

## 开发日志
https://zhuanlan.zhihu.com/p/810720751
 
## 安装管线
```URL
https://github.com/motoyinc/MotoyincLabRenderPipeline.git?path=/Packages/com.motoyinc.render-piplines.motoyinclab
```
- 将链接复制到**PackageManager -> install package from git URL** 后等待安装完成，有可能会出现网络原因的安装失败，多试几次就行
![image](https://github.com/user-attachments/assets/070b1156-4c7b-44dc-bbfe-e8a353808ca8)
![动画](https://github.com/user-attachments/assets/7c865d30-7948-48c0-9f9a-ac3b8a0f5dfe)
![image](https://github.com/user-attachments/assets/ff7ae794-f70f-4d33-b356-e28dcfea7068)





---

## **致谢 | Acknowledgments**

在本项目的开发过程中，我参考并受益于以下资源，在此对其作者和贡献者表示由衷的感谢：

During the development of this project, I referenced and benefited greatly from the following resources. I would like to extend my sincere gratitude to their authors and contributors:

1. **Jasper Flick**  
   感谢 Jasper Flick 在 Catlike Coding 网站上提供的详尽教程 Custom SRP Tutorial。该教程以清晰的思路和逐步实现的方法，帮助我深入了解 Unity 的渲染管线，并启发了本项目中部分架构和逻辑设计的实现。  
   Thanks to Jasper Flick for providing the comprehensive tutorial *Custom SRP Tutorial* on the Catlike Coding website. The tutorial's clear methodology and step-by-step implementation greatly aided my understanding of Unity’s rendering pipeline and inspired some of the architectural and logical designs implemented in this project.  
   Catlike Coding: [https://catlikecoding.com](https://catlikecoding.com)

2. **Unity Technologies**  
   感谢 Unity Technologies 提供的 Universal Render Pipeline (URP) 开源代码及文档支持。URP 的代码结构和实现逻辑为本项目提供了重要的参考，并帮助我理解了 Unity 渲染管线的底层机制。  
   Thanks to Unity Technologies for providing the open-source code and documentation for the Universal Render Pipeline (URP). The structure and implementation of URP served as a significant reference for this project, helping me better understand the underlying mechanisms of Unity’s rendering pipeline.  
   URP Repository: [https://github.com/Unity-Technologies/Graphics](https://github.com/Unity-Technologies/Graphics)

---

通过这些优秀资源的支持和启发，本项目得以更好地实现目标。在此对所有为技术社区提供支持的作者、开发者和贡献者致以最诚挚的谢意。

With the support and inspiration from these exceptional resources, this project was able to achieve its goals more effectively. I extend my heartfelt thanks to all authors, developers, and contributors who continue to support the technical community.
