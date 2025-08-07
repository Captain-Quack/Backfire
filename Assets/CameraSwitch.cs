using Backfire;
using UnityEngine;
using Unity.Cinemachine;

public class CameraTriggerZone : MonoBehaviour
{
    public CinemachineCamera zoomOutCam;
    public CinemachineCamera playerCam;
    public int phase = 1;
    public GameObject boss;

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            boss.SetActive(true);
            boss.GetComponent<BossController>().Phase(phase);
            other.GetComponent<PlayerController>().ZoomA = -20.35f;
            other.GetComponent<PlayerController>().ZoomOffset = -32.02f;
            zoomOutCam.Priority = 20;
            playerCam.Priority = 10;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().ZoomA = -0.01f;
            other.GetComponent<PlayerController>().ZoomOffset = -5;
            zoomOutCam.Priority = 10;
            playerCam.Priority = 20;
        }
    }
}