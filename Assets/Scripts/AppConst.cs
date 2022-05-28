using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum GameMode
{
    EditorMode,
    PackageBundle,
    UpdateMode
}

public class AppConst
{
    public const string BundleExtension = ".ab";
    public const string FileListName = "filelist.txt";
    /// <summary>
    /// 游戏模式
    /// </summary>
    public static GameMode GameMode = GameMode.EditorMode;
    /// <summary>
    /// 资源服务器目录
    /// </summary>
    public const string ResourceUrl = "http://127.0.0.1:4579/AssetBundles/";
}
