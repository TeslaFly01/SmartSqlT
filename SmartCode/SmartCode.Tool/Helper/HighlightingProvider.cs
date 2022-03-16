using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using HandyControl.Data;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace SmartCode
{
    internal class HighlightingProvider
    {
        public static HighlightingProvider Default = new Lazy<HighlightingProvider>(() => new HighlightingProvider()).Value;

        protected static readonly Lazy<IHighlightingDefinition> DefaultDefinition = new Lazy<IHighlightingDefinition>(() => HighlightingManager.Instance.GetDefinition("C#"));// new(() => HighlightingManager.Instance.GetDefinition("C#"));

        private static readonly Dictionary<SkinType, HighlightingProvider> Providers = new Dictionary<SkinType, HighlightingProvider>();

        protected Dictionary<string, Lazy<IHighlightingDefinition>> Definition;

        public static void Register(SkinType skinType, HighlightingProvider provider)
        {
            Providers[skinType] = provider ?? throw new ArgumentNullException(nameof(provider));
            provider.InitDefinitions();
        }

        public static IHighlightingDefinition GetDefinition(SkinType skinType, string name)
        {
            if (Providers.TryGetValue(skinType, out var provider))
            {
                return provider.GetDefinition(name);
            }

            return DefaultDefinition.Value;
        }

        protected static IHighlightingDefinition LoadDefinition(string xshdName)
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri($"pack://application:,,,/Resources/xshd/{xshdName}.xshd"));
            if (streamResourceInfo == null)
            {
                return DefaultDefinition.Value;
            }

            var reader = new XmlTextReader(streamResourceInfo.Stream);
            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        protected virtual IHighlightingDefinition GetDefinition(string name)
        {
            if (Definition.TryGetValue(name, out var definition))
            {
                return definition.Value;
            }

            return DefaultDefinition.Value;
        }

        protected virtual void InitDefinitions()
        {
            Definition = new Dictionary<string, Lazy<IHighlightingDefinition>>
            {
                ["XML"] = new Lazy<IHighlightingDefinition>(() => HighlightingManager.Instance.GetDefinition("XML")),
                ["C#"] = new Lazy<IHighlightingDefinition>(() => HighlightingManager.Instance.GetDefinition("C#")),
                ["SQL"] = new Lazy<IHighlightingDefinition>(() => HighlightingManager.Instance.GetDefinition("SQL"))
            };
        }
    }

    internal class HighlightingProviderDark : HighlightingProvider
    {
        protected override void InitDefinitions()
        {
            Definition = new Dictionary<string, Lazy<IHighlightingDefinition>>
            {
                ["XML"] = new Lazy<IHighlightingDefinition>(() => LoadDefinition("XML-Dark")),
                ["C#"] = new Lazy<IHighlightingDefinition>(() => LoadDefinition("CSharp-Dark")),
                ["SQL"] = new Lazy<IHighlightingDefinition>(() => LoadDefinition("SQL-Dark")),
            };
        }
    }

    //internal class HighlightingProviderDefault : HighlightingProvider
    //{
    //    protected override void InitDefinitions()
    //    {
    //        Definition = new Dictionary<string, Lazy<IHighlightingDefinition>>
    //        {
    //            ["C#"] = new Lazy<IHighlightingDefinition>(() => LoadDefinition("CSharp")),
    //            ["SQL"] = new Lazy<IHighlightingDefinition>(() => LoadDefinition("SQL"))
    //        };
    //    }
    //}
}
