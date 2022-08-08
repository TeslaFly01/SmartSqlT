<div align="center">
   <img alt="SmartSQL" src="https://gitee.com/izhaofu/SmartSQL/raw/master/Img/icon.png">
	<h1 align="center" style="color:#4da7fd"><b>SmartSQL</b></h1>
</div>
<div align="center">
<h5 align="center">⚡一款方便、快捷的数据库文档查询、生成工具</h3>
</div>

<p align="center">
<img alt="visual studio 2019" src="https://img.shields.io/badge/Visual Studio-2019-blue.svg">
<img alt="csharp" src="https://img.shields.io/badge/language-csharp-brightgreen.svg">
<img alt="license" src="https://img.shields.io/badge/license-Apache-blue.svg">
<img alt="release" src="https://img.shields.io/badge/release-1.0.3.2-green">
</p>

![首页](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Banner.png)

### 🚩 项目介绍

SmartSQL 是一款方便、快捷的数据库文档查询、导出工具！该工具从最初支持`CHM`文档格式开始，通过不断地探索开发、集思广益和不断改进，又陆续支持`Word`、`Excel`、`PDF`、`Html`、`Xml`、`Json`、`MarkDown`等文档格式的导出。同时支持`SqlServer`、`MySql`、`PostgreSQL`、`SQLite`等多种数据库的文档查询和导出功能。


##### 🏅开源地址
[![Gitee](https://shields.io/badge/Gitee-https://gitee.com/izhaofu/SmartSQL-green?logo=gitee&style=flat&logoColor=red)](https://gitee.com/izhaofu/SmartSQL)  <br/>
[![GitHub](https://shields.io/badge/GitHub-https%3A%2F%2Fgithub.com%2FTeslaFly01%2FSmartSqlT-green?logo=github&style=flat)](https://github.com/TeslaFly01/SmartSqlT)

> 🚀 本项目力求做最方便、快捷的数据库文档查询生成工具！

### 💎 数据库支持
- ✅ SqlServer
- ✅ MySQL
- ✅ PostgreSQL
- ✅ SQLite
- - [x] Oracle
- - [x] DB2
- - [x] TIDB
- - [x] 达梦
- - [x] 人大金仓

### 📖 主要功能 

#### 文档的内容都包含什么？
- `表` 序号 | 列名 | 主键 | 自增 | 数据类型 | 长度 | 允许NULL值 | 默认值 | 备注说明
- `视图` 视图内容SQL脚本
- `存储过程` 存储过程内容SQL脚本

#### 支持导出哪些文档格式？
![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/chm.png) | ![Excel文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/excel.png) | ![Word文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/word.png) | ![PDF文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/pdf.png)
--|--|--|--
![Html文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/html.png) | ![Xml文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/xml.png) | ![Json](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/json.png) | ![MarkDown](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/icon/markdown.png)

#### 更新表列的注释，有哪些方式？
- 通过 `文件`➡`导入备注`，选择文件导入进行更新批注(注释)：
    - 	[x] pdm 由`PowerDesigner`设计数据库时产生。
    - 	[x] xml 由`Visual Studio`设置 实体类库的项目属性，勾选  XML文档文件 后生成项目时产生。
    - 	[x] xml 由`SmartSQL`的 XML导出 而产生。
  
#### 什么是分组管理
- 可以对数据库中的表、视图、存储过程进行自定义分组
- 可以对分组对象进行文档批量导出

### 🎉 工具截图

#### 数据库连接

![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-ConnectType.png)|![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Connect.png)
--|--

> 连接管理里面可以对当前添加的连接进行`添加`、`删除`和`修改`等操作。
 
> 目前支持对`SQLServer`、`MySQL`、`PostgreSQL`、`SQLite`等4种数据库的支持。

> 对`Oracle`、`DB2`、`TIDB`、`达梦`等其他关系型数据库的支持也正在紧张进行中。

#### 分组管理
![分组管理](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Group.png)|![分组管理](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-GroupObject.png)
--|--

>  在此可以对数据库中的`表`、`视图`、`存储过程`等对象实现自定义分组，方便后续管理

>  同时还支持对分组进行排序，将鼠标放在需要排序的分组上，当鼠标箭头变成➕就可以进行拖动排序了

>  对`常用的分组`可以置为默认`展开`，`不常用的分组`置为默认`不展开`，保持左侧菜单界面干净、清爽

#### 快捷查询
| ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Objects.png) | ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-View.png) |
|--|--|
| ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Proc.png) | ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Column.png) |

#### 导入导出

![导入备注](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Import.png)|![导出文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Export.png)
:--:|:--:

#### 设置/关于
![设置](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Setting.png) | ![关于](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-About.png)
--|--

> 支持搜索时设置根据前缀模糊搜索和全量模糊搜索
> 支持对右侧主界面设置多选项卡和单选项卡设置


### 📰 文档截图

#### CHM文档
![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/chm.png)|![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/chmd.png)
|--|--|

#### HTML文档
![HTML文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/html.png)|![HTML文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/htmlt.png)
|--|--|


### ⬇️ 下载地址

[![立即下载](https://img.shields.io/badge/download-%E7%AB%8B%E5%8D%B3%E4%B8%8B%E8%BD%BD-blue)](https://gitee.com/izhaofu/SmartSQL/releases)  

> 下载解压后，双击运行 `SmartSQL.exe`即可

### 🔥 沟通交流

 ![](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-ContactQQ.png) | ![](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Contact1.png) 
|:--:|:--:|
| QQ交流群：`666483672` | 微信交流群 |

 > 由于`微信群`限制超`100`人后需群主邀请，如需加群请关注下方公众号获取本人微信拉你入群

![](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-mp.png)


### ⚡ 近期计划

	✅ 收集问题，修复完善基础功能
	✅ C#实体代码生成
	⏳ 进行中：优化现有Issues中反馈的bug
	⏳ 计划中：Oracle、DB2、达梦等数据库支持
	⏳ 计划中：对象结构对比功能
	⏳ 计划中：表、视图、存储过程同步功能
	⏳ ...

### ❓ 常见问题

-  [SmartSQL使用常见问题列表](https://gitee.com/dotnetchina/SmartSQL/blob/master/Question.md)

> PS：如果你有更好方法，欢迎提供改善建议，助力✊该工具越来越好使！