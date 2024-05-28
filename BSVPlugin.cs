using BilibiliDM_PluginFramework;
using Force.Crc32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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
		string pluginDir;
		string dllDir;

		public string voicesDir;
		string configFilePath;

		Config config;

		bool needInitAudio = true;
		bool needInitTTS = true;

		string roomId;

		List<TTSTask> taskQueue = new List<TTSTask>();

		public BSVPlugin()
		{
			instance = this;

			this.PluginName = "弹幕语音播报";
			this.PluginVer = "v0.0.3";
			this.PluginDesc = "读出观众发送的弹幕文字";
			this.PluginAuth = "毘耶离";
			this.PluginCont = "dolaham@qq.com";

            this.PropertyChanged += BSVPlugin_PropertyChanged;
			this.Connected += BSVPlugin_Connected;
			this.Disconnected += BSVPlugin_Disconnected;
			this.ReceivedDanmaku += BSVPlugin_ReceivedDanmaku;
			this.ReceivedRoomCount += BSVPlugin_ReceivedRoomCount;

			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			baseDir = Path.GetDirectoryName(path) + "\\";

			pluginDir = baseDir + "bsv\\";
			dllDir = pluginDir + "dll\\";

			loadDlls();

			voicesDir = pluginDir + "voices\\";
			configFilePath = pluginDir + "config.json";

			config = Config.load(configFilePath);
			if (config == null)
			{
				config = new Config();
			}

            if (!AudioService.findAudioDevice(config.audioDeviceId) && AudioService.allDevices.Length > 0)
            {
                config.audioDeviceId = AudioService.allDevices[0];
            }

            UserService.init();
		}

        private void BSVPlugin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			if(e.PropertyName == "Status")
			{
				if(this.Status)
				{
                    
                }
			}
        }

        bool loadDlls()
		{
			string[] dllFilenames = Directory.GetFiles(dllDir, "*.dll", SearchOption.AllDirectories);

			string curLoadingDllFilename = "";
			string curLoadingDllFullFilePath = "";

			try
			{
				for(int i = 0; i < dllFilenames.Length; ++i)
				{
					curLoadingDllFilename = dllFilenames[i];
					curLoadingDllFullFilePath = curLoadingDllFilename;
					Assembly.LoadFrom(curLoadingDllFullFilePath);
				}
			}
			catch (Exception e)
			{
				Log(e.Message);
				Log(string.Format("加载 {0} 时发生错误，请确定以下路径的文件存在：{1}", curLoadingDllFilename, curLoadingDllFullFilePath));

				return false;
			}

			return true;
		}

		// 连接至直播间
		private void BSVPlugin_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
		{
			roomId = e.roomid.ToString();

			if (config.readConnect)
			{
				string str = config.templateConnect.Replace("{roomId}", roomId);
				addTTSTask(str);
			}
        }

		// 与直播间断开连接
		private void BSVPlugin_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
		{
			if (config.readDisconnect)
			{
				string errMsg = e?.Error?.Message;
				string str = config.templateDisconnect.Replace("{err}", errMsg);
				addTTSTask(str);
			}
		}

		// 插件启用
		public override void Start()
		{
			base.Start();
			//請勿使用任何阻塞方法

			needInitAudio = true;
			needInitTTS = true;
		}

		// 插件停用
		public override void Stop()
		{
			base.Stop();
			//請勿使用任何阻塞方法

			AudioService.playStoppedEvent -= onPlayFinished;
			AudioService.uninit();

			TTSService.uninit();
		}

		// 打开插件设置界面
		public override void Admin()
		{
			base.Admin();

			AdminForm form = new AdminForm();
			form.displayConfig(config);
			DialogResult result = form.ShowDialog();
			Log("Admin: " + result);

			if (result == DialogResult.OK)
			{
				if (!Directory.Exists(pluginDir))
				{
					Directory.CreateDirectory(pluginDir);
				}

				string oldAudioDeviceGuid = config.audioDeviceId;

				form.grabConfig(config);
				Config.save(config, configFilePath);

				if (oldAudioDeviceGuid != config.audioDeviceId)
				{
					needInitAudio = true;
				}

				if (config.useCustomSecret)
				{
					needInitTTS = true;
				}
			}
		}

		// 收到直播间人气数据
		private void BSVPlugin_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
		{
		}

		// 上次弹幕用户
		string lastSayUserName;
		// 上次弹幕时间
		DateTime lastSayTime;

		// 上次礼物文本模板
        string lastGiftTemplate;
        // 上次礼物的用户id
        long lastGiftUserId;
		string lastGiftUserName;
		// 上次礼物名称
		string lastGiftName;
		// 上次礼物数量
		int lastGiftCount;

		// 收到弹幕、礼物等消息
		private void BSVPlugin_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
		{
			DanmakuModel dm = e.Danmaku;
			long userId = dm.UserID_long;

			switch(dm.MsgType)
			{
				case MsgTypeEnum.LiveStart:  // 直播开始
					if (config.readLiveBegin)
					{
						string str = makeStringFromTemplate(dm, config.templateLiveBegin);
						addTTSTask(str);
					}
					break;

				case MsgTypeEnum.LiveEnd:  // 直播结束
					if (config.readLiveEnd)
					{
						string str = makeStringFromTemplate(dm, config.templateLiveEnd);
						addTTSTask(str);
					}
					break;

				case MsgTypeEnum.Welcome:  // 有观众进入直播间
					{
						UserService.UserInfo userInfo = UserService.getOrCreateUserInfo(userId);
						userInfo.isMilord = dm.isVIP;

						int userCfgIndex = (int)getUserConfigIndex(userInfo);
						Config.UserConfig userCfg = config.userConfigs[userCfgIndex];
						if (userCfg.readWelcome)
						{
							string str = makeStringFromTemplate(dm, userCfg.templateWelcome);
							addTTSTask(str);
						}
					}
					break;

				case MsgTypeEnum.WelcomeGuard:  // 有普通观众或船员进入直播间
					{
						UserService.UserInfo userInfo = UserService.getOrCreateUserInfo(userId);
						userInfo.level = (UserLevel)dm.UserGuardLevel;

						int userCfgIndex = (int)getUserConfigIndex(userInfo);
						Config.UserConfig userCfg = config.userConfigs[userCfgIndex];
						if (userCfg.readWelcome)
						{
							string str = makeStringFromTemplate(dm, userCfg.templateWelcome);
							addTTSTask(str);
						}
					}
					break;

				case MsgTypeEnum.Comment:  // 弹幕文本
					{
						int userCfgIndex = dm.UserGuardLevel;
						if(userCfgIndex == (int)UserLevel.Common)
						{
							UserService.UserInfo userInfo = UserService.getUserInfo(userId);
							userCfgIndex = (int)getUserConfigIndex(userInfo);
						}

						Config.UserConfig userCfg = config.userConfigs[userCfgIndex];
						if(userCfg.readText)
						{
							string str = null;

							if(dm.UserName == lastSayUserName)
							{
								// 本次弹幕的用户与上一个弹幕的用户是同一个

								TimeSpan ts = DateTime.Now - lastSayTime;
								if(ts.TotalSeconds < config.nameInterval)
								{
									// 弹幕间隔小于指定值，不添加“用户说”前缀
									str = dm.CommentText;
								}
                            }

							if(str == null)
							{
                                str = makeStringFromTemplate(dm, userCfg.templateText);
                            }

							addTTSTask(str);

							lastSayUserName = dm.UserName;
							lastSayTime = DateTime.Now;
                        }
					}
					break;

				case MsgTypeEnum.GiftSend:  // 收到礼物
					{
						UserService.UserInfo userInfo = UserService.getUserInfo(userId);
						int userCfgIndex = (int)getUserConfigIndex(userInfo);
						Config.UserConfig userCfg = config.userConfigs[userCfgIndex];
						if(userCfg.readGift)
						{
							if(dm.UserID_long == lastGiftUserId && dm.GiftName == lastGiftName)
							{
								// 礼物用户与上一次礼物用户是同一个、且礼物相同，则合并礼物数量
								lastGiftCount += dm.GiftCount;

								// 延迟一段时间再一次性读出礼物信息
								makeDelayGiftVoiceTimer(1);
							}
							else
							{
								// 礼物用户与上次用户不同、或者礼物不同

								if(lastGiftUserId != 0)
								{
									// 还有待读的礼物信息，则现在读出

									clearDelayGiftVoiceTimer();

									string str = makeStringFromTemplate(lastGiftTemplate, lastGiftUserName, "", lastGiftName, lastGiftCount, "");
                                    addTTSTask(str);
                                }

								// 缓存礼物信息
								lastGiftTemplate = userCfg.templateGift;
                                lastGiftUserId = dm.UserID_long;
								lastGiftUserName = dm.UserName;
								lastGiftName = dm.GiftName;
								lastGiftCount = dm.GiftCount;

								// 延迟一段时间后再读出
								makeDelayGiftVoiceTimer(1);
                            }
						}
					}
					break;

				case MsgTypeEnum.GuardBuy:  // 购买船票
					{
						int userCfgIndex = dm.UserGuardLevel;
						if (userCfgIndex == (int)UserLevel.Common)
						{
							UserService.UserInfo userInfo = UserService.getUserInfo(userId);
							userCfgIndex = (int)getUserConfigIndex(userInfo);
						}

						Config.UserConfig userCfg = config.userConfigs[userCfgIndex];
						if(userCfg.readTicket)
						{
							string str = makeStringFromTemplate(dm, userCfg.templateTicket);
							addTTSTask(str);
						}
					}
					break;
			}

			// 互动消息
			switch(dm.InteractType)
			{
				case InteractTypeEnum.Follow:
					{
						// 关注
						if(config.bReadFollow)
						{
							string str = makeStringFromTemplate(dm, config.templateFollow);
							addTTSTask(str);
                        }
					}
					break;
			}
		}

		System.Timers.Timer timerDelayGiftVoice;

        void clearDelayGiftVoiceTimer()
		{
			if(timerDelayGiftVoice != null)
			{
				timerDelayGiftVoice.Stop();
				timerDelayGiftVoice.Close();
				timerDelayGiftVoice = null;
			}
		}

		void makeDelayGiftVoiceTimer(float delay)
		{
			clearDelayGiftVoiceTimer();

            timerDelayGiftVoice = new System.Timers.Timer();
			timerDelayGiftVoice.AutoReset = false;

            timerDelayGiftVoice.Interval = (int)(delay * 1000);
			timerDelayGiftVoice.Elapsed += DelayGiftVoiceFunc;

			timerDelayGiftVoice.Start();
        }

		void DelayGiftVoiceFunc(object sender, EventArgs e)
		{
			if(lastGiftUserId != 0)
			{
                string str = makeStringFromTemplate(lastGiftTemplate, lastGiftUserName, "", lastGiftName, lastGiftCount, "");
                addTTSTask(str);

				lastGiftUserId = 0;
                lastGiftUserName = "";
                lastGiftName = "";
				lastGiftCount = 0;
            }

			clearDelayGiftVoiceTimer();
        }

        static UserConfigIndex getUserConfigIndex(int id)
		{
			UserService.UserInfo userInfo = UserService.getUserInfo(id);
			return getUserConfigIndex(userInfo);
		}

		static UserConfigIndex getUserConfigIndex(UserService.UserInfo userInfo)
		{
			if(userInfo == null)
			{
				return UserConfigIndex.Common;
			}

			if(userInfo.level != UserLevel.Common)
			{
				return (UserConfigIndex)userInfo.level;
			}

			if(userInfo.isAdmin)
			{
				return UserConfigIndex.Admin;
			}
			else if(userInfo.isMilord)
			{
				return UserConfigIndex.Milord;
			}

			return UserConfigIndex.Common;
		}

		// 从模板构造字符串
		string makeStringFromTemplate(DanmakuModel dm, string template)
		{
			string userName = dm.UserName;
			if(string.IsNullOrEmpty(userName))
			{
				UserService.UserInfo userInfo = UserService.getUserInfo(dm.UserID_long);
				if(userInfo != null)
				{
					userName = userInfo.name;
				}
			}

			string roomId = dm.roomID;
			if(string.IsNullOrEmpty(roomId))
			{
				roomId = this.roomId;
			}

			return makeStringFromTemplate(template, userName, dm.CommentText, dm.GiftName, dm.GiftCount, roomId);
		}

		string makeStringFromTemplate(string template, string userName, string commentText, string giftName, int giftCount, string roomId)
		{
            string str = template.Replace("{user}", userName);
            str = str.Replace("{text}", commentText);
            str = str.Replace("{gift}", giftName);
            str = str.Replace("{count}", giftCount.ToString());
            str = str.Replace("{roomId}", roomId);

            return str;
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

					if(!Directory.Exists(pluginDir))
					{
						Directory.CreateDirectory(pluginDir);
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
