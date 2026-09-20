using System.Collections.Generic;
using UnityEngine;

namespace ShinA.Inventory
{
    public static class SampleItemCatalog
    {
        public static IReadOnlyList<ItemDefinition> CreateSamples()
        {
            List<ItemDefinition> items = new(15)
            {
                CreateNoEffect(1, "낡은 열쇠", "어디에 사용하는지 알 수 없는 녹슨 열쇠다.", new Color(0.66f, 0.54f, 0.25f)),
                CreateNoEffect(2, "찢어진 사진", "누군가의 얼굴이 지워진 오래된 사진이다.", new Color(0.76f, 0.72f, 0.62f)),
                CreateNoEffect(3, "깨진 인형", "한쪽 눈이 사라진 작은 인형이다.", new Color(0.48f, 0.22f, 0.25f)),
                CreateNoEffect(4, "오래된 동전", "사용처를 알 수 없는 무거운 동전이다.", new Color(0.55f, 0.47f, 0.22f)),
                CreateNoEffect(5, "연구 기록", "본부에 있었던 실험에 관한 기록이다.", new Color(0.65f, 0.67f, 0.7f)),

                CreateInteractive(6, "손전등", "어두운 장소를 확인할 때 사용한다.", "손전등이 켜졌다.", new Color(0.85f, 0.79f, 0.3f)),
                CreateInteractive(7, "무전기", "희미한 잡음이 들리는 휴대용 무전기다.", "무전기에서 정체불명의 잡음이 들린다.", new Color(0.16f, 0.42f, 0.35f)),
                CreateInteractive(8, "구급 상자", "응급 처치 도구가 들어 있다.", "구급 상자를 사용했다.", new Color(0.72f, 0.16f, 0.16f)),
                CreateInteractive(9, "보안 카드", "잠긴 보안 시설에 사용할 수 있다.", "보안 카드의 표시등이 깜빡인다.", new Color(0.18f, 0.46f, 0.7f)),
                CreateInteractive(10, "이상한 부적", "가까이 대면 미세하게 진동한다.", "부적이 차갑게 떨리기 시작한다.", new Color(0.44f, 0.18f, 0.55f)),

                CreateMelee(11, "쇠파이프", "단순하지만 튼튼한 근접 무기다.", 24f, 0.55f, new Color(0.42f, 0.45f, 0.48f)),
                CreateMelee(12, "소방 도끼", "무겁고 강력한 근접 무기다.", 42f, 0.85f, new Color(0.62f, 0.12f, 0.1f)),
                CreateMelee(13, "망치", "짧은 거리에서 사용할 수 있는 무기다.", 30f, 0.65f, new Color(0.28f, 0.31f, 0.34f)),

                CreateRanged(14, "권총", "빠르게 사격할 수 있는 원거리 무기다.", 28f, 0.3f, 45f, 12, 1.4f,
                    new Color(0.18f, 0.2f, 0.22f)),
                CreateRanged(15, "신호탄 발사기", "강한 빛을 발사하는 원거리 장비다.", 18f, 0.8f, 30f, 1, 2.2f,
                    new Color(0.78f, 0.3f, 0.1f))
            };

            return items;
        }

        private static NoEffectItemDefinition CreateNoEffect(int number, string name, string description, Color color)
        {
            NoEffectItemDefinition item = ScriptableObject.CreateInstance<NoEffectItemDefinition>();
            item.ConfigureSample(number, name, description, color);
            return item;
        }

        private static InteractiveItemDefinition CreateInteractive(int number, string name, string description,
            string response, Color color)
        {
            InteractiveItemDefinition item = ScriptableObject.CreateInstance<InteractiveItemDefinition>();
            item.ConfigureSample(number, name, description, color);
            item.ConfigureResponse(response);
            return item;
        }

        private static MeleeWeaponDefinition CreateMelee(int number, string name, string description,
            float damage, float cooldown, Color color)
        {
            MeleeWeaponDefinition item = ScriptableObject.CreateInstance<MeleeWeaponDefinition>();
            item.ConfigureSample(number, name, description, color);
            item.ConfigureWeapon(damage, cooldown);
            item.ConfigureMelee(2f, 0.45f);
            return item;
        }

        private static RangedWeaponDefinition CreateRanged(int number, string name, string description,
            float damage, float cooldown, float range, int magazineSize, float reloadDuration, Color color)
        {
            RangedWeaponDefinition item = ScriptableObject.CreateInstance<RangedWeaponDefinition>();
            item.ConfigureSample(number, name, description, color);
            item.ConfigureWeapon(damage, cooldown);
            item.ConfigureRange(range);
            item.ConfigureMagazine(magazineSize, reloadDuration);
            return item;
        }
    }
}
