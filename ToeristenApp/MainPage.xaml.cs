using ToeristenApp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using System.Xml.Linq;
using Newtonsoft.Json;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ToeristenApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        List<PushPin> pushpins = new List<PushPin>();

        public MainPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

    }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //base.OnNavigatedTo(e);
            Geolocator geolocator = new Geolocator();
            if (geolocator.LocationStatus == PositionStatus.Disabled)       //Check if the location settings are turned on and if the cellphone has gps available
            {
                MessageDialog dialog = new MessageDialog("There is no location available is your location aebled? Do you have GPS?", "No Location services");
                dialog.Commands.Add(new UICommand("Settings", new UICommandInvokedHandler(CommandHandler)));
                dialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(CommandHandler)));
                await dialog.ShowAsync();
            }
            else
            {
                string[] TypesOfPlaces = new string[1] { "hospital" }; //"airport", "atm","car-repair","city_hall","embassy","gas_station","pharmacy","lawyer"

                Geoposition position = await geolocator.GetGeopositionAsync();
                BasicGeoposition basicgeo = new BasicGeoposition();
                basicgeo.Latitude = position.Coordinate.Latitude;
                basicgeo.Longitude = position.Coordinate.Longitude;
                MyMap.ZoomLevel = 7;

                Geopoint positionpoint = new Geopoint(basicgeo);
                MyMap.Center = positionpoint;
                getPlaces(TypesOfPlaces,positionpoint.Position.Latitude,positionpoint.Position.Longitude);
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        public async void getPlaces(string[] TypesOfPlaces, double lat , double lon)
        {
            for (int i = 0; i < TypesOfPlaces.Length; i++)
            {
                string httpheader = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + lat + "," + lon + "&radius=10000&types=" + TypesOfPlaces[i] + "&key=AIzaSyDGGzD_RjFpH8HyUMjYq29j6wj4o0tSw9c";

                var client = new HttpClient();
                var result = await client.GetStringAsync(httpheader);
                GooglePlacesResponse gpr = (GooglePlacesResponse)JsonConvert.DeserializeObject<GooglePlacesResponse>(result);
                addPushPin(gpr);       
            }
        }

        public void addPushPin(GooglePlacesResponse response)
        {
            int count = response.results.Length;

            BasicGeoposition positiontest = new BasicGeoposition();     //test for json results
            //PushPin pushpin1 = new PushPin();
            Geopoint geopoint1 = new Geopoint(positiontest); 

            if (response.status == "OK")
            {
                for (int i = 0; i < count; i++)
                {
                    PushPin pushpin1 = new PushPin();
                    string name = response.results[i].name;
                    positiontest.Latitude = response.results[i].geometry.location.lat;
                    positiontest.Latitude = response.results[i].geometry.location.lng;
                    AddPushPinOnMap(pushpin1, response, name, count, i, geopoint1);

                }             
                //Pushpins.Items.Clear();
            }

            //pushpin1.Name = "Pub";
            //pushpin1.Location = geopoint1;
            //BasicGeoposition position2 = new BasicGeoposition();
            //position2.Latitude = 45.8052;
            //position2.Longitude = 9.0825;
            //Geopoint geopoint2 = new Geopoint(position2);
            //PushPin pushpin2 = new PushPin();
            //pushpin2.Name = "Bank";
            //pushpin2.Location = geopoint2;
            //pushpins.Add(pushpin1);
            //pushpins.Add(pushpin2);
            //Pushpins.ItemsSource = pushpins;

        }

        public void AddPushPinOnMap(PushPin pushpin, GooglePlacesResponse response,string name,int max,int count,Geopoint geopoint)
        {
            pushpin.Name = name;
            pushpin.Location = geopoint;
            
            if (max==count)
            {
                Pushpins.ItemsSource = pushpins;
            } 
        }

        private async void OnAddPushpinClicked(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;
            PushPin selectedPushpin = border.DataContext as PushPin;
            MessageDialog dialog = new MessageDialog("This Is a place ");
            await dialog.ShowAsync();

        }

        private async void CommandHandler(IUICommand command)       //Handle what button would do what 
        {
            var commandlabel = command.Label;
            switch (commandlabel)
            {
                case "Settings":
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));     //go to settings page of the cellphone itself
                    Application.Current.Exit();     //close the application so it can check again if the location is back on
                    break;
                case "Exit":
                    Application.Current.Exit();         //exit the application completely
                    break;
                default:
                    break;
            }
        }

        //---------------------------------------------------------CLASSES RIGHT NOW-----------------------------------------------------------------------------------------

        public class ItemPosition
        {
            public int MyProperty { get; set; }
        }
        public class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Geometry
        {
            public Location location { get; set; }
        }
   
        public class PushPin
        {
            public string Name { get; set; }
            public Geopoint Location { get; set; }
        }

        public class GooglePlacesResponse
        {
            public string status { get; set; }
            public results[] results { get; set; }
        }

        public class results
        {
            public Geometry geometry { get; set; }
            public string name { get; set; }
            public string reference { get; set; }
            public string vicinity { get; set; }
        }

        #endregion
    }
}
