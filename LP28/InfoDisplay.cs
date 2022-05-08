using System;
using System.Collections.Generic;
using UnityEngine;

namespace LP28;

public class InfoDisplay : MonoBehaviour
{
    private static InfoDisplay instance;

    private static List<(string, Func<object>)> infos;
    private static List<string> console;

    public InfoDisplay()
    {
        infos = new();
        console = new();
        if (instance != null)
        {
            throw new InvalidOperationException("Multiple InfoDisplay objects created");
        }

        instance = this;
    }

    public static void AddInfo(string label, Func<object> computeValue)
    {
        infos.Add((label, computeValue));
    }

    public static void WriteLine(string s)
    {
        console.Add(s);
        if (console.Count > 10)
        {
            console.RemoveRange(0, console.Count - 10);
        }
        LP28.instance.Log("[InfoDisplay] " + s);
    }
    
    private void OnGUI()
    {
        if ((infos.Count == 0 && console.Count == 0) || (GameManager.instance != null && GameManager.instance.sceneName == "Menu_Title"))
        {
            return;
        }
        GUIConfig cfg = SaveGUIConfig();
        float curYPos = 300f;
        foreach (var entry in infos)
        {
            GUI.Label(new Rect(1500, curYPos, 420, 200),
                $"{entry.Item1}: {entry.Item2.Invoke() ?? "(null)"}");
            curYPos += 20f;
        }

        curYPos = 600f;
        foreach (var s in console)
        {
            GUI.Label(new Rect(1500, curYPos, 420, 200), s);
            curYPos += 20f;
        }
        LoadGUIConfig(cfg);
    }

    private static GUIConfig SaveGUIConfig()
    {
        GUIConfig result = new GUIConfig
        {
            bgColor = GUI.backgroundColor,
            contentColor = GUI.contentColor,
            color = GUI.color,
            mat = GUI.matrix,
            fontSize = GUI.skin.label.fontSize,
            align = GUI.skin.label.alignment
        };
        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;
        GUI.color = Color.white;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3((float)Screen.width / 1920f, (float)Screen.height / 1080f, 1f));
        GUI.skin.label.fontSize = 16;
        GUI.skin.label.alignment = TextAnchor.MiddleRight;
        return result;
    }

    private static void LoadGUIConfig(GUIConfig cfg)
    {
        GUI.backgroundColor = cfg.bgColor;
        GUI.contentColor = cfg.contentColor;
        GUI.color = cfg.color;
        GUI.matrix = cfg.mat;
        GUI.skin.label.fontSize = cfg.fontSize;
        GUI.skin.label.alignment = cfg.align;
    }
    private struct GUIConfig
    {
        public Color bgColor;
        public Color contentColor;
        public Color color;
        public Matrix4x4 mat;
        public int fontSize;
        public TextAnchor align;
    }
}