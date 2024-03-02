using UnityEngine;

namespace Utility
{
    public static class ApplicationUtility
    {
        public static void Quit(int exitCode)
        {
#if UNITY_EDITOR
            Debug.Log("Application quit with exit code " + exitCode);
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(exitCode);
#endif
        }
    }
}