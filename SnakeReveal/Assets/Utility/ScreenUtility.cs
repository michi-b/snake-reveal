using UnityEngine;

namespace Utility
{
    public static class ScreenUtility
    {
        /// <summary>
        /// exact DPI of the device on android, instead of "logical bucket DPI" used by Unity
        /// <see href="https://docs.unity3d.com/ScriptReference/Screen-dpi.html"/>
        /// <see href="https://discussions.unity.com/t/is-there-a-way-to-determine-android-physical-screen-size/27597"/>
        /// <see href="https://developer.android.com/reference/android/util/DisplayMetrics#density"/>
        /// </summary>
        public static Vector2 Dpi { get; private set; }

        static ScreenUtility()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                using var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics");
                using var activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                using var windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager");
                using var displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay");
                displayInstance.Call("getMetrics", metricsInstance);
                Dpi = new Vector2(metricsInstance.Get<float>("xdpi"), metricsInstance.Get<float>("ydpi"));
            }
            else
            {
                Dpi = new Vector2(96f, 96f);
            }
        }
    }
}