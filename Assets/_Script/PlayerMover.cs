using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    enum Type {Button, Key};
    [SerializeField]
    Type type;

    public Vector3 cameraLocalPos;
    public Vector3 playerScale;
    PlayerController player;
    public PlayerMover nextPlayerMoverTrigger;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (type == Type.Key)
            if (Input.GetKeyDown(KeyCode.R)) MovePlayer();
    }

    public void MovePlayer()
    {
        player.GoToWithLerp(transform.position, playerScale, cameraLocalPos);
        if (nextPlayerMoverTrigger) 
        {
            nextPlayerMoverTrigger.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
