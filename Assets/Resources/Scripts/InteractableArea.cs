using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableArea : MonoBehaviour
{
    private PlayerController pMovement;
    private SpriteRenderer pRenderer;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("무언가 들어옴");
        if (other.CompareTag("Player"))
        {
            pMovement = other.GetComponent<PlayerController>();
            if (pMovement != null)
            {
                pMovement.canInteractive = true;
            }

            pRenderer = other.GetComponentInChildren<SpriteRenderer>();
            if (pRenderer != null)
            {
                pRenderer.color = Color.red;
            }
        }
    }

    private void OnTriggerExit(Collider other)
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