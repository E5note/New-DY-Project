using UnityEngine;
/// <summary>
/// 当值类型的值等于这个值时，可以判定为我们未赋值。 因为值类型的默认值有时会等于我们想要赋的值，因此不好判断他是默认值还是我们主动赋值的值 
/// </summary>
public class DefaultDef
{
    public static int Int = 1589654874;
    public static float Float = 5.7895145f;
    public static double Double = 4.8965711589654;
    public static string String = "E5战未来";
    public static string Key = "加密密钥,你就是猜不到";

    public static Color Color = new Color(0.558424f, 0.61551f, 0.416645f, 0.6821542f);
    public static Color32 Color32 = new Color(138, 254, 69, 12);

    public static Vector2 Vector2 = new Vector2(6.874125f, 7.814144f);
    public static Vector3 Vector3 = new Vector3(2.54795146f, 7.8956543f, 4.87456934f);
    public static Vector4 Vector4 = new Vector4(2.54795146f, 7.8956543f, 4.87456934f, 8.5177954f);
    public static Vector2Int Vector2Int = new Vector2Int(25899664, 14587462);
    public static Vector3Int Vector3Int = new Vector3Int(25899664, 14587462, 589632114);

}
