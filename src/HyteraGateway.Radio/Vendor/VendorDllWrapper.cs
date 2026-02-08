using System.Reflection;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Vendor;

/// <summary>
/// Wrapper for vendor DLL libraries
/// Loads DLLs dynamically with graceful fallback if not available
/// </summary>
public class VendorDllWrapper : IDisposable
{
    private readonly ILogger<VendorDllWrapper>? _logger;
    private readonly string _dllBasePath;
    private bool _isLoaded = false;
    private Assembly? _hyteraProtocolAssembly;
    private Assembly? _radioControllerAssembly;
    private Assembly? _alvasAudioAssembly;

    /// <summary>
    /// Indicates whether vendor DLLs are loaded and available
    /// </summary>
    public bool IsAvailable => _isLoaded;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dllBasePath">Base path where vendor DLLs are located</param>
    /// <param name="logger">Optional logger</param>
    public VendorDllWrapper(string dllBasePath, ILogger<VendorDllWrapper>? logger = null)
    {
        _dllBasePath = dllBasePath;
        _logger = logger;
    }

    /// <summary>
    /// Loads vendor DLLs from the specified path
    /// </summary>
    /// <returns>True if DLLs loaded successfully, false otherwise</returns>
    public bool LoadDlls()
    {
        try
        {
            _logger?.LogInformation("Loading vendor DLLs from: {Path}", _dllBasePath);

            // Check if directory exists
            if (!Directory.Exists(_dllBasePath))
            {
                _logger?.LogWarning("Vendor DLL directory not found: {Path}", _dllBasePath);
                return false;
            }

            // Try to load main DLLs
            _hyteraProtocolAssembly = TryLoadAssembly("HyteraProtocol.dll");
            _radioControllerAssembly = TryLoadAssembly("BPGRadioController.dll");
            _alvasAudioAssembly = TryLoadAssembly("Alvas.Audio.dll");

            // Consider loaded if at least HyteraProtocol is available
            _isLoaded = _hyteraProtocolAssembly != null;

            if (_isLoaded)
            {
                _logger?.LogInformation("Vendor DLLs loaded successfully");
            }
            else
            {
                _logger?.LogWarning("Failed to load vendor DLLs - operating in compatibility mode");
            }

            return _isLoaded;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading vendor DLLs");
            return false;
        }
    }

    /// <summary>
    /// Tries to load an assembly from the DLL path
    /// </summary>
    private Assembly? TryLoadAssembly(string dllFileName)
    {
        try
        {
            string fullPath = Path.Combine(_dllBasePath, dllFileName);
            
            if (!File.Exists(fullPath))
            {
                _logger?.LogWarning("DLL not found: {Path}", fullPath);
                return null;
            }

            _logger?.LogDebug("Loading assembly: {FileName}", dllFileName);
            var assembly = Assembly.LoadFrom(fullPath);
            _logger?.LogDebug("Successfully loaded: {FileName} ({Version})", 
                dllFileName, assembly.GetName().Version);
            
            return assembly;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load assembly: {FileName}", dllFileName);
            return null;
        }
    }

    /// <summary>
    /// Gets a type from a loaded assembly
    /// </summary>
    /// <param name="assembly">Assembly to search</param>
    /// <param name="typeName">Type name to find</param>
    /// <returns>Type if found, null otherwise</returns>
    public Type? GetType(Assembly? assembly, string typeName)
    {
        if (assembly == null)
            return null;

        try
        {
            return assembly.GetType(typeName);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get type {TypeName}", typeName);
            return null;
        }
    }

    /// <summary>
    /// Creates an instance of a type from a loaded assembly
    /// </summary>
    /// <param name="assembly">Assembly containing the type</param>
    /// <param name="typeName">Full type name</param>
    /// <param name="args">Constructor arguments</param>
    /// <returns>Instance if created successfully, null otherwise</returns>
    public object? CreateInstance(Assembly? assembly, string typeName, params object[] args)
    {
        if (assembly == null)
            return null;

        try
        {
            var type = GetType(assembly, typeName);
            if (type == null)
                return null;

            return Activator.CreateInstance(type, args);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to create instance of {TypeName}", typeName);
            return null;
        }
    }

    /// <summary>
    /// Invokes a method on an object using reflection
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="methodName">Method name</param>
    /// <param name="args">Method arguments</param>
    /// <returns>Method return value, or null if failed</returns>
    public object? InvokeMethod(object instance, string methodName, params object[] args)
    {
        try
        {
            var type = instance.GetType();
            var method = type.GetMethod(methodName);
            
            if (method == null)
            {
                _logger?.LogWarning("Method not found: {MethodName} on type {TypeName}", 
                    methodName, type.FullName);
                return null;
            }

            return method.Invoke(instance, args);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to invoke method {MethodName}", methodName);
            return null;
        }
    }

    /// <summary>
    /// Gets a property value from an object using reflection
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="propertyName">Property name</param>
    /// <returns>Property value, or null if failed</returns>
    public object? GetPropertyValue(object instance, string propertyName)
    {
        try
        {
            var type = instance.GetType();
            var property = type.GetProperty(propertyName);
            
            if (property == null)
            {
                _logger?.LogWarning("Property not found: {PropertyName} on type {TypeName}", 
                    propertyName, type.FullName);
                return null;
            }

            return property.GetValue(instance);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get property {PropertyName}", propertyName);
            return null;
        }
    }

    /// <summary>
    /// Sets a property value on an object using reflection
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="propertyName">Property name</param>
    /// <param name="value">Value to set</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SetPropertyValue(object instance, string propertyName, object value)
    {
        try
        {
            var type = instance.GetType();
            var property = type.GetProperty(propertyName);
            
            if (property == null)
            {
                _logger?.LogWarning("Property not found: {PropertyName} on type {TypeName}", 
                    propertyName, type.FullName);
                return false;
            }

            property.SetValue(instance, value);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to set property {PropertyName}", propertyName);
            return false;
        }
    }

    /// <summary>
    /// Gets the HyteraProtocol assembly
    /// </summary>
    public Assembly? HyteraProtocolAssembly => _hyteraProtocolAssembly;

    /// <summary>
    /// Gets the RadioController assembly
    /// </summary>
    public Assembly? RadioControllerAssembly => _radioControllerAssembly;

    /// <summary>
    /// Gets the Alvas.Audio assembly
    /// </summary>
    public Assembly? AlvasAudioAssembly => _alvasAudioAssembly;

    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        // Assemblies are managed by the runtime, no explicit disposal needed
        _isLoaded = false;
    }
}
