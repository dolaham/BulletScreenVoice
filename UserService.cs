using System.Collections.Generic;

public static class UserService
{
	public class UserInfo
	{
		public int id;
		public string name;
		public UserLevel level;
		public bool isMilord;
		public bool isAdmin;
	}

	static Dictionary<int, UserInfo> userDict;

	public static void init()
	{
		userDict = new Dictionary<int, UserInfo>();
	}

	public static UserInfo getUserInfo(int id)
	{
		UserInfo userInfo = null;
		userDict.TryGetValue(id, out userInfo);
		return userInfo;
	}

	public static UserInfo getOrCreateUserInfo(int id)
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