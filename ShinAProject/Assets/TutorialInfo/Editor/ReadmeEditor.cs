using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
sealed class ReadmeEditor : Editor
{
    const string k_ShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
    const string k_ReadmeSourceDirectory = "Assets/TutorialInfo";

    static ReadmeEditor()
        => EditorApplication.delayCall += SelectReadmeAutomatically;

    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(k_ShowedReadmeSessionStateName, false))
        {
            var readme = SelectReadme();
            SessionState.SetBool(k_ShowedReadmeSessionStateName, true);

            if (readme && !readme.loadedLayout)
            {
                EditorUtility.LoadWindowLayout(Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"));
                readme.loadedLayout = true;
            }
        }
    }

    static Readme SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("Readme t:Readme");
        if (ids.Length != 1)
        {
            Debug.Log("프로젝트 안내 파일을 찾지 못했습니다.");
            return null;
        }

        var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
        Selection.objects = new UnityEngine.Object[] { readmeObject };
        return (Readme)readmeObject;
    }
    
    void RemoveTutorial()
    {
        if (EditorUtility.DisplayDialog("프로젝트 안내 에셋 제거",
            
            $"{k_ReadmeSourceDirectory} 아래의 모든 내용을 제거합니다. 계속하시겠습니까?",
            "계속",
            "취소"))
        {
            if (Directory.Exists(k_ReadmeSourceDirectory))
            {
                FileUtil.DeleteFileOrDirectory(k_ReadmeSourceDirectory);
                FileUtil.DeleteFileOrDirectory(k_ReadmeSourceDirectory + ".meta");
            }
            else
            {
                Debug.Log($"프로젝트 안내 폴더를 찾지 못했습니다: {k_ReadmeSourceDirectory}");
            }

            var readmeAsset = SelectReadme();
            if (readmeAsset != null)
            {
                var path = AssetDatabase.GetAssetPath(readmeAsset);
                FileUtil.DeleteFileOrDirectory(path + ".meta");
                FileUtil.DeleteFileOrDirectory(path);
            }

            AssetDatabase.Refresh();
        }
    }

    // 기본 IMGUI 인스펙터를 숨기고 UI Toolkit 화면만 표시한다.
    protected sealed override void OnHeaderGUI() { }
    public sealed override void OnInspectorGUI() { }

    public override VisualElement CreateInspectorGUI()
    {
        var readme = (Readme)target;

        VisualElement root = new();
        root.styleSheets.Add(readme.commonStyle);
        root.styleSheets.Add(EditorGUIUtility.isProSkin ? readme.darkStyle : readme.lightStyle);

        VisualElement ChainWithClass(VisualElement created, string className)
        {
            created.AddToClassList(className);
            return created;
        }

        // 제목과 본문은 서로 다른 스타일을 적용하기 위해 별도 컨테이너로 구성한다.
        VisualElement title = new();
        title.AddToClassList("title");
        title.Add(ChainWithClass(new Image() { image = readme.icon }, "title__icon"));
        title.Add(ChainWithClass(new Label(readme.title), "title__text"));
        root.Add(title);

        foreach (var section in readme.sections)
        {
            VisualElement part = new();
            part.AddToClassList("section");

            if (!string.IsNullOrEmpty(section.heading))
                part.Add(ChainWithClass(new Label(section.heading), "section__header"));

            if (!string.IsNullOrEmpty(section.text))
                part.Add(ChainWithClass(new Label(section.text), "section__body"));

            if (!string.IsNullOrEmpty(section.linkText))
            {
                var link = ChainWithClass(new Label(section.linkText), "section__link");
                link.RegisterCallback<ClickEvent>(evt => Application.OpenURL(section.url));
                part.Add(link);
            }

            root.Add(part);
        }

        var button = new Button(RemoveTutorial) { text = "프로젝트 안내 에셋 제거" };
        button.AddToClassList("remove-readme-button");
        root.Add(button);

        return root;
    }
}
