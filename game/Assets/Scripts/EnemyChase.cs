using UnityEngine;
using System.Collections;

public class EnemyChase : MonoBehaviour
{
    public Transform player;
    public Camera playerCamera;

    public float speed = 15f;
    public float killDistance = 1.5f;

    public GameObject visual;

    private bool isChasing = false;
    private bool isJumpscare = false;

    void Update()
    {
        if (!isChasing || player == null || isJumpscare) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= killDistance)
        {
            StartCoroutine(Jumpscare());
        }
    }

    public void StartChase()
    {
        isChasing = true;
    }

    IEnumerator Jumpscare()
    {
        if (isJumpscare) yield break;

        isJumpscare = true;
        isChasing = false;

        // 1. Teleporta o monstro para a câmera (frente do player)
        transform.position = playerCamera.transform.position + playerCamera.transform.forward * 0.8f;

        // 2. Faz ele olhar pra câmera
        transform.rotation = playerCamera.transform.rotation;

        // 3. espera meio segundo (impacto)
        yield return new WaitForSeconds(0.5f);

        // 4. some
        if (visual != null)
            visual.SetActive(false);
        else
            gameObject.SetActive(false);

        Debug.Log("Jumpscare executado!");
    }
}