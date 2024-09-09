using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
   public static SoundFXManager instance {get; private set;}
   [SerializeField] private AudioSource SoundFXSource;
    private void Awake()
    {   
        if(instance != null) 
        {
            Debug.Log("Another instance of SoundFXManager already exists in the scene");
        }
        instance = this;
    }

   public void PlaySingleSoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
   {
        AudioSource source = Instantiate(SoundFXSource, spawnTransform.position, Quaternion.identity);
        source.PlayOneShot(audioClip, volume);
        float clipRunTime = audioClip.length;
        Destroy(source.gameObject, clipRunTime);
   }
}
