using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringExtension
{
    public static string ConvertEncoding(this string val, Encoding toEncoding)
    {
        byte[] fromBytes = toEncoding.GetBytes(val);
        return toEncoding.GetString(fromBytes);
    }


}
