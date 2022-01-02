using System.Reflection;

namespace LightADO;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DefaultValue : Attribute
{
    public DefaultValue()
    {
    }

    public DefaultValue(object value, Types.Directions direction = Types.Directions.WithNonQuery, params object[] parameters)
    {
        this.Value = value;
        this.Direction = direction;
        this.Parameters = parameters;
    }

    internal object Value { get; private set; }

    internal Types.Directions Direction { get; private set; }

    internal object[] Parameters { get; private set; }

    internal static T SetDefaultValus<T>(T objectToMapDefaultValues, Types.Directions directions = Types.Directions.WithBoth)
    {
        foreach (PropertyInfo property in objectToMapDefaultValues.GetType().GetProperties())
        {
            DefaultValue customAttribute = CustomAttributeExtensions.GetCustomAttribute<DefaultValue>((MemberInfo)property, true);
            if (customAttribute != null && (customAttribute.Direction == Types.Directions.WithBoth || customAttribute.Direction == directions))
                DefaultValue.SetDefaultValue<T>(objectToMapDefaultValues, customAttribute, objectToMapDefaultValues.GetType().GetProperty(((MemberInfo)property).Name));
        }
        return objectToMapDefaultValues;
    }

    private static T SetDefaultValue<T>(
      T objectToMapDefaultValues,
      DefaultValue defaultValueSettings,
      PropertyInfo property)
    {
        object obj = !(defaultValueSettings.Value.GetType() == typeof(string)) ? Convert.ChangeType(defaultValueSettings.Value, property.PropertyType) : (defaultValueSettings.Value.ToString().Contains(".") ? (!(property.PropertyType == typeof(string)) ? Convert.ChangeType(DefaultValue.GetDefaultValue(defaultValueSettings.Value.ToString()), property.PropertyType) : Convert.ChangeType((object)DefaultValue.GetDefaultValue(defaultValueSettings.Value.ToString()).ToString(), property.PropertyType)) : Convert.ChangeType(defaultValueSettings.Value, property.PropertyType));
        objectToMapDefaultValues.GetType().GetProperty(((MemberInfo)property).Name).SetValue((object)objectToMapDefaultValues, obj);
        return objectToMapDefaultValues;
    }

    private static object GetDefaultValue(string defaultValue)
    {
        string[] strArray = defaultValue.Split('.');
        Type defaultValueType = DefaultValue.GetDefaultValueType(strArray[0]);
        object obj = (object)null;
        for (int index = 1; index < strArray.Length; ++index)
        {
            MethodInfo defaultValueMethod = DefaultValue.GetDefaultValueMethod(strArray[index], defaultValueType);
            obj = defaultValueMethod != null ? DefaultValue.GetDefaultValueProperty(strArray[index], defaultValueType).GetValue((object)null) : ((MethodBase)defaultValueMethod).Invoke((object)null, (object[])null);
        }
        return obj;
    }

    private static Type GetDefaultValueType(string typeName)
    {
        Type type = Type.GetType("System." + typeName);
        return !(type == (Type)null) ? type : throw new LightAdoExcption("Type not found");
    }

    private static MethodInfo GetDefaultValueMethod(string methodName, Type type) => type.GetMethod(methodName);

    private static PropertyInfo GetDefaultValueProperty(string propertyName, Type type) => type.GetProperty(propertyName);
}
