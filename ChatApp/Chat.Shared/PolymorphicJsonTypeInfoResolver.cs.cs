// File: Chat.Shared/PolymorphicJsonTypeInfoResolver.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Chat.Shared;

/// <summary>
/// Cấu hình này giúp System.Text.Json tự động đăng ký
/// tất cả các lớp con (derived types) mà chúng ta đã định nghĩa
/// bằng [JsonDerivedType] trong file Protocol.cs.
/// </summary>
public class PolymorphicJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Type == typeof(BaseMessage))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            // Tự động thêm các kiểu đã định nghĩa trong Protocol.cs
            var derivedTypes = GetAttributeDerivedTypes(typeof(BaseMessage));
            foreach (var derivedType in derivedTypes)
            {
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(derivedType.Type, derivedType.TypeDiscriminator)
                );
            }
        }

        return jsonTypeInfo;
    }

    private static IEnumerable<(Type Type, string TypeDiscriminator)> GetAttributeDerivedTypes(Type baseType)
    {
        var attributes = baseType.GetCustomAttributes(typeof(JsonDerivedTypeAttribute), false);
        foreach (JsonDerivedTypeAttribute attribute in attributes)
        {
            yield return (attribute.DerivedType, attribute.TypeDiscriminator?.ToString() ?? attribute.DerivedType.Name);
        }
    }
}