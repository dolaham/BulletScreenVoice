using Newtonsoft.Json;
using System;
using System.IO;

public class Config
{
	// 语速定义（参见腾讯语音API文档 https://cloud.tencent.com/document/api/441/18086）
	public enum Speed
	{
		Speed_06 = -2,  // 0.6倍速
		Speed_08,  // 0.8倍速
		Speed_10,  // 正常倍速（默认）
		Speed_12,  // 1.2倍速
		Speed_15  // 1.5倍速
	}

	// 声音类型
	public enum VoiceType
	{
		AffinityFemale = 0,  // 亲和女声（默认）
		AffinityMale,  // 亲和男声
		MatureMale,  // 成熟男声
		EnergeticMale,  // 活力男声
		WarmFemale,  // 温暖女声
		EmotionalFemale,  // 情感女声
		EmotionalMale,  // 情感男声
		Count
	}

	public static readonly string[] VoiceTypeNames =
	{
		"亲和女声（默认）",
		"亲和男声",
		"成熟男声",
		"活力男声",
		"温暖女声",
		"情感女声",
		"情感男声"
	};

	public class UserConfig
	{
		public UserLevel userLevel = UserLevel.Common;

		public bool readWelcome = true;  // 是否读出欢迎信息
		public string templateWelcome = "欢迎{user}光临";  // 欢迎模板

		public bool readText = true;  // 是否读出文本弹幕
		public string templateText = "{user}说：{text}";  // 文本弹幕模板

		public bool readGift = true;  // 是否读出礼物信息
		public string templateGift = "感谢{user}赠送{gift}{count}个";  // 礼物模板
		
		public bool readTicket = true;  // 是否读出购买船票
		public string templateTicket = "感谢{user}购买船票";
	}

	public UserConfig[] userConfigs = new UserConfig[(int)UserLevel.Count];

	public bool readConnect = true;
	public string templateConnect = "已连接至房间{roomId}";

	public bool readDisconnect = true;
	public string templateDisconnect = "已断开连接。错误信息：{err}";

	public bool readLiveBegin = true;
	public string templateLiveBegin = "直播开始";

	public bool readLiveEnd = true;
	public string templateLiveEnd = "直播结束";

	public string audioDeviceId;  // 音频设备 guid

	public int volume = 0;  // 音量：0到10之间的整数，默认0
	public Speed speed = Speed.Speed_10;  // 语速
	public VoiceType voiceType = VoiceType.AffinityFemale;  // 声音类型

	public bool useCustomSecret = false;
	public string secretId = "";
	public string secretKey = "";

	public Config()
	{
		for(int i = 0; i < (int)UserLevel.Count; ++i)
		{
			userConfigs[i] = new UserConfig();
		}
	}

	public static Config load(string filePath)
	{
		try
		{
			if (!File.Exists(filePath))
			{
				return null;
			}

			string str = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

			JsonSerializer js = new JsonSerializer();
			Config cfg = js.Deserialize(new JsonTextReader(new StringReader(str)), typeof(Config)) as Config;
			return cfg;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static bool save(Config cfg, string filePath)
	{
		try
		{
			if(File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			StringWriter sw = new StringWriter();

			JsonSerializer js = new JsonSerializer();
			js.Serialize(new JsonTextWriter(sw), cfg);

			string str = sw.GetStringBuilder().ToString();

			File.WriteAllText(filePath, str, System.Text.Encoding.UTF8);

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}