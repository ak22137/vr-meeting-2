using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioDecoder))]
[CanEditMultipleObjects]
public class AudioDecoder_Editor : Editor
{
    private AudioDecoder ADecoder;
    SerializedProperty labelProp;
    SerializedProperty volumeProp;

    void OnEnable()
    {
        labelProp = serializedObject.FindProperty("label");
        volumeProp = serializedObject.FindProperty("volume");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if(ADecoder== null) ADecoder = (AudioDecoder)target;

        serializedObject.Update();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Playback");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(volumeProp, new GUIContent("Volume"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Label("- Audio Info");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Source Sample Rate: " + ADecoder.SourceSampleRate);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Source Channels: " + ADecoder.SourceChannels);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Device Sample Rate: " + ADecoder.DeviceSampleRate);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Pair Encoder & Decoder ");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(labelProp, new GUIContent("label"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
