using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextureTiler : MonoBehaviour
{
    Vector2 texScale;
    float scaleRatioX;
    float scaleRatioY;
    Vector3 prevScale;
    Vector2 prevTexScale;

    private void Start()
    {
        texScale = gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureScale;
        UpdateRatio();
        prevScale = gameObject.transform.lossyScale;
    }

    private void Update()
    {
        if (prevScale != gameObject.transform.lossyScale)
        {
            UpdateTexture();
            prevScale = gameObject.transform.lossyScale;
        }

        if (prevTexScale != gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureScale)
        {
            UpdateRatio();
            prevTexScale = gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureScale;
        }

    }

    void UpdateRatio()
    {
        scaleRatioX = texScale.x / gameObject.transform.lossyScale.x;
        scaleRatioY = texScale.y / gameObject.transform.lossyScale.y;
    }

    void UpdateTexture()
    {
        texScale.x = gameObject.transform.lossyScale.x * scaleRatioX;
        texScale.y = gameObject.transform.lossyScale.y * scaleRatioY;
        gameObject.GetComponent<Renderer>().sharedMaterial.mainTextureScale = texScale;
    }
}