using System.Reflection;
using HybridizerExtension.Commands;

namespace HybridizerExtension.Tests
{
    internal static class TestHelpers
    {
        /// <summary>
        /// Creates a HybridizeProjectCommand instance without VS services,
        /// for testing internal methods like AddCudaImportsReference.
        /// </summary>
        public static HybridizeProjectCommand CreateHybridizeProjectCommand()
        {
            // Use reflection to create instance without the constructor that requires VS services
            return (HybridizeProjectCommand)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(HybridizeProjectCommand));
        }
    }
}
