﻿#pragma checksum "C:\Users\User\Documents\GitHub\HFR8\HFR7\Settings.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D61A972E19FCDA9FCD4F39B361AF85DD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace HFR7 {
    
    
    public partial class Settings : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.PhoneApplicationPage SettingsPA;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Media.ImageBrush backgroundImageBrush;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock ApplicationTitle;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Controls.ListPicker fontSizeListPicker;
        
        internal Microsoft.Phone.Controls.ListPicker favorisListPicker;
        
        internal Microsoft.Phone.Controls.ListPicker displayAvatarsListPicker;
        
        internal Microsoft.Phone.Controls.ListPicker displayImagesListPicker;
        
        internal Microsoft.Phone.Controls.ToggleSwitch disableLandscape;
        
        internal Microsoft.Phone.Controls.ToggleSwitch pinchToZoom;
        
        internal Microsoft.Phone.Controls.ToggleSwitch activateFavAgent;
        
        internal Microsoft.Phone.Controls.ToggleSwitch activateMpNotif;
        
        internal Microsoft.Phone.Controls.ToggleSwitch activateCache;
        
        internal Microsoft.Phone.Controls.ToggleSwitch runUnderLockScreen;
        
        internal Microsoft.Phone.Controls.ToggleSwitch refreshFavWP;
        
        internal Microsoft.Phone.Controls.ToggleSwitch vibrateLoad;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/HFR7;component/Settings.xaml", System.UriKind.Relative));
            this.SettingsPA = ((Microsoft.Phone.Controls.PhoneApplicationPage)(this.FindName("SettingsPA")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.backgroundImageBrush = ((System.Windows.Media.ImageBrush)(this.FindName("backgroundImageBrush")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ApplicationTitle = ((System.Windows.Controls.TextBlock)(this.FindName("ApplicationTitle")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.fontSizeListPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("fontSizeListPicker")));
            this.favorisListPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("favorisListPicker")));
            this.displayAvatarsListPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("displayAvatarsListPicker")));
            this.displayImagesListPicker = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("displayImagesListPicker")));
            this.disableLandscape = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("disableLandscape")));
            this.pinchToZoom = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("pinchToZoom")));
            this.activateFavAgent = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("activateFavAgent")));
            this.activateMpNotif = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("activateMpNotif")));
            this.activateCache = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("activateCache")));
            this.runUnderLockScreen = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("runUnderLockScreen")));
            this.refreshFavWP = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("refreshFavWP")));
            this.vibrateLoad = ((Microsoft.Phone.Controls.ToggleSwitch)(this.FindName("vibrateLoad")));
        }
    }
}

