namespace DotNetHelper.MsDiKit.RegionServices
{
    internal sealed record RegionViewDescriptor(string Key, Type View, Type? ViewModel) : IRegionViewDescriptor;
}
