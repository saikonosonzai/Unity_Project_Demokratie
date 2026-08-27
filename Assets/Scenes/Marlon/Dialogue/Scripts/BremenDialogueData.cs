using UnityEngine;

[System.Serializable]
public class BremenDialogueLine
{
    [Header("Speaker")]
    public string speakerName;

    [Header("Portrait / Image")]
    public Sprite portrait;

    [Header("Optional Portrait Layout Override")]
    public bool overridePortraitLayout = false;
    public Vector2 customPortraitSize = new Vector2(250f, 330f);
    public Vector2 customPortraitPosition = new Vector2(-190f, 0f);

    [Header("Text")]
    [TextArea(2, 6)]
    public string text;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Bremen/Dialogue")]
public class BremenDialogueData : ScriptableObject
{
    public BremenDialogueLine[] lines;
}