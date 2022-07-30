using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

namespace Kumatta.BearTools.Editor
{
    public class CusutomTextField : TextField
    {
        public enum TextType
        {
            Free,
            Number,
            Alphabet,
            AlphabetOrNumber,
            Regex
        }

        public static readonly ReadOnlyDictionary<TextType, string> defaultRegexPattern = new ReadOnlyDictionary<TextType, string>(new Dictionary<TextType, string>()
        {
            { TextType.Number, "^[0-9]+$" },
            { TextType.Alphabet, "^[a-zA-Z]+$" },
            { TextType.AlphabetOrNumber, "^[0-9a-zA-Z]+$" }
        });



        public new class UxmlFactory : UxmlFactory<CusutomTextField, UxmlTraits> { }

        public new class UxmlTraits : TextField.UxmlTraits
        {
            UxmlEnumAttributeDescription<TextType> textType = new UxmlEnumAttributeDescription<TextType>() { name = "Text-Type", defaultValue = TextType.Free, use = UxmlAttributeDescription.Use.Required };
            UxmlStringAttributeDescription regexString = new UxmlStringAttributeDescription() { name = "Regex-Pattern", defaultValue = "" };
            UxmlBoolAttributeDescription required = new UxmlBoolAttributeDescription() { name = "Required" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var view = (CusutomTextField)ve;
                view.label = "Cusutom Text";

                view.FieldType = textType.GetValueFromBag(bag, cc);
                view.RegexPattern = regexString.GetValueFromBag(bag, cc);
                view.Required = required.GetValueFromBag(bag, cc);

                view.RegisterValueChangedCallback(view.OnChange);
            }
        }


        public TextType FieldType { get; set; } = TextType.Free;

        public string RegexPattern { get; set; }

        public bool Required { get; set; } = false;



        public CusutomTextField() : base()
        {
            this.RegisterValueChangedCallback(OnChange);
        }

        public CusutomTextField(string label) : base(label)
        {
            this.RegisterValueChangedCallback(OnChange);
        }

        public CusutomTextField(int maxLength, bool multiline, bool isPasswordField, char maskChar) : base(maxLength, multiline, isPasswordField, maskChar)
        {
            this.RegisterValueChangedCallback(OnChange);
        }

        public CusutomTextField(string label, int maxLength, bool multiline, bool isPasswordField, char maskChar) : base(label, maxLength, multiline, isPasswordField, maskChar)
        {
            this.RegisterValueChangedCallback(OnChange);
        }


        private void OnChange(ChangeEvent<string> evt)
        {
            Check();
        }

        public void Check()
        {
            if (IsValid())
            {
                textInputBase.RemoveFromClassList("valid-error");
            }
            else
            {
                textInputBase.AddToClassList("valid-error");
            }
        }

        public bool IsValid()
        {
            if (Required && string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (FieldType == TextType.Free || (FieldType == TextType.Regex && string.IsNullOrWhiteSpace(RegexPattern)))
            {
                return true;
            }

            RegexPattern = defaultRegexPattern[FieldType];
            return Regex.IsMatch(text, RegexPattern);
        }
    }
}
