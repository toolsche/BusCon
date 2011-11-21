using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;

namespace SLMultiBinding
{
    /// <summary>
    /// Allows multiple bindings to a single property.
    /// </summary>
    [ContentProperty("Bindings")]
    public class MultiBinding : Panel, INotifyPropertyChanged
    {
        #region ConvertedValue dependency property

        public static readonly DependencyProperty ConvertedValueProperty =
            DependencyProperty.Register("ConvertedValue", typeof(object), typeof(MultiBinding),
                new PropertyMetadata(null, OnConvertedValue));

        /// <summary>
        /// This dependency property is set to the resulting output of the
        /// associated Converter.
        /// </summary>
        public object ConvertedValue
        {
            get { return GetValue(ConvertedValueProperty); }
            set { SetValue(ConvertedValueProperty, value); }
        }

        private static void OnConvertedValue(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            MultiBinding relay = depObj as MultiBinding;
            Debug.Assert(relay != null);
            relay.OnPropertyChanged("ConvertedValue");
        }

        #endregion

        #region CLR properties

        /// <summary>
        /// The target property on the element which this MultiBinding is assocaited with.
        /// </summary>
        public string TargetProperty { get; set; }

        /// <summary>
        /// The Converter which is invoked to compute the result of the multiple bindings
        /// </summary>
        public IMultiValueConverter Converter { get; set; }

        /// <summary>
        /// The configuration parameter supplied to the converter
        /// </summary>
        public object ConverterParameter { get; set; }

        /// <summary>
        /// The bindings, the result of which are supplied to the converter.
        /// </summary>
        public BindingCollection Bindings { get; set; }

        #endregion

        public MultiBinding()
        {
            Bindings = new BindingCollection();
        }

        /// <summary>
        /// Invoked when any of the BindingSlave's Value property changes.
        /// </summary>
        private void SlavePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateConvertedValue();
        }

        /// <summary>
        /// Uses the Converter to update the ConvertedValue in order to reflect
        /// the current state of the bindings.
        /// </summary>
        private void UpdateConvertedValue()
        {
            List<object> values = new List<object>();
            foreach (BindingSlave slave in Children)
            {
                values.Add(slave.Value);
            }
            ConvertedValue = Converter.Convert(values.ToArray(), typeof(object), ConverterParameter,CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a BindingSlave for each Binding and binds the Value
        /// accordingly.
        /// </summary>
        internal void Initialise()
        {
            Children.Clear();
            foreach (Binding binding in Bindings)
            {
                BindingSlave slave = new BindingSlave();
                slave.SetBinding(BindingSlave.ValueProperty, binding);
                slave.PropertyChanged += SlavePropertyChanged;
                Children.Add(slave);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    /// <summary>
    /// A simple element with a single Value property, used as a 'slave'
    /// for a Binding.
    /// </summary>
    public class BindingSlave : FrameworkElement, INotifyPropertyChanged
    {
        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(BindingSlave),
                new PropertyMetadata(null, OnValueChanged));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            BindingSlave slave = depObj as BindingSlave;
            Debug.Assert(slave!=null);
            slave.OnPropertyChanged("Value");
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }

    internal delegate void BindingCollectionChangedCallback();

    public class BindingCollection : Collection<BindingBase>
    {
        // Fields
        private readonly BindingCollectionChangedCallback _collectionChangedCallback;

        // Methods
        //internal BindingCollection(BindingCollectionChangedCallback callback)
        //{
        //    _collectionChangedCallback = callback;
        //}

        protected override void ClearItems()
        {
            base.ClearItems();
            OnBindingCollectionChanged();
        }

        protected override void InsertItem(int index, BindingBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            ValidateItem(item);
            base.InsertItem(index, item);
            OnBindingCollectionChanged();
        }

        private void OnBindingCollectionChanged()
        {
            if (_collectionChangedCallback != null)
            {
                _collectionChangedCallback();
            }
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            OnBindingCollectionChanged();
        }

        protected override void SetItem(int index, BindingBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            ValidateItem(item);
            base.SetItem(index, item);
            OnBindingCollectionChanged();
        }

        private static void ValidateItem(BindingBase binding)
        {
            if (!(binding is Binding))
            {
                throw new NotSupportedException("BindingCollectionContainsNonBinding");
            }
        }
    }
}
