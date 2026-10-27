using Unity.VisualScripting;
using UnityEngine;

public class NPCChovani : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool FollowPlayer;
    public bool HasMovement;
    public GameObject sprite;
    public int Animation;
    public bool Dir;
    public bool PassedMidpoint;
    public byte PathIdx = 0;
}
