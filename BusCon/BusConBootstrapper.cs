using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using BusCon.ViewModels;
using Caliburn.Micro;
using System.Windows.Controls.Primitives;

namespace BusCon 
{
    public class BusConBootstrapper : PhoneBootstrapper 
    {
        PhoneContainer container;

        protected override void Configure() {
            container = new PhoneContainer(RootFrame);

            container.RegisterPhoneServices();

            container.Singleton<ExceptionViewModel>();
            container.Singleton<ConnectionQueryViewModel>();

            container.Singleton<DepartureViewModel>();
            container.Singleton<FavoriteStationsViewModel>();
            container.Singleton<SearchStationsViewModel>();
            container.Singleton<NearbyStationsViewModel>();

            container.Singleton<MainPageViewModel>();
            container.Singleton<SettingsViewModel>();
            container.Singleton<HistoryViewModel>();
            container.Singleton<SearchViewModel>();
            container.PerRequest<ConnectionViewModel>();
            container.PerRequest<DetailViewModel>();
            container.Singleton<StationsViewModel>();

            container.PerRequest<MessageViewModel>();
            container.PerRequest<DialogViewModel>();

            AddCustomConventions();
        }

        static void AddCustomConventions() {
            ConventionManager.AddElementConvention<Pivot>(Pivot.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) => {
                    if(ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention)) {
                        ConventionManager
                            .ConfigureSelectedItem(element, Pivot.SelectedItemProperty, viewModelType, path);
                        ConventionManager
                            .ApplyHeaderTemplate(element, Pivot.HeaderTemplateProperty, viewModelType);
                        return true;
                    }

                    return false;
                };

            ConventionManager.AddElementConvention<Panorama>(Panorama.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) => {
                    if(ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention)) {
                        ConventionManager
                            .ConfigureSelectedItem(element, Panorama.SelectedItemProperty, viewModelType, path);
                        ConventionManager
                            .ApplyHeaderTemplate(element, Panorama.HeaderTemplateProperty, viewModelType);
                        return true;
                    }

                    return false;
                };

            ConventionManager.AddElementConvention<ToggleButton>(ToggleButton.IsCheckedProperty, "IsChecked", "Checked");
            ConventionManager.AddElementConvention<ToggleSwitch>(ToggleSwitch.IsCheckedProperty, "IsChecked", "Checked");
        }

        protected override object GetInstance(Type service, string key) {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            container.BuildUp(instance);
        }
    }
}