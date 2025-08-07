using UnityEngine;

public sealed class LaserRetract : MonoBehaviour
{ 
    public GameObject gate;
    public bool leftOrRight;
    void Start()
    {
        gate = GameObject.Find("Gate");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            gate.GetComponent<Laser>().Retract();
            gate.GetComponent<Laser>().retractLeftOrRight = leftOrRight;
        }
    }
}
