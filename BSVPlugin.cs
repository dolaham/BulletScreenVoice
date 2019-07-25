using BilibiliDM_PluginFramework;
using Force.Crc32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulletScreenVoice
{
    public class BSVPlugin: DMPlugin
	{
		class TTSTask
		{
			internal enum State
			{
				Queuing,
				Translating,
				Translated,
				Playing,
				Fin
			}

			internal State state;
			internal string text;
			internal string filePath;
		}

		public static BSVPlugin instance;

		string baseDir;
		string dataDir;
		string voicesDir;
		string configFilePath;

		Config config;

		bool needInitAudio = true;
		bool needInitTTS = true;

		List<TTSTask> taskQueue = new List<TTSTask>();

		public BSVPlugin()
		{
			instance = this;

			this.PluginName = "弹幕语音播报";
			this.PluginVer = "v0.0.1";
			this.PluginDesc = "读出观众发送的弹幕文字";
			this.PluginAuth = "毘耶离";
			this.PluginCont = "dolaham@qq.com";

			this.Connected += BSVPlugin_Connected;
			this.Disconnected += BSVPlugin_Disconnected;
			this.ReceivedDanmaku += BSVPlugin_ReceivedDanmaku;
			this.ReceivedRoomCount += BSVPlugin_ReceivedRoomCount;

			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			baseDir = Path.GetDirectoryName(path);

			dataDir = baseDir + "\\bsv\\";
			voicesDir = dataDir + "voices\\";
			configFilePath = dataDir + "config.json";

			config = Config.load(configFilePath);
			if (config == null)
			{
				config = new Config();
			}

			if (!AudioService.findAudioDevice(config.audioDeviceId) && AudioService.allDevices.Length > 0)
			{
				config.audioDeviceId = AudioService.allDevices[0];
			}
		}


		private void BSVPlugin_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
		{
		}

		private void BSVPlugin_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
		{
			switch(e.Danmaku.MsgType)
			{
				case MsgTypeEnum.Comment:
					{
						string str = string.Format("{0}说：{1}", e.Danmaku.UserName, e.Danmaku.CommentText);
						addTTSTask(str);
						break;
					}

				case MsgTypeEnum.GiftSend:
					{
						string str = string.Format("感谢{0}赠送{1}{2}个", e.Danmaku.UserName, e.Danmaku.GiftName, e.Danmaku.GiftCount);
						addTTSTask(str);
						break;
					}

				case MsgTypeEnum.Welcome:
					{
						string str = string.Format("欢迎老爷{0}进入直播间，喜欢您就关注一下呗", e.Danmaku.UserName);
						addTTSTask(str);
						break;
					}

				case MsgTypeEnum.LiveStart:
					addTTSTask("直播开始");
					break;

				case MsgTypeEnum.LiveEnd:
					addTTSTask("直播结束");
					break;

				case MsgTypeEnum.WelcomeGuard:
					{
						string str = string.Format("欢迎{0}进入直播间，喜欢您就关注一下呗", e.Danmaku.UserName);
						addTTSTask(str);
					}
					break;

				case MsgTypeEnum.GuardBuy:
					break;
			}
		}

		private void BSVPlugin_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
		{
			if(config.readConnect)
			{
				string str = config.templateConnect.Replace("{roomId}", e.roomid.ToString());
				addTTSTask(str);
			}
		}

		private void BSVPlugin_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
		{
			if(config.readDisconnect)
			{
				string str = config.templateDisconnect.Replace("{err}", e.Error.Message);
				addTTSTask(str);
			}
		}

		public override void Start()
		{
			base.Start();
			//請勿使用任何阻塞方法

			needInitAudio = true;
			needInitTTS = true;
		}

		public override void Stop()
		{
			base.Stop();
			//請勿使用任何阻塞方法

			AudioService.playStoppedEvent -= onPlayFinished;
			AudioService.uninit();

			TTSService.uninit();
		}

		public override void Admin()
		{
			base.Admin();

			AdminForm form = new AdminForm();
			form.displayConfig(config);
			DialogResult result = form.ShowDialog();
			Log("Admin: " + result);

			if(result == DialogResult.OK)
			{
				if (!Directory.Exists(dataDir))
				{
					Directory.CreateDirectory(dataDir);
				}

				string oldAudioDeviceGuid = config.audioDeviceId;

				form.grabConfig(config);
				Config.save(config, configFilePath);

				if(oldAudioDeviceGuid != config.audioDeviceId)
				{
					needInitAudio = true;
				}

				if(config.useCustomSecret)
				{
					needInitTTS = true;
				}
			}
		}

		// 添加一个文本转语音任务
		void addTTSTask(string text)
		{
			if(string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
			{
				return;
			}

			TTSTask task = new TTSTask() { state = TTSTask.State.Queuing, text = text };

			byte[] bytes = Encoding.UTF8.GetBytes(text);
			uint crc32 = Crc32Algorithm.Compute(bytes);

			task.filePath = string.Format("{0}{1}_{2}_{3}_{4}.mp3", voicesDir, crc32, config.volume, config.speed, config.voiceType);

			taskQueue.Add(task);

			updateTaskQueue();
		}

		// 开始文本转语音
		void translateTTSTask(TTSTask task)
		{
			task.state = TTSTask.State.Translating;

			if(needInitTTS)
			{
				needInitTTS = false;

				TTSService.uninit();
				string secretId = config.useCustomSecret ? config.secretId : TencentSecret.secretId;
				string secretKey = config.useCustomSecret ? config.secretKey : TencentSecret.secretKey;
				TTSService.init(secretId, secretKey);
			}

			TTSService.translate(task.text, config.volume, (int)config.speed, (int)config.voiceType, (text, audioData, err) =>
			{
				onTranslateFinished(err, task, audioData);
			});
		}

		// 文本转语音完毕
		void onTranslateFinished(string err, TTSTask task, byte[] audioData)
		{
			if(string.IsNullOrEmpty(err))
			{
				// 没有错误，转换成功

				try
				{
					if (File.Exists(task.filePath))
					{
						File.Delete(task.filePath);
					}

					if(!Directory.Exists(dataDir))
					{
						Directory.CreateDirectory(dataDir);
					}
					if(!Directory.Exists(voicesDir))
					{
						Directory.CreateDirectory(voicesDir);
					}

					// 声音数据写入文件
					File.WriteAllBytes(task.filePath, audioData);

					foreach(TTSTask itTask in taskQueue)
					{
						if(itTask.filePath == task.filePath)
						{
							itTask.state = TTSTask.State.Translated;
						}
					}
				}
				catch (Exception e)
				{
					Log(e.Message);

					task.state = TTSTask.State.Fin;
				}
			}
			else
			{
				// 有错误，转换失败

				Log(err);

				// 出错的任务直接设为 fin
				task.state = TTSTask.State.Fin;
			}

			translatingTasks.Remove(task.filePath);

			updateTaskQueue();
		}

		// 播放文本语音
		void playTTSTask(TTSTask task)
		{
			task.state = TTSTask.State.Playing;

			if (needInitAudio)
			{
				needInitAudio = false;

				AudioService.playStoppedEvent -= onPlayFinished;
				AudioService.uninit();
				AudioService.init(config);
				AudioService.playStoppedEvent += onPlayFinished;
			}

			AudioService.playAudio(task.filePath);
		}

		// 播放完毕
		void onPlayFinished()
		{
			foreach(TTSTask task in taskQueue)
			{
				if(task.state == TTSTask.State.Playing)
				{
					task.state = TTSTask.State.Fin;
				}
			}

			updateTaskQueue();
		}

		HashSet<string> translatingTasks = new HashSet<string>();

		// 更新文本转语音任务队列
		void updateTaskQueue()
		{
			for(int i = 0; i < taskQueue.Count; )
			{
				TTSTask task = taskQueue[i];
				bool removed = false;

				switch(task.state)
				{
					case TTSTask.State.Queuing:
						{
							if(File.Exists(task.filePath))
							{
								task.state = TTSTask.State.Translated;

								if (i == 0)
								{
									playTTSTask(task);
								}
							}
							else
							{
								if(!translatingTasks.Contains(task.filePath))
								{
									translatingTasks.Add(task.filePath);
									translateTTSTask(task);
								}
							}
						}
						
						break;

					case TTSTask.State.Translating:
						break;

					case TTSTask.State.Translated:
						if(i == 0)
						{
							playTTSTask(task);
						}
						break;

					case TTSTask.State.Playing:
						break;

					case TTSTask.State.Fin:
						taskQueue.RemoveAt(i);
						removed = true;
						break;
				}

				if(!removed)
				{
					++i;
				}
			}
		}

		public void deleteVoices()
		{
			try
			{
				if (Directory.Exists(voicesDir))
				{
					string[] files = Directory.GetFiles(voicesDir, "*.*");
					foreach (string file in files)
					{
						File.Delete(file);
					}
					Directory.Delete(voicesDir);
				}
			}
			catch (Exception e)
			{
				Log(e.Message);
			}
		}
	}
}
