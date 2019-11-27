﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Foundation;
using Global.InputForms;
using Global.InputForms.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BlankTimePicker), typeof(BlankTimePickerRenderer))]

namespace Global.InputForms.iOS.Renderers
{
    public class BlankTimePickerRenderer : EntryRenderer
    {
        private BlankTimePicker blankPicker;

        UIDatePicker _picker;
        UIColor _defaultTextColor;
        bool _disposed;
        bool _useLegacyColorManagement;

        IElementController ElementController => Element as IElementController;


        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (!(e.NewElement is BlankTimePicker bPicker)) return;
            blankPicker = bPicker;

            if (Control != null)
            {
                if (!string.IsNullOrEmpty(Control.Text))
                    bPicker.Text = Control.Text;

                Control.EditingDidBegin += OnStarted;
                Control.EditingDidEnd += OnEnded;


                _picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };



                SetInputAccessoryView();

                Control.InputView = _picker;

                Control.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
                Control.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

                Control.InputAssistantItem.LeadingBarButtonGroups = null;
                Control.InputAssistantItem.TrailingBarButtonGroups = null;

                _picker.ValueChanged += OnValueChanged;

                Control.AccessibilityTraits = UIAccessibilityTrait.Button;

                //textField.EditingChanged += (sender, arg)
                //    => bPicker.Text = Control.Text;

                //textField.EditingDidEnd += (sender, arg) =>
                //{
                //    var controlText = Control.Text ?? string.Empty;
                //    var entryText = bPicker.Text ?? string.Empty;
                //    if (controlText != entryText)
                //    {
                //        bPicker.Text = Control.Text;
                //    }
                //};
                UpdateTime();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == nameof(BlankTimePicker.Time)
                || e.PropertyName == nameof(BlankTimePicker.Format))
                UpdateTime();
        }

        public void SetInputAccessoryView()
        {
            if (string.IsNullOrEmpty(blankPicker.DoneButtonText) && string.IsNullOrEmpty(blankPicker.CancelButtonText))
            {
                Control.InputAccessoryView = null;
                return;
            }

            var toolbar = new UIToolbar
            {
                BarStyle = UIBarStyle.Default,
                Translucent = true
            };
            toolbar.SizeToFit();

            var items = new List<UIBarButtonItem>();

            if (!string.IsNullOrEmpty(blankPicker.CancelButtonText))
            {
                var cancelButton = new UIBarButtonItem(blankPicker.CancelButtonText, UIBarButtonItemStyle.Done,
                    (s, ev) => { Control.ResignFirstResponder(); });
                cancelButton.Clicked += (sender, e) => { blankPicker.SendCancelClicked(); };
                items.Add(cancelButton);
            }

            var flexible = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            items.Add(flexible);

            if (!string.IsNullOrEmpty(blankPicker.DoneButtonText))
            {
                var doneButton = new UIBarButtonItem(blankPicker.DoneButtonText, UIBarButtonItemStyle.Done,
                    (s, ev) =>
                    {
                        blankPicker.Text = Control.Text = _picker.Date.ToDateTime().TimeOfDay.ToString(blankPicker.Format);
                        blankPicker.SetValueFromRenderer(BlankTimePicker.TimeProperty, _picker.Date.ToDateTime() - new DateTime(1, 1, 1).TimeOfDay);
                        blankPicker.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
                        Control.ResignFirstResponder();
                    });
                doneButton.Clicked += (sender, e) => { blankPicker.SendDoneClicked(); };
                items.Add(doneButton);
            }

            toolbar.SetItems(items.ToArray(), true);
            Control.InputAccessoryView = toolbar;
        }

        void OnEnded(object sender, EventArgs eventArgs)
        {
            ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
        }

        void OnStarted(object sender, EventArgs eventArgs)
        {
            ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
        }

        void OnValueChanged(object sender, EventArgs e)
        {
            //ElementController.SetValueFromRenderer(BlankTimePicker.TimeProperty, _picker.Date.ToDateTime() - new DateTime(1, 1, 1));
        }

        void UpdateTime()
        {
            if (blankPicker.TimeSet)
            {
                _picker.Date = new DateTime(1, 1, 1).Add(blankPicker.Time).ToNSDate();
                Control.Text = DateTime.Today.Add(blankPicker.Time).ToString(blankPicker.Format);
            }
            else
                Control.Text = string.Empty;
            //blankPicker.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.MeasureChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            if (disposing)
            {
                _defaultTextColor = null;

                if (_picker != null)
                {
                    _picker.RemoveFromSuperview();
                    _picker.ValueChanged -= OnValueChanged;
                    _picker.Dispose();
                    _picker = null;
                }

                if (Control != null)
                {
                    Control.EditingDidBegin -= OnStarted;
                    Control.EditingDidEnd -= OnEnded;
                }
            }

            base.Dispose(disposing);
        }
    }
}