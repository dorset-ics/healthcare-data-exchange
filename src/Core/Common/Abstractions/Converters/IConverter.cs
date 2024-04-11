namespace Core.Common.Abstractions.Converters;

public interface IConverter<in TSource, out TTarget>
{
    public TTarget Convert(TSource source);
}