using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;


public class CropTexture : EditorWindow
{

    static string       fileName = "texture_cropped";

    static int          cropTop;
    static int          cropBot;
    static int          cropLeft;
    static int          cropRight;
    static int          prevTop;
    static int          prevBot;
    static int          prevLeft;
    static int          prevRight;
    static int          croppedWidth;
    static int          croppedHeight;

    static Texture2D    texture;
    static Texture2D    croppedTexture;


    [MenuItem("Assets/Olmi/Crop Texture", false, 0)]
    static void _CropTexture(MenuCommand menuCommand)
    {
        if (Selection.objects[0] != null && Selection.objects[0] is Texture2D)
        {
            texture = Selection.objects[0] as Texture2D;

            string path = AssetDatabase.GetAssetPath(Selection.objects[0].GetInstanceID());
            TextureImporter importer = AssetImporter.GetAtPath (path) as TextureImporter;

            if (!importer.isReadable)
            {
                Debug.Log("Texture is not read/write enabled.");
            }
            else {
                CropTexture window = ScriptableObject.CreateInstance<CropTexture>();
                window.position = new Rect(Screen.width / 2, Screen.height / 2, 200, 370);
                window.maxSize = new Vector2(200, 370);
                window.minSize = window.maxSize;
                window.ShowUtility();
            }
        }
    }


    // UI rendering and interaction
    void OnGUI()
    {
        if (texture != null)
        {
            EditorGUILayout.LabelField("Crops the selected texture in Assets.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(15);

            var texRect = new Rect(40, 40, 128, 128);
            if (croppedTexture == null)
            {
                EditorGUI.DrawPreviewTexture(texRect, texture);
            }
            else if (croppedTexture != null)
            {
                EditorGUI.DrawPreviewTexture(texRect, croppedTexture);
            }

            GUILayout.Space(140);

            EditorGUILayout.LabelField("Source dimensions: " + texture.width + ", " + texture.height);
            GUILayout.Space(15);

            cropLeft = EditorGUILayout.IntField(new GUIContent("Crop left: ", "How many pixels to crop from left."), cropLeft);
            cropRight = EditorGUILayout.IntField(new GUIContent("Crop right: ", "How many pixels to crop from right."), cropRight);
            cropTop = EditorGUILayout.IntField(new GUIContent("Crop top: ", "How many pixels to crop from top."), cropTop);
            cropBot = EditorGUILayout.IntField(new GUIContent("Crop bottom: ", "How many pixels to crop from bottom."), cropBot);

            cropLeft = Mathf.Clamp(cropLeft, 0, texture.width-1);
            cropRight = Mathf.Clamp(cropRight, 0, texture.width-1);
            cropTop = Mathf.Clamp(cropTop, 0, texture.height-1);
            cropBot = Mathf.Clamp(cropBot, 0, texture.height-1);

            croppedWidth = texture.width - cropLeft - cropRight;
            croppedHeight = texture.height - cropTop - cropBot;
            croppedWidth = Mathf.Clamp(croppedWidth, 1, texture.width);
            croppedHeight = Mathf.Clamp(croppedHeight, 1, texture.height);

            if (cropLeft != prevLeft || cropRight != prevRight || cropTop != prevTop || cropBot != prevBot)
            {
                croppedTexture = Crop(texture);
            }

            prevLeft = cropLeft;
            prevRight = cropRight;
            prevTop = cropTop;
            prevBot = cropBot;

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Cropped dimensions: " + croppedWidth + ", " + croppedHeight);

            GUILayout.Space(15);

            if (GUILayout.Button("Crop and Save"))
            {
                var path = EditorUtility.SaveFilePanel ("Select location and name", Application.dataPath, fileName + ".png", "png");

                if (path.Length != 0)
                {
                    croppedTexture = Crop(texture);

                    var pngData = croppedTexture.EncodeToPNG();

                    if (pngData != null)
                        File.WriteAllBytes(path, pngData);

                    AssetDatabase.Refresh();
                }

                this.Close();
            }
        }
        else
        {
            return;
        }

    }


    static Texture2D Crop(Texture2D texture)
    {
        var pixels = texture.GetPixels(cropLeft, cropBot, croppedWidth, croppedHeight, 0);

        var croppedTexture = new Texture2D(croppedWidth, croppedHeight);
        croppedTexture.filterMode = FilterMode.Point;
        croppedTexture.wrapMode = TextureWrapMode.Clamp;

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

}