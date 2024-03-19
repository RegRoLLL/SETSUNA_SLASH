using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RegUtil
{
    /// <summary>
    /// <br>==============================</br>
    /// <br>必ずシーン内のオブジェクトにアタッチすること！！！</br>
    /// <br>==============================</br>
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class RegTimeKeeper:MonoBehaviour
    {
        private static float timeScale;
        private static bool forceSetMode;

        void Update()
        {
            UpdateTimeScale();

            Initialize();
        }

        static void UpdateTimeScale()
        {
            Time.timeScale = timeScale;
        }

        void Initialize()
        {
            timeScale = 1f;
            forceSetMode = false;
        }

        static public float MultipleTimeScale(float ratio)
        {
            if (forceSetMode) return timeScale;

            timeScale *= ratio;

            return timeScale;
        }

        static public void Pause()
        {
            timeScale = 0f;
        }

        static public void ForceSetTimeScale(float scale)
        {
            if (timeScale == 0) return;

            timeScale = scale;
            forceSetMode = true;
        }
    }
}