using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Content")]
    public DialogueLine[] dialogueLines;

    [Header("Settings")]
    [Tooltip("If true, dialogue can be skipped by pressing a key")]
    public bool canSkip = true;

    [Tooltip("If true, the entire dialogue sequence plays only once")]
    public bool playOnce = false;
}