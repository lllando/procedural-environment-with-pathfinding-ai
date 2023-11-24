using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMap : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFiler;
    public MeshRenderer meshRenderer;

    public void DrawTextureMap(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        // Use 'shared' version of variables so that meshes can be generated from the editor too
        Mesh mesh = meshData.CreateMesh();
        meshFiler.sharedMesh = mesh;
        meshRenderer.sharedMaterial.mainTexture = texture;
        
        // Update meshcollider
        MeshCollider meshCollider = meshRenderer.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

    }
}
