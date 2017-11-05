namespace Dast.Extensibility
{
    public interface IExtensionAdapter<out TExtension, TAdaptee> : IExtensible<TAdaptee>
    {
        TExtension Adapt(TAdaptee adaptee);
    }
}