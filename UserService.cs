using System.Collections.Generic;

public static class UserService
{
	public class UserInfo
	{
		public long id;
		public string name;
		public UserLevel level;
		public bool isMilord;
		public bool isAdmin;
	}

	static Dictionary<long, UserInfo> userDict;

	public static void init()
	{
		userDict = new Dictionary<long, UserInfo>();
	}

	public static UserInfo getUserInfo(long id)
	{
		UserInfo userInfo = null;
		userDict.TryGetValue(id, out userInfo);
		return userInfo;
	}

	public static UserInfo getOrCreateUserInfo(long id)
	{
		UserInfo userInfo = null;
		if(!userDict.TryGetValue(id, out userInfo))
		{
			userInfo = new UserInfo() { id = id };
			userDict.Add(id, userInfo);
		}
		return userInfo;
	}
}