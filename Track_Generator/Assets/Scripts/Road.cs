// @khenkel 
// parabox llc

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text;
using Parabox.Road;

public class Road : MonoBehaviour 
{
	public bool acceptInput = false;
	public bool connectEnds = false;
	public int insertPoint = -1;
	public List<Vector3> points = new List<Vector3>();
	public float roadWidth = 1f;
	public float groundOffset = 0f;
	public float[] theta;
	public int terrainLayer = 8;

	// uv options 
	public bool swapUV = false;
	public bool flipU = true;
	public bool flipV = true;
	public Vector2 uvScale = Vector2.one;
	public Vector2 uvOffset = Vector2.zero;

	// texture
	public Material mat;


    //Generation Values
    public float nbr = 300f;

    public float Size = 300f;

    public float Cos_FRQ = 1f;
    public float Cos_MGT = 1f;

    public float Sin_FRQ = 2f;
    public float Sin_MGT = 2f;

    public float Circle_Offset_FRQ = 10f;
    public float Circle_Offset_MGT = 0f;

    public float Amplitude_Offset_FRQ = 3f;
    public float Amplitude_Offset_MGT = 0.1f;

    public float Amplitude_Perlin_Zoom = 2f;
    public float Amplitude_Perlin_MGT = 2f;
    public float Amplitude_Perlin_OffsetX = 1f;
    public float Amplitude_Perlin_OffsetY = 1f;


    public int Seed = 1;

    [SerializeField]
    public Terrain_Generator terrain_Generator;

	public List<GameObject> Decorations; 

    public Vector3 cen;

    public float point1H;

    public Portal portal; 

	public GameObject plane; 
	public GameObject terrain; 


	public Decoration_References decoration_References;

	public Transform Decoration_Parent; 

    public void Generate_Road(int _Seed)
    {

        points.Clear();
        terrain_Generator.GenerateTerrain(_Seed);

        float inc = 2f * Mathf.PI / nbr;

        Vector3 last = Vector3.zero;

        Random.InitState(_Seed); 

        // *100 because overwise it was everytime in the very close center og the perlin 
        float rdm_x = Random.value * 100 + Amplitude_Perlin_OffsetX;
        float rdm_y = Random.value * 100 + Amplitude_Perlin_OffsetY;


		foreach(GameObject decoration in Decorations)
		{
			GameObject.DestroyImmediate(decoration);
		}

		Decorations = new List<GameObject>();


        for (int i = 0; i < nbr - 1; i++)
        {

            float offset01 = (Mathf.Cos(inc * i * Circle_Offset_FRQ) + 1 ) * Circle_Offset_MGT;

            float offset02 = (Mathf.Sin(inc * i * Circle_Offset_FRQ) + 1 ) * Circle_Offset_MGT;

            //Direction
            Vector3 vec = new Vector3(Mathf.Cos(inc * i * Cos_FRQ + offset02) * Cos_MGT, 0, Mathf.Sin(inc * i * Sin_FRQ + offset01) * Sin_MGT);

            //Amplitude
            vec *= Size * 
                //Base amplitude
                (1
                //Perlin Offsest
                + Mathf.PerlinNoise(rdm_x + Mathf.Cos(inc*i) * Amplitude_Perlin_Zoom, rdm_y + Mathf.Sin(inc * i) * Amplitude_Perlin_Zoom) * Amplitude_Perlin_MGT
                //Cos Offset
                + Mathf.Cos(inc * i * Amplitude_Offset_FRQ) * Amplitude_Offset_MGT);

            //vec *= (Size * (1 + Mathf.Cos(inc * i * Amplitude_Offset_FRQ) * Amplitude_Offset_MGT));

            //Useless
            Vector3 normal = (last - vec).normalized;
            last = vec;


            points.Add(vec);
 			if(i%20 == 0)
			{
				GameObject rock = Instantiate(decoration_References.Rocks_Prefabs[Random.Range(0, decoration_References.Rocks_Prefabs.Count - 1)], vec, Quaternion.identity, Decoration_Parent);

				Vector3 right = Vector3.Cross(normal, Vector3.up);

				rock.AddComponent<MeshCollider>();

				float _scale = Random.value * 3f + 1f;

				rock.transform.Translate(right * roadWidth * (_scale +1f)  );


				rock.transform.rotation = Random.rotation;

				rock.transform.localScale = new Vector3(_scale, _scale, _scale);


				Decorations.Add(rock);
				
			} 

        }

    }

    public void Refresh()
	{

		portal = GameObject.FindGameObjectWithTag("Portal").GetComponent<Portal>(); 
		Decoration_Parent = GameObject.FindGameObjectWithTag("Decoration").transform; 

		plane.transform.position = new Vector3(plane.transform.position.x, 2f,plane.transform.position.z);
		terrain.transform.position = new Vector3(terrain.transform.position.x, -7f,terrain.transform.position.z);



		if(portal != null)
        {
            portal.gameObject.transform.position = Vector3.zero;
        }

        points.Clear();

        Generate_Road(Seed);

        if (points.Count < 2)
			return;

        transform.localScale = Vector3.one;

		if(!gameObject.GetComponent<MeshFilter>())
			gameObject.AddComponent<MeshFilter>();
		else
		{
			if(gameObject.GetComponent<MeshFilter>().sharedMesh != null)
				DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);
		}

		if(!gameObject.GetComponent<MeshRenderer>())
			gameObject.AddComponent<MeshRenderer>();

		List<Vector3> v = new List<Vector3>();
		List<int> t = new List<int>();

		// calculate angles for each line segment, then build out a plane for it
		int tri_index = 0;
		int segments = connectEnds ? points.Count : points.Count-1;
		theta = new float[segments];

		for(int i = 0; i < segments; i++)
		{
			Vector2 a = points[i+0].ToXZVector2();
			Vector2 b = (connectEnds && i == segments-1) ? points[0].ToXZVector2() : points[i+1].ToXZVector2();
			
			bool flip = (a.x > b.x);// ? theta[i] : -theta[i];

			Vector3 rght = flip ? new Vector3(0,0,-1) : new Vector3(0,0,1);
			Vector3 lft = flip ? new Vector3(0,0,1) : new Vector3(0,0,-1);

			theta[i] = RoadMath.AngleRadian(a, b);

			// seg a
			v.Add(points[i] + rght * roadWidth);		
			v.Add(points[i] + lft * roadWidth);
			// seg b
			int u = (connectEnds && i == segments-1) ? 0 : i+1;
			v.Add(points[u] + rght * roadWidth);		
			v.Add(points[u] + lft * roadWidth);

			// apply angular rotation to points
			int l = v.Count-4;

			v[l+0] = v[l+0].RotateAroundPoint(points[i+0], -theta[i]);
			v[l+1] = v[l+1].RotateAroundPoint(points[i+0], -theta[i]);

			v[l+2] = v[l+2].RotateAroundPoint(points[u], -theta[i]);
			v[l+3] = v[l+3].RotateAroundPoint(points[u], -theta[i]);

			t.AddRange(new int[6]{
				tri_index + 2,
				tri_index + 1,
				tri_index + 0,
				
				tri_index + 2,
				tri_index + 3, 
				tri_index + 1
				});

			tri_index += 4;
		}	

		// join edge vertices
		if(points.Count > 2)
		{
			segments = connectEnds ? v.Count : v.Count - 4;
			for(int i = 0; i < segments; i+=4)
			{
				int p4 = (connectEnds && i == segments-4) ? 0 : i + 4;
				int p5 = (connectEnds && i == segments-4) ? 1 : i + 5;
				int p6 = (connectEnds && i == segments-4) ? 2 : i + 6;
				int p7 = (connectEnds && i == segments-4) ? 3 : i + 7;

				Vector2 leftIntercept;
				if( !RoadMath.InterceptPoint(
					v[i+0].ToXZVector2(), v[i+2].ToXZVector2(), 
					v[p4].ToXZVector2(), v[p6].ToXZVector2(), out leftIntercept) )
					Debug.LogWarning("Parallel Lines!");

				Vector2 rightIntercept;
				if( !RoadMath.InterceptPoint(
					v[i+1].ToXZVector2(), v[i+3].ToXZVector2(), 
					v[p5].ToXZVector2(), v[p7].ToXZVector2(), out rightIntercept))
					Debug.LogWarning("Parallel lines!");

				v[i+2] = leftIntercept.ToVector3();			
				v[p4] = leftIntercept.ToVector3();

				v[i+3] = rightIntercept.ToVector3();
				v[p5] = rightIntercept.ToVector3();
			}
		}

		transform.position = Vector3.zero;

		// // center pivot point and set height offset
		cen = v.Average();
		Vector3 diff = cen - transform.position;
		transform.position = cen;

		transform.position = new Vector3(cen.x, cen.y + 0.4f, cen.z);

		for(int i = 0; i < v.Count; i++)
		{

			v[i] = RoadUtils.GroundHeight(v[i]) + new Vector3(0f, groundOffset, 0f);
			v[i] -= diff;

            if (i == 0)
            {
                point1H = v[i].y;
            }
        }


		Mesh m = new Mesh();
		m.vertices = v.ToArray();
		m.triangles = t.ToArray();
		m.uv = CalculateUV(m.vertices);
		m.RecalculateNormals();
		gameObject.GetComponent<MeshFilter>().sharedMesh = m;
		gameObject.GetComponent<MeshCollider>().sharedMesh = m;
		gameObject.GetComponent<MeshRenderer>().sharedMaterial = mat;
#if UNITY_EDITOR
		Unwrapping.GenerateSecondaryUVSet(gameObject.GetComponent<MeshFilter>().sharedMesh);
#endif

        if(portal != null)
        {
            portal.ResetPosition();
        }

		plane.transform.position = new Vector3(plane.transform.position.x, 2.4f,plane.transform.position.z);
		terrain.transform.position = new Vector3(terrain.transform.position.x, -7.3f,terrain.transform.position.z);


	}

	public Vector2[] CalculateUV(Vector3[] vertices)
	{

		Vector2[] uvs = new Vector2[vertices.Length];

		float scale = (1f / Vector3.Distance(vertices[0], vertices[1]));
		Vector2 topLeft = Vector2.zero;

		int v = 0; // vertex iterator
		int segments = connectEnds ? points.Count : points.Count-1;
		for(int i = 0; i < segments; i++)
		{		
			Vector3 segCenter = (vertices[v+0] + vertices[v+1] + vertices[v+2] + vertices[v+3]) / 4f;

			Vector2 u0 = vertices[v+0].RotateAroundPoint(segCenter, theta[i] + (90f * Mathf.Deg2Rad) ).ToXZVector2();
			Vector2 u1 = vertices[v+1].RotateAroundPoint(segCenter, theta[i] + (90f * Mathf.Deg2Rad) ).ToXZVector2();
			Vector2 u2 = vertices[v+2].RotateAroundPoint(segCenter, theta[i] + (90f * Mathf.Deg2Rad) ).ToXZVector2();
			Vector2 u3 = vertices[v+3].RotateAroundPoint(segCenter, theta[i] + (90f * Mathf.Deg2Rad) ).ToXZVector2();

			// normalizes uv scale
			uvs[v+0] = u0 * scale;
			uvs[v+1] = u1 * scale;
			uvs[v+2] = u2 * scale;
			uvs[v+3] = u3 * scale;

			Vector2 delta = topLeft - uvs[v+0];
			uvs[v+0] += delta;
			uvs[v+1] += delta;
			uvs[v+2] += delta;
			uvs[v+3] += delta;

			topLeft = uvs[v+2];
			v += 4;
		}

		// Normalize X axis, apply to Y
		scale = 1f / uvs[1].x - uvs[0].x;
		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] *= scale;
		}

		// optional uv modifications
		if(swapUV)
		{
			for(int i = 0; i < uvs.Length; i++)
				uvs[i] = new Vector2(uvs[i].y, uvs[i].x);
		}
			
		if(flipU)
		{
			for(int i = 0; i < uvs.Length; i++)
				uvs[i] = new Vector2(-uvs[i].x, uvs[i].y);
		}

		if(flipV)
		{
			for(int i = 0; i < uvs.Length; i++)
				uvs[i] = new Vector2(uvs[i].x, -uvs[i].y);
		}

		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] += uvOffset;
			uvs[i] = Vector2.Scale(uvs[i], uvScale);
		}

		return uvs;

	}
}


