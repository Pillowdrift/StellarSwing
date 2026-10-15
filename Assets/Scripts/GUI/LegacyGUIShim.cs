using UnityEngine;

// TEMPORARY compatibility shim for the Unity 2019.2 -> Unity 6 upgrade.
//
// Unity removed the GUIText, GUITexture and GUILayer components in 2019.3.
// Their data was stripped from every scene and prefab on first open in
// Unity 6, so these classes exist only so the legacy HUD scripts under
// Scripts/GUI and the old tutorial scripts still compile. Nothing in a scene
// has one attached, so GetComponent<GUIText>() etc. return null at runtime.
//
// Delete this file together with the legacy HUD (Default Scene/GUI.prefab)
// once the uGUI rebuild replaces it.

public class GUIText : MonoBehaviour
{
    public string text = "";
    public Vector2 pixelOffset;
    public Color color = Color.white;
    public int fontSize;
    public Font font;
    public TextAnchor anchor;
    public TextAlignment alignment;

    private Material cachedMaterial;
    public Material material
    {
        get
        {
            if (cachedMaterial == null)
                cachedMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            return cachedMaterial;
        }
        set { cachedMaterial = value; }
    }
}

public class GUITexture : MonoBehaviour
{
    public Texture texture;
    public Color color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Rect pixelInset;
    public bool HitTest(Vector3 screenPosition) { return false; }
    public bool HitTest(Vector3 screenPosition, Camera camera) { return false; }
}

public class GUIElement : MonoBehaviour
{
}

public class GUILayer : MonoBehaviour
{
    public GUIElement HitTest(Vector3 screenPosition) { return null; }
}
