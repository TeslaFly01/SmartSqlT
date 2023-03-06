<div align="center">
   <img alt="SmartSQL" src="https://gitee.com/izhaofu/SmartSQL/raw/master/Img/icon.png">
	<h1 align="center" style="color:#4da7fd"><b>SmartSQL</b></h1>
</div>
<div align="center">
<h4 align="center">⚡一款方便、快捷的数据库文档查询、生成工具</h4>
<span align="center" style="letter-spacing:1.8px" >致力于成为帮助企业快速实现数字化转型的元数据管理工具</span>
</div>
<br>
<p align="center">
<img alt="visual studio 2019" src="https://img.shields.io/badge/Visual Studio-2019-blue.svg">
<img alt="csharp" src="https://img.shields.io/badge/language-csharp-brightgreen.svg">
<img alt="license" src="https://img.shields.io/badge/license-Apache-blue.svg">
<img alt="release" src="https://img.shields.io/badge/release-1.0.3.7-green">
</p>

![首页](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Banner_01.png)

### 🚩 项目介绍

SmartSQL 是一款方便、快捷的数据库文档查询、导出工具！从最初仅支持`SqlServer`数据库、`CHM`文档格式开始，通过不断地探索开发、集思广益和不断改进，又陆续支持`Word`、`Excel`、`PDF`、`Html`、`Xml`、`Json`、`MarkDown`等文档格式的导出。同时又扩展支持包括`SqlServer`、`MySql`、`PostgreSQL`、`SQLite`等多种数据库的文档查询和导出功能。


##### 🏅开源地址
[![Gitee](https://shields.io/badge/Gitee-https://gitee.com/izhaofu/SmartSQL-green?logo=gitee&style=flat&logoColor=red)](https://gitee.com/izhaofu/SmartSQL)  <br/>
[![GitHub](https://shields.io/badge/GitHub-https://github.com/TeslaFly01/SmartSqlT-green?logo=github&style=flat)](https://github.com/TeslaFly01/SmartSqlT)


##### 🎯下载地址

码云： [https://gitee.com/izhaofu/SmartSQL/releases](https://gitee.com/izhaofu/SmartSQL/releases)  

蓝奏云下载：[https://wwoc.lanzoum.com/b04dpvcxe](https://wwoc.lanzoum.com/b04dpvcxe)

蓝奏云密码：123

> 文件下载解压后，双击运行 `SmartSQL.exe`即可

> 🚀 本项目力求做最方便、快捷的数据库文档查询生成工具！

### 💎 数据库支持
- ✅ SqlServer
- ✅ MySQL
- ✅ Oracle
- ✅ PostgreSQL
- ✅ SQLite
- - [x] DB2
- - [x] TIDB
- - [x] 达梦
- - [x] 瀚高
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
    - 	[x] xml 由`Visual Studio`设置 实体类库的项目属性，勾选`XML文档文件`后生成项目时产生。
    - 	[x] xml 由`SmartSQL`的 XML导出而产生。
  
#### 什么是分组管理
- 可以对数据库中的表、视图、存储过程进行自定义分组
- 可以对分组对象进行文档批量导出

#### 功能架构

<div align="center">
<img src="https://gitee.com/izhaofu/SmartSQL/raw/master/Img/MindMap.jpg" width="80%">
</div>

### 🎉 功能介绍

#### Dashbord
![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Dashbord.png)

#### 工具箱
![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/MainTool.png)

#### 数据库连接

![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-ConnectType.png)|![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Connect.png)
--|--

> 连接管理中可以对当前添加的连接进行`添加`、`修改`和`删除`等操作。
 
> 目前支持对`SQLServer`、`MySQL`、`Oracle`、`PostgreSQL`...等5种数据库的支持。

> 对`DB2`、`TIDB`、`达梦`等其他关系型数据库的支持正在进行中。

#### 分组管理
![分组管理](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Group.png)|![分组管理](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-GroupObject.png)
--|--

>  在此可以对数据库中的`表`、`视图`、`存储过程`实现自定义分组，方便管理

>  同时支持对分组进行排序，将鼠标放在需排序的分组上，当箭头变成➕就可以拖动排序了

>  对`常用的分组`可置为默认`展开`，`不常用的分组`置为默认`不展开`

#### 快捷查询
| ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Objects.png) | ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-View.png) |
|--|--|
| ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Proc.png) | ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Column.png) |

> 支持左侧菜单快速检索对象

> 同时支持右侧主界面快速检索`表`、`列`、`视图`等信息

> 最大亮点是支持双击`备注说明`列对应单元格快速设置对象注释信息

> 对`视图`、`存储过程`支持一键查看内容结构、一键复制


#### 导入导出

![导入备注](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Import.png)|![导出文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Export.png)
:--:|:--:

> 支持对`表`、`视图`、`存储过程`进行导出成多种格式的离线文档

> 支持对`XML`格式的文档进行导入`表`、`列`、`视图`、`存储过程`注释

#### 设置/关于
![设置](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-Setting.png) | ![关于](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/sshot-About.png)
--|--

> 支持搜索时设置根据前缀模糊搜索和全量模糊搜索

> 支持对右侧主界面设置多选项卡和单选项卡设置


### 📰 文档截图

#### CHM文档
![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/chm.png)|![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/chmd.png)
|--|--|

#### Html文档
![Html文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/html.png)|![html文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/htmlt.png)
|--|--|

#### Word文档
![Word文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/word.png)|![Word文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/wordh.png)
|--|--|

#### Excel文档
![Word文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/excel.png)|![Word文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/excelp.png)
|--|--|

#### PDF文档
![PDF文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/pdf.png)|![PDF文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/pdfn.png)
|--|--|


### ⚡ 近期计划

	✅ 收集问题，修复完善基础功能
	✅ C#实体代码生成
	⏳ 进行中：优化现有Issues中反馈的bug
	⏳ 计划中：DB2、TIDB、达梦等数据库支持
	⏳ 计划中：对象结构对比功能
	⏳ 计划中：表、视图、存储过程同步功能
	⏳ ...


### ❓ 常见问题

-  [SmartSQL使用常见问题列表](https://gitee.com/dotnetchina/SmartSQL/blob/master/Question.md)
- [Bug反馈](https://gitee.com/dotnetchina/SmartSQL/issues/new)

> PS：如果你有更好方法，欢迎提供改善建议，助力✊该工具越来越好使！