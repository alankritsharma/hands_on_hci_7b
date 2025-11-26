using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowHolster : MonoBehaviour
{
    [SerializeField] private Bow bow;

    [Header("Audio")]
    [SerializeField] private AudioClip grabAudio;
    [SerializeField] private AudioClip storeAudio;

    private AudioSource audioSource;

    private void Start()
    {
        this.audioSource = this.GetComponent<AudioSource>();
    }

    public bool HasBow()
    {
        return this.bow != null;
    }

    public Bow GetBow()
    {
        Bow bowToReturn = this.bow;
        this.bow = null;

        this.PlayAudio(this.grabAudio);

        return bowToReturn;
    }

    public void StoreBow(Bow bow)
    {
        this.bow = bow;
        this.PlayAudio(this.storeAudio);
    }

    private void PlayAudio(AudioClip clip)
    {
        this.audioSource.PlayOneShot(clip);
    }
}
