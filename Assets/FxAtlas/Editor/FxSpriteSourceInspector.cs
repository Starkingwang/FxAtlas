using UnityEngine;
using UnityEditor;

namespace FxAtlas
{
    [CustomEditor(typeof(FxSpriteSource))]
    public class FxSpriteSourceInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = target as FxSpriteSource;

            ShowInspector(t);
        }

        public static void ShowInspector(FxSpriteSource fxSprite)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    var l = EditorGUILayout.TextField("Identity Name", fxSprite.IdentityName);

                    if (c.changed)
                    {
                        Undo.RecordObject(fxSprite, "Value Change");
                        EditorUtility.SetDirty(fxSprite);
                        fxSprite.IdentityName = l;
                    }
                }

                if (GUILayout.Button("Use Texture Name", EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    if (fxSprite.Texture)
                    {
                        Undo.RecordObject(fxSprite, "Value Change");
                        EditorUtility.SetDirty(fxSprite);
                        fxSprite.IdentityName = fxSprite.Texture.name;
                    }
                }
            }

            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var id = EditorGUILayout.IntField("ID", fxSprite.ID);

                if (c.changed)
                {
                    Undo.RecordObject(fxSprite, "Value Change");
                    EditorUtility.SetDirty(fxSprite);
                    fxSprite.ID = id;
                }
            }

            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var tex = EditorGUILayout.ObjectField(
                    fxSprite.Texture ? fxSprite.Texture.name : "", fxSprite.Texture, typeof(Texture), false);

                if (c.changed)
                {
                    Undo.RecordObject(fxSprite, "Value Change");
                    EditorUtility.SetDirty(fxSprite);
                    fxSprite.Texture = tex as Texture2D;
                }
            }

            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var w = EditorGUILayout.IntSlider("Width(2x)", (int)Mathf.Log(fxSprite.PixelRect.width, 2), 2, 10);
                var h = EditorGUILayout.IntSlider("Height(2x)", (int)Mathf.Log(fxSprite.PixelRect.height, 2), 2, 10);

                if (c.changed)
                {
                    Undo.RecordObject(fxSprite, "Value Change");
                    EditorUtility.SetDirty(fxSprite);
                    fxSprite.PixelRect = new Rect(fxSprite.PixelRect.position, new Vector2Int(1 << w, 1 << h));
                }
            }

            EditorGUILayout.RectField("Pixel Rect", fxSprite.PixelRect);
        }
    }

}

