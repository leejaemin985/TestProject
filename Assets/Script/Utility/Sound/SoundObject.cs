using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

namespace Utility.Sound
{
	[RequireComponent(typeof(AudioSource))]
	public abstract class SoundObject<TEnum> : MonoBehaviour where TEnum : Enum
	{
		private bool initialized = false;
		private AudioSource audioSource = default;

		public AudioSource Audio
		{
			get
			{
				if (initialized == false) throw new InvalidOperationException("SoundObject is not initialized.");
				if (audioSource==null)
				{
					audioSource = GetComponent<AudioSource>();
				}
				return audioSource;
			}
		}

		public abstract TEnum SoundType { get; }

		public virtual void Initialize(AudioMixerGroup outputGroup)
		{
			if (outputGroup == null) return;

			initialized = true;
			Audio.outputAudioMixerGroup = outputGroup;
		}

		public void PlayOneShot(AudioClip audioClip)
		{
			Audio.PlayOneShot(audioClip);
		}
	}
}