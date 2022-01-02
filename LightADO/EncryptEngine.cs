namespace LightADO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public abstract class EncryptEngine : Attribute
{
    public abstract string Encrypt(string valueToEncrypt);

    public abstract string Decrypt(string valueToDecrypt);

    internal static T EncryptOrDecryptObject<T>(T objectToEncrypt, Types.OprationType oprationType)
    {
        if (IsDefined((MemberInfo)typeof(T), typeof(EncryptEngine), true))
            EncryptOrDecryptProperties(objectToEncrypt, oprationType);
        else
            EncrypOrDecrypProperty(objectToEncrypt, oprationType);
        return objectToEncrypt;
    }

    private static void EncryptOrDecryptProperties<T>(
      T objectToEncrypt,
      Types.OprationType oprationType)
    {
        object customAttribute = ((MemberInfo)objectToEncrypt.GetType()).GetCustomAttribute(typeof(EncryptEngine), true);
        foreach (PropertyInfo property in objectToEncrypt.GetType().GetProperties())
        {
            if (property.GetValue((object)objectToEncrypt) != null && property.GetValue((object)objectToEncrypt) is string)
            {
                if (oprationType == Types.OprationType.Encrypt)
                    property.SetValue(objectToEncrypt, CallEncryptMethod(customAttribute, property.GetValue((object)objectToEncrypt).ToString()));
                else
                    property.SetValue(objectToEncrypt, CallDecryptMethod(customAttribute, property.GetValue((object)objectToEncrypt).ToString()));
            }
        }
    }

    private static void EncrypOrDecrypProperty<T>(
      T objectToEncrypt,
      Types.OprationType oprationType)
    {
        foreach (PropertyInfo propertyInfo in ((IEnumerable<PropertyInfo>)objectToEncrypt.GetType().GetProperties()).Where<PropertyInfo>((Func<PropertyInfo, bool>)(prop => ((MemberInfo)prop).IsDefined(typeof(EncryptEngine), false))))
        {
            if (propertyInfo.GetValue((object)objectToEncrypt) != null && propertyInfo.GetValue((object)objectToEncrypt) is string)
            {
                if (oprationType == Types.OprationType.Encrypt)
                    propertyInfo.SetValue((object)objectToEncrypt, (object)EncryptEngine.CallEncryptMethod((object)CustomAttributeExtensions.GetCustomAttribute((MemberInfo)propertyInfo, typeof(EncryptEngine), true), propertyInfo.GetValue((object)objectToEncrypt).ToString()));
                else
                    propertyInfo.SetValue((object)objectToEncrypt, (object)EncryptEngine.CallDecryptMethod((object)CustomAttributeExtensions.GetCustomAttribute((MemberInfo)propertyInfo, typeof(EncryptEngine), true), propertyInfo.GetValue((object)objectToEncrypt).ToString()));
            }
        }
    }

    private static string CallEncryptMethod(object customEncryptObject, string value) => ((MethodBase)customEncryptObject.GetType().GetMethod("Encrypt")).Invoke(customEncryptObject, new object[1]
    {
      (object) value
    }).ToString();

    private static string CallDecryptMethod(object customEncryptObject, string value) => ((MethodBase)customEncryptObject.GetType().GetMethod("Decrypt")).Invoke(customEncryptObject, new object[1]
    {
      (object) value
    }).ToString();
}
