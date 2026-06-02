using UnityEngine;

public class StartChaseTrigger : MonoBehaviour
{
    public EnemyChase enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.StartChase();
        }
    }
}