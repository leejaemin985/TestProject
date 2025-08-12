using System;
using Fusion;

public static class SessionMetaReader
{
    public static bool TryGetInt(SessionInfo info, SessionMetaKeys metaKey, out int value)
    {
        value = int.MinValue;
        
        var dict = info?.Properties;
        if (dict == null) return false;
    
        if (!dict.TryGetValue(metaKey.key, out var sp)) return false;

        try
        {
            value = (int)sp;
            return true;
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError($"Failed Read Session MetaData ({e})");
            return false;
        }
    }

    public static bool IsLock(SessionInfo info) => TryGetInt(info, SessionMetaKeys.Lock, out int value) && value == 1;


}