namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Microsoft.CodeAnalysis;

internal class EndpointAttributeMetadata
{
    internal string AttributeFullyQualifiedName { get; }
    internal string AttributeShortName { get; }
    internal string AttributeInstanceSource { get; }

    internal bool IsAuthAttribute { get; }
    internal bool IsAllowAnonymous { get; }
    internal bool IsAuthorize { get; }
    internal bool IsAntiforgeryAttribute { get; }
    internal bool IsOpenApiOrCacheAttribute { get; }

    internal string? Roles { get; }
    internal string? Policy { get; }
    internal string? AuthenticationSchemes { get; }

    private EndpointAttributeMetadata(AttributeData attrData)
    {
        AttributeFullyQualifiedName = attrData.AttributeClass!.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT);
        AttributeShortName = attrData.AttributeClass!.Name;

        IsAuthorize = IsKnownAuthorizeAttribute(AttributeFullyQualifiedName);
        IsAllowAnonymous = IsKnownAllowAnonymousAttribute(AttributeFullyQualifiedName);
        IsAuthAttribute = IsAuthorize || IsAllowAnonymous;
        IsAntiforgeryAttribute = IsKnownAntiforgeryAttribute(AttributeFullyQualifiedName);
        IsOpenApiOrCacheAttribute = IsKnownOpenApiOrCacheAttribute(AttributeFullyQualifiedName);

        if (IsAuthorize)
        {
            if (0 < attrData.ConstructorArguments.Length)
            {
                var firstArg = attrData.ConstructorArguments[0];
                if (firstArg.Value is string policyFromCtor)
                    Policy = policyFromCtor;
            }

            foreach (var namedArg in attrData.NamedArguments)
            {
                if (namedArg.Key == "Roles" && namedArg.Value.Value is string roles)
                    Roles = roles;
                else if (namedArg.Key == "Policy" && namedArg.Value.Value is string policy)
                    Policy = policy;
                else if (namedArg.Key == "AuthenticationSchemes" && namedArg.Value.Value is string schemes)
                    AuthenticationSchemes = schemes;
            }
        }

        var sb = new System.Text.StringBuilder();
        sb.Append("new ");
        sb.Append(attrData.AttributeClass!.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT));

        if (0 < attrData.ConstructorArguments.Length)
        {
            sb.Append('(');
            var first = true;
            foreach (var arg in attrData.ConstructorArguments)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                sb.Append(FormatTypedConstant(arg));
            }
            sb.Append(')');
        }
        else
        {
            sb.Append("()");
        }

        if (0 < attrData.NamedArguments.Length)
        {
            sb.Append(" { ");
            var first = true;
            foreach (var named in attrData.NamedArguments)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                sb.Append(named.Key);
                sb.Append(" = ");
                sb.Append(FormatTypedConstant(named.Value));
            }
            sb.Append(" }");
        }

        AttributeInstanceSource = sb.ToString();
    }

    private string FormatTypedConstant(TypedConstant tc)
    {
        switch (tc.Kind)
        {
            case TypedConstantKind.Primitive:
                if (tc.Value is null)
                    return "null";
                if (tc.Value is string s)
                    return $"\"{s.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
                if (tc.Value is bool b)
                    return b ? "true" : "false";
                if (tc.Value is char c)
                    return $"'{c}'";
                return tc.Value.ToString()!;

            case TypedConstantKind.Type:
                var typeSymb = (INamedTypeSymbol)tc.Value!;
                return $"typeof({typeSymb.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS)})";

            case TypedConstantKind.Enum:
                if (tc.Type is null)
                    return tc.Value?.ToString() ?? "0";
                var enumType = tc.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT);
                return $"({enumType}){tc.Value}";

            case TypedConstantKind.Array:
                if (tc.Values.IsDefaultOrEmpty)
                {
                    var arrElementType = "object";
                    if (tc.Type is IArrayTypeSymbol arrType)
                        arrElementType = arrType.ElementType.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);
                    return $"new {arrElementType}[0]";
                }
                var elementType = "object";
                if (tc.Type is IArrayTypeSymbol arrayType)
                    elementType = arrayType.ElementType.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);
                var elements = string.Join(", ", tc.Values.Select(FormatTypedConstant));
                return $"new {elementType}[] {{ {elements} }}";

            default:
                return "null";
        }
    }

    private static bool IsKnownAuthorizeAttribute(string fqn)
        => fqn is
            "Microsoft.AspNetCore.Authorization.AuthorizeAttribute" or
            "Microsoft.AspNetCore.Mvc.AuthorizeAttribute";

    private static bool IsKnownAllowAnonymousAttribute(string fqn)
        => fqn is
            "Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute" or
            "Microsoft.AspNetCore.Mvc.AllowAnonymousAttribute";

    private static bool IsKnownAntiforgeryAttribute(string fqn)
        => fqn is
            "Microsoft.AspNetCore.Antiforgery.RequireAntiforgeryTokenAttribute" or
            "Microsoft.AspNetCore.Antiforgery.IgnoreAntiforgeryTokenAttribute" or
            "Microsoft.AspNetCore.Mvc.RequireAntiforgeryTokenAttribute" or
            "Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute";

    private static bool IsKnownOpenApiOrCacheAttribute(string fqn)
        => fqn is
            "Microsoft.AspNetCore.Http.EndpointSummaryAttribute" or
            "Microsoft.AspNetCore.Http.EndpointDescriptionAttribute" or
            "Microsoft.AspNetCore.Http.TagsAttribute" or
            "Microsoft.AspNetCore.Http.ProducesResponseTypeAttribute" or
            "Microsoft.AspNetCore.Http.ExcludeFromDescriptionAttribute" or
            "Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute" or
            "Microsoft.AspNetCore.OutputCaching.OutputCacheAttribute" or
            "Microsoft.AspNetCore.OutputCaching.DisableOutputCacheAttribute";

    internal static bool IsKnownEndpointAttribute(string attributeFullyQualifiedName)
        => IsKnownAuthorizeAttribute(attributeFullyQualifiedName)
        || IsKnownAllowAnonymousAttribute(attributeFullyQualifiedName)
        || IsKnownAntiforgeryAttribute(attributeFullyQualifiedName)
        || IsKnownOpenApiOrCacheAttribute(attributeFullyQualifiedName)
        || attributeFullyQualifiedName is
            "Microsoft.AspNetCore.Cors.EnableCorsAttribute" or
            "Microsoft.AspNetCore.Cors.DisableCorsAttribute" or
            "Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute" or
            "Microsoft.AspNetCore.RateLimiting.DisableRateLimitingAttribute" or
            "Microsoft.AspNetCore.Routing.HostAttribute";

    internal static bool TryCreate(AttributeData attrData, out EndpointAttributeMetadata? metadata)
    {
        metadata = null;

        if (attrData.AttributeClass is null)
            return false;

        var fqn = attrData.AttributeClass.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT);
        if (!IsKnownEndpointAttribute(fqn))
            return false;

        metadata = new EndpointAttributeMetadata(attrData);
        return true;
    }
}
