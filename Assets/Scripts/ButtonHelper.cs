using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHelper : MonoBehaviour
{
    public SpriteRenderer sprrenderer;
    public AudioSource source;

    public void FlashPiece(bool silent)
    {
        if (silent)
        {
            sprrenderer.enabled = true;
        }
        else
        {
            sprrenderer.enabled = true;
            source.Play();
        }
    }

    public void ResetPiece()
    {
        sprrenderer.enabled = false;
        source.Stop();
    }
}
