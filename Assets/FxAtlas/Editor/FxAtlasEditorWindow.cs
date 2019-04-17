using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FxAtlas
{
    public class FxAtlasEditorWindow : EditorWindow
    {
        public const float BORDER_SIZE = 10;
        public const float TITLE_SIZE = 40;
        public static GUIStyle style = new GUIStyle();
        public FxAtlasAssembler assembler;
        Dictionary<Rect, FxSpriteSource> _spriteIndex = new Dictionary<Rect, FxSpriteSource>();
        Rect atlasRect;
        FxSpriteSource spriteSelected;

        [MenuItem("Window/Fx Atlas", false, 1000)]
        public static void OpenWindow(FxAtlasAssembler assembler = null)
        {
            var win = GetWindow<FxAtlasEditorWindow>();
            win.titleContent = new GUIContent("FxAtlas Editor");
            win.Show();
            if (assembler)
            {
                win.assembler = assembler;
            }
        }

        private void OnGUI()
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                GetAssembler();
                CreateAtlas();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                ChooseSprite();
            }

            DrawAtlas();

            HandleMove();

            if (assembler && assembler.gameObject.scene != null)
            {
                HandleDrag();
                HandleCommand();
            }
        }

        public void CreateAtlas()
        {
            if (GUILayout.Button("Save Atlas", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                if (!FxAtlasAssemblerInspector.SaveAtlas(assembler))
                {
                    Debug.LogError("Save Atlas Failed!");
                }
            }
        }

        void GetAssembler()
        {
            EditorGUILayout.PrefixLabel("FxAtlas Editor");
            assembler = (FxAtlasAssembler)EditorGUILayout.ObjectField(
                assembler, typeof(FxAtlasAssembler), true);
        }

        void ChooseSprite()
        {
            EditorGUILayout.PrefixLabel("Add Sprite");
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var o = EditorGUILayout.ObjectField(null, typeof(Texture2D), false);
                if (c.changed)
                {
                    if (o)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(assembler.gameObject, "Delete Sprite");
                        EditorUtility.SetDirty(assembler.gameObject);
                        AddSpriteToAtlas(o as Texture2D, Vector2.zero);
                        assembler.UpdateSprites();
                        Repaint();
                    }
                }
            }
        }

        void DrawAtlas()
        {
            if (assembler)
            {
                atlasRect = GetAtlasRect();
                DrawTextureFrame(atlasRect, Color.black, true);

                _spriteIndex.Clear();

                if (assembler.Sprites != null)
                {
                    foreach (var sprite in assembler.Sprites)
                    {
                        DrawSprite(sprite, assembler.AtlasSize, atlasRect);
                    }
                }
            }
        }

        Rect GetAtlasRect()
        {
            float aspect = (float)assembler.AtlasWidth / assembler.AtlasHeight;
            Vector2 atlasDisplayPosition;
            Vector2 atlasDisplaySize;
            atlasDisplaySize.x = position.width - BORDER_SIZE;
            atlasDisplaySize.y = position.height - BORDER_SIZE - TITLE_SIZE;
            if (atlasDisplaySize.y * aspect < atlasDisplaySize.x)
                atlasDisplaySize.x = atlasDisplaySize.y * aspect;
            else if (atlasDisplaySize.y * aspect > atlasDisplaySize.x)
                atlasDisplaySize.y = atlasDisplaySize.x / aspect;
            atlasDisplayPosition.x = (position.width - atlasDisplaySize.x) / 2;
            atlasDisplayPosition.y = (position.height + TITLE_SIZE - atlasDisplaySize.y) / 2;

            return new Rect(atlasDisplayPosition, atlasDisplaySize);
        }

        void DrawSprite(FxSpriteSource sprite, Vector2Int atlasSize, Rect atlasRect)
        {
            var spriteDrawRect = sprite.GetRect(atlasRect, atlasSize.x, atlasSize.y);

            _spriteIndex[spriteDrawRect] = sprite;

            if (sprite.Texture != null)
                GUI.DrawTextureWithTexCoords(spriteDrawRect, sprite.Texture, Rect.MinMaxRect(0, 0, 1, 1));

            Color color = Color.green;
            bool fill = false;

            if (!sprite.Texture)
            {
                color = Color.gray;
                fill = true;
            }
            else if (sprite.PositionOverlap && sprite.NameConflict)
            {
                color = Color.cyan;
                fill = true;
            }
            else if (sprite.PositionOverlap)
            {
                color = Color.red;
                fill = true;
            }
            else if (sprite.NameConflict)
            {
                color = Color.yellow;
                fill = true;
            }

            DrawTextureFrame(spriteDrawRect, color, fill);
        }

        void DrawTextureFrame(Rect rect, Color color, bool fill)
        {
            var orgColor = GUI.color;

            style.normal.background = EditorGUIUtility.whiteTexture;

            Rect topLine = Rect.MinMaxRect(rect.xMin, rect.yMin - 0.5f, rect.xMax, rect.yMin + 0.5f);
            Rect bottomLine = Rect.MinMaxRect(rect.xMin, rect.yMax - 0.5f, rect.xMax, rect.yMax + 0.5f);
            Rect leftLine = Rect.MinMaxRect(rect.xMin - 0.5f, rect.yMin, rect.xMin + 0.5f, rect.yMax);
            Rect rightLine = Rect.MinMaxRect(rect.xMax - 0.5f, rect.yMin, rect.xMax + 0.5f, rect.yMax);

            color.a *= 0.8f;
            GUI.color = color;
            GUI.Box(topLine, "", style);
            GUI.Box(bottomLine, "", style);
            GUI.Box(leftLine, "", style);
            GUI.Box(rightLine, "", style);

            if (fill)
            {
                color.a *= 0.5f;
                GUI.color = color;
                GUI.Box(rect, "", style);
            }

            GUI.color = orgColor;
        }

        void HandleMove()
        {
            var evt = Event.current;
            if (evt.type == EventType.MouseDown)
            {
                bool selected = false;
                foreach (var s in _spriteIndex)
                {
                    if (s.Value == null)
                        continue;

                    if (!s.Key.Contains(evt.mousePosition))
                        continue;

                    spriteSelected = s.Value;
                    SelectSprite(spriteSelected, evt.shift);
                    selected = true;
                    break;
                }

                if (!selected && !evt.shift)
                {
                    spriteSelected = null;
                    SelectSprite(null, false);
                }

                EditorGUI.FocusTextInControl("");
            }
            else if (evt.type == EventType.MouseDrag)
            {
                if (spriteSelected != null)
                {
                    bool pass = false;
                    var norCenterPos = Rect.PointToNormalized(atlasRect, evt.mousePosition);

                    if (!new Rect(0, 0, 1, 1).Contains(norCenterPos))
                        return;

                    foreach (var s in _spriteIndex)
                    {
                        if (s.Key.Contains(evt.mousePosition) && s.Value != spriteSelected)
                        {
                            pass = true;
                            break;
                        }
                    }

                    if (!pass)
                    {
                        Undo.RecordObject(spriteSelected, "Move Sprite");
                        EditorUtility.SetDirty(spriteSelected);
                        spriteSelected.SetUVPosition(norCenterPos, assembler.AtlasWidth, assembler.AtlasHeight);
                        CheckSpriteValid();
                        Repaint();
                    }
                }
            }
        }
        void HandleDrag()
        {
            Event evt = Event.current;
            var norCenterPos = Rect.PointToNormalized(atlasRect, evt.mousePosition);

            if (!new Rect(0, 0, 1, 1).Contains(norCenterPos))
                return;

            switch (evt.type)
            {
                case EventType.DragPerform:
                case EventType.DragUpdated:
                    if (!atlasRect.Contains(evt.mousePosition))
                        break;

                    bool validate = false;

                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        validate |= obj is Texture2D;
                    }

                    DragAndDrop.visualMode = validate ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

                    if (!validate)
                        break;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        Undo.RegisterFullObjectHierarchyUndo(assembler.gameObject, "Delete Sprite");
                        EditorUtility.SetDirty(assembler.gameObject);

                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            var tex = obj as Texture2D;
                            if (tex)
                            {
                                AddSpriteToAtlas(tex, norCenterPos);
                            }
                        }

                        assembler.UpdateSprites();
                        CheckSpriteValid();
                        Repaint();
                    }
                    break;
            }
        }

        void HandleCommand()
        {
            Event evt = Event.current;
            if (evt.type == EventType.ValidateCommand)
            {
                if (evt.commandName == "Delete" || evt.commandName == "SoftDelete")
                {
                    if (assembler != null)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(assembler.gameObject, "Delete Sprite");
                        EditorUtility.SetDirty(assembler.gameObject);

                        foreach (var go in Selection.gameObjects)
                        {
                            var sprite = go ? go.GetComponent<FxSpriteSource>() : null;
                            if (sprite)
                            {
                                DestroyImmediate(sprite.gameObject);
                            }
                        }

                        assembler.UpdateSprites();
                        CheckSpriteValid();
                        Repaint();
                    }
                }
            }
        }

        void AddSpriteToAtlas(Texture2D texture, Vector2 pos)
        {
            var sprite = new GameObject().AddComponent<FxSpriteSource>();
            sprite.transform.SetParent(assembler.transform, false);
            sprite.Texture = texture;

            sprite.SetUVPosition(pos, assembler.AtlasWidth, assembler.AtlasHeight);
        }

        void CheckSpriteValid()
        {
            foreach (var sl in _spriteIndex)
            {
                foreach (var sr in _spriteIndex)
                {
                    if (sl.Value == sr.Value)
                        continue;

                    bool overlap = sl.Key.Overlaps(sr.Key);
                    bool nameConflict = sl.Value.IdentityName == sr.Value.IdentityName;

                    sl.Value.PositionOverlap = overlap;
                    sr.Value.PositionOverlap = overlap;
                    sl.Value.NameConflict = nameConflict;
                    sr.Value.NameConflict = nameConflict;
                }
            }
        }

        void SelectSprite(FxSpriteSource sprite, bool additive)
        {
            if (additive)
            {
                if (sprite != null)
                {
                    List<Object> objs = new List<Object>(Selection.objects);
                    if (objs.Contains(sprite.gameObject))
                        objs.Remove(sprite.gameObject);
                    else
                        objs.Add(sprite.gameObject);

                    Selection.objects = objs.ToArray();
                }
            }
            else
                Selection.objects = sprite != null ? new Object[] { sprite.gameObject } : new Object[] { assembler.gameObject };
        }
    }
}


