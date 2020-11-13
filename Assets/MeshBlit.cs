using UnityEngine;

public class MeshBlit : MonoBehaviour
{
    public RenderTexture rt;

    public Mesh mesh;
    public Material material;

    public Vector3 meshPosition = new Vector3(0.5f, 0.5f, -1);
    public float rotSpeed;

    float angle;
    Matrix4x4 objectMatrix;

    void Update()
    {
        // Spinny rotation
        angle += rotSpeed;
        Quaternion meshRotation = Quaternion.AngleAxis(angle, Vector3.up);

        // Create the object transform matrix
        objectMatrix = Matrix4x4.TRS(meshPosition, meshRotation, Vector3.one * 0.3f);

        Blit();
    }

    void Blit()
    {
        // Create an orthographic matrix (for 2D rendering)
        // You can otherwise use Matrix4x4.Perspective()
        Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 100);

        // This fixes flickering (by @guycalledfrank)
        // (because there's some switching back and forth between cameras, I don't fully understand)
        if (Camera.current != null)
            projectionMatrix *= Camera.current.worldToCameraMatrix.inverse;

        // Remember the current texture and set our own as "active".
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rt;

        // Set material as "active". Without this, Unity editor will freeze.
        material.SetPass(0);

        // Push the projection matrix
        GL.PushMatrix();
        GL.LoadProjectionMatrix(projectionMatrix);

        // Clear the texture
        GL.Clear(true, true, Color.black);

        GL.wireframe = true;

        // Draw the mesh!
        Graphics.DrawMeshNow(mesh, objectMatrix);

        GL.wireframe = false;

        // Pop the projection matrix to set it back to the previous one
        GL.PopMatrix();

        // Re-set the RenderTexture to the last used one
        RenderTexture.active = prevRT;
    }

    // Just for live preview
    private void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Graphics.DrawTexture(new Rect(0, 0, 256, 256), rt);
        }
    }
}