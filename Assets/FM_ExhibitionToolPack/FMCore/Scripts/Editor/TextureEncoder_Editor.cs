using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(TextureEncoder))]
[CanEditMultipleObjects]
public class TextureEncoder_Editor: Editor
{
    private TextureEncoder TEncoder;

    SerializedProperty StreamTextureProp;

    SerializedProperty FastModeProp;
    SerializedProperty AsyncModeProp;
    SerializedProperty GZipModeProp;

    SerializedProperty QualityProp;
    SerializedProperty StreamFPSProp;

    SerializedProperty ignoreSimilarTextureProp;
    SerializedProperty similarByteSizeThresholdProp;

    SerializedProperty OnDataByteReadyEventProp;

    SerializedProperty labelProp;
    SerializedProperty dataLengthProp;

    void OnEnable()
    {
        StreamTextureProp = serializedObject.FindProperty("StreamTexture");

        FastModeProp = serializedObject.FindProperty("FastMode");
        AsyncModeProp = serializedObject.FindProperty("AsyncMode");
        GZipModeProp = serializedObject.FindProperty("GZipMode");

        QualityProp = serializedObject.FindProperty("Quality");
        StreamFPSProp = serializedObject.FindProperty("StreamFPS");

        ignoreSimilarTextureProp = serializedObject.FindProperty("ignoreSimilarTexture");
        similarByteSizeThresholdProp = serializedObject.FindProperty("similarByteSizeThreshold");

        OnDataByteReadyEventProp = serializedObject.FindProperty("OnDataByteReadyEvent");

        labelProp = serializedObject.FindProperty("label");
        dataLengthProp = serializedObject.FindProperty("dataLength");
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        if (TEncoder == null) TEncoder = (TextureEncoder)target;

        serializedObject.Update();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Target Texture");

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamTextureProp, new GUIContent("Stream Texture"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Settings");

            GUILayout.BeginHorizontal();
            GUILayout.Label("(supported format: RGB24, RGBA32)");
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(QualityProp, new GUIContent("Quality"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(StreamFPSProp, new GUIContent("StreamFPS"));
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FastModeProp, new GUIContent("Fast Encode Mode"));
                    GUILayout.EndHorizontal();

                    if (TEncoder.FastMode)
                    {
                        //GUILayout.BeginVertical("box");
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(AsyncModeProp, new GUIContent("Async (multi-threading)"));
                            GUILayout.EndHorizontal();
                        }
                        //GUILayout.EndVertical();
                    }

                    GUILayout.BeginHorizontal();
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    GUILayout.Label(" Experiment for Mac, Windows, Android (Forced Enabled on iOS)", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(GZipModeProp, new GUIContent("GZip Mode"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.yellow;
                    GUILayout.Label(" Experiment feature: Reduce network traffic", style);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.BeginVertical("box");
            {
                GUILayout.Label("- Networking");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ignoreSimilarTextureProp, new GUIContent("ignore Similar Texture"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(similarByteSizeThresholdProp, new GUIContent("similar Byte Size Threshold"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Label("- Encoded");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(OnDataByteReadyEventProp, new GUIContent("OnDataByteReadyEvent"));
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

                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(dataLengthProp, new GUIContent("Encoded Size(byte)"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
