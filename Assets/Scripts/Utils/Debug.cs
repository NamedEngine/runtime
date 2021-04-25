using System;
using System.Collections;

namespace Utils {
    public static class Debug {
        public static void LogArr(string[] arr, string sep = ", ") {
            UnityEngine.Debug.Log(String.Join(sep, arr));
        }
    }
}