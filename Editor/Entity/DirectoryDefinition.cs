using System;
using System.Runtime.Serialization;


namespace Kumatta.BearTools.Editor
{
    [DataContract]
    public sealed class DirectoryDefinition
    {
        public enum InputType
        {
            Free,
            Number,
            Alphabet,
            AlphabetOrNumber,
            Regex
        }

        public enum HierarchicalDefinitionType
        {
            Directory,
            Text,
            Json,
            Markdown,
            Asmdef,
            Binary
        }


        [DataContract]
        public class ReplaceInput
        {
            [DataMember(Name = "label")]
            public string Label { get; set; }

            [DataMember(Name = "input-type")]
            public string InputTypeStr { get; set; }

            public InputType InputType
            {
                get
                {
                    // Enum.Parse<InputType>(InputTypeStr, true); 2019だと非対応
                    Enum.TryParse(InputTypeStr, true, out InputType result);
                    return result;
                }
            }

            [DataMember(Name = "replace-string")]
            public string ReplaceString { get; set; }

            [DataMember(Name = "defalut")]
            public string Defalut { get; set; }

            [DataMember(Name = "required")]
            public bool Required { get; set; }
        }


        [DataContract]
        public class HierarchicalDefinition
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "type")]
            public string TypeStr { get; set; }

            public HierarchicalDefinitionType Type
            {
                get
                {
                    //Enum.Parse<HierarchicalDefinitionType>(TypeStr, true); 2019だと非対応
                    Enum.TryParse(TypeStr, true, out HierarchicalDefinitionType result);
                    return result;
                }
            }

            [DataMember(Name = "tmpPath")]
            public string TmpPath { get; set; }

            [DataMember(Name = "children")]
            public HierarchicalDefinition[] Children { get; set; }
        }


        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "root-directory")]
        public string RootDirectory { get; set; }

        [DataMember(Name = "replace-input")]
        public ReplaceInput[] ReplaceInputs { get; set; }

        [DataMember(Name = "directory-definition")]
        public HierarchicalDefinition[] HirectoryDefinitions { get; set; }
    }
}


