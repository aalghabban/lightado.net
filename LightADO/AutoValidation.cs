using System.Reflection;
namespace LightADO;

[AttributeUsage(AttributeTargets.Property)]
public class AutoValidation : Attribute
{
    /// <summary>
    /// Gets or sets the validatoin method name to be called during the mapping.
    /// </summary>
    public string ValidationMethodName { get; set; }

    public AutoValidation() => this.ValidationMethodName = "Validate";


    public AutoValidation(string validationMethodName) => this.ValidationMethodName = !string.IsNullOrEmpty(validationMethodName) ? validationMethodName : throw new LightAdoExcption("validation method name can't be null or empty");

    /// <summary>
    /// Validate Object
    /// </summary>
    /// <param name="T">Type of the object to validate</param>
    /// <param name="objectToValidate">object To Validate</param>
    /// <exception cref="LightAdoExcption">in case of any issues generated from LightAdo</exception>
    internal static void ValidateObject<T>(T objectToValidate)
    {
        if (objectToValidate == null)
            throw new LightAdoExcption("object to validate is null.");

        foreach (PropertyInfo property in objectToValidate.GetType().GetProperties())
        {
            object[] customAttributes = property.GetCustomAttributes(typeof(AutoValidation), true);
            if (customAttributes != null && (uint)customAttributes.Length > 0U)
            {
                foreach (AutoValidation autoValidation in customAttributes)
                    ValidateProperty(autoValidation, objectToValidate.GetType().GetProperty(property.Name).GetValue(objectToValidate));
            }
        }
    }

    /// <summary>
    /// Validate single property
    /// </summary>
    /// <param name="autoValidation">Auto Validation class</param>
    /// <param name="propertyValue">property to validate</param>
    /// <exception cref="LightAdoExcption">in case of any issues generated from LightAdo</exception>
    private static void ValidateProperty(AutoValidation autoValidation, object propertyValue)
    {
        if (autoValidation == null)
            throw new LightAdoExcption("null auto validation object.");

        if (autoValidation.GetType().GetMethod(autoValidation.ValidationMethodName) == null)
            throw new LightAdoExcption(string.Format("no validation method with name {0} found.", autoValidation.ValidationMethodName));

        autoValidation.GetType().GetMethod(autoValidation.ValidationMethodName).Invoke(autoValidation, new object[1] {propertyValue});
    }
}
