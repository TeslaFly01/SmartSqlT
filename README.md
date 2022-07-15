<div align="center">
   <img alt="SmartSQL" src="https://gitee.com/izhaofu/SmartSQL/raw/master/Img/icon.png">
	<a href="#"><h1 align="center" style="color:#4da7fd"><b>SmartSQL</b></h1></a>
</div>
<div align="center">
<h5 align="center">⚡一款方便、快捷的数据库文档查询、生成工具</h3>
</div>

<p align="center">
<img alt="visual studio 2019" src="https://img.shields.io/badge/Visual Studio-2019-blue.svg">
<img alt="csharp" src="https://img.shields.io/badge/language-csharp-brightgreen.svg">
<img alt="license" src="https://img.shields.io/badge/license-Apache-blue.svg">
<img alt="release" src="https://img.shields.io/badge/release-1.0.3.1-green">

</p>

### 🚩 一、项目介绍

SmartSQL 是一款方便、快捷的数据库文档查询、导出工具！该工具从最初支持`CHM`文档格式开始，通过不断地探索开发、集思广益和不断改进，又陆续支持`Word`、`Excel`、`PDF`、`Html`、`Xml`、`MarkDown`等文档格式的导出。同时支持`SqlServer`、`MySql`、`PostgreSQL`等多种数据库的文档查询和导出功能。

##### 🏅开源地址
[![Gitee](https://img.shields.io/badge/Gitee-https%3A%2F%2Fgitee.com%2Fizhaofu%2FSmartSQL-green)](https://gitee.com/izhaofu/SmartSQL)  <br/>
[![GitHub](https://img.shields.io/badge/GitHub-https%3A%2F%2Fgithub.com%2FTeslaFly01%2FSmartSqlT-green)](https://github.com/TeslaFly01/SmartSqlT)

> 🚀 本项目力求做最简单、最好用的数据库文档(字典)查询生成工具！

### 🥝 二、数据库支持
- ✅ SqlServer
- ✅ MySQL
- ✅ PostgreSQL
- - [x] Oracle
- - [x] DB2
- - [x] SQLite

### 📖 三、主要功能 

#### 1.文档的内容都包含什么？
- `表` 序号 | 列名 | 主键 | 自增 | 数据类型 | 长度 | 允许NULL值 | 默认值 | 备注说明
- `视图` 视图内容SQL脚本
- `存储过程` 存储过程内容SQL脚本

#### 2.支持哪些文档格式的导出？
- ✅ CHM
- ✅ Word
- ✅ Excel
- ✅ PDF
- ✅ Html
- ✅ Xml
- ✅ MarkDown
#### 3.更新表列的注释，有哪些方式？
- 通过 `文件`➡`导入备注`，选择文件导入进行更新批注(注释)：
    - 	[x] pdm 由`PowerDesigner`设计数据库时产生。
    - 	[x] xml 由`Visual Studio`设置 实体类库的项目属性，勾选  XML文档文件 后生成项目时产生。
    - 	[x] xml 由`SmartSQL`的 XML导出 而产生。

### 🎉 四、工具截图

![首页](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Top.png)

#### 1.数据库连接

![数据库连接](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Connect.png)

#### 2.分组管理
|||
|--|--|
| ![分组管理](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Group.png) | ![分组管理](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/GroupObject.png) |

#### 3.快捷查询
|||
|--|--|
| ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Objects.png) | ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/View.png) |
| ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Pro.png) | ![快捷查询](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Column.png) |

#### 4.导入导出

|导入备注|导出文档
|:--:|:--:|
| ![导入备注](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Import.png) | ![导出文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Export.png) |

#### 5.设置
![设置](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/Setting.png)

#### 6.关于
![关于](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/About.png)


### 🎉 五、文档截图

#### 1.CHM文档
|||
|--|--|
|![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/chm.png)|![CHM文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/chmd.png)|

#### 2.HTML文档
|||
|--|--|
|![HTML文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/html.png)|![HTML文档](https://gitee.com/izhaofu/SmartSQL/raw/master/Img/docImg/htmlt.png)|

### 💪 六、作者

  [![作者](https://img.shields.io/badge/%E4%BD%9C%E8%80%85-MicLuo-green)](https://gitee.com/izhaofu)

### 🍻 七、贡献代码

- `SmartSQL` 遵循 `Apache-2.0` 开源协议，欢迎大家提交 `PR` 或 `Issue`。

### ⬇️ 八、下载地址

[![立即下载](https://img.shields.io/badge/%E7%AB%8B%E5%8D%B3%E4%B8%8B%E8%BD%BD----green)](https://gitee.com/izhaofu/SmartSQL/releases) 

下载解压后，双击运行 `SmartSQL.exe`。

### 🍄 附一、其他工具
- [![htmlhelp](https://img.shields.io/badge/CHM%E6%8F%92%E4%BB%B6-htmlhelp-green)](https://gitee.com/izhaofu/SmartSQL/attach_files/1124263/download)，生成chm文件时，需提前安装。
- [![PDMToCHM](https://img.shields.io/badge/CHM%E6%8F%92%E4%BB%B6-PDMToCHM-green)](https://gitee.com/izhaofu/SmartSQL/attach_files/1124266/download)，将PDM表结构文件导出到CHM文件。

### ⚡ 附二、近期计划

	✅ 收集问题，修复完善基础功能
	✅ C#实体代码生成
	⏳ 计划中：对象结构对比功能
	⏳ 计划中：表、视图、存储过程同步功能
	⏳ ...

### ❓ 附三、常见问题
- **连接不上，怎么办？**
	
>	1. `连接数据库`界面填写的`连接信息`真的正确无误?
>	2. `数据库服务器`有`防火墙/安全组`限制？
>	3. 用 [Navicat Premium](https://gitee.com/dotnetchina/DBCHM/attach_files) 连接数据库服务器试试！
	
- **连接数据库时，点了 `连接/测试` ，半天没响应？**
	
>	可能是连接远程数据库网络不好的原因，可以把`连接超时`设置的小一些。
	
- **SmartSQL可以连接上，但显示不了数据怎么办？**
>	- 导出文档前，数据库使用账号要给予`root级别`的权限，非root级别账号连接，可能会出现`表数据显示不全`或数据查询因权限不足，会`查不出来数据`！
>	- SmartSQL有Bug， [提Issue](https://gitee.com/izhaofu/SmartSQL/issues/new) 反馈。
	
- **表列的批注数据我想迁移，怎么办？**
>	1. 使用 SmartSQL 的 `XML导出`，对当前数据库的批注数据 就会导出一个xml文件。
>	2. 点`数据连接`， 切换至 目标数据库连
>	3. 再用`批注上载` 就可以选择刚刚的xml文件，如果数据库表结构相同，批注就会更新到目标数据库服上。
	
- **数据库比较老，如  `Sql Server 2000 `，怎么使用SmartSQL？**
>	1. 下载安装 [Navicat Premium](https://gitee.com/dotnetchina/DBCHM/attach_files)
>	2. 连接上老旧的数据库服务器，将数据库表结构脚本导出。
>	3. 找一台高版本的数据库服务器，新建一个临时数据库，将导出的脚本导入。
>	4. 然后用SmartSQL连接高版本的数据库服务器。
	
- **chm文件可以正常导出，但是文件名中文乱码，打开显示 无法访问此页**
	
> 	这种情况，有一种可能是win系统的**区域设置**，勾选了

>  `Beta 版：使用Unicode UTF-8提供全球语言支持` 。取消勾选后，可能不存在该问题。
	
- **其他问题**
	
>	如遇其他问题，可以通过Issues反馈，记录问题，请写清楚遇到问题的原因、软件版本、系统环境、数据库版本、甚至数据库结构、复现步骤以及期望达到的效果；建议配上多张全屏大图，请勿使用局部截屏小图！方便我们这边可以迅速定位，解决问题。

> PS：如果你有更好方法，欢迎提供改善建议，助力✊该工具越来越好使！