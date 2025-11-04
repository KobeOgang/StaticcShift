using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Note", menuName = "Note System/Note Data")]
public class NoteData : ScriptableObject
{
    [Header("Note Content")]
    [Tooltip("The title of the note")]
    public string noteTitle;

    [Tooltip("The content of the note")]
    [TextArea(5, 15)]
    public string noteContent;
}
