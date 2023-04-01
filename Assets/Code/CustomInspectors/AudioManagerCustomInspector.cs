using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AudioManager myTarget = (AudioManager)target;

        EditorGUILayout.Space(10);

        foreach (MusicSound s in myTarget.music)
        {
            if (s != null)
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Play Music: " + s.name))
                {
                    s.EstablishSource(myTarget.musicSource, true);
                }
            }
        }

        EditorGUILayout.Space(10);
        if (myTarget.soundGroupMap != null)
        {
            foreach (SoundEffectGroup group in myTarget.soundGroupMap.Values)
            {
                if (group != null)
                {
                    EditorGUILayout.Space(10);
                    if (GUILayout.Button("Play Sound from Group: " + group.groupName))
                    {
                        group.PlaySound(myTarget);
                    }
                }
            }
        }

        EditorGUILayout.Space(10);

        foreach (EffectSound s in myTarget.sounds)
        {
            if (s != null)
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Play Sound: " + s.name))
                {
                    s.source.Play();
                }
            }
        }
    }
}
