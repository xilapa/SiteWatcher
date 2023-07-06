namespace SiteWatcher.IntegrationTests.Utils;

public static class EnvironmentUtils
{
    public static void ApplyEnvironmentVariables(params object[] envValueObjects)
    {
        foreach (var envValueObject in envValueObjects)
        {
            foreach (var prop in envValueObject.GetType().GetProperties())
            {
                var value = prop.PropertyType == typeof(byte[])
                    ? Convert.ToBase64String((prop.GetValue(envValueObject) as byte[])!)
                    : prop.GetValue(envValueObject)!.ToString();
                if(value == null) continue;
                Environment.SetEnvironmentVariable(prop.Name, value);
            }
        }
    }
}