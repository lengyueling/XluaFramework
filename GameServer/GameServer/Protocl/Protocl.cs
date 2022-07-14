using System;
using System.Collections.Generic;
using GameServer;

public enum ErrorCode
{
    None                    = 0,
    AccountRepetition       = 1001, //注册失败,角色名重复
    NotAccount              = 1002, //登陆失败,用户名不存在
    PasswordWrong           = 1003, //登陆失败,密码错误
    RoleNameRepet           = 1004, //角色名已存在
    RoleIndexError          = 1005, //进入游戏的角色索引不正确
}

public class ResponseBase
{
    //错误码
    public ErrorCode code = ErrorCode.None;
}

public class Test
{
    public int id;
    public string user;
    public string password;
    public List<int> listTest;
}

public class TestRes:ResponseBase
{
    public Vector3 v3;
    public int id;
    public string user;
    public string password;
    public List<int> listTest;
}

public class Login
{
    public string user;
    public string password;
}

public class LoginRes : ResponseBase
{
    
}
