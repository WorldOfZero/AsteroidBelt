using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class InstancedRenderer : MonoBehaviour {

    public Mesh mesh;
    public Material material;

    public int size;
    public float spacing;

    private List<Matrix4x4> instances = new List<Matrix4x4>();
    private Vector3 position;
    private Vector3 scale;
    private Quaternion rotation;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	    if (mesh == null || material == null)
	    {
            return;
	    }

        if (this.position != this.transform.position ||
            this.scale != this.transform.localScale ||
            this.rotation != this.transform.rotation)
        {
            instances.Clear();
            Vector3 positionMod;
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int z = 0; z < size; ++z)
                    {
                        positionMod.x = x * spacing;
                        positionMod.y = y * spacing;
                        positionMod.z = z * spacing;
                        instances.Add(Matrix4x4.TRS(this.transform.position + positionMod, this.transform.rotation, this.transform.localScale));
                    }
                }
            }
        }

        position = this.transform.position;
        scale = this.transform.localScale;
        rotation = this.transform.rotation;

        Graphics.DrawMeshInstanced(mesh, 0, material, instances);
	}
}
