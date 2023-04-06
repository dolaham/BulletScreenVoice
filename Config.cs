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
	public static readonly int[] VoiceType =
    {
        0,  // 亲和女声（默认）
		1,  // 亲和男声
		2,  // 成熟男声
		3,  // 活力男声
		4,  // 温暖女声
		5,  // 情感女声
		6,  // 情感男声
		10510000,
		1001,
		1002,
		1003,
		1004,
		1005,
		1007,
		1008,
		1009,
		1010,
		1017,
		1018,
		101001,
		101002,
		101003,
		101004,
		101005,
		101006,
		101007,
		101008,
		101009,
		101010,
		101011,
		101012,
		101013,
		101014,
		101015,
		101016,
		101017,
		101018,
		101019,
		101020,
		101021,
		101022,
		101023,
		101024,
		101025,
		101026,
		101027,
		101028,
		101029,
		101030,
		101031,
		101032,
		101033,
		101034,
		101035,
		101040,
		101050,
		101051,
		101052,
		101053,
		101054,
		101055,
		101056,
    };

    public static readonly string[] VoiceTypeNames =
	{
		"亲和女声（默认）",
		"亲和男声",
		"成熟男声",
		"活力男声",
		"温暖女声",
		"情感女声",
		"情感男声",
        "智逍遥，阅读男声",
        "智瑜，情感女声",
        "智聆，通用女声",
        "智美，客服女声",
        "智云，通用男声",
        "智莉，通用女声",
        "智娜，客服女声",
        "智琪，客服女声",
        "智芸，知性女声",
        "智华，通用男声",
        "智蓉，情感女声",
        "智靖，情感男声",
        "智瑜，情感女声",
        "智聆，通用女声",
        "智美，客服女声",
        "智云，通用男声",
        "智莉，通用女声",
        "智言，助手女声",
        "智娜，客服女声",
        "智琪，客服女声",
        "智芸，知性女声",
        "智华，通用男声",
        "智燕，新闻女声",
        "智丹，新闻女声",
        "智辉，新闻男声",
        "智宁，新闻男声",
        "智萌，男童声",
        "智甜，女童声",
        "智蓉，情感女声",
        "智靖，情感男声",
        "智彤，粤语女声",
        "智刚，新闻男声",
        "智瑞，新闻男声",
        "智虹，新闻女声",
        "智萱，聊天女声",
        "智皓，聊天男声",
        "智薇，聊天女声",
        "智希，通用女声",
        "智梅，通用女声",
        "智洁，通用女声",
        "智凯，通用男声",
        "智柯，通用男声",
        "智奎，通用男声",
        "智芳，通用女声",
        "智蓓，客服女声",
        "智莲，通用女声",
        "智依，通用女声",
        "智川，四川女声",
        "WeJack，英文男声",
        "WeRose，英文女声",
        "智味，通用男声",
        "智方，通用男声",
        "智友，通用男声",
        "智付，通用女声",
        "智林，东北男声",
    };

	public class UserConfig
	{
		public bool readWelcome = true;  // 是否读出欢迎信息
		public string templateWelcome = "欢迎{user}光临";  // 欢迎模板

		public bool readText = true;  // 是否读出文本弹幕
		public string templateText = "{user}说：{text}";  // 文本弹幕模板

		public bool readGift = true;  // 是否读出礼物信息
		public string templateGift = "感谢{user}赠送{gift}{count}个";  // 礼物模板
		
		public bool readTicket = true;  // 是否读出购买船票
		public string templateTicket = "感谢{user}购买船票";
	}

	public UserConfig[] userConfigs = new UserConfig[(int)UserConfigIndex.Count];

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
	public int voiceType = 0;  // 声音类型

	public double nameInterval = 0;

	public bool useCustomSecret = false;
	public string secretId = "";
	public string secretKey = "";

	public Config()
	{
		for(int i = 0; i < (int)UserConfigIndex.Count; ++i)
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