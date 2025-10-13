using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]private AudioClip audioClip;

    private void PlaySound(UnityEngine.AudioClip[] clips,Vector3 position,float volume=1.0f) 
    { 
        int index=Random.Range(0,clips.Length);

        AudioSource.PlayClipAtPoint(clips[index], position, volume);
    }
}
