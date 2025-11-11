// File: Chat.Shared/PolymorphicJsonTypeInfoResolver.cs
// (Người 1 - Vũ Trí Dũng: Bộ giải mã Đa hình (Polymorphism))
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Chat.Shared;

public class PolymorphicJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        // Chỉ áp dụng cho lớp BaseMessage
        if (jsonTypeInfo.Type == typeof(BaseMessage))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "type", // Tự động thêm/đọc trường "type"
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            };

            // Tự động thêm các lớp con đã định nghĩa trong Protocol.cs
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

    // Hàm helper để đọc các [JsonDerivedType] từ lớp BaseMessage
    private static IEnumerable<(Type Type, string TypeDiscriminator)> GetAttributeDerivedTypes(Type baseType)
    {
        var attributes = baseType.GetCustomAttributes(typeof(JsonDerivedTypeAttribute), false);
        foreach (JsonDerivedTypeAttribute attribute in attributes)
        {
            yield return (attribute.DerivedType, attribute.TypeDiscriminator?.ToString() ?? attribute.DerivedType.Name);
        }
    }
}