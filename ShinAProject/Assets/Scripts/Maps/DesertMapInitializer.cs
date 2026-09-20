using UnityEngine;
using UnityEngine.Rendering;

namespace ShinA.Maps
{
    public sealed class DesertMapInitializer : MapSceneInitializer
    {
        [SerializeField, Range(8, 32)] private int duneCount = 18;
        [SerializeField, Range(4, 16)] private int pillarCount = 8;

        private Material sand;
        private Material stone;
        private Material paleStone;

        protected override void InitializeEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.87f, 0.77f, 0.55f);
            RenderSettings.fogDensity = 0.016f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.66f, 0.59f, 0.43f);

            sand = CreateMaterial(new Color(0.76f, 0.57f, 0.3f));
            stone = CreateMaterial(new Color(0.1f, 0.12f, 0.13f));
            paleStone = CreateMaterial(new Color(0.65f, 0.55f, 0.37f));
            GameObject environment = new("Desert Environment");
            environment.transform.SetParent(transform, false);

            CreateBlock("Sand Ground", environment.transform, new Vector3(0f, -0.3f, 0f),
                new Vector3(72f, 0.6f, 72f), sand);

            for (int i = 0; i < duneCount; i++)
            {
                float angle = i * 360f / duneCount;
                float radius = Mathf.Lerp(22f, 26f, (float)GenerationRandom.NextDouble());
                float height = Mathf.Lerp(4f, 8f, (float)GenerationRandom.NextDouble());
                GameObject dune = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dune.name = $"Dune {i + 1}";
                dune.transform.SetParent(environment.transform, false);
                dune.transform.localPosition = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
                dune.transform.localScale = new Vector3(17f, height, 14f);
                dune.GetComponent<Renderer>().sharedMaterial = sand;
            }

            // 생성 장애물이 시작 지점과 랜드마크 진입로를 막지 않도록 이 구역을 비워 둔다.
            Vector3 landmark = new(0f, 0f, 13f);
            for (int i = 0; i < pillarCount; i++)
            {
                float angle = (i + 0.5f) * 360f / pillarCount;
                Vector3 position = landmark + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 7f;
                float height = Mathf.Lerp(3f, 6f, (float)GenerationRandom.NextDouble());
                position.y = height * 0.5f;
                GameObject pillar = CreateBlock($"Black Pillar {i + 1}", environment.transform,
                    position, new Vector3(0.9f, height, 0.9f), stone);
                pillar.transform.localRotation = Quaternion.Euler(0f, angle,
                    Mathf.Lerp(-8f, 8f, (float)GenerationRandom.NextDouble()));
            }

            CreateBlock("Sealed Monolith", environment.transform, landmark + Vector3.up * 3.5f,
                new Vector3(2f, 7f, 1.4f), stone);
            CreateBlock("Monolith Inlay", environment.transform, landmark + new Vector3(0f, 3.5f, -0.71f),
                new Vector3(0.12f, 4f, 0.04f), paleStone);
            CreateMissionBase(environment.transform, paleStone, sand, stone);

            for (int i = 0; i < 4; i++)
            {
                GameObject ridge = CreateBlock($"Outer Sand Ridge {i + 1}", environment.transform,
                    Quaternion.Euler(0f, i * 90f, 0f) * new Vector3(0f, 2f, 35f),
                    new Vector3(72f, 4f, 2f), sand);
                ridge.transform.localRotation = Quaternion.Euler(0f, i * 90f, 0f);
            }

            GameObject lightObject = new("Desert Sun");
            lightObject.transform.SetParent(environment.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(62f, -35f, 0f);
            Light sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.9f, 0.69f);
            sun.intensity = 1.6f;
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
            Destroy(sand);
            Destroy(stone);
            Destroy(paleStone);
        }
    }
}
