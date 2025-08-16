public class SessionMetaKeys
{
    public readonly string key;

    public SessionMetaKeys(string key)
    {
        this.key = key;
    }

    public static SessionMetaKeys Lock = new("lc");
}