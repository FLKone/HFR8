﻿#pragma checksum "C:\Users\User\Documents\GitHub\HFR8\HFR7\Mp.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "95E86A29837C78ECCAD535FC5A14795B"
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
using Microsoft.Phone.Shell;
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
    
    
    public partial class Mp : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.PhoneApplicationPage MpPA;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar progressBar;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Controls.WebBrowser mpWebBrowser;
        
        internal System.Windows.Controls.ScrollViewer messageScrollViewer;
        
        internal System.Windows.Controls.TextBox messageTextBox;
        
        internal System.Windows.Shapes.Rectangle mpMiniRectangle;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton answerButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton hfrrehostButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton smileyButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/HFR7;component/Mp.xaml", System.UriKind.Relative));
            this.MpPA = ((Microsoft.Phone.Controls.PhoneApplicationPage)(this.FindName("MpPA")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.progressBar = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("progressBar")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.mpWebBrowser = ((Microsoft.Phone.Controls.WebBrowser)(this.FindName("mpWebBrowser")));
            this.messageScrollViewer = ((System.Windows.Controls.ScrollViewer)(this.FindName("messageScrollViewer")));
            this.messageTextBox = ((System.Windows.Controls.TextBox)(this.FindName("messageTextBox")));
            this.mpMiniRectangle = ((System.Windows.Shapes.Rectangle)(this.FindName("mpMiniRectangle")));
            this.answerButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("answerButton")));
            this.hfrrehostButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("hfrrehostButton")));
            this.smileyButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("smileyButton")));
        }
    }
}
