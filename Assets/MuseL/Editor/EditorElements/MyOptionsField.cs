using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MuseL
{
    public class MyOptionsField : BaseField<int>
    {
        /// <summary> 
        /// Instantiates an <see cref="EnumField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<MyOptionsField, UxmlTraits> { }

        /// <summary>
        /// Defines <see cref="UxmlTraits"/> for the <see cref="EnumField"/>.
        /// </summary>
        public new class UxmlTraits : BaseField<int>.UxmlTraits
        {
            //#pragma warning disable 414
            //            private UxmlTypeAttributeDescription<int> m_Type = EnumFieldHelpers.type;
            //            private UxmlStringAttributeDescription m_Value = EnumFieldHelpers.value;
            //            private UxmlBoolAttributeDescription m_IncludeObsoleteValues = EnumFieldHelpers.includeObsoleteValues;
            //#pragma warning restore 414

            /// <summary>
            /// Initialize <see cref="EnumField"/> properties using values from the attribute bag.
            /// </summary>
            /// <param name="ve">The object to initialize.</param>
            /// <param name="bag">The attribute bag.</param>
            /// <param name="cc">The creation context; unused.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                MyOptionsField optionsField = (MyOptionsField)ve;
                optionsField.Init(0, new string[] { "empty" });
            }
        }

        private VisualElement visualInput;

        private TextElement m_TextElement;
        private VisualElement m_ArrowElement;
        private string[] options;

        /// <summary>
        /// Return the text value of the currently selected enum.
        /// </summary>
        public string text
        {
            get { return m_TextElement.text; }
        }

        private void Initialize(int defaultValue, string[] options)
        {
            m_TextElement = new TextElement();
            m_TextElement.AddToClassList(textUssClassName);
            m_TextElement.pickingMode = PickingMode.Ignore;
            this.visualInput.Add(m_TextElement);

            m_ArrowElement = new VisualElement();
            m_ArrowElement.AddToClassList(arrowUssClassName);
            m_ArrowElement.pickingMode = PickingMode.Ignore;
            visualInput.Add(m_ArrowElement);

            if (options != null)
            {
                Init(defaultValue, options);
            }
        }

        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public new static readonly string ussClassName = "unity-enum-field";
        /// <summary>
        /// USS class name of text elements in elements of this type.
        /// </summary>
        public static readonly string textUssClassName = ussClassName + "__text";
        /// <summary>
        /// USS class name of arrow indicators in elements of this type.
        /// </summary>
        public static readonly string arrowUssClassName = ussClassName + "__arrow";
        /// <summary>
        /// USS class name of labels in elements of this type.
        /// </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";
        /// <summary>
        /// USS class name of input elements in elements of this type.
        /// </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// Construct an EnumField.
        /// </summary>
        public MyOptionsField()
            : this(0, null) { }

        public MyOptionsField(int defaultValue, string[] options)
            : this(defaultValue, options, null)
        {

        }

        /// <summary>
        /// Construct an EnumField.
        /// </summary>
        /// <param name="defaultValue">Initial value. Also used to detect Enum type.</param>
        public MyOptionsField(int defaultValue, string[] options, string label)
            : this(defaultValue, options, label, new VisualElement() { pickingMode = PickingMode.Ignore })
        {

        }

        private MyOptionsField(int defaultValue, string[] options, string label, VisualElement visualInput) : base(label, visualInput)
        {
            this.visualInput = visualInput;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            visualInput.AddToClassList(inputUssClassName);
            Initialize(defaultValue, options);
        }

        /// <summary>
        /// Initializes the EnumField with a default value, and initializes its underlying type.
        /// </summary>
        /// <param name="defaultValue">The typed enum value.</param>
        /// <param name="includeObsoleteValues">Set to true to display obsolete values as choices.</param>
        public void Init(int defaultValue, string[] options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            SetValueWithoutNotify(defaultValue);
        }

        public override void SetValueWithoutNotify(int newValue)
        {

            if (rawValue != newValue)
            {

                base.SetValueWithoutNotify(newValue);

                if (options == null)
                    return;

                int v = Mathf.Clamp(newValue, 0, options.Length - 1);

                m_TextElement.text = options[v];
            }
        }

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);

            if (evt == null)
            {
                return;
            }
            var showEnumMenu = false;
            KeyDownEvent kde = (evt as KeyDownEvent);
            if (kde != null)
            {
                if ((kde.keyCode == KeyCode.Space) ||
                    (kde.keyCode == KeyCode.KeypadEnter) ||
                    (kde.keyCode == KeyCode.Return))
                {
                    showEnumMenu = true;
                }
            }
            else if ((evt as MouseDownEvent)?.button == (int)MouseButton.LeftMouse)
            {
                var mde = (MouseDownEvent)evt;
                if (visualInput.ContainsPoint(visualInput.WorldToLocal(mde.mousePosition)))
                {
                    showEnumMenu = true;
                }
            }

            if (showEnumMenu)
            {
                ShowMenu();
                evt.StopPropagation();
            }
        }

        private void ShowMenu()
        {
            if (options == null)
                return;

            var menu = new GenericMenu();

            int selectedIndex = value;

            for (int i = 0; i < options.Length; ++i)
            {
                bool isSelected = selectedIndex == i;
                menu.AddItem(new GUIContent(options[i]), isSelected,
                    contentView => ChangeValueFromMenu(contentView),
                    options[i]);
            }

            var menuPosition = new Vector2(visualInput.layout.xMin, visualInput.layout.height);
            menuPosition = this.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            menu.DropDown(menuRect);
        }

        private void ChangeValueFromMenu(object menuItem)
        {
            if (menuItem is int)
            {
                value = (int)menuItem;
            }
            else if (menuItem is string s)
            {
                value = Array.IndexOf(options, s);
            }
        }
    }

}

