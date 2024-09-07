using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioR : MonoBehaviour
{
    public AudioSource Audio;
    public AudioClip audio;

    private void Start() {
        Audio = GetComponent<AudioSource>();
    }
    public void Sound(){
        Audio.Stop();
        Audio.Play();
    }
    public void SoundStop(GameObject gameObject){
        if(!Audio.isPlaying){
            gameObject.SetActive(false);
        }
    }
}
