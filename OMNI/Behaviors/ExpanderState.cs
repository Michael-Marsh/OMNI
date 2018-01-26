using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace OMNI.Behaviors
{
    public class ExpanderState : Behavior<Expander>
    {
        #region Static Fields

        public static readonly DependencyProperty groupNameProperty = DependencyProperty.Register(
            nameof(GroupName),
            typeof(object),
            typeof(ExpanderState),
            new PropertyMetadata(default(object)));

        private static readonly DependencyProperty ExpandedStateStoreProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedStateStore",
                typeof(IDictionary<object, bool>),
                typeof(ExpanderState),
                new PropertyMetadata(default(IDictionary<object, bool>)));

        public static DependencyProperty GroupNameProperty => groupNameProperty;

        #endregion

        #region Public Properties

        public object GroupName
        {
            get
            {
                return GetValue(GroupNameProperty);
            }

            set
            {
                SetValue(GroupNameProperty, value);
            }
        }

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            var expanded = GetExpandedState();

            if (expanded != null)
            {
                AssociatedObject.IsExpanded = expanded.Value;
            }

            AssociatedObject.Expanded += OnExpanded;
            AssociatedObject.Collapsed += OnCollapsed;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Expanded -= OnExpanded;
            AssociatedObject.Collapsed -= OnCollapsed;

            base.OnDetaching();
        }

        private ItemsControl FindItemsControl()
        {
            DependencyObject current = AssociatedObject;

            while (current != null && !(current is ItemsControl))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current == null)
            {
                return null;
            }

            return current as ItemsControl;
        }

        private bool? GetExpandedState()
        {
            var dict = GetExpandedStateStore();

            if (!dict.ContainsKey(GroupName))
            {
                return null;
            }

            return dict[GroupName];
        }

        private IDictionary<object, bool> GetExpandedStateStore()
        {
            var itemsControl = FindItemsControl();
            var dict = (IDictionary<object, bool>)itemsControl?.GetValue(ExpandedStateStoreProperty);
            if (dict == null)
            {
                dict = new Dictionary<object, bool>();
                itemsControl?.SetValue(ExpandedStateStoreProperty, dict);
            }

            return dict;
        }

        private void OnCollapsed(object sender, RoutedEventArgs e)
        {
            SetExpanded(false);
        }

        private void OnExpanded(object sender, RoutedEventArgs e)
        {
            SetExpanded(true);
        }

        private void SetExpanded(bool expanded)
        {
            var dict = GetExpandedStateStore();

            dict[GroupName] = expanded;
        }

        #endregion
    }
}
