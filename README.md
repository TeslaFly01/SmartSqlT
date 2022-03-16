<div align="center">
   <img alt="DBCHM" src="https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/dbchm.png">
	<h2 align="center">DBCHM</a></h2>
</div>
<div align="center">
<h3 align="center">最简单、最实用的数据库文档生成工具</h3>
</div>

<p align="center">
<img alt="visual studio 2019" src="https://img.shields.io/badge/Visual Studio-2019-blue.svg">
<img alt="csharp" src="https://img.shields.io/badge/ language-csharp-brightgreen.svg">
<img alt="license" src="https://img.shields.io/badge/license-MIT-blue.svg">
</p>

## 🚩 项目介绍

DBCHM 是一款数据库文档生成工具！
该工具从最初支持chm文档格式开始，通过开源，集思广益，不断改进，又陆续支持word、excel、pdf、html、xml、markdown等文档格式的导出。

### 🎯 本项目力求做最简单、最实用的数据库文档(字典)生成工具！

## 🍀支持的数据库
- [x] SqlServer
- [x] MySQL
- [x] Oracle
- [x] PostgreSQL
- [x] DB2
- [x] SQLite

## 🥝主要功能 

### 文档的内容都包含什么？
- 序号 | 列名 | 数据类型 | 长度 | 小数位数 | 主键 | 自增 | 允许空 | 默认值 | 列说明
- 视图 视图具体内容
- 存储过程 存储过程具体内容

🔹注：Oracle在v1.8.0.3-beta版本及以后暂不会查询显示自增数据。

### 支持哪些文档格式的导出？
- [x] chm
- [x] word
- [x] excel
- [x] pdf
- [x] html
- [x] xml
- [x] markdown
### 更新表列的注释，有哪些方式？
- 通过 工具-批注上载，选择文件导入进行更新批注(注释)：
    - 	[x] pdm 由`powerdesigner`设计数据库时产生。
    - 	[x] xml 由`visual studio`设置 实体类库的项目属性，勾选  XML文档文件 后生成项目时产生。
    - 	[x] xml 由`dbchm`的 XML导出 而产生。
- 列批注 在编辑前的选中状态下，可以从 选定行开始 粘贴多行文本内容 对多个列注释批量赋值。

## 🎉效果展示

### 1 数据库连接配置示例
![数据库连接配置](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/DBCHM000.png)

### 2 数据库连接管理
![数据库连接管理](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/DBCHM001.png)

### 3 表名模糊匹配
![表名模糊搜索](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/DBCHM002.png)

### 4 执行批注更新
![表批注更新](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/DBCHM003.png)

### 5 导出CHM文件
![导出CHM文件](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/DBCHM004.png)

### 6 表结构信息
![表结构信息](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/DBCHM005.png)

### 7 更多格式的效果，请[下载体验](https://gitee.com/dotnetchina/DBCHM/releases)哈:wink:！！

查看chm效果：[某微信开发框架表结构信息(示例).chm](https://gitee.com/dotnetchina/DBCHM/attach_files)

## 📘发行历史

​	<a target='_blank' href ='./ReleaseNote.md'>ReleaseNote</a>

## 💪贡献者

- @[trycache](https://gitee.com/trycache) 主要开发者
- @[空无一物](https://gitee.com/dotnetchina/) 先驱者

##  ⬇️下载使用
- **[下载发行版本](https://gitee.com/dotnetchina/DBCHM/releases)**，下载解压后，双击运行 `DBCHM.exe`。

- **[下载最新功能的内测版本](http://shang.qq.com/wpa/qunwpa?idkey=43619cbe3b2a10ded01b5354ac6928b30cc91bda45176f89a191796b7a7c0e26)**，Bug修复早知道，最新功能尝鲜，请在[![加入QQ群](https://img.shields.io/badge/QQ群-132941648-blue.svg)](http://shang.qq.com/wpa/qunwpa?idkey=43619cbe3b2a10ded01b5354ac6928b30cc91bda45176f89a191796b7a7c0e26)共享中获取:yum:！

## 🍄 其他工具
- [htmlhelp](https://gitee.com/dotnetchina/DBCHM/attach_files)，生成chm文件时，需提前安装。
- [PDMToCHM](https://gitee.com/dotnetchina/DBCHM/attach_files)，将PDM表结构文件导出到CHM文件。

##  🌱开发计划

- [x] 收集问题，修复完善基础功能
- [x] MJTop.Data 类库完善
- [ ]  测试数据生成器
- [ ]  C#实体代码生成器
- [ ]  ...

## 📖常见问题
- **连接不上，怎么办？**
	
	1. `连接数据库`界面填写的`连接信息`真的正确无误?
	2. `数据库服务器`有`防火墙/安全组`限制？
	3. 用 [Navicat Premium](https://gitee.com/dotnetchina/DBCHM/attach_files) 连接数据库服务器试试！
	
- **连接数据库时，点了 `连接/测试` ，半天没响应？**
	
	可能是连接远程数据库网络不好的原因，可以把`连接超时`设置的小一些。
	
- **dbchm可以连接上，但显示不了数据怎么办？**
	- 导出文档前，数据库使用账号要给予`root级别`的权限，非root级别账号连接，可能会出现`表数据显示不全`或数据查询因权限不足，会`查不出来数据`！
	- dbchm有Bug， [提Issue](https://gitee.com/dotnetchina/DBCHM/issues/new) 或 [进群里](http://shang.qq.com/wpa/qunwpa?idkey=43619cbe3b2a10ded01b5354ac6928b30cc91bda45176f89a191796b7a7c0e26) 反馈。
	
- **表列的批注数据我想迁移，怎么办？**
	1. 使用 dbchm 的 `XML导出`，对当前数据库的批注数据 就会导出一个xml文件。
	2. 点`数据连接`， 切换至 目标数据库连
	3. 再用`批注上载` 就可以选择刚刚的xml文件，如果数据库表结构相同，批注就会更新到目标数据库服上。
	
- **数据库比较老，如  `Sql Server 2000 `，怎么使用dbchm？**
	1. 下载安装 [Navicat Premium](https://gitee.com/dotnetchina/DBCHM/attach_files)
	2. 连接上老旧的数据库服务器，将数据库表结构脚本导出。
	3. 找一台高版本的数据库服务器，新建一个临时数据库，将导出的脚本导入。
	4. 然后用dbchm连接高版本的数据库服务器。
	
- **chm文件可以正常导出，但是文件名中文乱码，打开显示 无法访问此页**
	

  这种情况，有一种可能是win系统的**区域设置**，勾选了

  `Beta 版：使用Unicode UTF-8提供全球语言支持` 。取消勾选后，可能不存在该问题。

- **Oracle数据库连上之后，一直未响应，像卡死了一样，怎么办？**
	
	因为Oracle的 `列是否自增` 的sql语句，查询效率比较低，查的比较慢，没有卡死！！请耐心等待！！
	

🔹**注：因Oracle查询自增相当耗时，Oracle在v1.8.0.3-beta版本及以后暂不会查询自增数据。**
	
PS：如果你有更好方法，欢迎提供改善建议，助力✊该工具越来越好使！
	
- **Oracle 11g、Oracle 12c测试连接显示“[28040]ORA-28040:没有匹配的验证协议”？**

	目前群里及isuues反馈的问题，可能11g以后的版本均会出现此项问题。

	该问题描述：navicat等工具可以直接连接，但是本程序连接不上有上述问题。

	目前想到的解决问题办法是，需在sqlnet.ora添加设置

	```shell
	SQLNET.ALLOWED_LOGON_VERSION=8
	SQLNET.ALLOWED_LOGON_VERSION_SERVER=8
  SQLNET.ALLOWED_LOGON_VERSION_CLIENT=8
	```
	
	参数值可设置8、10等，使用者可根据需要自行设置。
	
	![ORA-28040修改兼容](https://gitee.com/dotnetchina/DBCHM/raw/master/DBChm/Images/ORA-28040.png)

	注意：改完后其他相关用户的密码必须重置,或直接更新为原来的密码也是可以的（修改密码sql示例：alter user System identified by oldpassword;），此项操作慎重。

	要么在建库的初期添加此参数，然后重置相关密码；要么新建测试环境，进行此项操作。
	
	- **其他问题**
	
	如遇其他问题，可以通过Issues或群里反馈，记录问题，请写清楚遇到问题的原因、软件版本、系统环境、数据库版本、甚至数据库结构、复显步骤以及期望达到的效果；建议配上多张全屏大图，请勿使用局部截屏小图！方便我们这边可以迅速定位，就事论事，解决问题。
	
	如果你有更好的解决方法，欢迎提供改善建议或直接提pr，我们一起完善该工具！

## 🔗交流
- QQ交流群：[![加入QQ群](https://img.shields.io/badge/QQ群-132941648-blue.svg)](http://shang.qq.com/wpa/qunwpa?idkey=43619cbe3b2a10ded01b5354ac6928b30cc91bda45176f89a191796b7a7c0e26) ，推荐点击按钮入群，当然如果无法成功操作，请自行搜索群号132941648进行添加 ），其它疑问或idea欢迎入群交流！