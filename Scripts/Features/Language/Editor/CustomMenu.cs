namespace GameFoundation.Scripts.Features.Language.Editor
{
    using GameFoundation.Scripts.Features.Language.Components;
    using UnityEditor;
    using UnityEngine;

    public static class CustomMenu
    {
        [MenuItem("GameObject/UI/Language TMP Text", false, 10)]
        private static void CreateCustomEmpty()
        {
            var customObj = new GameObject("Language TMP Text");
            var text      = customObj.AddComponent<Language_TMP_Text>();
            var tf        = customObj.GetComponent<RectTransform>();
            tf.anchoredPosition                   = Vector2.zero;
            text.text                             = "New Language Text";
            text.enabled                          = true;
            customObj.gameObject.transform.parent = Selection.activeTransform;
            Undo.RegisterCreatedObjectUndo(customObj, "Create Custom Empty");
            Selection.activeObject = customObj;
        }

        [MenuItem("GameObject/UI/Language TMP Text", true)]
        private static bool ValidateCreateCustomEmpty()
        {
            return true;
        }
    }
}