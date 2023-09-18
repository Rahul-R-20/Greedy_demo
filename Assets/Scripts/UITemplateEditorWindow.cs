using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UITemplateEditorWindow : EditorWindow
{

    private string jsonFilePath = "Assets/UIObjectTemplates/template.json";
    private string jsonContent;
    private Transform selectedUITransform;
    Vector2 scroll;

    [MenuItem("Custom/UI Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<UITemplateEditorWindow>("UI Template Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("UI Template Editor");

        // Load JSON data from the file
        if (GUILayout.Button("Load JSON Data"))
        {
            LoadJsonData();
        }

        // Display and edit the JSON content

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(200));
        jsonContent = EditorGUILayout.TextArea(jsonContent);
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // Create a new UI template
        EditorGUILayout.LabelField("Create New UI Template");

        if (GUILayout.Button("Create New Template"))
        {
            CreateNewTemplate();
        }

        GUILayout.Space(10);

        // Customize selected UI element properties
        EditorGUILayout.LabelField("Customize Selected UI Element");

        // Select a UI element in the scene hierarchy to customize its properties

        if (GUILayout.Button("Select UI Element in Scene"))
        {
            if (Selection.activeTransform)
            {
                selectedUITransform = Selection.activeTransform;
            }
        }

        GUILayout.Label("Selected UI Element:");
        EditorGUI.BeginChangeCheck();
        selectedUITransform = EditorGUILayout.ObjectField(selectedUITransform, typeof(Transform), true) as Transform;
        if (selectedUITransform != null)
        {
            Text textComponent = selectedUITransform.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = EditorGUILayout.TextField("Text", textComponent.text);
            }
            EditorGUI.indentLevel++;
            selectedUITransform.localPosition = EditorGUILayout.Vector3Field("Position",selectedUITransform.localPosition);
            selectedUITransform.localRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation",selectedUITransform.localRotation.eulerAngles));
            selectedUITransform.localScale = EditorGUILayout.Vector3Field("Scale",selectedUITransform.localScale);
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndChangeCheck();

        GUILayout.Space(10);

        // Save changes back to the JSON file
        if (GUILayout.Button("Save JSON Data"))
        {
            if (SaveJsonData())
            {
                Debug.Log("JSON data saved successfully.");
            }
            else
            {
                Debug.LogError("Failed to save JSON data.");
            }
        }

        GUILayout.Space(10);

        // Instantiate a selected UI template in the scene
        EditorGUILayout.LabelField("Instantiate UI Template");

        if (GUILayout.Button("Instantiate Selected Template"))
        {
            if (InstantiateSelectedTemplate())
            {
                Debug.Log("UI template instantiated successfully.");
            }
            else
            {
                Debug.LogError("Failed to instantiate UI template.");
            }
        }
    }


    private bool InstantiateSelectedTemplate()
    {
        try
        {
            // Deserialize the selected template from JSON
            UIObjectTemplate selectedTemplate = JsonUtility.FromJson<UIObjectTemplate>(jsonContent);

            if (selectedTemplate != null)
            {
                // Create a new GameObject to hold the UI hierarchy
                GameObject uiContainer = new GameObject(selectedTemplate.TemplateName);
                uiContainer.transform.position = selectedTemplate.CanvasProperties.Position;
                uiContainer.transform.rotation = Quaternion.Euler(
                    selectedTemplate.CanvasProperties.Rotation
                );
                uiContainer.transform.localScale = selectedTemplate.CanvasProperties.Scale;

                // Create a Canvas as the root
                Canvas canvas = uiContainer.AddComponent<Canvas>();
                Image image_C = uiContainer.AddComponent<Image>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Instantiate UI elements based on the template
                foreach (var uiElementData in selectedTemplate.UIElements)
                {
                    GameObject uiElement = new GameObject(uiElementData.Name);
                    uiElement.transform.SetParent(canvas.transform);
                    uiElement.transform.localPosition = uiElementData.Position;
                    uiElement.transform.localRotation = Quaternion.Euler(uiElementData.Rotation);
                    uiElement.transform.localScale = uiElementData.Scale;
                    switch (uiElementData.Type)
                    {
                        case "Image":
                            Image image = uiElement.AddComponent<Image>();
                            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiElementData.ImageSource);
                            image.sprite = sprite;
                            Debug.Log("adding image");

                            break;
                        case "Button":
                            Button button = uiElement.AddComponent<Button>();
                            GameObject buttonText = new GameObject("Text");
                            buttonText.transform.SetParent(button.transform);
                            Text text = buttonText.AddComponent<Text>();
                            text.text = uiElementData.Text;
                            break;
                        case "ImageButton":
                            Image imgButtonImage = uiElement.AddComponent<Image>();
                            Sprite imgButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiElementData.ImageSource);
                            imgButtonImage.sprite = imgButtonSprite;

                            Button imgButton = uiElement.AddComponent<Button>();
                            // GameObject imgButtonTextObject = new GameObject("Text");
                            // imgButtonTextObject.transform.SetParent(imgButton.transform, true);
                            // Text imgButtonText = imgButtonTextObject.AddComponent<Text>();
                            // imgButtonText.text = uiElementData.Text;
                            break;
                        case "Text":
                            Text uiText = uiElement.AddComponent<Text>();
                            uiText.text = uiElementData.Text;
                            uiText.fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), uiElementData.FontStyle);
                            uiText.color = uiElementData.Color;
                            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
                            break;
                    }

                    // Customize the UI element based on its properties

                }
            }
            else
            {
                Debug.LogError("Failed to instantiate template. JSON data is invalid.");
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error instantiating UI template: {e.Message}");
            return false;
        }
    }

    private void LoadJsonData()
    {
        try
        {
            jsonContent = System.IO.File.ReadAllText(jsonFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading JSON data: {e.Message}");
        }
    }

    private bool SaveJsonData()
    {
        try
        {
            // Save the edited JSON content back to the file
            System.IO.File.WriteAllText(jsonFilePath, jsonContent);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving JSON data: {e.Message}");
            return false;
        }
    }

    private void CreateNewTemplate()
    {
        // Create a new UI template object with default properties
        UIObjectTemplate newTemplate = new UIObjectTemplate
        {
            TemplateName = "NewTemplate",
            CanvasProperties = new CanvasProperties(),
            UIElements = new UIElement[0] // Initialize with an empty array of UI elements
        };

        // Serialize the new template to JSON
        string newTemplateJson = JsonUtility.ToJson(newTemplate, true);

        // Append the new template to the existing JSON content
        jsonContent += "\n" + newTemplateJson;
    }
}

[System.Serializable]
public class CanvasProperties
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
}

[System.Serializable]
public class UIElement
{
    public string Name;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public string Type;
    public string Text;
    public string ImageSource;
    public string FontStyle;
    public Color32 Color;


    // Add more properties as needed (e.g., color, text, etc.).
}

[System.Serializable]
public class UIObjectTemplate
{
    public string TemplateName;
    public CanvasProperties CanvasProperties;
    public UIElement[] UIElements;
}