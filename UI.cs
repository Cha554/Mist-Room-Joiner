using BepInEx;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[BepInPlugin("com.mist.selftracker", "Mist Room Joiner", "1.0.0")]
public class RoomTools : BaseUnityPlugin
{
    private string roomInput = "";
    private Rect windowRect = new(100f, 100f, 310f, 130f);
    private bool showUI = true;

    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle windowStyle;
    private GUIStyle hintStyle;
    private bool stylesInitialized = false;

    private const float WindowWidth = 310f;
    private const float WindowHeight = 130f;
    private const float TitleBarHeight = 28f;
    private const float Divider = 1f;
    private const float ButtonY = 38f;
    private const float ButtonHeight = 28f;
    private const float ButtonWidth = 88f;
    private const float Padding = 10f;
    private const float Spacing = 7f;
    private const float TextFieldY = 90f;
    private const float TextFieldHeight = 28f;

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(UnityEngine.KeyCode.F1))
            showUI = !showUI;
        
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        windowStyle = new GUIStyle(GUI.skin.box);
        windowStyle.normal.background = MakeTex(1, 1, new Color(0.08f, 0.08f, 0.08f, 0.97f));
        windowStyle.border = new RectOffset(6, 6, 6, 6);
        windowStyle.padding = new RectOffset(0, 0, 0, 0);

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.normal.textColor = Color.white;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 13;
        headerStyle.alignment = TextAnchor.MiddleLeft;

        hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        hintStyle.fontSize = 10;
        hintStyle.alignment = TextAnchor.MiddleRight;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = MakeTex(1, 1, new Color(0.18f, 0.18f, 0.18f, 1f));
        buttonStyle.hover.background = MakeTex(1, 1, new Color(0.28f, 0.28f, 0.28f, 1f));
        buttonStyle.active.background = MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f, 1f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fontSize = 12;
        buttonStyle.border = new RectOffset(4, 4, 4, 4);

        textFieldStyle = new GUIStyle(GUI.skin.textField);
        textFieldStyle.normal.background = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f, 1f));
        textFieldStyle.normal.textColor = Color.white;
        textFieldStyle.focused.textColor = Color.white;
        textFieldStyle.focused.background = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f, 1f));
        textFieldStyle.fontSize = 12;
        textFieldStyle.alignment = TextAnchor.MiddleLeft;
        textFieldStyle.padding = new RectOffset(8, 8, 6, 6);

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        GUI.Label(
            new Rect(Screen.width - 110f, 5f, 105f, 20f),
            "Press F1 to toggle",
            new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.45f) }, fontSize = 11, alignment = TextAnchor.MiddleRight
            }
        );

        if (!showUI) return;

        InitStyles();

        windowRect = GUI.Window(0, windowRect, DrawWindow, "", windowStyle);
    }

    private void DrawWindow(int id)
    {
        float innerWidth = WindowWidth - Padding * 2;

        GUI.DrawTexture(new Rect(0, 0, WindowWidth, TitleBarHeight), MakeTex(1, 1, new Color(0.13f, 0.13f, 0.13f, 1f)));
        GUI.Label(new Rect(Padding, 0f, 180f, TitleBarHeight), "  Mist Utilities", headerStyle);
        GUI.Label(new Rect(WindowWidth - 130f, 0f, 95f, TitleBarHeight), "[F1] Toggle", hintStyle);
        GUIStyle closeStyle = new GUIStyle(buttonStyle);
        closeStyle.normal.background = MakeTex(1, 1, new Color(0.55f, 0.1f, 0.1f, 1f));
        closeStyle.hover.background = MakeTex(1, 1, new Color(0.75f, 0.15f, 0.15f, 1f));
        closeStyle.fontSize = 11;
        if (GUI.Button(new Rect(WindowWidth - 26f, 4f, 20f, 20f), "✕", closeStyle)) showUI = false;

        GUI.DrawTexture(new Rect(0, TitleBarHeight, WindowWidth, Divider), MakeTex(1, 1, new Color(0.22f, 0.22f, 0.22f, 1f)));
        
        float totalButtonWidth = ButtonWidth * 3 + Spacing * 2;
        float buttonStartX = Padding + (innerWidth - totalButtonWidth) / 2f;

        if (GUI.Button(new Rect(buttonStartX, ButtonY, ButtonWidth, ButtonHeight), "Leave", buttonStyle))
        {
            PhotonNetwork.Disconnect();
        }

        if (GUI.Button(new Rect(buttonStartX + ButtonWidth + Spacing, ButtonY, ButtonWidth, ButtonHeight), "Join Room", buttonStyle))
        {
            if (!string.IsNullOrWhiteSpace(roomInput))
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomInput.ToUpper(), 0);
        }

        if (GUI.Button(new Rect(buttonStartX + (ButtonWidth + Spacing) * 2, ButtonY, ButtonWidth, ButtonHeight), "Set Name", buttonStyle))
        {
            if (!string.IsNullOrWhiteSpace(roomInput))
                ChangeName(roomInput);
        }

        GUI.DrawTexture(new Rect(0, TextFieldY - 6f, WindowWidth, Divider), MakeTex(1, 1, new Color(0.22f, 0.22f, 0.22f, 1f)));
        roomInput = GUI.TextField(new Rect(Padding, TextFieldY, innerWidth, TextFieldHeight), roomInput, textFieldStyle);

        GUI.DragWindow(new Rect(0, 0, WindowWidth - 30f, TitleBarHeight));
    }

    public static void ChangeName(string playerName)
    {
        if (GorillaComputer.instance == null) return;
        GorillaComputer.instance.currentName = playerName;
        GorillaComputer.instance.SetLocalNameTagText(playerName);
        GorillaComputer.instance.savedName = playerName;
        PhotonNetwork.LocalPlayer.NickName = playerName;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}