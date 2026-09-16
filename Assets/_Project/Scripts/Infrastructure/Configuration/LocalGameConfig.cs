using UnityEngine;

// 2. SO для удобства работы в Unity Editor
[CreateAssetMenu(fileName = "LocalGameConfig", menuName = "Configs/LocalGameConfig")]
public class LocalGameConfig : ScriptableObject, IConfigProvider
{
    [SerializeField] private GameConfig _configData;

    public GameConfig GetConfig() => _configData;
}