using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Chipflake_AP.Archipelago;
using UnityEngine;

namespace Chipflake_AP.Utils;

// shamelessly stolen from oc2-modding https://github.com/toasterparty/oc2-modding/blob/main/OC2Modding/GameLog.cs
public static class ArchipelagoConsole
{
    public static bool Hidden = true;

    private static List<string> logLines = new();
    private static Vector2 scrollView;
    private static Rect window;
    private static Rect scroll;
    private static Rect text;
    private static Rect hideShowButton;

    private static GUIStyle textStyle = new();
    private static string scrollText = "";
    private static float lastUpdateTime = Time.time;
    private const int MaxLogLines = 80;
    private const float HideTimeout = 15f;

    private static string CommandText = "!help";
    private static Rect CommandTextRect;
    private static Rect SendCommandButton;

    public static void Awake()
    {
        UpdateWindow();
    }

    public static void LogMessage(string message)
    {
        if (message.IsNullOrWhiteSpace()) return;

        if (logLines.Count == MaxLogLines)
        {
            logLines.RemoveAt(0);
        }
        logLines.Add(message);
        Plugin.BepinLogger.LogMessage(message);
        lastUpdateTime = Time.time;
        UpdateWindow();
    }

    public static void OnGUI()
    {
        if (logLines.Count == 0) return;

        if (!Hidden || Time.time - lastUpdateTime < HideTimeout)
        {
            // Replace GUI.BeginScrollView/EndScrollView with a manual scroll region.
            GUI.BeginGroup(window);

            // Mouse wheel scrolling when the cursor is over the console window.
            var mouse = Event.current;
            if (mouse != null && window.Contains(mouse.mousePosition) && mouse.type == EventType.ScrollWheel)
            {
                scrollView.y += mouse.delta.y * 20f;
                mouse.Use();
            }

            // Clamp scroll to content
            var visibleHeight = window.height;
            var contentHeight = scroll.height;
            var maxScroll = Mathf.Max(0f, contentHeight - visibleHeight);
            scrollView.y = Mathf.Clamp(scrollView.y, 0f, maxScroll);

            // Draw content "shifted up" by scroll offset
            GUI.BeginGroup(new Rect(0f, -scrollView.y, scroll.width, scroll.height));
            GUI.Box(text, "");
            GUI.Box(text, scrollText, textStyle);
            GUI.EndGroup();

            // Draw a scrollbar if needed
            if (contentHeight > visibleHeight)
            {
                const float scrollbarWidth = 16f;
                scrollView.y = GUI.VerticalScrollbar(
                    new Rect(window.width - scrollbarWidth, 0f, scrollbarWidth, window.height),
                    scrollView.y,
                    window.height,
                    0f,
                    contentHeight);
            }

            GUI.EndGroup();
        }

        /*
        if (!Hidden || Time.time - lastUpdateTime < HideTimeout)
        {
            GUI.BeginGroup(window);
            
            var mouse = Event.current;
            if (mouse != null && window.Contains(mouse.mousePosition) && mouse.type == EventType.ScrollWheel)
            {
                scrollView.y += mouse.delta.y * 20f;
                mouse.Use();
            }
            
            var visibleHeight = window.height;
            var contentHeight = scroll.height;
            var maxScroll = Mathf.Max(0f, contentHeight - visibleHeight);
            scrollView.y = Mathf.Clamp(scrollView.y, 0f, maxScroll);
            
            scrollView = GUI.BeginScrollView(window, scrollView, scroll);
            GUI.Box(text, "");
            GUI.Box(text, scrollText, textStyle);
            GUI.EndScrollView();
        }
        */
        
        if (GUI.Button(hideShowButton, Hidden ? "Show" : "Hide"))
        {
            Hidden = !Hidden;
            UpdateWindow();
        }
        
        // draw client/server commands entry
        if (Hidden || !ArchipelagoClient.Authenticated) return;

        CommandText = GUI.TextField(CommandTextRect, CommandText);
        if (!CommandText.IsNullOrWhiteSpace() && GUI.Button(SendCommandButton, "Send"))
        {
            Plugin.ArchipelagoClient.SendMessage(CommandText);
            CommandText = "";
        }
    }

    public static void UpdateWindow()
    {
        scrollText = "";

        if (Hidden)
        {
            if (logLines.Count > 0)
            {
                scrollText = logLines[logLines.Count - 1];
            }
        }
        else
        {
            for (var i = 0; i < logLines.Count; i++)
            {
                scrollText += "> ";
                scrollText += logLines.ElementAt(i);
                if (i < logLines.Count - 1)
                {
                    scrollText += "\n\n";
                }
            }
        }

        var width = (int)(Screen.width * 0.4f);
        int height;
        int scrollDepth;
        if (Hidden)
        {
            height = (int)(Screen.height * 0.03f);
            scrollDepth = height;
        }
        else
        {
            height = (int)(Screen.height * 0.3f);
            scrollDepth = height * 10;
        }

        window = new Rect(Screen.width / 2 - width / 2, 0, width, height);
        scroll = new Rect(0, 0, width * 0.9f, scrollDepth);
        scrollView = new Vector2(0, scrollDepth);
        text = new Rect(0, 0, width, scrollDepth);

        textStyle.alignment = TextAnchor.LowerLeft;
        textStyle.fontSize = Hidden ? (int)(Screen.height * 0.01f) : (int)(Screen.height * 0.011f);
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = !Hidden;

        var xPadding = (int)(Screen.width * 0.01f);
        var yPadding = (int)(Screen.height * 0.01f);

        textStyle.padding = Hidden
            ? new RectOffset(xPadding / 2, xPadding / 2, yPadding / 2, yPadding / 2)
            : new RectOffset(xPadding, xPadding, yPadding, yPadding);

        var buttonWidth = (int)(Screen.width * 0.12f);
        var buttonHeight = (int)(Screen.height * 0.03f);

        //hideShowButton = new Rect(Screen.width / 4 + width / 4 + buttonWidth / 6, Screen.height * 0.002f, buttonWidth, buttonHeight);

        // draw server command text field and button
        width = (int)(Screen.width * 0.4f);
        var xPos = (int)(Screen.width / 2.0f - width / 2.0f);
        var yPos = (int)(Screen.height * 0.307f);
        height = (int)(Screen.height * 0.022f);

        CommandTextRect = new Rect(xPos, yPos, width, height);

        width = (int)(Screen.width * 0.035f);
        yPos += (int)(Screen.height * 0.03f);
        SendCommandButton = new Rect(xPos, yPos, width, height);
    }
}