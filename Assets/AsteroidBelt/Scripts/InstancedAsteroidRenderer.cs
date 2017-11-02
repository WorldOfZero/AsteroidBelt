using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsteroidData
{
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
    public Vector3 angularVelocity;
    public float speed;
    public float seed;
    public GameObject physicalBody;

    public Matrix4x4 matrix { get { return Matrix4x4.TRS(position, rotation, scale); } }

    public AsteroidData(Vector3 position, Quaternion rotation, Vector3 scale, float speed)
    {
        this.position = position;
        this.scale = scale;
        this.rotation = rotation;
        this.speed = speed;
        this.seed = Random.value;
        this.angularVelocity = Random.insideUnitSphere;
    }
}

public class InstancedAsteroidRenderer : MonoBehaviour
{

    public Transform renderOrigin;

    public Mesh mesh;
    public Material material;

    [Range(1,1023)]
    public int maximumAsteroids = 1000;

    public float createDistance = 400;
    public float destroyDistance = 500;

    public GenericObjectPool pool;
    public float lodCreateDistance = 100;
    public float lodDestroyDistance = 150;

    public Vector3 scale;

    private List<AsteroidData> asteroids = new List<AsteroidData>();

    // Use this for initialization
    void Start () {
        if (createDistance > destroyDistance)
        {
            Debug.LogError("Incorrect Create and Destroy distances");
        }

        while (asteroids.Count < maximumAsteroids)
        {
            CreateNewAsteriodRandom();
        }
    }

    private void CreateNewAsteriodRandom()
    {
        var center = Camera.main.transform.position;
        AsteroidData asteroid = new AsteroidData(center + Random.insideUnitSphere * createDistance, Random.rotation, scale, Random.Range(2.0f,10.0f));
        asteroid.position.y *= 0.125f;
        asteroids.Add(asteroid);
    }

    // Update is called once per frame
    void Update () {
        Vector3 asteroidHeading = Vector3.Cross((renderOrigin.position - this.transform.position).normalized, this.transform.up);
        asteroidHeading.Normalize();

        foreach (var asteroid in asteroids)
        {
            if (asteroid.physicalBody == null)
            {
                var differenceFromPlayer = asteroid.position - renderOrigin.position;
                var difference = asteroid.position - this.transform.position;
                var angle = Mathf.Atan2(difference.z, difference.x);
                Vector3 velocity = asteroidHeading * asteroid.speed;
                var x = Mathf.Cos(angle * 10 + asteroid.seed * 1000) *  asteroid.speed;
                var y = Mathf.Cos(Time.time * 0.1f + asteroid.seed * 50) * asteroid.speed * 0.33f;
                var z = Mathf.Cos(angle * 10 + asteroid.seed * 500) * asteroid.speed;
                velocity += new Vector3(x, y, z);

                if (differenceFromPlayer.magnitude < lodCreateDistance)
                {
                    var gobj = pool.Instantiate(asteroid.position, asteroid.rotation);
                    gobj.transform.localScale = asteroid.scale;
                    var rigidbody = gobj.GetComponent<Rigidbody>();
                    rigidbody.velocity = velocity;
                    rigidbody.angularVelocity = asteroid.angularVelocity;
                    gobj.transform.parent = this.transform;
                    asteroid.physicalBody = gobj;
                }
                else
                {
                    asteroid.rotation = Quaternion.Euler(asteroid.angularVelocity * Time.time * 5 * asteroid.speed);
                    asteroid.position += velocity * Time.deltaTime;
                    if ((asteroid.position - renderOrigin.position).magnitude > destroyDistance)
                    {
                        if (asteroid.scale != Vector3.zero)
                        {
                            asteroid.scale = Vector3.MoveTowards(asteroid.scale, Vector3.zero, Time.deltaTime * 0.1f);
                        }
                        else
                        {
                            ReplaceAsteroid(asteroid, asteroidHeading);
                        }
                    }
                    else if (asteroid.scale != scale)
                    {
                        asteroid.scale = Vector3.MoveTowards(asteroid.scale, scale,
                            Time.deltaTime * 0.1f);
                    }
                }
            }
            else
            {
                var differenceFromPlayer = asteroid.physicalBody.transform.position - renderOrigin.position;
                if (differenceFromPlayer.magnitude > lodDestroyDistance)
                {
                    asteroid.position = asteroid.physicalBody.transform.position;
                    asteroid.scale = asteroid.physicalBody.transform.localScale;
                    asteroid.rotation = asteroid.physicalBody.transform.rotation;
                    var rigidbody = asteroid.physicalBody.GetComponent<Rigidbody>();
                    asteroid.speed = rigidbody.velocity.magnitude;
                    asteroid.angularVelocity = rigidbody.angularVelocity;
                    pool.Destroy(asteroid.physicalBody);
                    asteroid.physicalBody = null;
                }
            }
        }

        Graphics.DrawMeshInstanced(mesh, 0, material, asteroids.Where(a => a.physicalBody == null).Select((a) => a.matrix).ToList());
    }

    private void ReplaceAsteroid(AsteroidData asteroid, Vector3 asteroidHeading)
    {
        var vector = Random.onUnitSphere;
        vector.y = 0;
        vector.Normalize();
        asteroid.position = renderOrigin.position + vector * createDistance;
        asteroid.position.y = Random.Range(-25,25);
        asteroid.scale = Vector3.zero;
    }

    //private Vector3 CalculateSpawnDistance(Vector3 scale)
    //{
    //    return renderOrigin.position + new Vector3(Random.Range(-scale.x, scale.x), Random.Range(-scale.y, scale.y), Random.Range(-scale.z, scale.z));
    //}
}
