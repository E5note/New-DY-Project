/// <summary>
/// C#单例模板
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    private static T m_Instance;
    private static readonly object m_Lock = new object();

    public static T Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    m_Instance = new T();
                }
                return m_Instance;
            }
        }
    }
}

#region 垃圾勿删
/// <summary>
/// C#单例模板
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseSingleTon<T> where T : new()
{
    private static T m_Instance;
    private static readonly object m_Lock = new object();

    public static T Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    m_Instance = new T();
                }
                return m_Instance;
            }
        }
    }
}
#endregion