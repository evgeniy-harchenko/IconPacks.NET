namespace IconPacksGenerator;
internal static class Paths
{
    internal static readonly string RootPath = Path.GetFullPath(
        "../../../../",
        AppDomain.CurrentDomain.BaseDirectory
    );
    internal static readonly string FeatherIconPath = Path.Combine(RootPath, "3rdparty/Feather/");
    internal static readonly string FontAwesomeIconPath = Path.Combine(RootPath, "3rdparty/FontAwesome/");
    internal static readonly string IonicIconPath = Path.Combine(RootPath, "3rdparty/Ionic/");
    internal static readonly string MaterialIconPath = Path.Combine(RootPath, "3rdparty/Material/");
    internal static readonly string MaterialCommunityIconPath = Path.Combine(RootPath, "3rdparty/MaterialCommunity/");
    internal static readonly string SimpleIconPath = Path.Combine(RootPath, "3rdparty/Simple/");
    internal static readonly string TablerIconPath = Path.Combine(RootPath, "3rdparty/Tabler/");
}
