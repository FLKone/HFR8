﻿#pragma checksum "C:\Users\User\Documents\GitHub\HFR8\HFR7\HFRrehost.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3C7644535338497BEE06EEBC3E062C84"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Jayway.Controls;
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
    
    
    public partial class HFRrehost : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.PhoneApplicationPage HFRrehostPage;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Media.ImageBrush backgroundImageBrush;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.StackPanel choosePhotoStackPanel;
        
        internal System.Windows.Controls.Button choosePicButton;
        
        internal System.Windows.Controls.Button takePicButton;
        
        internal System.Windows.Controls.StackPanel waitStackPanel;
        
        internal System.Windows.Controls.TextBlock messageWaitTextBlock;
        
        internal System.Windows.Controls.StackPanel copyUrlStackPanel;
        
        internal System.Windows.Controls.TextBox reelleSansLienTextBox;
        
        internal Jayway.Controls.RoundButton reelleSansLienButton;
        
        internal System.Windows.Controls.TextBox previewAvecLienTextBox;
        
        internal Jayway.Controls.RoundButton previewAvecLienButton;
        
        internal System.Windows.Controls.TextBox miniatureAvecLienTextBox;
        
        internal Jayway.Controls.RoundButton miniatureAvecLienButton;
        
        internal System.Windows.Controls.TextBox urlTextBox;
        
        internal Jayway.Controls.RoundButton urlButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/HFR7;component/HFRrehost.xaml", System.UriKind.Relative));
            this.HFRrehostPage = ((Microsoft.Phone.Controls.PhoneApplicationPage)(this.FindName("HFRrehostPage")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.backgroundImageBrush = ((System.Windows.Media.ImageBrush)(this.FindName("backgroundImageBrush")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.choosePhotoStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("choosePhotoStackPanel")));
            this.choosePicButton = ((System.Windows.Controls.Button)(this.FindName("choosePicButton")));
            this.takePicButton = ((System.Windows.Controls.Button)(this.FindName("takePicButton")));
            this.waitStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("waitStackPanel")));
            this.messageWaitTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("messageWaitTextBlock")));
            this.copyUrlStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("copyUrlStackPanel")));
            this.reelleSansLienTextBox = ((System.Windows.Controls.TextBox)(this.FindName("reelleSansLienTextBox")));
            this.reelleSansLienButton = ((Jayway.Controls.RoundButton)(this.FindName("reelleSansLienButton")));
            this.previewAvecLienTextBox = ((System.Windows.Controls.TextBox)(this.FindName("previewAvecLienTextBox")));
            this.previewAvecLienButton = ((Jayway.Controls.RoundButton)(this.FindName("previewAvecLienButton")));
            this.miniatureAvecLienTextBox = ((System.Windows.Controls.TextBox)(this.FindName("miniatureAvecLienTextBox")));
            this.miniatureAvecLienButton = ((Jayway.Controls.RoundButton)(this.FindName("miniatureAvecLienButton")));
            this.urlTextBox = ((System.Windows.Controls.TextBox)(this.FindName("urlTextBox")));
            this.urlButton = ((Jayway.Controls.RoundButton)(this.FindName("urlButton")));
        }
    }
}

