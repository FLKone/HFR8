﻿#pragma checksum "C:\Users\User\Documents\GitHub\HFR8\HFR7\ConnectPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BF6863FDE277B4B81C9FC7A43628FB2B"
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
    
    
    public partial class ConnectPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock ApplicationTitle;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.StackPanel identificationStackPanel;
        
        internal System.Windows.Controls.TextBox pseudoTextBox;
        
        internal System.Windows.Controls.PasswordBox passwordPasswordBox;
        
        internal System.Windows.Controls.Button connectButton;
        
        internal System.Windows.Controls.StackPanel premiereConnexionStackPanel;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/HFR7;component/ConnectPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ApplicationTitle = ((System.Windows.Controls.TextBlock)(this.FindName("ApplicationTitle")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.identificationStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("identificationStackPanel")));
            this.pseudoTextBox = ((System.Windows.Controls.TextBox)(this.FindName("pseudoTextBox")));
            this.passwordPasswordBox = ((System.Windows.Controls.PasswordBox)(this.FindName("passwordPasswordBox")));
            this.connectButton = ((System.Windows.Controls.Button)(this.FindName("connectButton")));
            this.premiereConnexionStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("premiereConnexionStackPanel")));
        }
    }
}

