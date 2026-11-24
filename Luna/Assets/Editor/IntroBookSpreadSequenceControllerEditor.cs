
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IntroBookSpreadSequenceController))]
public class IntroBookSpreadSequenceControllerEditor : Editor
{
    private SerializedProperty pages;
    private bool listChanged;
    private readonly Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    private bool Fold(string id, string title, bool defaultOpen = false)
    {
        if (!foldouts.TryGetValue(id, out bool open)) open = defaultOpen;
        open = EditorGUILayout.Foldout(open, title, true);
        foldouts[id] = open;
        return open;
    }

    private static void Field(SerializedProperty parent, string field, string label = null)
    {
        SerializedProperty p = parent.FindPropertyRelative(field);
        if (p != null)
            EditorGUILayout.PropertyField(p, new GUIContent(label ?? p.displayName), true);
    }

    private void OnEnable()
    {
        pages = serializedObject.FindProperty("pages");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        listChanged = false;

        DrawPages();

        if (listChanged)
        {
            serializedObject.ApplyModifiedProperties();
            Repaint();
            return;
        }

        EditorGUILayout.Space(12);
        DrawOtherSettings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPages()
    {
        EditorGUILayout.LabelField(
            "BOOK SPREADS", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Each spread owns its page object and artwork. " +
            "Artwork can be anywhere in the Canvas hierarchy.",
            MessageType.Info);

        if (pages == null)
            return;

        for (int i = 0; i < pages.arraySize; i++)
        {
            SerializedProperty page =
                pages.GetArrayElementAtIndex(i);

            DrawPage(page, i);

            if (listChanged)
                return;
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("+ Add Spread", GUILayout.Height(28)))
        {
            int index = pages.arraySize;
            pages.InsertArrayElementAtIndex(index);

            SerializedProperty page =
                pages.GetArrayElementAtIndex(index);

            page.FindPropertyRelative("name").stringValue =
                "Spread " + (index + 1);

            page.FindPropertyRelative("pageObject")
                .objectReferenceValue = null;

            page.FindPropertyRelative("artwork").ClearArray();
            page.FindPropertyRelative("pagePrefab").objectReferenceValue = null;
            page.FindPropertyRelative("interactKeys").ClearArray();
            page.FindPropertyRelative("pageId").stringValue = "";
            page.FindPropertyRelative("availableFromStart").boolValue = true;
            page.FindPropertyRelative("isOpeningSpread").boolValue = false;
            page.FindPropertyRelative("autoAdvanceAfterInteractions").boolValue = false;
            page.FindPropertyRelative("autoAdvanceAfterArtwork").stringValue = "";
            page.FindPropertyRelative("autoAdvanceAfterEvent").enumValueIndex = 1;
            page.FindPropertyRelative("autoAdvanceDelay").floatValue = 0f;
            page.FindPropertyRelative("requireInteractionsBeforeTurning").boolValue = true;
            page.FindPropertyRelative("replayInteractionsOnRevisit").boolValue = false;
            page.FindPropertyRelative("waitForArtworkBeforeTurning").boolValue = false;
            page.FindPropertyRelative("unlockArtworkName").stringValue = "";
            page.FindPropertyRelative("unlockEvent").enumValueIndex = 1;
            page.FindPropertyRelative("unlockDelay").floatValue = 0f;
            page.FindPropertyRelative("ribbonGroup").objectReferenceValue = null;
            page.FindPropertyRelative("specialRibbon").objectReferenceValue = null;
            page.FindPropertyRelative("pageTurnRibbon").objectReferenceValue = null;
            page.FindPropertyRelative("closeBookRibbon").objectReferenceValue = null;
            page.FindPropertyRelative("useOneTimeSpecialRibbon").boolValue = false;
            page.FindPropertyRelative("specialRibbonId").stringValue = "";
            page.FindPropertyRelative("specialRibbonKey").intValue = (int)KeyCode.W;
            page.FindPropertyRelative("specialRibbonExitsBook").boolValue = true;
            page.FindPropertyRelative("ribbonReadyArtwork").stringValue = "";
            page.FindPropertyRelative("ribbonReadyEvent").enumValueIndex = 1;
            page.FindPropertyRelative("ribbonFadeDuration").floatValue = 0.25f;
            // The event is a new serialized field; clear copied listeners on a new spread.
            SerializedProperty eventProperty = page.FindPropertyRelative("onSpecialRibbonActivated");
            SerializedProperty calls = eventProperty != null
                ? eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls") : null;
            if (calls != null) calls.ClearArray();

            page.isExpanded = true;
            listChanged = true;
        }
    }

    private void DrawPage(SerializedProperty page, int index)
    {
        SerializedProperty name =
            page.FindPropertyRelative("name");

        SerializedProperty pageObject =
            page.FindPropertyRelative("pageObject");

        SerializedProperty artwork =
            page.FindPropertyRelative("artwork");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        string title = string.IsNullOrWhiteSpace(name.stringValue)
            ? "Spread " + (index + 1)
            : name.stringValue;

        page.isExpanded = EditorGUILayout.Foldout(
            page.isExpanded,
            (index + 1) + ". " + title,
            true);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("↑", GUILayout.Width(25)) && index > 0)
        {
            pages.MoveArrayElement(index, index - 1);
            listChanged = true;
        }

        if (GUILayout.Button("↓", GUILayout.Width(25)) &&
            index < pages.arraySize - 1)
        {
            pages.MoveArrayElement(index, index + 1);
            listChanged = true;
        }

        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            pages.DeleteArrayElementAtIndex(index);
            listChanged = true;
        }

        EditorGUILayout.EndHorizontal();

        if (listChanged)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        if (page.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                name, new GUIContent("Spread Name"));

            EditorGUILayout.PropertyField(
                pageObject, new GUIContent("Page GameObject"));

            EditorGUILayout.Space(5);
            if (Fold(page.propertyPath + ".availability", "PAGE ACCESS & OPENING"))
            {
                EditorGUI.indentLevel++;
                Field(page, "pageId", "Page ID");
                Field(page, "availableFromStart", "Available From Start");
                Field(page, "isOpeningSpread", "Opening Spread (No Back Navigation)");
                Field(page, "autoAdvanceAfterInteractions", "Auto Advance After Interactions");
                if (page.FindPropertyRelative("autoAdvanceAfterInteractions").boolValue)
                {
                    Field(page, "autoAdvanceAfterArtwork", "Wait For Artwork (Optional)");
                    if (!string.IsNullOrWhiteSpace(page.FindPropertyRelative("autoAdvanceAfterArtwork").stringValue))
                        Field(page, "autoAdvanceAfterEvent", "Wait For Event");
                    Field(page, "autoAdvanceDelay", "Additional Delay");
                }
                EditorGUI.indentLevel--;
            }
            DrawRibbonSettings(page);
            DrawInteractKeys(page);
            if (listChanged)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField(
                "ARTWORK", EditorStyles.boldLabel);

            for (int j = 0; j < artwork.arraySize; j++)
            {
                DrawArtwork(artwork, j);

                if (listChanged)
                    break;
            }

            if (!listChanged && GUILayout.Button("+ Add Artwork"))
            {
                int newIndex = artwork.arraySize;
                artwork.InsertArrayElementAtIndex(newIndex);

                SerializedProperty entry =
                    artwork.GetArrayElementAtIndex(newIndex);

                entry.FindPropertyRelative("name").stringValue =
                    "New Artwork";

                entry.FindPropertyRelative("graphic")
                    .objectReferenceValue = null;

                entry.FindPropertyRelative("canvasGroup")
                    .objectReferenceValue = null;

                entry.FindPropertyRelative("animation")
                    .enumValueIndex = 1;

                entry.FindPropertyRelative("delay").floatValue = 0f;
                entry.FindPropertyRelative("duration").floatValue = 1f;

                entry.FindPropertyRelative("startingAlpha")
                    .floatValue = 0f;

                entry.FindPropertyRelative("targetAlpha")
                    .floatValue = 1f;

                entry.FindPropertyRelative("startingColor")
                    .colorValue = Color.white;

                entry.FindPropertyRelative("shifts").ClearArray();
                entry.FindPropertyRelative("steps").ClearArray();
                entry.FindPropertyRelative("keepVisibleForBookSession").boolValue = false;
                entry.FindPropertyRelative("keepVisibleWhenShared").boolValue = false;
                entry.FindPropertyRelative("keepFinalStateOnRevisit").boolValue = false;
                entry.FindPropertyRelative("waitForInteractKey").boolValue = false;
                entry.FindPropertyRelative("interactionName").stringValue = "";
                entry.FindPropertyRelative("dependencies").ClearArray();
                entry.FindPropertyRelative("specialDependencies").ClearArray();
                entry.FindPropertyRelative("initialSFX").objectReferenceValue = null;
                entry.FindPropertyRelative("ambientSFX").objectReferenceValue = null;
                entry.FindPropertyRelative("initialVolume").floatValue = 1f;
                entry.FindPropertyRelative("ambientVolume").floatValue = 1f;
                entry.FindPropertyRelative("initialSFXTriggerAlpha").floatValue = 0.9f;
                entry.FindPropertyRelative("ambientStartAlpha").floatValue = 0.9f;
                entry.FindPropertyRelative("specialAnimation").enumValueIndex = 0;
                entry.FindPropertyRelative("dewExitDelay").floatValue = 0f;
                entry.FindPropertyRelative("dewExitDuration").floatValue = 1f;
                entry.FindPropertyRelative("dewEndScale").floatValue = 0.5f;
                entry.FindPropertyRelative("dewDropDistance").floatValue = 30f;
                entry.FindPropertyRelative("steamGraphic").objectReferenceValue = null;
                entry.FindPropertyRelative("steamMinimumAlpha").floatValue = 0.65f;
                entry.FindPropertyRelative("steamMaximumAlpha").floatValue = 1f;
                entry.FindPropertyRelative("steamCycleDuration").floatValue = 2f;
                entry.FindPropertyRelative("steamPhaseOffset").floatValue = 0f;

                entry.isExpanded = true;
                listChanged = true;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);
    }

    private void DrawRibbonSettings(SerializedProperty page)
    {
        if (!Fold(page.propertyPath + ".ribbons", "RIBBON GROUP (OPTIONAL)")) return;
        EditorGUI.indentLevel++;
        EditorGUILayout.HelpBox(
            "Assign one parent group per spread (left or right), outside the Page GameObjects. " +
            "The controller fades this parent and activates exactly one assigned child. " +
            "An unassigned ribbon group preserves the original W/Continue behavior.",
            MessageType.Info);
        Field(page, "ribbonGroup", "Ribbon Group Parent");
        if (page.FindPropertyRelative("ribbonGroup").objectReferenceValue != null)
        {
            Field(page, "specialRibbon", "Special Action Child");
            Field(page, "pageTurnRibbon", "Page Turn Child (A / D)");
            Field(page, "closeBookRibbon", "Close Book Child (E)");
            Field(page, "ribbonReadyArtwork", "Reveal After Artwork (Optional)");
            if (!string.IsNullOrWhiteSpace(page.FindPropertyRelative("ribbonReadyArtwork").stringValue))
                Field(page, "ribbonReadyEvent", "Reveal After Event");
            Field(page, "ribbonFadeDuration", "Ribbon Fade Duration");
            Field(page, "useOneTimeSpecialRibbon", "Use One-Time Special Ribbon");
            if (page.FindPropertyRelative("useOneTimeSpecialRibbon").boolValue)
            {
                EditorGUI.indentLevel++;
                Field(page, "specialRibbonId", "Persistent Action ID");
                Field(page, "specialRibbonKey", "Special Action Key");
                Field(page, "specialRibbonExitsBook", "Exit Book On Action");
                Field(page, "onSpecialRibbonActivated", "On Special Action (Optional)");
                EditorGUI.indentLevel--;
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawInteractKeys(SerializedProperty page)
    {
        SerializedProperty keys = page.FindPropertyRelative("interactKeys");
        if (keys == null) return;
        if (!Fold(page.propertyPath + ".interactions", "INTERACT KEYS (" + keys.arraySize + ")"))
            return;
        EditorGUI.indentLevel++;
        EditorGUILayout.HelpBox("Interactions run in this list's order. " +
            "Artwork can share an Interaction Name. After Artwork waits for the named artwork event.",
            MessageType.Info);
        Field(page, "requireInteractionsBeforeTurning", "Lock Page Turning Until Keys Complete");
        Field(page, "replayInteractionsOnRevisit", "Replay Keys On Revisit");
        Field(page, "waitForArtworkBeforeTurning", "Wait For Artwork Before Turning");
        if (page.FindPropertyRelative("waitForArtworkBeforeTurning").boolValue)
        {
            Field(page, "unlockArtworkName", "Unlock After Artwork");
            Field(page, "unlockEvent", "Unlock Event");
            Field(page, "unlockDelay", "Additional Unlock Delay");
        }
        for (int i = 0; i < keys.arraySize; i++)
        {
            SerializedProperty key = keys.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            string title = key.FindPropertyRelative("name").stringValue;
            key.isExpanded = EditorGUILayout.Foldout(key.isExpanded,
                string.IsNullOrWhiteSpace(title) ? "Interaction " + (i + 1) : title, true);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
            { keys.MoveArrayElement(i, i - 1); listChanged = true; }
            if (GUILayout.Button("↓", GUILayout.Width(25)) && i < keys.arraySize - 1)
            { keys.MoveArrayElement(i, i + 1); listChanged = true; }
            if (GUILayout.Button("×", GUILayout.Width(25)))
            { keys.DeleteArrayElementAtIndex(i); listChanged = true; }
            EditorGUILayout.EndHorizontal();
            if (listChanged) { EditorGUILayout.EndVertical(); break; }
            if (key.isExpanded)
            {
                EditorGUI.indentLevel++;
                Field(key, "name", "Interaction Name");
                Field(key, "key", "Key");
                Field(key, "promptGraphic", "Prompt Graphic");
                Field(key, "promptCanvasGroup", "Prompt Canvas Group");
                Field(key, "promptTiming", "Prompt Timing");
                if (key.FindPropertyRelative("promptTiming").enumValueIndex == 1)
                {
                    Field(key, "afterArtwork", "After Artwork Named");
                    Field(key, "afterEvent", "After Event");
                }
                Field(key, "promptDelay", "Prompt Delay");
                Field(key, "promptFadeIn", "Prompt Fade In");
                Field(key, "promptFadeOut", "Prompt Fade Out");
                Field(key, "pressSFX", "Key Press SFX");
                if (key.FindPropertyRelative("pressSFX").objectReferenceValue != null)
                    Field(key, "pressVolume", "SFX Volume");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        if (!listChanged && GUILayout.Button("+ Add Interact Key"))
        {
            int i = keys.arraySize;
            keys.InsertArrayElementAtIndex(i);
            SerializedProperty key = keys.GetArrayElementAtIndex(i);
            key.FindPropertyRelative("name").stringValue = "Interaction " + (i + 1);
            key.FindPropertyRelative("key").intValue = (int)KeyCode.E;
            key.FindPropertyRelative("promptGraphic").objectReferenceValue = null;
            key.FindPropertyRelative("promptCanvasGroup").objectReferenceValue = null;
            key.FindPropertyRelative("promptTiming").enumValueIndex = 0;
            key.FindPropertyRelative("afterArtwork").stringValue = "";
            key.FindPropertyRelative("afterEvent").enumValueIndex = 1;
            key.FindPropertyRelative("promptDelay").floatValue = 0f;
            key.FindPropertyRelative("promptFadeIn").floatValue = 0.25f;
            key.FindPropertyRelative("promptFadeOut").floatValue = 0.25f;
            key.FindPropertyRelative("pressSFX").objectReferenceValue = null;
            key.FindPropertyRelative("pressVolume").floatValue = 1f;
            key.isExpanded = true;
            listChanged = true;
        }
        EditorGUI.indentLevel--;
    }

    private void DrawArtwork(SerializedProperty artwork, int index)
    {
        SerializedProperty entry = artwork.GetArrayElementAtIndex(index);
        SerializedProperty name = entry.FindPropertyRelative("name");
        SerializedProperty animation = entry.FindPropertyRelative("animation");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        string title = string.IsNullOrWhiteSpace(name.stringValue)
            ? "Artwork " + (index + 1) : name.stringValue;
        entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, title, true);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("↑", GUILayout.Width(25)) && index > 0)
        { artwork.MoveArrayElement(index, index - 1); listChanged = true; }
        if (GUILayout.Button("↓", GUILayout.Width(25)) && index < artwork.arraySize - 1)
        { artwork.MoveArrayElement(index, index + 1); listChanged = true; }
        if (GUILayout.Button("×", GUILayout.Width(25)))
        { artwork.DeleteArrayElementAtIndex(index); listChanged = true; }
        EditorGUILayout.EndHorizontal();
        if (listChanged) { EditorGUILayout.EndVertical(); return; }

        if (entry.isExpanded)
        {
            EditorGUI.indentLevel++;
            Field(entry, "name", "Name");
            Field(entry, "graphic", "Image / Graphic");
            Field(entry, "canvasGroup", "Canvas Group (Optional)");
            if (Fold(entry.propertyPath + ".settings", "Settings"))
            {
                EditorGUI.indentLevel++;
                if (Fold(entry.propertyPath + ".persistence", "Persistence (Optional)"))
                {
                    EditorGUI.indentLevel++;
                    Field(entry, "keepVisibleForBookSession", "Keep Visible For Book Session");
                    Field(entry, "keepVisibleWhenShared", "Keep Visible On Shared Spreads");
                    Field(entry, "keepFinalStateOnRevisit", "Keep Final State On Revisit");
                    EditorGUILayout.HelpBox(
                        "Book Session keeps a revealed target visible on every page. " +
                        "Shared Spreads skips fading only when both adjacent spreads assign " +
                        "the same target with that option on. Shared targets must be outside " +
                        "individual Page GameObjects.", MessageType.None);
                    EditorGUI.indentLevel--;
                }
                if (Fold(entry.propertyPath + ".interaction", "Interaction & Dependencies"))
                {
                    EditorGUI.indentLevel++;
                    Field(entry, "waitForInteractKey", "Wait For Interact Key");
                    if (entry.FindPropertyRelative("waitForInteractKey").boolValue)
                        Field(entry, "interactionName", "Interaction Name");
                    DrawArtworkDependencies(entry.FindPropertyRelative("dependencies"));
                    EditorGUI.indentLevel--;
                }
                if (Fold(entry.propertyPath + ".animation", "Animation"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(animation, new GUIContent("Animation"));
                    int mode = animation.enumValueIndex;
                    if (mode == 1)
                    {
                        Field(entry, "delay", "Delay");
                        Field(entry, "duration", "Duration");
                        Field(entry, "startingAlpha", "Start Alpha");
                        Field(entry, "targetAlpha", "End Alpha");
                    }
                    else if (mode == 2)
                    {
                        Field(entry, "startingColor", "Starting Color");
                        DrawColorShifts(entry.FindPropertyRelative("shifts"));
                    }
                    else if (mode == 3)
                    {
                        Field(entry, "startingColor", "Starting Color (RGBA)");
                        DrawSequenceSteps(entry.FindPropertyRelative("steps"));
                    }
                    EditorGUI.indentLevel--;
                }
                if (Fold(entry.propertyPath + ".sounds", "Sounds (Optional)"))
                {
                    EditorGUI.indentLevel++;
                    Field(entry, "initialSFX", "Initial SFX");
                    if (entry.FindPropertyRelative("initialSFX").objectReferenceValue != null)
                    {
                        Field(entry, "initialVolume", "Initial Volume");
                        Field(entry, "initialSFXTriggerAlpha", "Initial Trigger Alpha");
                    }
                    Field(entry, "ambientSFX", "Ambient SFX (Loop)");
                    if (entry.FindPropertyRelative("ambientSFX").objectReferenceValue != null)
                    {
                        Field(entry, "ambientVolume", "Ambient Volume");
                        Field(entry, "ambientStartAlpha", "Ambient Trigger Alpha");
                    }
                    EditorGUI.indentLevel--;
                }
                if (Fold(entry.propertyPath + ".special", "Special Animation (Optional)"))
                {
                    EditorGUI.indentLevel++;
                    Field(entry, "specialAnimation", "Special Animation");
                    int special = entry.FindPropertyRelative("specialAnimation").enumValueIndex;
                    if (special != 0)
                    {
                        EditorGUILayout.HelpBox("Special dependencies delay only the special effect, not the entry fade.", MessageType.None);
                        DrawArtworkDependencies(entry.FindPropertyRelative("specialDependencies"));
                    }
                    if (special == 1)
                    {
                        Field(entry, "dewExitDelay", "Exit Delay");
                        Field(entry, "dewExitDuration", "Exit Duration");
                        Field(entry, "dewEndScale", "End Scale");
                        Field(entry, "dewDropDistance", "Drop Distance");
                    }
                    else if (special == 2)
                    {
                        Field(entry, "steamGraphic", "Steam Graphic");
                        Field(entry, "steamMinimumAlpha", "Minimum Alpha");
                        Field(entry, "steamMaximumAlpha", "Maximum Alpha");
                        Field(entry, "steamCycleDuration", "Cycle Duration");
                        Field(entry, "steamPhaseOffset", "Phase Offset");
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawArtworkDependencies(SerializedProperty dependencies)
    {
        if (dependencies == null) return;
        EditorGUILayout.LabelField("Start After Artwork", EditorStyles.miniBoldLabel);
        for (int i = 0; i < dependencies.arraySize; i++)
        {
            SerializedProperty dependency = dependencies.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dependency.FindPropertyRelative("artworkName"),
                GUIContent.none, true);
            EditorGUILayout.PropertyField(dependency.FindPropertyRelative("waitFor"),
                GUIContent.none, GUILayout.MaxWidth(145));
            if (GUILayout.Button("×", GUILayout.Width(24)))
            { dependencies.DeleteArrayElementAtIndex(i); listChanged = true; }
            EditorGUILayout.EndHorizontal();
            if (listChanged) break;
        }
        if (!listChanged && GUILayout.Button("+ Add Dependency"))
        {
            int index = dependencies.arraySize;
            dependencies.InsertArrayElementAtIndex(index);
            SerializedProperty d = dependencies.GetArrayElementAtIndex(index);
            d.FindPropertyRelative("artworkName").stringValue = "";
            d.FindPropertyRelative("waitFor").enumValueIndex = 1;
            listChanged = true;
        }
    }

    private void DrawColorShifts(SerializedProperty shifts)
    {
        EditorGUILayout.Space(3);

        EditorGUILayout.LabelField(
            "COLOR SHIFTS", EditorStyles.boldLabel);

        for (int i = 0; i < shifts.arraySize; i++)
        {
            SerializedProperty shift =
                shifts.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Shift " + (i + 1), EditorStyles.miniBoldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                shifts.DeleteArrayElementAtIndex(i);
                listChanged = true;
            }

            EditorGUILayout.EndHorizontal();

            if (listChanged)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.PropertyField(
                shift.FindPropertyRelative("delay"));

            EditorGUILayout.PropertyField(
                shift.FindPropertyRelative("duration"));

            EditorGUILayout.PropertyField(
                shift.FindPropertyRelative("targetColor"),
                new GUIContent("Target Color"));

            EditorGUILayout.EndVertical();
        }

        if (!listChanged && GUILayout.Button("+ Add Color Shift"))
        {
            int index = shifts.arraySize;
            shifts.InsertArrayElementAtIndex(index);

            SerializedProperty shift =
                shifts.GetArrayElementAtIndex(index);

            shift.FindPropertyRelative("delay").floatValue = 0f;
            shift.FindPropertyRelative("duration").floatValue = 1f;

            shift.FindPropertyRelative("targetColor").colorValue =
                Color.white;

            listChanged = true;
        }
    }

    private void DrawSequenceSteps(SerializedProperty steps)
    {
        if (steps == null)
        {
            EditorGUILayout.HelpBox(
                "Sequence Steps property not found.",
                MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField(
            "ANIMATION SEQUENCE", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Steps play in order. Each delay starts after the " +
            "previous step finishes. Fade changes alpha; " +
            "Color Shift changes RGB while preserving alpha.",
            MessageType.None);

        for (int i = 0; i < steps.arraySize; i++)
        {
            SerializedProperty step =
                steps.GetArrayElementAtIndex(i);

            SerializedProperty type =
                step.FindPropertyRelative("type");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            string stepName = type.enumValueIndex == 0
                ? "Fade"
                : "Color Shift";

            EditorGUILayout.LabelField(
                "Step " + (i + 1) + " — " + stepName,
                EditorStyles.miniBoldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
            {
                steps.MoveArrayElement(i, i - 1);
                listChanged = true;
            }

            if (GUILayout.Button("↓", GUILayout.Width(25)) &&
                i < steps.arraySize - 1)
            {
                steps.MoveArrayElement(i, i + 1);
                listChanged = true;
            }

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                steps.DeleteArrayElementAtIndex(i);
                listChanged = true;
            }

            EditorGUILayout.EndHorizontal();

            if (listChanged)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.PropertyField(
                type, new GUIContent("Type"));

            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("delay"),
                new GUIContent("Delay"));

            EditorGUILayout.PropertyField(
                step.FindPropertyRelative("duration"),
                new GUIContent("Duration"));

            if (type.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(
                    step.FindPropertyRelative("targetAlpha"),
                    new GUIContent("Target Alpha"));
            }
            else
            {
                EditorGUILayout.PropertyField(
                    step.FindPropertyRelative("targetColor"),
                    new GUIContent("Target Color"));
            }

            EditorGUILayout.EndVertical();
        }

        if (!listChanged && GUILayout.Button("+ Add Animation Step"))
        {
            int index = steps.arraySize;
            steps.InsertArrayElementAtIndex(index);

            SerializedProperty step =
                steps.GetArrayElementAtIndex(index);

            step.FindPropertyRelative("type").enumValueIndex = 0;
            step.FindPropertyRelative("delay").floatValue = 0f;
            step.FindPropertyRelative("duration").floatValue = 1f;

            step.FindPropertyRelative("targetAlpha").floatValue = 1f;

            step.FindPropertyRelative("targetColor").colorValue =
                Color.white;

            step.isExpanded = true;
            listChanged = true;
        }
    }

    private void DrawOtherSettings()
    {
        if (!Fold("controllerSettings", "CONTROLLER SETTINGS")) return;
        EditorGUI.indentLevel++;
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (iterator.name == "m_Script" || iterator.name == "pages") continue;
            EditorGUILayout.PropertyField(iterator, true);
        }
        EditorGUI.indentLevel--;
    }

}
