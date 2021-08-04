using UnityEngine;

namespace Nettle {

public interface IProjectConfigController
{
    string Name { get; }
    string Path { get; }

    void SetName(string name);
    void SetPath(string path);
}

public abstract class ProjectConfig<TConfig> : MonoBehaviour, IProjectConfigController where TConfig : class 
{
    public string Name { get; private set; }
    public string Path { get; private set; }

    [SerializeField]
    private TConfig _config;

    private static ProjectConfig<TConfig> Instance { get; set; }

    void Awake()
    {
        _config = Application.isEditor ? CreateConfigFile() : LoadConfigFile();

        Instance = this;
    }

    public static TConfig GetConfig()
    {
        return Instance._config;
    }

    private TConfig LoadConfigFile()
    {
        return null;
    }

    private TConfig CreateConfigFile()
    {
        return null;
    }

    public void SetName(string name)
    {
        
    }

    public void SetPath(string path)
    {
        
    }
}
}
