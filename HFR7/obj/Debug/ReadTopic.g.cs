﻿#pragma checksum "C:\Users\User\Documents\GitHub\HFR8\HFR7\ReadTopic.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B8D5623365B7D365E41664A799123B36"
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
    
    
    public partial class ReadTopic : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.PhoneApplicationPage ReadTopicPA;
        
        internal System.Windows.Media.Animation.Storyboard quitSB;
        
        internal System.Windows.Media.Animation.Storyboard enterWB;
        
        internal System.Windows.Media.Animation.DoubleAnimation opacityWebBrowserDA;
        
        internal System.Windows.Media.Animation.Storyboard quitToLeftWB;
        
        internal System.Windows.Media.Animation.Storyboard quitToRightWB;
        
        internal System.Windows.Media.Animation.Storyboard quitLoadingTextBlock;
        
        internal System.Windows.Media.Animation.Storyboard enterLoadingTextBlock;
        
        internal System.Windows.Media.Animation.Storyboard quitSearch;
        
        internal System.Windows.Media.Animation.Storyboard enterSearch;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid TopicPanel;
        
        internal Microsoft.Phone.Controls.WebBrowser readTopicWebBrowser;
        
        internal System.Windows.Controls.Canvas loadingCanvas;
        
        internal System.Windows.Controls.StackPanel loadingStackPanel;
        
        internal System.Windows.Controls.TextBlock topicNameTextBlock;
        
        internal System.Windows.Controls.TextBlock pagesTextBlock;
        
        internal System.Windows.Controls.Canvas searchCanvas;
        
        internal System.Windows.Controls.StackPanel searchStackPanel;
        
        internal System.Windows.Controls.TextBox searchPseudoTextBox;
        
        internal System.Windows.Controls.TextBox searchWordTextBox;
        
        internal System.Windows.Controls.Button searchStartButton;
        
        internal System.Windows.Controls.Canvas choosePageCanvas;
        
        internal System.Windows.Controls.TextBox pageNumberChooseTextBox;
        
        internal System.Windows.Controls.Button pageNumberChooseButton;
        
        internal System.Windows.Controls.Canvas internWebBrowserCanvas;
        
        internal Microsoft.Phone.Controls.WebBrowser internWebBrowser;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton refreshButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton answerButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton previousPageAppbarButton;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton nextPageAppbarButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/HFR7;component/ReadTopic.xaml", System.UriKind.Relative));
            this.ReadTopicPA = ((Microsoft.Phone.Controls.PhoneApplicationPage)(this.FindName("ReadTopicPA")));
            this.quitSB = ((System.Windows.Media.Animation.Storyboard)(this.FindName("quitSB")));
            this.enterWB = ((System.Windows.Media.Animation.Storyboard)(this.FindName("enterWB")));
            this.opacityWebBrowserDA = ((System.Windows.Media.Animation.DoubleAnimation)(this.FindName("opacityWebBrowserDA")));
            this.quitToLeftWB = ((System.Windows.Media.Animation.Storyboard)(this.FindName("quitToLeftWB")));
            this.quitToRightWB = ((System.Windows.Media.Animation.Storyboard)(this.FindName("quitToRightWB")));
            this.quitLoadingTextBlock = ((System.Windows.Media.Animation.Storyboard)(this.FindName("quitLoadingTextBlock")));
            this.enterLoadingTextBlock = ((System.Windows.Media.Animation.Storyboard)(this.FindName("enterLoadingTextBlock")));
            this.quitSearch = ((System.Windows.Media.Animation.Storyboard)(this.FindName("quitSearch")));
            this.enterSearch = ((System.Windows.Media.Animation.Storyboard)(this.FindName("enterSearch")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TopicPanel = ((System.Windows.Controls.Grid)(this.FindName("TopicPanel")));
            this.readTopicWebBrowser = ((Microsoft.Phone.Controls.WebBrowser)(this.FindName("readTopicWebBrowser")));
            this.loadingCanvas = ((System.Windows.Controls.Canvas)(this.FindName("loadingCanvas")));
            this.loadingStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("loadingStackPanel")));
            this.topicNameTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("topicNameTextBlock")));
            this.pagesTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("pagesTextBlock")));
            this.searchCanvas = ((System.Windows.Controls.Canvas)(this.FindName("searchCanvas")));
            this.searchStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("searchStackPanel")));
            this.searchPseudoTextBox = ((System.Windows.Controls.TextBox)(this.FindName("searchPseudoTextBox")));
            this.searchWordTextBox = ((System.Windows.Controls.TextBox)(this.FindName("searchWordTextBox")));
            this.searchStartButton = ((System.Windows.Controls.Button)(this.FindName("searchStartButton")));
            this.choosePageCanvas = ((System.Windows.Controls.Canvas)(this.FindName("choosePageCanvas")));
            this.pageNumberChooseTextBox = ((System.Windows.Controls.TextBox)(this.FindName("pageNumberChooseTextBox")));
            this.pageNumberChooseButton = ((System.Windows.Controls.Button)(this.FindName("pageNumberChooseButton")));
            this.internWebBrowserCanvas = ((System.Windows.Controls.Canvas)(this.FindName("internWebBrowserCanvas")));
            this.internWebBrowser = ((Microsoft.Phone.Controls.WebBrowser)(this.FindName("internWebBrowser")));
            this.refreshButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("refreshButton")));
            this.answerButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("answerButton")));
            this.previousPageAppbarButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("previousPageAppbarButton")));
            this.nextPageAppbarButton = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("nextPageAppbarButton")));
        }
    }
}
