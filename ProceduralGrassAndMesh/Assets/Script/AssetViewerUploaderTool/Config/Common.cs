using System;

using System.Text.RegularExpressions;
using UnityEngine;

public static class Common
{
    public static int GetUnityCurrentVersion()
    {
        string currentVersion = Application.unityVersion;
        String numberOnly = Regex.Replace(currentVersion, "[^0-9_]+", string.Empty);

        int currVersionNumber;
        Int32.TryParse(numberOnly, out currVersionNumber);
      
        return currVersionNumber;
    }
}
