﻿// 用户等级
public enum UserLevel
{
	Common = 0,  // 一般
	Viceroy,  // 总督
	Governor,  // 提督
	Captain  // 舰长
}

public enum UserConfigIndex
{
	Common = 0,  // 一般
	Viceroy,  // 总督
	Governor,  // 提督
	Captain,  // 舰长
	Milord,  // 老爷
	Admin,  // 房管
	Count
}

// 需要去腾讯云申请 https://cloud.tencent.com/
internal class TencentSecret
{
	internal static string secretId = "";
	internal static string secretKey = "";
}