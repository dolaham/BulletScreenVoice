using NAudio.Wave;
using System;
using System.Linq;

class AutoDisposeWaveProvider : IWaveProvider
{
	WaveStream stream;
	WaveFormat format;

	public WaveFormat WaveFormat { get { return format; } }

	public AutoDisposeWaveProvider(WaveStream waveStream)
	{
		stream = waveStream;
		format = waveStream.WaveFormat;
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		int readCount = 0;

		if(stream != null)
		{
			readCount = stream.Read(buffer, offset, count);
			if (readCount == 0)
			{
				stream.Dispose();
				stream = null;
			}
		}

		return readCount;
	}
}

// 音频服务
public class AudioService
{
	public delegate void PlayStoppedCallback();

	// 所有的音频设备
	static string[] _allDevices;
	public static string[] allDevices
	{
		get
		{
			if(_allDevices == null)
			{
				grabDevices();
			}
			return _allDevices;
		}
	}

	// 当前使用的音频设备
	static IWavePlayer device;
	public static event PlayStoppedCallback playStoppedEvent;

	public static void grabDevices()
	{
		_allDevices = new string[WaveOut.DeviceCount];
		for(int i = 0; i < _allDevices.Length; ++i)
		{
			WaveOutCapabilities cap = WaveOut.GetCapabilities(i);
			_allDevices[i] = cap.ProductName;
		}
	}

	// 初始化
	public static bool init(Config cfg)
	{
		if(allDevices == null || allDevices.Length == 0)
		{
			return false;
		}

		bool foundDevice = false;
		int deviceIndex = -1;

		for (int i = 0; i < allDevices.Length; ++i)
		{
			string itDev = allDevices[i];
			if(itDev == cfg.audioDeviceId)
			{
				foundDevice = true;
				deviceIndex = i;
				break;
			}
		}

		if(!foundDevice)
		{
			deviceIndex = 0;
			cfg.audioDeviceId = allDevices[deviceIndex];
		}

		WaveOutEvent wavDev;
		device = wavDev = new WaveOutEvent();
		wavDev.DeviceNumber = deviceIndex;
		device.PlaybackStopped += onPlaybackStopped;

		return true;
	}

	public static void uninit()
	{
		if(device == null)
		{
			return;
		}

		device.PlaybackStopped -= onPlaybackStopped;
		device.Dispose();
		device = null;
	}

	public static bool findAudioDevice(string deviceId)
	{
		foreach(string itDev in allDevices)
		{
			if(itDev == deviceId)
			{
				return true;
			}
		}
		return false;
	}

	public static bool playAudio(string filePath)
	{
		if(device == null)
		{
			return false;
		}

		try
		{
			WaveStream stream = new AudioFileReader(filePath);
			AutoDisposeWaveProvider autoDisposeWave = new AutoDisposeWaveProvider(stream);
			device.Init(autoDisposeWave);
			device.Play();

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	static void onPlaybackStopped(object sender, StoppedEventArgs args)
	{
		playStoppedEvent?.Invoke();
	}
}