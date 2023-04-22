using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AudioManager myTarget = (AudioManager)target;

        if (EditorSceneManager.IsPreviewScene(myTarget.gameObject.scene))
        {
            EditorGUILayout.HelpBox("Unable to \"Prepare Audio Sources\" because you are in preview mode.", MessageType.Info);
            return;
        }

        if (EditorUtility.IsPersistent(myTarget.gameObject)) // returns false if object lives in scene
        {
            EditorGUILayout.HelpBox("Unable to view object further because it is not instantiated (in the scene) yet.", MessageType.Info);
            return;
        }

        if (myTarget.musicMap == null || myTarget.soundMap == null || myTarget.soundGroupMap == null)
            myTarget.Prepared = false;

        bool prepared = myTarget.Prepared;
        if (!prepared)
        {
            EditorGUILayout.HelpBox("AudioManager is not prepared. Likely missing a audio target reference object.", MessageType.Error);
            return;
        }
        GameSettings.Instance.VerifyInitialization();
        GameSettings.Instance.SettingsUpdate();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Stop Music."))
        {
            myTarget.StopMusic();
        }
        foreach (MusicSound s in myTarget.music)
        {
            if (s != null)
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Play Music: " + s.name))
                {
                    myTarget.SetMusic(s.name);
                    // s.EstablishSource(myTarget.musicSource, true);
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
                        myTarget.PlaySoundGroup(group.groupName);
                        // group.PlaySound(myTarget);
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
                    myTarget.PlaySound(s.name);
                    // s.source.Play();
                }
                if (GUILayout.Button("Stop Sound: " + s.name))
                {
                    myTarget.StopSound(s.name);
                }
            }
        }

        EditorGUILayout.Space(20);
        if (GUILayout.Button("Reprepare Sound Sources"))
        {
            myTarget.Prepared = false;
        }

        myTarget.UpdateSounds();
    }
}
