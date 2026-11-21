// Helper kiểm tra tự viết (KHÔNG phải code từ APK). Chạy batch:
//   Unity -batchmode -projectPath REPO -executeMethod PlayProbe.Run -logFile LOG
// Mở MainScene, vào Play mode N giây (env PROBE_SECONDS, mặc định 40),
// ghi exception/error vào Temp/probe_errors.txt (gộp trùng, đếm số lần), rồi thoát.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayProbe
{
    const string Key = "PlayProbe.Active";
    static double _start;
    static readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
    static readonly List<string> _order = new List<string>();

    static PlayProbe()
    {
        bool env = Environment.GetEnvironmentVariable("PLAYPROBE") == "1";
        if (!env && !SessionState.GetBool(Key, false)) return;
        if (env && !SessionState.GetBool(Key + ".Started", false))
        {
            SessionState.SetBool(Key + ".Started", true);
            EditorApplication.delayCall += Run;
        }
        if (!SessionState.GetBool(Key, false)) return;
        Application.logMessageReceived += OnLog;
        EditorApplication.playModeStateChanged += s =>
        {
            if (s == PlayModeStateChange.EnteredPlayMode) _start = EditorApplication.timeSinceStartup;
        };
        EditorApplication.update += Tick;
    }

    [MenuItem("Tools/Play Probe (40s, ghi Temp/probe_errors.txt)")]
    static void RunManual()
    {
        SessionState.SetBool(Key + ".Manual", true);
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Run();
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainScene.unity");
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }

    static void OnLog(string msg, string stack, LogType t)
    {
        if (t == LogType.Log || t == LogType.Warning) return;
        string first = stack.Split('\n')[0];
        string k = t + ": " + msg + "\n    at " + first;
        if (_counts.ContainsKey(k)) _counts[k]++; else { _counts[k] = 1; _order.Add(k + "\n" + stack); }
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying || _start == 0) return;
        double secs = 40;
        double.TryParse(Environment.GetEnvironmentVariable("PROBE_SECONDS") ?? "40", out secs);
        if (EditorApplication.timeSinceStartup - _start < secs) return;
        var sb = new StringBuilder();
        sb.AppendLine("unique=" + _order.Count + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        foreach (var e in _order)
        {
            string k = e.Substring(0, e.IndexOf('\n', e.IndexOf("    at ")) < 0 ? e.Length : e.IndexOf('\n', e.IndexOf("    at ")));
            sb.AppendLine("[x" + (_counts.TryGetValue(k, out var c) ? c : 1) + "] " + e);
            sb.AppendLine("----");
        }
        File.WriteAllText("Temp/probe_errors.txt", sb.ToString());
        SessionState.SetBool(Key, false);
        EditorApplication.update -= Tick;
        if (SessionState.GetBool(Key + ".Manual", false))
        {
            SessionState.SetBool(Key + ".Manual", false);
            EditorApplication.ExitPlaymode();
            Debug.Log("PlayProbe xong: Temp/probe_errors.txt");
        }
        else EditorApplication.Exit(0);
    }
}
#endif
