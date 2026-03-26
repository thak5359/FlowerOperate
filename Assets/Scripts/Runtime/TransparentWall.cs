using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentWall : MonoBehaviour
{
    protected PlayerController pMovement;
    protected SpriteRenderer pRenderer;
   

    virtual protected void OnCollisonEnter(Collision other)
    {
        other.collider.CompareTag("Player");
        Debug.Log("무언가 들어옴");
        if (other.collider.CompareTag("Player"))
        {
            pMovement = other.collider.GetComponent<PlayerController>();
            if (pMovement != null)
            {

                Debug.Log("Player Tag is dectected");
            }

            pRenderer = other.collider.GetComponentInChildren<SpriteRenderer>();
            if (pRenderer != null)
            {
                pRenderer.color = Color.red;
            }
        }

    }


    virtual protected void OnTriggerEnter(Collider other )
    {
        Debug.Log("무언가 들어옴");
        if (other.CompareTag("Player"))
        {
            pMovement = other.GetComponent<PlayerController>();
            if (pMovement != null)
            {
                
                Debug.Log("Player Tag is dectected");
            }

            pRenderer = other.GetComponentInChildren<SpriteRenderer>();
            if (pRenderer != null)
            {
                pRenderer.color = Color.red;
            }
        }
    }

    virtual protected void OnTriggerExit(Collider other)
    {
        Debug.Log("무언가 나감");
        if (other.CompareTag("Player"))
        {
            pMovement = other.GetComponent<PlayerController>();
            if (pMovement != null)
            {
                pMovement.canInteractive = false;
            }

            pRenderer = other.GetComponentInChildren<SpriteRenderer>();
            if (pRenderer != null)
            {
                pRenderer.color = Color.white;
            }
        }

        pMovement = null;
        pRenderer = null;
    }
}
