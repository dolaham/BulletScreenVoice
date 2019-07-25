using System;
using System.Threading.Tasks;
using TencentCloud.Aai.V20180522;
using TencentCloud.Aai.V20180522.Models;
using TencentCloud.Common;
using TencentCloud.Common.Profile;

public class TTSService
{
	public delegate void TranslateCallback(string text, byte[] audioData, string err);

	static AaiClient client;
	static int sessionId;

	public static bool init(string secretId, string secretKey)
	{
		Credential cre = new Credential() { SecretId = secretId, SecretKey = secretKey };
		ClientProfile clientProfile = new ClientProfile();
		HttpProfile httpProfile = new HttpProfile();
		httpProfile.Endpoint = ("aai.tencentcloudapi.com");
		clientProfile.HttpProfile = httpProfile;

		client = new AaiClient(cre, "ap-beijing", clientProfile);

		return true;
	}

	public static void uninit()
	{
	}

	public static async void translate(string text, int volume, int speed, int voiceType, TranslateCallback cb)
	{
		byte[] audioData = null;
		string err = null;

		try
		{
			TextToVoiceRequest req = new TextToVoiceRequest();
			req.Text = text;
			req.SessionId = Convert.ToString(sessionId++);
			req.ModelType = 1;
			req.Volume = volume;
			req.Speed = speed;
			req.VoiceType = voiceType;
			req.Codec = "mp3";

			TextToVoiceResponse res = await client.TextToVoice(req);

			audioData = Convert.FromBase64String(res.Audio);
		}
		catch (Exception e)
		{
			err = e.Message;
		}

		cb?.Invoke(text, audioData, err);
	}
}