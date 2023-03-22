// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VirtualBeings.UnityIntegration;

public class VBIntegrationEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset _UXMLTree;

    private VirtualBeingsSettings _vbSettings;
    private TextField _tokenField;

    [MenuItem("Virtual Beings/Settings")]
    public static void ShowExample()
    {
        VBIntegrationEditor wnd = GetWindow<VBIntegrationEditor>();
        wnd.titleContent = new GUIContent("Virtual Beings Settings");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;


        rootVisualElement.Add(_UXMLTree.Instantiate());

        _tokenField = root.Query<TextField>("Token").First();

        string[] settingsGuid = AssetDatabase.FindAssets("Virtual Beings Settings t:scriptableobject", new string[] { "Assets" });
        if (settingsGuid == null || settingsGuid.Length <= 0)
        {
            throw new System.Exception("Could not find Virtual Beings Settings file. Did you delete it or move it from your Assets folder ?");
        }
        _vbSettings = (VirtualBeingsSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(settingsGuid[0]), typeof(VirtualBeingsSettings));

        _tokenField.SetValueWithoutNotify(_vbSettings.Key);

        //Call the event handler
        SetupButtonHandler();

    }

    private void SetupButtonHandler()
    {
        VisualElement root = rootVisualElement;

        Button saveSettingsButton = root.Query<Button>("SaveSettings").First();

        saveSettingsButton.RegisterCallback<ClickEvent>(SetupBeingInstaller);
    }


    private void SetupBeingInstaller(ClickEvent evt)
    {
        VisualElement root = rootVisualElement;

        //VirtualBeingsSettings.Instance.Token = tokenField.text;

        Debug.Log("Token set to " + _tokenField.text);

        //GameObject beingInstallerPrefab = AssetDatabase.LoadAssetAtPath("Packages/com.virtualbeings.tech.virtualbeingstech/Runtime/Prefabs/BeingInstaller.prefab", typeof(GameObject)) as GameObject;

       
        _vbSettings.Key = _tokenField.text;
        //BeingInstallerSettings beingInstallerSettings = ObjectFactory.CreateInstance<Material>();
        //material.shader = Shader.Find("Transparent/Diffuse");
        //AssetDatabase.CreateAsset(material, "Assets/newMaterial.mat");

        //Debug.Log("Todo..." + beingInstallerPrefab);
    }
}