using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinGoreDebug : MonoBehaviour
{
    public new SkinGoreRenderer renderer;

    RenderTexture tex_hit;
    RenderTexture tex_damage;

    public UnityEngine.UI.RawImage image_hit;
    public UnityEngine.UI.RawImage image_damage;
    public UnityEngine.UI.RawImage image_skin;
    public UnityEngine.UI.RawImage[] images_wireframe;

    static Material addMat;
    Texture skinTex;

    Material wireframeMat;


    private void Start()
    {
        if (addMat == null) addMat = new Material(Shader.Find("Hidden/AddBlit"));
        if (wireframeMat == null) wireframeMat = new Material(Shader.Find("Hidden/DamageBaker"));

        tex_hit = new RenderTexture(renderer.goreMapResolution, renderer.goreMapResolution, 0);
        tex_damage = new RenderTexture(renderer.goreMapResolution, renderer.goreMapResolution, 0);

        skinTex = renderer.skin.sharedMaterial.mainTexture;

        image_hit.texture = tex_hit;
        image_damage.texture = tex_damage;
        image_skin.texture = skinTex;

        RenderUVWireframe();

        renderer.OnDebugOutput += Renderer_OnDebugOutput;
    }

    void RenderUVWireframe()
    {
        RenderTexture tex_wireframe = new RenderTexture(512, 512, 0);
        wireframeMat.SetFloat("_DamageRadius", 1000000);
        wireframeMat.SetFloat("_DamageAmount", 1); // set these to just render a white wireframe

        Matrix4x4 objectMatrix = Matrix4x4.TRS(Vector3.forward * -1, Quaternion.identity, Vector3.one * 0.3f);

        Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 100);
        if (Camera.current != null)
            projectionMatrix *= Camera.current.worldToCameraMatrix.inverse;

        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = tex_wireframe;

        wireframeMat.SetPass(0);

        GL.PushMatrix();
        GL.LoadProjectionMatrix(projectionMatrix);

        GL.Clear(true, true, Color.clear);

        GL.wireframe = true;

        Graphics.DrawMeshNow(renderer.skin.sharedMesh, objectMatrix);

        GL.wireframe = false;

        GL.PopMatrix();

        RenderTexture.active = prevRT;


        for (int i = 0; i < images_wireframe.Length; i++)
        {
            images_wireframe[i].texture = tex_wireframe;
        }
    }

    private void Renderer_OnDebugOutput(RenderTexture hit, RenderTexture damage)
    {
        Graphics.Blit(hit, tex_hit);
        Graphics.Blit(damage, tex_damage);
    }
}
