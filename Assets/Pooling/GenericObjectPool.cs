using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool : MonoBehaviour
{
    public int count;
    public GameObject prefab;

    private int lastSelected = 0;
    private GameObject[] instances;

	// Use this for initialization
	void Start () {
		instances = new GameObject[count];
	    for (int i = 0; i < count; ++i)
	    {
	        var instance = Instantiate(prefab);
            instance.SetActive(false);
	        instance.transform.parent = this.transform;
	        instances[i] = instance;
	    }
	}

    public GameObject Instantiate(Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < instances.Length; i++)
        {
            int index = (lastSelected + 1 + i) % instances.Length;
            if (!instances[index].activeSelf)
            {
                lastSelected = index;
                instances[index].SetActive(true);
                instances[index].transform.position = position;
                instances[index].transform.rotation = rotation;
                return instances[index];
            }
        }
        return null;
    }

    public void Destroy(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
