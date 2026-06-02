using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioTriggerZone : MonoBehaviour
{
    [Header("Configurações")]
    public string playerTag = "Player";
    public bool playOnce = true; // toca só na primeira entrada?

    private AudioSource audioSource;
    private bool hasPlayed = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (playOnce && hasPlayed) return;

        audioSource.Play();
        hasPlayed = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Opcional: para o som ao sair da área
        // audioSource.Stop();
        // hasPlayed = false; // permite tocar de novo na próxima entrada
    }
}