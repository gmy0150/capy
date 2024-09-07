using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioR : MonoBehaviour
{
    public AudioClip Audio;

    private void Start() {
        
    }
    public void Sound(){
        GetComponent<AudioSource>().Stop();

        GetComponent<AudioSource>().Play();
    }
}
