namespace Utility.Sound
{
    public enum SoundType
    {
        Effect,
        BGM
    }

    public abstract class GameSoundObject : CommonSoundObject<SoundType>
    {

    }
}