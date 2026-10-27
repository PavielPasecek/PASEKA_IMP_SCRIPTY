using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class NPCManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject player;
    public List<Transform> StartPoint;
    public List<Transform> MidPoint;
    public List<Transform> EndPoint;
    void Start()
    {
        foreach (Transform NPC in transform)
        {
            NPCChovani script = NPC.gameObject.GetComponent<NPCChovani>();
            Animator animator = NPC.gameObject.GetComponent<Animator>();
            switch (script.Animation)
            {
                case 1:
                    animator.Play("Sit");
                    break;
                case 2:
                    animator.Play("PhoneTalkNormal");
                    break;
                case 3:
                    animator.Play("PhoneTalk2");
                    break;
                case 4:
                    animator.Play("WalkNormal");
                    break;
                case 5:
                    animator.Play("PhoneWalk");
                    break;
                case 6:
                    animator.Play("PhoneWalk2");
                    break;
                default:
                    animator.Play("Idle");
                    break;
            }
        }
    }

    void MoveNPC(float speed,NPCChovani script, Transform NPC)
    {
        Vector3 sp = StartPoint[script.PathIdx].position;
        Vector3 ep = EndPoint[script.PathIdx].position;
        Vector3 mp = MidPoint[script.PathIdx].position;

        if (script.PassedMidpoint)
        {

            if (script.Dir)
            {
                NPC.gameObject.transform.position = Vector3.MoveTowards(NPC.gameObject.transform.position, sp, speed);
                RotateNPC(NPC, sp);
            }
            else
            {
                NPC.gameObject.transform.position = Vector3.MoveTowards(NPC.gameObject.transform.position, ep, speed);
                RotateNPC(NPC, ep);

            }
            if (Vector3.Distance(NPC.gameObject.transform.position, sp) < 1)
            {
                script.PassedMidpoint = false;
                script.Dir = false;
            }
            else if (Vector3.Distance(NPC.gameObject.transform.position, ep) < 1)
            {
                script.PassedMidpoint = false;
                script.Dir = true;
            }

        }
        else
        {
            NPC.gameObject.transform.position = Vector3.MoveTowards(NPC.gameObject.transform.position, mp, speed);
            RotateNPC(NPC,mp);
            if (Vector3.Distance(NPC.gameObject.transform.position, mp) < 1)
            {
                script.PassedMidpoint = true;
            }
        }
    }
    void FollowPlayer(float speed, NPCChovani script, Transform NPC)
    {
        Vector3 playerPos = player.transform.position - Vector3.up;
        if (player && Vector3.Distance(playerPos, NPC.transform.position) > 3f)
        {
            NPC.gameObject.transform.position = Vector3.MoveTowards(NPC.gameObject.transform.position, playerPos, speed);
            RotateNPC(NPC, playerPos);
            NPC.gameObject.GetComponent<Animator>().SetFloat("Speed", 1);
        }
        else
        {
            NPC.gameObject.GetComponent<Animator>().SetFloat("Speed", 0);
        }
    }
    void RotateNPC(Transform NPC, Vector3 pos)
    {
        NPC.gameObject.transform.LookAt(new Vector3(pos.x, NPC.gameObject.transform.position.y, pos.z));
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Transform NPC in transform)
        {
            NPCChovani script =  NPC.gameObject.GetComponent<NPCChovani>();
            if (script.HasMovement)
            {
                float speed = Time.deltaTime * 2.5f;
                if (script.FollowPlayer)
                {
                    FollowPlayer(speed, script, NPC);
                }
                else
                {
                    MoveNPC(speed,script,NPC);
                }
            }
            
        }
    }
}
