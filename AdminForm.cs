using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulletScreenVoice
{
	public partial class AdminForm : Form
	{
		internal class ReadControlPair
		{
			internal CheckBox checkBoxRead;
			internal TextBox textBoxTemplate;
		}
		internal class UserLevelConfigPage
		{
			internal ReadControlPair pairWelcome;
			internal ReadControlPair pairText;
			internal ReadControlPair pairGift;
			internal ReadControlPair pairTicket;
		}

		

		UserLevelConfigPage[] userLevelConfigPages = new UserLevelConfigPage[(int)UserConfigIndex.Count];

		public AdminForm()
		{
			InitializeComponent();

			grabUserLevelConfigControls();
			addUserLevelConfigCheckBoxHandles();
		}

		void grabUserLevelConfigControls()
		{
			userLevelConfigPages[(int)UserConfigIndex.Common] = new UserLevelConfigPage()
			{
				pairWelcome = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadWelcomeCommon,
					textBoxTemplate = textBoxWelcomeTemplateCommon
				}
				, pairText = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTextCommon,
					textBoxTemplate = textBoxTextTemplateCommon
				}
				, pairGift = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadGiftCommon,
					textBoxTemplate = textBoxGiftTemplateCommon
				}
				, pairTicket = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTicketCommon,
					textBoxTemplate = textBoxTicketTemplateCommon
				}
			};

			userLevelConfigPages[(int)UserConfigIndex.Milord] = new UserLevelConfigPage()
			{
				pairWelcome = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadWelcomeMilord,
					textBoxTemplate = textBoxWelcomeTemplateMilord
				}
				,
				pairText = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTextMilord,
					textBoxTemplate = textBoxTextTemplateMilord
				}
				,
				pairGift = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadGiftMilord,
					textBoxTemplate = textBoxGiftTemplateMilord
				}
				,
				pairTicket = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTicketMilord,
					textBoxTemplate = textBoxTicketTemplateMilord
				}
			};

			userLevelConfigPages[(int)UserConfigIndex.Admin] = new UserLevelConfigPage()
			{
				pairWelcome = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadWelcomeAdmin,
					textBoxTemplate = textBoxWelcomeTemplateAdmin
				}
				,
				pairText = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTextAdmin,
					textBoxTemplate = textBoxTextTemplateAdmin
				}
				,
				pairGift = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadGiftAdmin,
					textBoxTemplate = textBoxGiftTemplateAdmin
				}
				,
				pairTicket = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTicketAdmin,
					textBoxTemplate = textBoxTicketTemplateAdmin
				}
			};

			userLevelConfigPages[(int)UserConfigIndex.Viceroy] = new UserLevelConfigPage()
			{
				pairWelcome = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadWelcomeViceroy,
					textBoxTemplate = textBoxWelcomeTemplateViceroy
				}
				,
				pairText = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTextViceroy,
					textBoxTemplate = textBoxTextTemplateViceroy
				}
				,
				pairGift = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadGiftViceroy,
					textBoxTemplate = textBoxGiftTemplateViceroy
				}
				,
				pairTicket = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTicketViceroy,
					textBoxTemplate = textBoxTicketTemplateViceroy
				}
			};

			userLevelConfigPages[(int)UserConfigIndex.Governor] = new UserLevelConfigPage()
			{
				pairWelcome = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadWelcomeGovernor,
					textBoxTemplate = textBoxWelcomeTemplateGovernor
				}
				,
				pairText = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTextGovernor,
					textBoxTemplate = textBoxTextTemplateGovernor
				}
				,
				pairGift = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadGiftGovernor,
					textBoxTemplate = textBoxGiftTemplateGovernor
				}
				,
				pairTicket = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTicketGovernor,
					textBoxTemplate = textBoxTicketTemplateGovernor
				}
			};

			userLevelConfigPages[(int)UserConfigIndex.Captain] = new UserLevelConfigPage()
			{
				pairWelcome = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadWelcomeCaptain,
					textBoxTemplate = textBoxWelcomeTemplateCaptain
				}
				,
				pairText = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTextCaptain,
					textBoxTemplate = textBoxTextTemplateCaptain
				}
				,
				pairGift = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadGiftCaptain,
					textBoxTemplate = textBoxGiftTemplateCaptain
				}
				,
				pairTicket = new ReadControlPair()
				{
					checkBoxRead = checkBoxReadTicketCaptain,
					textBoxTemplate = textBoxTicketTemplateCaptain
				}
			};
		}

		void addUserLevelConfigCheckBoxHandles()
		{
			for(int i = 0; i < (int)UserConfigIndex.Count; ++i)
			{
				UserLevelConfigPage page = userLevelConfigPages[i];
				page.pairWelcome.checkBoxRead.CheckedChanged += new EventHandler((sender, e)=> { page.pairWelcome.textBoxTemplate.Enabled = page.pairWelcome.checkBoxRead.Checked; });
				page.pairText.checkBoxRead.CheckedChanged += new EventHandler((sender, e) => { page.pairText.textBoxTemplate.Enabled = page.pairText.checkBoxRead.Checked; });
				page.pairGift.checkBoxRead.CheckedChanged += new EventHandler((sender, e) => { page.pairGift.textBoxTemplate.Enabled = page.pairGift.checkBoxRead.Checked; });
				page.pairTicket.checkBoxRead.CheckedChanged += new EventHandler((sender, e) => { page.pairTicket.textBoxTemplate.Enabled = page.pairTicket.checkBoxRead.Checked; });
			}
		}

		// 显示配置
		public void displayConfig(Config cfg)
		{
			// 响应用户配置
			for(int i = 0; i < (int)UserConfigIndex.Count; ++i)
			{
				UserLevelConfigPage page = userLevelConfigPages[i];
				Config.UserConfig userCfg = cfg.userConfigs[i];

				page.pairWelcome.checkBoxRead.Checked = userCfg.readWelcome;
				page.pairWelcome.textBoxTemplate.Text = userCfg.templateWelcome;
				page.pairWelcome.textBoxTemplate.Enabled = userCfg.readWelcome;

				page.pairText.checkBoxRead.Checked = userCfg.readText;
				page.pairText.textBoxTemplate.Text = userCfg.templateText;
				page.pairText.textBoxTemplate.Enabled = userCfg.readText;

				page.pairGift.checkBoxRead.Checked = userCfg.readGift;
				page.pairGift.textBoxTemplate.Text = userCfg.templateGift;
				page.pairGift.textBoxTemplate.Enabled = userCfg.readGift;

				page.pairTicket.checkBoxRead.Checked = userCfg.readTicket;
				page.pairTicket.textBoxTemplate.Text = userCfg.templateTicket;
				page.pairTicket.textBoxTemplate.Enabled = userCfg.readTicket;
			}

			// 其它

			checkBoxReadConnect.Checked = cfg.readConnect;
			textBoxConnectTemplate.Text = cfg.templateConnect;

			checkBoxReadDisconnect.Checked = cfg.readDisconnect;
			textBoxDisconnectTemplate.Text = cfg.templateDisconnect;

			checkBoxReadLiveBegin.Checked = cfg.readLiveBegin;
			textBoxLiveBeginTemplate.Text = cfg.templateLiveBegin;

			checkBoxReadLiveEnd.Checked = cfg.readLiveEnd;
			textBoxLiveEndTemplate.Text = cfg.templateLiveEnd;

			// 音频配置

			// 音频设备列表
			comboAudioDevices.Items.Clear();
			int selectedIndex = -1;
			for(int i = 0; i < AudioService.allDevices.Length; ++i)
			{
				var audioDeviceId = AudioService.allDevices[i];
				comboAudioDevices.Items.Add(audioDeviceId);
				if(audioDeviceId == cfg.audioDeviceId)
				{
					selectedIndex = i;
				}
			}
			if(selectedIndex == -1)
			{
				selectedIndex = 0;
			}
			comboAudioDevices.SelectedIndex = selectedIndex;

			// 音量
			trackBarVolume.Value = cfg.volume;
			// 语速
			trackBarSpeed.Value = (int)cfg.speed;

			// 声音类型
			comboBoxVoiceTypes.Items.Clear();
			for(int i = 0; i < (int)Config.VoiceType.Count; ++i)
			{
				string voiceTypeName = Config.VoiceTypeNames[i];
				comboBoxVoiceTypes.Items.Add(voiceTypeName);
			}
			comboBoxVoiceTypes.SelectedIndex = (int)cfg.voiceType;

			// 高级

			checkBoxUseCustomeSecret.Checked = cfg.useCustomSecret;
			textBoxSecretId.Text = cfg.secretId;
			textBoxSecretKey.Text = cfg.secretKey;
			textBoxSecretId.Enabled = textBoxSecretKey.Enabled = cfg.useCustomSecret;
		}

		public void grabConfig(Config cfg)
		{
			// 响应用户配置
			for (int i = 0; i < (int)UserConfigIndex.Count; ++i)
			{
				UserLevelConfigPage page = userLevelConfigPages[i];
				Config.UserConfig userCfg = cfg.userConfigs[i];

				userCfg.readWelcome = page.pairWelcome.checkBoxRead.Checked;
				userCfg.templateWelcome = page.pairWelcome.textBoxTemplate.Text;

				userCfg.readText = page.pairText.checkBoxRead.Checked;
				userCfg.templateText = page.pairText.textBoxTemplate.Text;

				userCfg.readGift = page.pairGift.checkBoxRead.Checked;
				userCfg.templateGift = page.pairGift.textBoxTemplate.Text;

				userCfg.readTicket = page.pairTicket.checkBoxRead.Checked;
				userCfg.templateTicket = page.pairTicket.textBoxTemplate.Text;
			}

			// 其它

			cfg.readConnect = checkBoxReadConnect.Checked;
			cfg.templateConnect = textBoxConnectTemplate.Text;

			cfg.readDisconnect = checkBoxReadDisconnect.Checked;
			cfg.templateDisconnect = textBoxDisconnectTemplate.Text;

			cfg.readLiveBegin = checkBoxReadLiveBegin.Checked;
			cfg.templateLiveBegin = textBoxLiveBeginTemplate.Text;

			cfg.readLiveEnd = checkBoxReadLiveEnd.Checked;
			cfg.templateLiveEnd = textBoxLiveEndTemplate.Text;

			// 音频配置

			// 音频设备列表
			cfg.audioDeviceId = AudioService.allDevices[comboAudioDevices.SelectedIndex];

			// 音量
			cfg.volume = trackBarVolume.Value;
			// 语速
			cfg.speed = (Config.Speed)trackBarSpeed.Value;

			// 声音类型
			cfg.voiceType = (Config.VoiceType)comboBoxVoiceTypes.SelectedIndex;

			// 高级

			cfg.useCustomSecret = checkBoxUseCustomeSecret.Checked;
			cfg.secretId = textBoxSecretId.Text;
			cfg.secretKey = textBoxSecretKey.Text;
		}

		private void CheckBoxUseCustomeSecret_CheckedChanged(object sender, EventArgs e)
		{
			textBoxSecretId.Enabled = textBoxSecretKey.Enabled = checkBoxUseCustomeSecret.Checked;
		}

		private void ButtonOk_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void ButtonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void TabPage8_Paint(object sender, PaintEventArgs e)
		{
			string voicesDir = BSVPlugin.instance.voicesDir;
			textBoxVoicesPath.Text = voicesDir;

			buttonOpenVoicesPath.Enabled = Directory.Exists(voicesDir);

			int fileCount = 0;
			ulong fileSize = 0;
			if(Directory.Exists(voicesDir))
			{
				string[] fileArr = Directory.GetFiles(voicesDir, "*.*");
				fileCount = fileArr.Length;

				foreach(string filePath in fileArr)
				{
					FileInfo fileInf = new FileInfo(filePath);
					fileSize += (ulong)fileInf.Length;
				}
			}

			buttonDeleteVoiceFiles.Enabled = fileCount > 0;

			const ulong KB = 1024;
			const ulong MB = KB * 1024;
			const ulong GB = MB * 1024;

			string unitName = "字节";

			if(fileSize >= GB)
			{
				fileSize /= GB;
				unitName = "GB";
			}
			else if(fileSize >= MB)
			{
				fileSize /= MB;
				unitName = "MB";
			}
			else if(fileSize >= KB)
			{
				fileSize /= KB;
				unitName = "KB";
			}
			else
			{
				unitName = "字节";
			}

			labelVoicesStatistic.Text = string.Format("文件 {0} 个，共计 {1} {2}", fileCount, fileSize, unitName);
		}

		private void ButtonOpenVoicesPath_Click(object sender, EventArgs e)
		{
			string voicesPath = BSVPlugin.instance.voicesDir;
			if(Directory.Exists(voicesPath))
			{
				System.Diagnostics.Process.Start("explorer.exe", voicesPath);
			}
		}

		private void ButtonDeleteVoiceFiles_Click(object sender, EventArgs e)
		{
			BSVPlugin.instance.deleteVoices();

			Invalidate(true);
		}
	}
}
