using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.Utilities
{
    /// <summary>
    /// Represents the configuration settings for the EasySave application.
    /// It includes all the necessary settings for the application to run correctly.
    /// When the application starts, it loads the configuration from a JSON file.
    /// If the file does not exist or is empty, it creates a default configuration.
    /// </summary>
    public abstract class EasySaveConfigurationBase : IJsonSerializable
    {
        /// <summary>
        /// Saves the current configuration to a JSON file.
        /// As <see cref="EasySaveConfigurationBase"/> is a singleton, it can be a static method.
        /// </summary>
        public abstract void SaveConfiguration();

        /// <summary>
        /// Loads the configuration from a JSON file.
        /// As soon as the application starts, it loads the configuration from a JSON file.
        /// As <see cref="EasySaveConfigurationBase"/> is a singleton, it can be a static method.
        /// </summary>
        public abstract void LoadConfiguration();

        public abstract JsonObject JsonSerialize();
        public abstract void JsonDeserialize(JsonObject data);
    }
}