using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using RQSimulation.Core.Plugins;

namespace RqSim.PluginManager.UI.Configuration;

/// <summary>
/// Serializer for plugin pipeline configurations.
/// Handles saving/loading configurations to/from JSON files.
/// </summary>
public static class PluginConfigSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Default configuration file path in user's AppData folder.
    /// </summary>
    public static string DefaultConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RqSim",
        "plugins.json");

    /// <summary>
    /// Saves the pipeline configuration to a JSON file.
    /// </summary>
    /// <param name="config">Configuration to save</param>
    /// <param name="filePath">Target file path (uses default if null)</param>
    public static void Save(PluginPipelineConfig config, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        
        filePath ??= DefaultConfigPath;
        
        // Ensure directory exists
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        config.LastModified = DateTime.UtcNow;
        
        string json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a pipeline configuration from a JSON file.
    /// </summary>
    /// <param name="filePath">Source file path (uses default if null)</param>
    /// <returns>Loaded configuration, or null if file doesn't exist</returns>
    public static PluginPipelineConfig? Load(string? filePath = null)
    {
        filePath ??= DefaultConfigPath;
        
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<PluginPipelineConfig>(json, JsonOptions);
    }

    /// <summary>
    /// Creates a configuration snapshot from the current pipeline state.
    /// </summary>
    /// <param name="pipeline">Pipeline to capture</param>
    /// <param name="presetName">Optional preset name</param>
    /// <returns>Configuration representing current pipeline state</returns>
    public static PluginPipelineConfig CaptureFromPipeline(PhysicsPipeline pipeline, string? presetName = null)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        
        var config = new PluginPipelineConfig(presetName ?? "Custom");
        
        int orderIndex = 0;
        foreach (var module in pipeline.Modules)
        {
            var moduleConfig = new PluginModuleConfig
            {
                TypeName = module.GetType().FullName ?? module.GetType().Name,
                AssemblyPath = GetAssemblyPath(module.GetType()),
                IsEnabled = module.IsEnabled,
                OrderIndex = orderIndex++,
                Parameters = []
            };
            
            config.Modules.Add(moduleConfig);
        }
        
        return config;
    }

    /// <summary>
    /// Restores pipeline state from a configuration.
    /// </summary>
    /// <param name="pipeline">Pipeline to restore to</param>
    /// <param name="config">Configuration to restore from</param>
    /// <param name="clearExisting">Whether to clear existing modules first</param>
    /// <returns>Number of modules successfully restored</returns>
    public static int RestoreToPipeline(PhysicsPipeline pipeline, PluginPipelineConfig config, bool clearExisting = true)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(config);
        
        if (clearExisting)
        {
            pipeline.Clear();
        }
        
        int restoredCount = 0;
        
        // Sort modules by order index
        var orderedModules = config.Modules.OrderBy(m => m.OrderIndex).ToList();
        
        foreach (var moduleConfig in orderedModules)
        {
            try
            {
                var module = CreateModuleFromConfig(moduleConfig);
                if (module is not null)
                {
                    module.IsEnabled = moduleConfig.IsEnabled;
                    pipeline.RegisterModule(module);
                    restoredCount++;
                }
            }
            catch
            {
                // Skip modules that fail to create
            }
        }
        
        return restoredCount;
    }

    /// <summary>
    /// Creates a module instance from configuration.
    /// </summary>
    private static IPhysicsModule? CreateModuleFromConfig(PluginModuleConfig config)
    {
        Type? moduleType = null;
        
        // Try to load from external assembly first
        if (!string.IsNullOrEmpty(config.AssemblyPath) && File.Exists(config.AssemblyPath))
        {
            try
            {
                var assembly = Assembly.LoadFrom(config.AssemblyPath);
                moduleType = assembly.GetType(config.TypeName);
            }
            catch
            {
                // Fall back to searching loaded assemblies
            }
        }
        
        // Search in already loaded assemblies
        if (moduleType is null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                moduleType = assembly.GetType(config.TypeName);
                if (moduleType is not null)
                    break;
            }
        }
        
        // Try partial name match (for built-in modules)
        if (moduleType is null)
        {
            string shortName = config.TypeName.Split('.').Last();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    moduleType = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == shortName && 
                                            typeof(IPhysicsModule).IsAssignableFrom(t) &&
                                            !t.IsAbstract && !t.IsInterface);
                    if (moduleType is not null)
                        break;
                }
                catch
                {
                    // Skip assemblies that fail to enumerate types
                }
            }
        }
        
        if (moduleType is null)
            return null;
        
        // Create instance using parameterless constructor
        return Activator.CreateInstance(moduleType) as IPhysicsModule;
    }

    /// <summary>
    /// Gets the assembly path for external modules, null for built-in.
    /// </summary>
    private static string? GetAssemblyPath(Type moduleType)
    {
        var assembly = moduleType.Assembly;
        
        // Check if it's a built-in module (from RqSimGraphEngine)
        if (assembly.GetName().Name?.StartsWith("RqSim") == true ||
            assembly.GetName().Name?.StartsWith("RQSimulation") == true)
        {
            return null; // Built-in, no path needed
        }
        
        return assembly.Location;
    }

    /// <summary>
    /// Checks if a configuration file exists at the specified path.
    /// </summary>
    public static bool ConfigExists(string? filePath = null)
    {
        filePath ??= DefaultConfigPath;
        return File.Exists(filePath);
    }

    /// <summary>
    /// Deletes the configuration file at the specified path.
    /// </summary>
    public static bool DeleteConfig(string? filePath = null)
    {
        filePath ??= DefaultConfigPath;
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        
        return false;
    }
}
