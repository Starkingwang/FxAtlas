using UnityEngine;
using UnityEditor;

namespace FxAtlas
{
    [CustomEditor(typeof(FxAtlasAssembler))]
    public class FxAtlasAssemblerInspector : Editor
    {
        private void OnEnable()
        {
            var t = target as FxAtlasAssembler;
            t.UpdateSprites();
        }

        public override void OnInspectorGUI()
        {
            var t = target as FxAtlasAssembler;

            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var w = EditorGUILayout.IntSlider("Width(2x)", (int)Mathf.Log(t.AtlasSize.x, 2), 2, 11);
                var h = EditorGUILayout.IntSlider("Height(2x)", (int)Mathf.Log(t.AtlasSize.y, 2), 2, 11);
                if (c.changed)
                {
                    Undo.RecordObject(t, "Value Change");
                    EditorUtility.SetDirty(t);
                    t.AtlasSize = new Vector2Int(1 << w, 1 << h);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    var a = EditorGUILayout.ObjectField("Atlas Asset", t.AtlasAsset, typeof(FxAtlas), true);
                    if (c.changed)
                    {
                        Undo.RecordObject(t, "Value Change");
                        EditorUtility.SetDirty(t);
                        t.AtlasAsset = (FxAtlas)a;
                    }
                }

                if (GUILayout.Button("Editor", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    FxAtlasEditorWindow.OpenWindow(t);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (GUILayout.Button("Update"))
                {
                    t.UpdateSprites();
                }

                if (t.Sprites != null)
                {
                    foreach (var s in t.Sprites)
                    {
                        using (new EditorGUILayout.VerticalScope("box"))
                        {
                            FxSpriteSourceInspector.ShowInspector(s);
                        }
                    }
                }
            }
        }

        public static bool SaveAtlas(FxAtlasAssembler assembler)
        {
            if (!assembler)
            {
                return false;
            }

            string atlasAssetPath;
            string atlasTexturePath;
            FxAtlas atlasAsset = assembler.AtlasAsset;
            Texture2D atlasTexture;

            //没有atlas的时候，新建一个
            if (atlasAsset == null)
            {
                atlasAssetPath = EditorUtility.SaveFilePanel("Save", "Assets", "atlas.asset", "asset");
                if (string.IsNullOrEmpty(atlasAssetPath))
                {
                    return false;
                }
                else if (atlasAssetPath.EndsWith(".asset") == false)
                {
                    Debug.LogError("must has .asset extension");
                    return false;
                }

                atlasAssetPath = FileUtil.GetProjectRelativePath(atlasAssetPath);

                atlasAsset = ScriptableObject.CreateInstance<FxAtlas>();
                AssetDatabase.CreateAsset(atlasAsset, atlasAssetPath);
                AssetDatabase.ImportAsset(atlasAssetPath, ImportAssetOptions.ForceUpdate);
                atlasAsset = AssetDatabase.LoadAssetAtPath<FxAtlas>(atlasAssetPath);
            }
            else
            {
                //得到当前atlas的路径
                atlasAssetPath = AssetDatabase.GetAssetPath(atlasAsset);              
            }

            atlasTexturePath = atlasAssetPath.Replace(".asset", ".png");

            atlasTexture = GeneratePackedTexture(assembler);

            SavePackedTexture(atlasTexturePath, ref atlasTexture);

            Undo.RecordObjects(new Object[] { assembler, atlasAsset }, "Save Atlas");
            EditorUtility.SetDirty(assembler);
            EditorUtility.SetDirty(atlasAsset);

            atlasAsset.AtlasTexture = atlasTexture;

            var sprites = new FxSprite[assembler.Sprites.Length];

            for (int i = 0; i < assembler.Sprites.Length; i++)
            {
                sprites[i] = new FxSprite()
                {
                    id = assembler.Sprites[i].ID,
                    name = assembler.Sprites[i].IdentityName,
                    scaleOffset = assembler.Sprites[i].GetUVRect(assembler.AtlasWidth, assembler.AtlasHeight)
                };
            }

            atlasAsset.Sprites = sprites;

            assembler.AtlasAsset = atlasAsset;

            return true;
        }

        static Material m_DrawTextureMaterial;
        public static Material DrawTextureMaterial
        {
            get
            {
                if (m_DrawTextureMaterial)
                {
                    return m_DrawTextureMaterial;
                }

                var shader = Shader.Find("Hidden/DrawTexture");
                if (shader)
                {
                    m_DrawTextureMaterial = new Material(shader);
                    return m_DrawTextureMaterial;
                }

                return null;
            }
        }

        static Texture2D GeneratePackedTexture(FxAtlasAssembler assembler)
        {
            var renderTexture = new RenderTexture(assembler.AtlasWidth, assembler.AtlasHeight, 0);
            renderTexture.Create();
            var originalActive = RenderTexture.active;

            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);

            GL.Clear(true, true, Color.clear);

            foreach (var sprite in assembler.Sprites)
            {
                if (sprite.Texture)
                {
                    if (DrawTextureMaterial)
                    {
                        Graphics.DrawTexture(sprite.GetPixelRectFlipY(assembler.AtlasHeight),
                            sprite.Texture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, DrawTextureMaterial, 0);//drawColor
                        Graphics.DrawTexture(sprite.GetPixelRectFlipY(assembler.AtlasHeight),
                            sprite.Texture, new Rect(0, 0, 1, 1), 0, 0, 0, 0, DrawTextureMaterial, 1);//drawAlpha
                    }
                    else
                    {
                        Graphics.DrawTexture(sprite.GetPixelRectFlipY(assembler.AtlasHeight),
                            sprite.Texture, new Rect(0, 0, 1, 1), 0, 0, 0, 0);
                    }
                }
            }

            Texture2D tex2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            tex2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex2D.Apply();
            GL.PopMatrix();
            RenderTexture.active = originalActive;

            renderTexture.Release();

            return tex2D;
        }

        static void SavePackedTexture(string path, ref Texture2D texture)
        {
            var pngBytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngBytes);

            var tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (tImporter != null)
                tImporter.wrapMode = TextureWrapMode.Clamp;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }

}
