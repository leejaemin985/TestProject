using UnityEngine;
using Utility.Sound;

namespace GameOption
{
    public class GameOption : MonoSingleton<GameOption>
    {
        [SerializeField] private GameOptionUI optionUI;


        private float mouseSensitive;

        private InputSystemSettings inputSystemSetter;

        private GameAudioMixerController audioMixerController => GameAudioMixerController.Instance;

        //public float GetBGMSoundVolume() => GameAudioMixerController.Instance.GetBGMVolume();
        //public float GetEffectSoundVolume() => GameAudioMixerController.Instance.GetEffectVolume();
        //public void SetBGMSoundVolume(float targetVolume) => GameAudioMixerController.Instance.SetBGMVolume(targetVolume);
        //public void SetEffectSoundVolume(float targetVolume) => GameAudioMixerController.Instance.SetEffectVolume(targetVolume);
    }
}