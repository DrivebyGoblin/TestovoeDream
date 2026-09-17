#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveEditorTools
{
    [MenuItem("Tools/Clear Save Data")]
    public static void ClearSaveData()
    {
        // 1. Очищаем PlayerPrefs (если используете его)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Если сохраняете в JSON файл в persistentDataPath:
        string filePath = Path.Combine(Application.persistentDataPath, "save.json"); // Укажите имя вашего файла
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"<color=green>[SAVE SYSTEM]</color> Файл сохранения удален: {filePath}");
        }
        else
        {
            Debug.Log("<color=yellow>[SAVE SYSTEM]</color> Файл сохранения не найден.");
        }
    }
}
#endif