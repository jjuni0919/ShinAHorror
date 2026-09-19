using UnityEngine;

namespace ShinA.Maps
{
    public sealed class SaltFarmMapInitializer : MapSceneInitializer
    {
        protected override void InitializeEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = new Color(0.25f, 0.29f, 0.3f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.17f, 0.19f, 0.2f);

            GameObject environment = new("Salt Farm Environment");
            Material salt = CreateMaterial(new Color(0.48f, 0.5f, 0.49f));
            Material water = CreateMaterial(new Color(0.12f, 0.18f, 0.2f));
            Material wood = CreateMaterial(new Color(0.2f, 0.16f, 0.12f));

            CreateBlock("Salt Ground", environment.transform, new Vector3(0f, -0.3f, 0f),
                new Vector3(42f, 0.5f, 42f), salt);

            for (int i = -3; i <= 3; i++)
            {
                CreateBlock($"Water Channel {i}", environment.transform, new Vector3(i * 5f, 0.01f, 2f),
                    new Vector3(3.2f, 0.08f, 32f), water);
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                float radius = Mathf.Lerp(8f, 17f, (float)GenerationRandom.NextDouble());
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
                GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mound.name = $"Salt Mound {i + 1}";
                mound.transform.SetParent(environment.transform, false);
                mound.transform.localPosition = new Vector3(position.x, 0.25f, position.z);
                mound.transform.localScale = new Vector3(2.4f, 0.8f, 2.4f);
                mound.GetComponent<Renderer>().sharedMaterial = salt;
            }

            CreateBlock("Abandoned Walkway", environment.transform, new Vector3(0f, 0.22f, -5f),
                new Vector3(2.2f, 0.35f, 22f), wood);
            CreateLighting();
            CreateTargets(environment.transform);
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new("Gloomy Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.55f, 0.62f, 0.66f);
            light.intensity = 0.55f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(55f, -25f, 0f);
        }

        private static void CreateTargets(Transform parent)
        {
            Material targetMaterial = CreateMaterial(new Color(0.25f, 0.08f, 0.07f));
            for (int i = 0; i < 3; i++)
            {
                GameObject target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                target.name = $"Damageable Target {i + 1}";
                target.transform.SetParent(parent, false);
                target.transform.localPosition = new Vector3(-4f + i * 4f, 1f, 8f);
                target.GetComponent<Renderer>().sharedMaterial = targetMaterial;
                target.AddComponent<DamageableTarget>();
            }
        }
    }
}
