using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fungus
{
    public class InteractableArea : MonoBehaviour
    {
        [SerializeField] protected PlayerController playerCtrl;
        protected SpriteRenderer pRenderer;

        virtual protected void OnTriggerEnter(Collider other)
        {
            Debug.Log("무언가 들어옴");

            if (other != null && other.CompareTag("Interactable"))
            {
                playerCtrl.setTag(other.gameObject.name);
                playerCtrl.canInteractive = true;
            }

        }



        virtual protected void OnTriggerExit(Collider other)
        {
            Debug.Log("무언가 나감");
            if (other.CompareTag("Player"))
            {
                playerCtrl = other.GetComponent<PlayerController>();
                if (playerCtrl != null)
                {
                    playerCtrl.canInteractive = false;
                }

                pRenderer = other.GetComponentInChildren<SpriteRenderer>();
                if (pRenderer != null)
                {
                    pRenderer.color = Color.white;
                }
            }

            playerCtrl = null;
            pRenderer = null;
        }
    }

}
