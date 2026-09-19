using UnityEngine;
using UnityEngine.Rendering;

namespace ShinA.Maps
{
    public sealed class ForestMapInitializer : MapSceneInitializer
    {
        [SerializeField, Range(4f, 6f)] private float treeSpacing = 4.5f;
        [SerializeField, Range(8, 40)] private int rockCount = 24;

        private Material earth;
        private Material bark;
        private Material foliage;
        private Material moss;

        protected override void InitializeEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.12f, 0.18f, 0.17f);
            RenderSettings.fogDensity = 0.052f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.21f, 0.19f);

            earth = CreateMaterial(new Color(0.1f, 0.13f, 0.09f));
            bark = CreateMaterial(new Color(0.12f, 0.1f, 0.08f));
            foliage = CreateMaterial(new Color(0.055f, 0.11f, 0.075f));
            moss = CreateMaterial(new Color(0.18f, 0.23f, 0.16f));
            GameObject environment = new("Forest Environment");
            environment.transform.SetParent(transform, false);
            CreateBlock("Forest Floor", environment.transform, new Vector3(0f, -0.3f, 0f),
                new Vector3(64f, 0.6f, 64f), earth);

            for (float x = -27f; x <= 27f; x += treeSpacing)
            {
                for (float z = -27f; z <= 27f; z += treeSpacing)
                {
                    Vector3 position = new(x + Mathf.Lerp(-0.8f, 0.8f, (float)GenerationRandom.NextDouble()),
                        0f, z + Mathf.Lerp(-0.8f, 0.8f, (float)GenerationRandom.NextDouble()));
                    if ((Mathf.Abs(position.x) < 6f && Mathf.Abs(position.z) < 6f) ||
                        Mathf.Abs(position.x) < 2.8f)
                    {
                        continue;
                    }

                    float height = Mathf.Lerp(7f, 11f, (float)GenerationRandom.NextDouble());
                    GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    trunk.name = "Tree Trunk";
                    trunk.transform.SetParent(environment.transform, false);
                    trunk.transform.localPosition = position + Vector3.up * (height * 0.5f);
                    trunk.transform.localScale = new Vector3(0.65f, height * 0.5f, 0.65f);
                    trunk.GetComponent<Renderer>().sharedMaterial = bark;

                    GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.name = "Tree Canopy";
                    crown.transform.SetParent(environment.transform, false);
                    crown.transform.localPosition = position + Vector3.up * height;
                    crown.transform.localScale = new Vector3(6.5f, 4f, 6.5f);
                    crown.GetComponent<Renderer>().sharedMaterial = foliage;
                    crown.GetComponent<Collider>().enabled = false;
                }
            }

            for (int i = 0; i < rockCount; i++)
            {
                float angle = i * 360f / rockCount;
                float radius = Mathf.Lerp(8f, 26f, (float)GenerationRandom.NextDouble());
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
                if (Mathf.Abs(position.x) < 3f)
                {
                    continue;
                }

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Moss Rock {i + 1}";
                rock.transform.SetParent(environment.transform, false);
                rock.transform.localPosition = position;
                rock.transform.localScale = new Vector3(3f, 2f, 2.5f);
                rock.GetComponent<Renderer>().sharedMaterial = moss;
            }

            CreateBlock("Old Trail Marker", environment.transform, new Vector3(2f, 0.7f, 8f),
                new Vector3(0.35f, 1.4f, 0.35f), moss);
            CreateMissionBase(environment.transform, bark, earth, moss);
            for (int i = 0; i < 4; i++)
            {
                GameObject bank = CreateBlock($"Outer Rock Bank {i + 1}", environment.transform,
                    Quaternion.Euler(0f, i * 90f, 0f) * new Vector3(0f, 2f, 31f),
                    new Vector3(64f, 4f, 2f), moss);
                bank.transform.localRotation = Quaternion.Euler(0f, i * 90f, 0f);
            }

            GameObject lightObject = new("Forest Overcast Light");
            lightObject.transform.SetParent(environment.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(65f, 20f, 0f);
            Light sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.57f, 0.69f, 0.7f);
            sun.intensity = 0.45f;
            sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun;
        }

        protected override void ConfigurePlayer(GameObject player)
        {
            Camera camera = player.GetComponentInChildren<Camera>();
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = RenderSettings.fogColor;
            }
        }

        private void OnDestroy()
        {
            Destroy(earth);
            Destroy(bark);
            Destroy(foliage);
            Destroy(moss);
        }
    }
}
