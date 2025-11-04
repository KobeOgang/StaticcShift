using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Tutorial", menuName = "Tutorial System/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [Header("Tutorial Content")]
    [Tooltip("The title of the tutorial")]
    public string tutorialTitle;

    [Tooltip("The description or instructions for this tutorial")]
    [TextArea(3, 10)]
    public string tutorialDescription;

    [Header("Visual Content (Optional)")]
    [Tooltip("Choose what type of visual content to display")]
    public VisualContentType contentType = VisualContentType.None;

    [Tooltip("Sprite image to display in the tutorial")]
    public Sprite tutorialImage;

    [Tooltip("Video clip for animated content")]
    public VideoClip tutorialVideo;

    public enum VisualContentType
    {
        None,
        Image,
        Video
    }
}
