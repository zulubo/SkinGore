using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkinGoreRenderer : MonoBehaviour
{
    public SkinnedMeshRenderer skin;
    public Material goreMaterial;
    public int goreMapResolution = 64;

    static Material damageRendererMat;
    static Material dilateMat;
    static Material addMat;

    SkinnedMeshRenderer goreSkin;
    SkinnedMeshRenderer damageRendererSkin;
    Camera damageRendererCam;
    Transform skinTransformRoot => skin.rootBone != null ? skin.rootBone : skin.transform;

    /// <summary>
    /// the accumulated gore map
    /// </summary>
    RenderTexture goreMap;

    MaterialPropertyBlock matProps;

    /// <summary>
    /// Temporary render tex. Used for pooling textures for efficiency when there are lots of characters taking damage.
    /// </summary>
    private class TempTex
    {
        public RenderTexture tex;
        public int resolution => tex.width;
        public bool inUse;
        public void Free() => inUse = false;

        public TempTex(int resolution)
        {
            tex = new RenderTexture(resolution, resolution, 0);
        }
    }
    static List<TempTex> tempTexPool = new List<TempTex>();
    TempTex GetTempTex()
    {
        TempTex tex = tempTexPool.Find(t => !t.inUse && t.resolution == goreMapResolution);
        if(tex == null)
        { 
            tex = new TempTex(goreMapResolution);
            tempTexPool.Add(tex);
        }
        tex.inUse = true;
        return tex;
    }

    const int renderLayer = 15; // use post processing layer - it should be empty

    bool hasBlendShapes;

    public delegate void OutputDebug(RenderTexture hit, RenderTexture damage);
    public event OutputDebug OnDebugOutput;

    private void Start()
    {
        hasBlendShapes = skin.sharedMesh.blendShapeCount > 0;
    }

    bool skinInit;
    void InitSkin()
    {
        // make copy of skin for rendering gore
        goreSkin = Instantiate(skin.gameObject, skin.transform).GetComponent<SkinnedMeshRenderer>();
        goreSkin.sharedMaterials = new Material[] { goreMaterial };
        CleanSkinnedMeshGO(goreSkin.gameObject);

        // create materials
        if (damageRendererMat == null) damageRendererMat = new Material(Shader.Find("Hidden/DamageBaker"));
        if (addMat == null) addMat = new Material(Shader.Find("Hidden/AddBlit"));
        if (dilateMat == null) dilateMat = new Material(Shader.Find("Hidden/Dilate"));

        // make copy of skin for rendering to damage buffer
        damageRendererSkin = Instantiate(skin.gameObject, skin.transform.parent).GetComponent<SkinnedMeshRenderer>();
        damageRendererSkin.sharedMaterials = new Material[] { damageRendererMat };
        damageRendererSkin.gameObject.layer = renderLayer;
        CleanSkinnedMeshGO(damageRendererSkin.gameObject);
        damageRendererSkin.gameObject.SetActive(false);

        // create camera to render to damage buffer
        damageRendererCam = new GameObject("damageRenderer").AddComponent<Camera>();
        damageRendererCam.transform.parent = damageRendererSkin.transform;
        damageRendererCam.transform.localPosition = Vector3.forward * -10;
        damageRendererCam.transform.localRotation = Quaternion.identity;
        damageRendererCam.orthographic = true;
        damageRendererCam.orthographicSize = 5;
        damageRendererCam.farClipPlane = 15;
        damageRendererCam.cullingMask = 1 << renderLayer;
        damageRendererCam.clearFlags = CameraClearFlags.SolidColor;
        damageRendererCam.backgroundColor = Color.clear;
        damageRendererCam.enabled = false;
        damageRendererCam.useOcclusionCulling = false;

        goreMap = new RenderTexture(goreMapResolution, goreMapResolution, 0);

        matProps = new MaterialPropertyBlock();
        matProps.SetTexture("_GoreDamage", goreMap);
        goreSkin.SetPropertyBlock(matProps);

        skinInit = true;
    }

    /// <summary>
    /// Remove any irrelevant components and children
    /// </summary>
    void CleanSkinnedMeshGO(GameObject go)
    {
        foreach (Component c in go.GetComponents<Component>())
        {
            if(!(c is Transform || c is SkinnedMeshRenderer))
            {
                Destroy(c);
            }
        }

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Destroy(go.transform.GetChild(i).gameObject);
        }
    }

    int id_position = Shader.PropertyToID("_DamagePosition");
    int id_radius = Shader.PropertyToID("_DamageRadius");
    int id_amount = Shader.PropertyToID("_DamageAmount");
    /// <summary>
    /// Add damage by rendering into damage bugger
    /// </summary>
    /// <param name="position">position in world space</param>
    /// <param name="radius">radius of damage</param>
    /// <param name="amount">amount of damage</param>
    public void AddDamage(Vector3 position, float radius, float amount)
    {
        if (!skinInit) InitSkin();

        position = skinTransformRoot.transform.InverseTransformPoint(position);

        // render damage spot
        damageRendererSkin.gameObject.SetActive(true);
        damageRendererMat.SetVector(id_position, new Vector4(position.x, position.y, position.z, 0));
        damageRendererMat.SetFloat(id_radius, radius);
        damageRendererMat.SetFloat(id_amount, amount);
        var tex_damage = GetTempTex();
        var tex_dilated = GetTempTex();
        damageRendererCam.transform.position = damageRendererSkin.bounds.center + damageRendererSkin.transform.forward * -10;
        damageRendererCam.targetTexture = tex_damage.tex;
        damageRendererCam.Render();
        damageRendererSkin.gameObject.SetActive(false);

        // add padding to damage spot
        Graphics.Blit(tex_damage.tex, tex_dilated.tex, dilateMat);
        // additively blend with existing gore map
        Graphics.Blit(tex_dilated.tex, goreMap, addMat);

        OnDebugOutput?.Invoke(tex_dilated.tex, goreMap);

        // free temporary textures back into pool
        tex_dilated.Free();
        tex_damage.Free();
    }


    private void LateUpdate()
    {
        if (skinInit && hasBlendShapes)
        {
            // update blend shapes to match main renderer if necessary
            for (int i = 0; i < goreSkin.sharedMesh.blendShapeCount; i++)
            {
                goreSkin.SetBlendShapeWeight(i, skin.GetBlendShapeWeight(i));
            }
        }
    }

    /// <summary>
    /// Resets any damage this renderer has taken
    /// </summary>
    public void ResetDamage()
    {
        if (!skinInit) return;
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = goreMap;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
        OnDebugOutput?.Invoke(goreMap, goreMap);
    }

    /// <summary>
    /// Copy this GoreRenderer's data to a different one. I use this for making ragdolls keep the damage the alive enemy had!
    /// </summary>
    /// <param name="newGore">The new GoreRenderer to copy to</param>
    /// <param name="canReuseTextures">Whether we can reuse textures. If the old renderer is being deleted, leave true. Only needs to be false if you're creating a copy.</param>
    public void TransferToNewGoreRenderer(SkinGoreRenderer newGore, bool canReuseTextures = true)
    {
        newGore.goreMapResolution = goreMapResolution;
        if (!newGore.skinInit) newGore.InitSkin();
        if (canReuseTextures) newGore.goreMap = goreMap;
        else Graphics.Blit(goreMap, newGore.goreMap);

        newGore.matProps.SetTexture("_GoreDamage", newGore.goreMap);
        newGore.goreSkin.SetPropertyBlock(newGore.matProps);
    }
}
