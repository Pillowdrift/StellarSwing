using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OutlineText
    : MonoBehaviour
{
    public Color color = Color.black;
    public int thickness = 1;

    private List<GameObject> children = new List<GameObject>();

    private GameObject prefab;

    public void Start()
    {
        // Create copy of gameobject
        prefab = (GameObject)GameObject.Instantiate(gameObject);
        prefab.SetActive(false);

        // Reset
        Reset();
    }

    private GameObject CreateText(GameObject prefab, int x, int y)
    {
        GameObject newText = (GameObject)GameObject.Instantiate(prefab);
        newText.SetActive(true);

        // Set the z back a bit so you can always see the top layer
        // (this is relative to the top layer so any negative value should work)
        newText.transform.Translate(0, 0, -1);

        newText.transform.parent = transform;

        Destroy(newText.GetComponent<OutlineText>());

        Vector2 pixelOffset = newText.GetComponent<GUIText>().pixelOffset;
        pixelOffset.x += x;
        pixelOffset.y += y;
        newText.GetComponent<GUIText>().pixelOffset = pixelOffset;

        newText.GetComponent<GUIText>().color = color;

        return newText;
    }

    public void LateUpdate()
    {
        for (int i = 0; i < children.Count; ++i)
        {
            children[i].GetComponent<GUIText>().text = GetComponent<GUIText>().text;
            children[i].GetComponent<GUIText>().enabled = GetComponent<GUIText>().enabled;
            children[i].transform.position = GetComponent<GUIText>().transform.position + new Vector3(0.0f, 0.0f, -1.0f);

            Color col = color;
            col.a = GetComponent<GUIText>().material.color.a;
            children[i].GetComponent<GUIText>().material.color = col;
        }
    }

    public void Reset()
    {
        // Destroy existing children
        for (int i = 0; i < children.Count; ++i)
        {
            Destroy(children[i]);
        }
        children.Clear();

        // Update prefab
        prefab.GetComponent<GUIText>().text = GetComponent<GUIText>().text;
        prefab.GetComponent<GUIText>().enabled = GetComponent<GUIText>().enabled;

        if (GetComponent<GUIText>() != null)
        {
            int offset = 1 * thickness;

            for (int i = 1; i <= offset; ++i)
            {
                children.Add(CreateText(prefab, -i, -i));
                children.Add(CreateText(prefab, -i,  0));
                children.Add(CreateText(prefab, -i,  i));
                children.Add(CreateText(prefab,  i, -i));
                children.Add(CreateText(prefab,  i,  0));
                children.Add(CreateText(prefab,  i,  i));
                children.Add(CreateText(prefab,  0,  i));
                children.Add(CreateText(prefab,  0, -i));
            }
        }
    }
}
