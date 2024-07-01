using MyBox;
using UnityEngine;

public class FixMaterial : MonoBehaviour
{
    [ButtonMethod]
    public void Fix()
    {
        Renderer textureRender = GetComponent<Renderer>();
        textureRender.sharedMaterial.mainTexture = null;
        textureRender.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}
