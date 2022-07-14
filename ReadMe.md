# XLua热更新框架知识点梳理

## 资源目录的划分

1. 需要打ab包的资源：
  - Lua
  - UI
    - 预设
    - 图片
  - 模型
    - 预设
    - 动作
    - 贴图
    - 材质
  - 特效
    - 预设
    - 贴图
  - 声音
    - 音效
    - 音乐
  - 动画
2. 不需要打ab包的资源：
  - xlua
  - C#
    - 工具
      - 构建工具
      - 其他工具
    - 与Lua交互
      - C#调用Lua
        - 加载Lua脚本
        - 执行Lua逻辑
      - Lua调用C#，C#给Lua提供得接口
        - 资源加载
        - 资源管理
        - 事件管理
        - 场景加载
        - 声音加载
        - 模型加载
        - UI管理

## 框架开发流程

1. Bundle处理

   - 构建

   - 加载

   - 更新

2. C#调用Lua

   - Lua加载和管理

   - Lua绑定与执行

3. 向Lua提供接口

4. 完善和优化



## AssetBundle打包

### 打包策略

- 按文件夹打包
  - 优势：bundle数量少，小包模式首次下载快
  - 劣势：后期更新的时候，更新补丁大
- 按文件打包
  - 与按文件夹打包相反

### 建立版本文件

版本号：1.0.1

文件信息：文件路径|bundle名|依赖文件列表

### AssetDatebase

AssetDatebase是一个静态类，他的作用是管理整个工程的所有文件。直观地说就是管理整个project窗口中的所有内容，比如，你可以增加、删除、修改文件等等。

这里有几个常常用到：

- CreateAsset：创建文件
- CreateFolder：创建文件夹
- DeleteAsset：删除文件
- GetAssetPath：获取文件相对于Assets所在目录的相对位置，如“Assets/Images/test.png”
- LoadAssetAtPath：加载文件
- Refresh：刷新整个project窗口
- SaveAssets：保存所有文件

## AssetBunle资源加载

资源加载过程：

- 解析版本文件
  - 获取文件信息

- 加载资源（异步加载）
  - 加载以来bundle
  - 加载自身bundle
  - 加载资源

## 资源热更新

### 热更新方案

- 整包
  - 策略：完整更新资源放在安装包内
  - 优点：首次更新少
  - 缺点：安装包下载时间长，首次安装久
  - 初次安装需要将Application.streamingAssets（只读）目录的资源拷贝到Application.persistentDataPath（可读写），再检查版本文件下载补丁
- 分包
  - 策略：安装包内放少量或者不放热更新资源
  - 优点：安装包小，下载快，安装迅速
  - 缺点：首次更新时间久
  - 有些平台有安装包大小限制，需要用分包

### 热更新流程

1. 检查是否首次安装（判断是否有filelist）
2. 若是，从服务器下载资源或者从只读文件夹streamingAssets拷贝资源到可读写文件夹persistentDataPath（释放资源）
3. 完成后最后写入filelist
4. 检查更新（下载资源服务器的filelist与本地对比是否一致，解析filelist）
5. 若版本号与服务器不同则开始更新
6. 进入游戏

只读目录：Application.streamingAssetsPath/+**ui/testui/prefab.ab**

可读写目录：Application.persistentDataPath/+**ui/testui/prefab.ab**

资源服务器地址：http://127.0.0.1:PortNum/AssetBundles/+**ui/testui/prefab.ab**

只需要知道后面的相对路径，就可以找到对应的目录

<img src="https://lengyueling.oss-cn-shenzhen.aliyuncs.com/image-20220525130457230.png" style="zoom:80%;" />  

## XLua热更新

管理并使用上一步资源热更新的资源

### LuaManager

- 提供LuaEnv虚拟机，在解析版本文件时，将lua文件名添加至集合public List< string >LuaNames=new List< string >();中
- LuaEnv.Addloader(Loader)中注册回调，在lua代码里调用require时，完成回调加载lua脚本。
- 在编辑器模式下加载lua脚本
  - 则直接在Assets下lua脚本目录读取所有文件
  - 遍历获取文件的数据。
  - 覆盖添加至 m_LuaScriptslua脚本的内容的缓存字典中。

- 获取lua数据之后，在调用lua时，Loader回调直接从字典中获取对应字节数据。
- Lua的垃圾回收处理：每固定时间调用LuaEnv.Tick()；

### LuaBehaviour

- LuaBehaviour模拟了类似monoBehaviour的环境，作为其他需要热更新的脚本的基类

- 在Awake函数为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突 

- ```csharp
  m_ScriptEnv=m_LuaEnv.NewTable();   
  LuaTable meta=m_LuaEnv.NewTable();
  meta.Set("__index",m_LuaEnv.Global);
  m_ScriptEnv.SetMetaTable(meta);  //设原表
  meta.Dispose();
  m_ScriptEnv.Set("self",this);   //.Set(<参数>，<赋值>)； 将lua中的self进行绑定
  ```

- lua函数的绑定与执行，用于LuaBehavior及其子类模拟monobehavior相关操作

  - ```csharp
    m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName),luaName,m_ScriptEnv);
    <lua脚本字节数据>，<lua脚本名>，<运行环境>
    ```

  - 例如：
    
    - scripEnv.Get("start",outluaStart);将lua中的start方法进行绑定，传递给C#中的ActionluaStart；
    - 在C#中执行调用在执行销毁函数时，要对这些委托事件、LuaTable进行释放和清空。

- UILogic：继承于LuaBehaviour，Init自定义需要调用lua的Action委托。


### UI管理

主要负责下面四个工作：

- 加载UI（UIManager中OpenUI函数）
- 绑定和执行（LuaBehaviour、UILogic）
- UI对象管理（委托给对象池处理
- UI层级管理：
    - 界面类型分为以下几种， 分组可以实现热更新功能：
        - 一级界面
        - 二级弹窗
        - 三级弹窗
        - 特殊界面
    - UGUI层级的特点：根据节点顺序渲染
    - UI的渲染，后渲染会覆盖先渲染UI。
    - 不同级别的UI需要管理显示问题。
    - 所以设置UI父节点，即可方便管理和显示。
    - 在生成UI时，直接设置UI级别对应的层级节点。
- 实体管理（模型特效等）和UI管理几乎一样所以不详细讲解了，详见EntityManager、EntityLogic脚本

### 场景管理

- 需要完成哪些功能：
  - 场景加载：
    - 场景切换
    - 场景叠加
    - 场景激活

  - 场景卸载（叠加）

- 异步加载的加载方式：叠加、切换。
- 场景切换的回调方法：获取两个场景的SceneLogic脚本，场景1调用非激活方法，场景2调用激活方法。
- 场景的Bundle加载中，后缀为.unity的文件，直接不执行回调事件。

### 声音管理

- 需要完成哪些功能：

  - 背景音乐
    - 播放
    - 暂停
    - 恢复
    - 停止
    - 音量

  - 音效
    - 播放
    - 音量

- SoundManager生成两个AudioSource。

- 在Lua中添加监听事件，调用C#中的方法。

- 因为lua中要调用slider的onValueChanged事件，需要float类型的参数，所以在xlua脚本中需要加一个标签，不然会报错

  - Xlua->Editor->ExampleConfig.cs

  - ```csharp
    [CSharpCallLua]
    public static List<Type> mymodule_cs_call_lua_list = new List<Type>()
    {
        typeof(UnityEngine.Events.UnityAction<float>),
    };
    ```

- 打包出现的报错：

  - ```api
    Assets\XLua\Gen\UnityEngine_LightWrap.cs(767,60): error CS1061: 'Light' does not contain a definition for 'shadowRadius' and no accessible extension method 'shadowRadius' accepting a first argument of type 'Light' could be found (are you missing a using directive or an assembly reference?)
    ```

  - ```csharp
    //黑名单加入
    //Xlua->Editor->ExampleConfig.cs
    new List<string>(){"UnityEngine.Light","shadowRadius"},
    new List<string>(){"UnityEngine.Light","SetLightDirty"},
    new List<string>(){"UnityEngine.Light","shadowAngle"},
    ```

- 不可重复加载Bundle资源，定义已加载Bundle资源集合。

- 修改ResourceManager在每次加载Bundle时，先从集合中获取，若没有再添加加载。

### 事件管理

- 通过EventManager中m_Events<int, EventHandler>字典管理所有的事件
- m_Events字典每个id可以通过多播委托存放多个事件，通过EventManager中的函数进行订阅、取消订阅、执行等操作
- 比如GameStart中Init中的回调函数就通过EventManager来管理

### UnityExtension

- 在程序结束时，LuaEnv虚拟机先关闭了，导致Lua中的事件监听没释放，会产生报错，所以不能直接添加监听。

- 所以在Lua中button调用下面的监听事件（slider同理）：

  - ```csharp
    [LuaCallCSharp]
    public static class UnityEx
    {
        public static void OnClickSet(this Button button, object callback)
        {
    LuaFunction func = callback as LuaFunction;
    button.onClick.RemoveAllListeners();
    button.onClick.AddListener(
        () =>
        {
    func?.Call();
        });
        }
    }
    ```


### 对象池

- 对象池作用：减少对象创建和销毁的次数，让对象可以重复使用
- 对象的特点：
    - 使用频率高
    - 使用次数多

- 对象类型：
    - AssetBundle
    - GameObject（UI、Entity、Sound等）

- 传统的对象池：
    - 原理：先创建一定数量的对象，使用时从池子里取，用完了再还回去
    - 特点：相同对象，多次使用
    - 问题：只能创建相同类型的对象

- 我们使用的对象池：
    - 特点：
        - 对象是可以多种类型的
        - 短时间内可以重复使用
        - 过期自动销毁

    - 设计原理：池内不创建对象，对象在外部创建，使用完放入对象池，再次使用时从对象池里取

- PoolBase：
    - 对象池的基类
    - 定义PoolObject的集合，则为对象池集合
    - 此基类记录释放资源时间

- PoolObject：
    - 实际对对象的绑定
    - 记录绑定的对象、名字、最后一次使用的时间

- GameObjectPool、AssetPool：
    - PoolBase的派生类
    - 重写基类的回收和取出方法
    - 取出：
        - 调用基类中的方法，得到基类中对象池的目标对象，然后将此对象转化为GameObject类型。

    - 回收：
        - 将被回收的对象转化为GameObject类型
        - 设置父节点在对应对象池下
        - 调用基类的回收方法，将此对象纳入PoolObject对象池集合中

- PoolManager：
    - 提供创建实体对象池和资源对象池的方法，传入T类型对象，设置对象池存储的类型
    - 将创建的对象池，设置在Pool节点下，命名为对象池对应名称，将对象池纳入对象池总集合

- 概述：
    - 在对象调用Close方法时，不直接销毁对象，而是调用目标对象池的回收方法，将此对象存入对象池中。
    - 在创建对象，调用Open方法时。会先从对象池中查询是否存在对应对象，若存在则直接从对象池中返回对象。

### AssetBundle的卸载

<img src="https://lengyueling.oss-cn-shenzhen.aliyuncs.com/image-20220714000157265.png" alt="image-20220714000157265" style="zoom:67%;" /> 

- 在对象池中删除对象时，执行bundle依赖引用计数，将对象名字传入。
- 获取bundleName之后，在已经加载Bundle资源中获取对象，并减少引用次数。
- 若引用次数为0，则将此bundle纳入对象池中。
- 待对象池释放对象时，调用bundle卸载若在对象释放前，重新加载对象，则直接从对象池中获取。

AssetPool的作用：

<img src="https://lengyueling.oss-cn-shenzhen.aliyuncs.com/image-20220714000252117.png" alt="image-20220714000252117" style="zoom:67%;" /> 

### 网络模块

#### 编译Xlua第三方库

- 网络模块组成：

  - 通信协议：用于服务器和客户端通信的数据格式
    - protobuf
    - sproto
    - pbc
    - pblua
    - json
    - cjson
    - ...
  - C#客户端
  - Lua客户端
  - 简单服务器

- 步骤：

  - 添加扩展&编译：

    - https://github.com/chexiongsheng/build_xlua_with_libs中集成了常用的库，可以直接使用
    - 注意要使用到cmake来编译

  - C#侧集成：

    - 在LuaDLL中加入一个静态函数进行初始化

    - ```csharp
      [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
      public static extern int luaopen_rapidjson(System.IntPtr L);
      
      [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
      public static int LoadRapidJson(System.IntPtr L)
      {
          return luaopen_rapidjson(L);
      }
      ```

    - 在LuaEnv中调用AddBuildin（luaenv.AddBuildin("rapidjson", LuaDLL.Lua.LoadRapidJson);）

    - ```lua
      --lua rapidjson测试
      local rapidjson = require('rapidjson')
      --将json解析为lua
      local t = rapidjson.decode('{"a":123}')
      print(t.a)
      t.a = 456
      --将lua解析成json
      local s = rapidjson.encode(t)
      print('json', s)
      ```

#### 网络C#客户端

- 需要做的工作：
  - 服务器连接
  - 消息发送
  - 消息接收
  - 数据解析

#### 网络Lua客户端

- 功能模块化：
  - 消息注册
  - 消息发送
  - 消息接收
- 模块管理器：
  - 模块初始化
  - 模块获取
  - 消息接收
  - 消息发送

### 真机测试

- 启动热更新流程

- 制作Loading界面

- 打包APK：https://blog.csdn.net/qq_36480278/article/details/109803833

## 项目工程

https://github.com/lengyueling/XluaFramework
