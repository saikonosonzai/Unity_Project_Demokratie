using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform target;

    public Transform player;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
    private void OnTriggerEnter(Collider collision){
        if (collision.CompareTag("Player"))
        {
            print(collision.gameObject.name);
            player.position = target.position;;
        }
    }
}
