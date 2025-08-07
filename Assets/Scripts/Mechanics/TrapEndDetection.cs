using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public class TrapEndDetection : MonoBehaviour
{
    private EventInstance inst;
    public void Start()
    {
        inst = RuntimeManager.CreateInstance("event:/SFX/Game/Trap");
    }
    public void Trigger()
    {
        inst.start();
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment"))
        {
            inst.setParameterByName("Hit Ground", 1);
            inst.release();
        }
    }
}
