using System;

using UnityEngine;
using UnityEngine.Audio;

namespace Utility.Sound
{
	public abstract class CommonAudioMixerController<TEnum> where TEnum : Enum
	{
		protected abstract string MIXER_PATH { get; }
		protected abstract string GROUP_BASE { get; } // {0}_Group
		protected abstract string VOLUME_BASE { get; } // {0}_Volume

		// AudioMixer 범위 특성상(-80dB ~ 0dB) 최솟값이 0.0001f여야함. (ex. Log(0.0001) * 20=> -80dB)
		private const float MIN_VOLUME = 0.0001f;
		private const float MAX_VOLUME = 1f;

		public static float ConvertVolumeToDecibel(float value) => Mathf.Log10(value) * 20f;
		public static float ConvertDecibelToVolume(float decibel) => Mathf.Pow(10f, decibel / 20f);

		protected AudioMixer mixer { get; private set; }

		public virtual void Initialize()
		{
			mixer = Resources.Load<AudioMixer>(MIXER_PATH);
			if (mixer == null)
			{
				Debug.LogError($"Failed to load mixer at path: {MIXER_PATH}");
				return;
			}
		}

		protected AudioMixerGroup GetAudioMixerGroup(TEnum val)
		{
			var group = mixer.FindMatchingGroups(string.Format(GROUP_BASE, val));

			if (group == null || group.Length == 0)
			{
				Debug.LogError($"AudioMixer failed find MixerGroup (groupname: {val.ToString()})");
				return null;
			}
			return group[0];
		}

		protected float GetMixerGroupVolume(TEnum val)
		{
			var contain = mixer.GetFloat(string.Format(VOLUME_BASE, val), out float getValue);
			return contain ? ConvertDecibelToVolume(getValue) : MAX_VOLUME;
		}

		protected void SetMixerGroupVolume(TEnum val, float volume)
		{
			volume = Mathf.Clamp(volume, MIN_VOLUME, MAX_VOLUME);
			mixer.SetFloat(string.Format(VOLUME_BASE, val), ConvertVolumeToDecibel(volume));
		}

		public void InitSoundObject(CommonSoundObject<TEnum> soundObject)
		{
			if (soundObject == null) return;
			soundObject.Initialize(GetAudioMixerGroup(soundObject.SoundType));
		}
	}
}