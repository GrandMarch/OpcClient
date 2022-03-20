## 项目结构
解决方案中包含两个项目：Test和OpcDaClient。
Test是一个基本的例子，OpcDaClient是opc读取的类库。该类库主体上是基于西门子的例子做的。但是做了很多的改动：
1. 将西门子的例子改为了类库形。
2. 实现了订阅、异步读取、异步写入的事件
3. 用manger类管理数据，用this索引器查询数据。
4. 基本上90%的opc都是32位的。本类库是64位类库，避免了因为一个类库导致整个项目不能以64位运行的问题。
## 配置文件
工程中默认已经创建了一个opc.json。Manager类启动时会读取opc.json并以此配置文件的内容启动opcserver、创建opc组、创建opc item。
配置文件对应的类为OpcClient.Db.Config类，想了解配置文件的格式，查看OpcClient.Db.Config类即可。

### 数据类型定义

        public enum OpcDataType:short
        {
            Boolean =11,
            Byte =17,
            Short=2,
            Word=18,
            Int =3,
            UInt =19,
            Float=4,
            Double=5,
            Long=20,
            String=8
        }
