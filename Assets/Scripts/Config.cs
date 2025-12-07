using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public static class Config
{
    //起動時
    [RuntimeInitializeOnLoadMethod()]
    static void Initialized()
    {
        /*システム設定*/
        Application.targetFrameRate = 60;//フレームレートを固定
    }
}
