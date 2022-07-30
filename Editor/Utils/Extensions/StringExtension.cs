using System.Text;

public static class StringExtension
{
    /// <summary>
    /// 文字列のエンコード変更
    /// </summary>
    /// <param name="val"></param>
    /// <param name="toEncoding"></param>
    /// <returns></returns>
    public static string ConvertEncoding(this string val, Encoding toEncoding)
    {
        byte[] fromBytes = toEncoding.GetBytes(val);
        return toEncoding.GetString(fromBytes);
    }



    public static string ToTopUpper(this string val)
    {
        return val.Substring(0, 1).ToUpper() + val.Substring(1);
    }

}
