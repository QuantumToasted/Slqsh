namespace Slqsh;

[AttributeUsage(AttributeTargets.Parameter)]
public class ExcludeEnumValuesAttribute : Attribute
{
    public ExcludeEnumValuesAttribute(params object[] excludedValues)
    {
        if (excludedValues.Length == 0)
            throw new ArgumentException("You must supply at least one enum value.", nameof(excludedValues));

        object firstValue = null;
        foreach (var value in excludedValues)
        {
            if (!value.GetType().IsEnum)
                throw new ArgumentException($"Only enum values may be supplied. Expected enum value, got {value.GetType()}.", 
                    nameof(excludedValues));

            if (firstValue is null)
            {
                firstValue = value;
                EnumType = value.GetType();
            }

            if (value.GetType() != firstValue.GetType())
                throw new ArgumentException($"All enum values supplied must match the first value's type. Expected {firstValue.GetType()}, got {value.GetType()}.",
                    nameof(excludedValues));
        }

        ExcludedValueNames = excludedValues.Select(x => x.ToString()).ToArray();
    }

    public Type EnumType { get; }

    public string[] ExcludedValueNames { get; }
}