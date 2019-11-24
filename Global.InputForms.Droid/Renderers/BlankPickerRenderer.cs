﻿using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using Global.InputForms;
using Global.InputForms.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using TextAlignment = Android.Views.TextAlignment;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(BlankPicker), typeof(BlankPickerRenderer))]

namespace Global.InputForms.Droid.Renderers
{
    public class BlankPickerRenderer : PickerRenderer, View.IOnClickListener
    {
        private AlertDialog _dialog;
        private BlankPicker blankPicker;

        public BlankPickerRenderer(Context context) : base(context)
        {
        }

        private IElementController EController => Element;

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (!(e.NewElement is BlankPicker bPicker)) return;
            blankPicker = bPicker;
            if (e.NewElement != null)
                if (Control != null)
                {
                    if (!string.IsNullOrEmpty(Control.Text))
                        bPicker.Text = Control.Text;
                    Control.SetOnClickListener(this);
                    Clickable = true;
                    Control.InputType = InputTypes.Null;
                    Control.Text = Element.SelectedItem?.ToString();
                    Control.KeyListener = null;

                    Control.TextChanged += (sender, arg)
                        => bPicker.Text = arg.Text.ToString();

                    SetPlaceholder();
                    SetAlignment();
                    Control.SetPadding(0, 7, 0, 3);
                    Control.Gravity = GravityFlags.Fill;
                }
            if (e.OldElement != null)
            {
                Control.SetOnClickListener(null);
            }
        }

        public void OnClick(View v)
        {
            var model = Element;

            var picker = new NumberPicker(Context);
            if (model.Items != null && model.Items.Any())
            {
                picker.MaxValue = model.Items.Count - 1;
                picker.MinValue = 0;
                picker.SetDisplayedValues(model.Items.ToArray());
                picker.WrapSelectorWheel = false;
                picker.DescendantFocusability = DescendantFocusability.BlockDescendants;
                picker.Value = model.SelectedIndex;
            }

            var layout = new LinearLayout(Context) {Orientation = Orientation.Vertical};
            layout.AddView(picker);

            EController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

            var builder = new AlertDialog.Builder(Context);
            builder.SetView(layout);
            builder.SetTitle(model.Title ?? "");
            builder.SetNegativeButton(blankPicker.CancelButtonText ?? "Cancel", (s, a) =>
            {
                blankPicker.SendCancelClicked();
                EController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
                _dialog = null;
            });
            builder.SetPositiveButton(blankPicker.DoneButtonText ?? "OK", (s, a) =>
            {
                EController.SetValueFromRenderer(Picker.SelectedIndexProperty, picker.Value);
                //blankPicker.SelectedItem = picker.Value;
                blankPicker.SendDoneClicked();
                // It is possible for the Content of the Page to be changed on SelectedIndexChanged. 
                // In this case, the Element & Control will no longer exist.
                if (Element != null)
                {
                    if (model.Items.Count > 0 && Element.SelectedIndex >= 0)
                        Control.Text = model.Items[Element.SelectedIndex];
                    EController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
                }

                _dialog = null;
            });

            _dialog = builder.Create();
            _dialog.DismissEvent += (sender, args) =>
            {
                EController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
            };
            _dialog.Show();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(Entry.Placeholder)) SetPlaceholder();

            if (e.PropertyName == nameof(BlankPicker.HorizontalTextAlignment)) SetAlignment();

            if (e.PropertyName == nameof(BlankPicker.Text)) UpdateText();
        }


        private void SetPlaceholder()
        {
            Control.SetBackgroundColor(Color.Transparent);

            if (Element is BlankPicker picker && !string.IsNullOrWhiteSpace(picker.Placeholder))
                Control.Text = picker.Placeholder;
        }

        private void SetAlignment()
        {
            switch (((BlankPicker) Element).HorizontalTextAlignment)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    Control.TextAlignment = TextAlignment.Center;
                    break;
                case Xamarin.Forms.TextAlignment.End:
                    Control.TextAlignment = TextAlignment.ViewEnd;
                    break;
                case Xamarin.Forms.TextAlignment.Start:
                    Control.TextAlignment = TextAlignment.ViewStart;
                    break;
            }
        }

        void UpdateText()
        {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (Control.Text != blankPicker.Text)
                Control.Text = blankPicker.Text;
        }
    }
}