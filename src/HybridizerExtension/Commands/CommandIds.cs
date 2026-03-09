using System;

namespace HybridizerExtension.Commands
{
    internal static class CommandIds
    {
        public const string PackageGuidString = "d4a3b5c7-8e9f-4a1b-c2d3-e4f5a6b7c8d9";
        public static readonly Guid PackageGuid = new Guid(PackageGuidString);

        public const string CommandSetGuidString = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);

        public const string TopMenuGroupGuidString = "b2c3d4e5-f6a7-8901-bcde-f12345678901";
        public static readonly Guid TopMenuGroupGuid = new Guid(TopMenuGroupGuidString);

        public const int HybridizeProjectCommandId = 0x0100;
        public const int HybridizeProjectContextCommandId = 0x0101;
        public const int TelemetrySettingsCommandId = 0x0102;
        public const int ToolsMenuGroupId = 0x1020;
        public const int HybridizerMenuId = 0x1030;
        public const int ProjectContextMenuGroupId = 0x1040;
    }
}
