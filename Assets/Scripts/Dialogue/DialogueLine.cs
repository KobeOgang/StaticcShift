using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string dialogueText;

    [Tooltip("How long this line stays on screen after typing completes. Set to 0 to use auto-calculated duration.")]
    public float displayDuration;
}
