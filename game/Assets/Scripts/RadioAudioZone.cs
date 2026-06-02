using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RadioAudioZone : MonoBehaviour
{
    [Header("Referência ao Rádio")]
    public RadioInteractable radio; // arraste o objeto do rádio aqui

    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        radio.LigarRadio();
    }
}