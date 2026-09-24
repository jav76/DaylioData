namespace DaylioData;

/// <summary>
/// The <see cref="SummaryPropertyAttribute"/> is used to denote properties that are used in the summary of the Daylio data.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class SummaryPropertyAttribute : Attribute
{
}
