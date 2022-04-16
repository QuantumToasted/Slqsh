namespace Slqsh;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public class ChoiceAttribute : Attribute
{
    public ChoiceAttribute(string name, object value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public object Value { get; }
}