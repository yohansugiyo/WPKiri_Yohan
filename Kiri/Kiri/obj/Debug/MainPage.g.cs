﻿#pragma checksum "C:\Users\Yohan Sugiyo\Documents\Visual Studio 2012\Projects\Kiri\Kiri\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4B7B73D5AEF44F0D070F3BD80663D51F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
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


namespace Kiri {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.Primitives.Popup popup;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.TextBox fromBox;
        
        internal System.Windows.Controls.TextBox toBox;
        
        internal Microsoft.Phone.Controls.ListPicker cmbCurrFrom;
        
        internal System.Windows.Controls.ProgressBar progressFindPlace;
        
        internal System.Windows.Controls.StackPanel panelFrom;
        
        internal System.Windows.Controls.TextBlock textFrom;
        
        internal System.Windows.Controls.ListBox listPlaceFrom;
        
        internal System.Windows.Controls.StackPanel panelTo;
        
        internal System.Windows.Controls.TextBlock textTo;
        
        internal System.Windows.Controls.ListBox listPlaceTo;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Kiri;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.popup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("popup")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.fromBox = ((System.Windows.Controls.TextBox)(this.FindName("fromBox")));
            this.toBox = ((System.Windows.Controls.TextBox)(this.FindName("toBox")));
            this.cmbCurrFrom = ((Microsoft.Phone.Controls.ListPicker)(this.FindName("cmbCurrFrom")));
            this.progressFindPlace = ((System.Windows.Controls.ProgressBar)(this.FindName("progressFindPlace")));
            this.panelFrom = ((System.Windows.Controls.StackPanel)(this.FindName("panelFrom")));
            this.textFrom = ((System.Windows.Controls.TextBlock)(this.FindName("textFrom")));
            this.listPlaceFrom = ((System.Windows.Controls.ListBox)(this.FindName("listPlaceFrom")));
            this.panelTo = ((System.Windows.Controls.StackPanel)(this.FindName("panelTo")));
            this.textTo = ((System.Windows.Controls.TextBlock)(this.FindName("textTo")));
            this.listPlaceTo = ((System.Windows.Controls.ListBox)(this.FindName("listPlaceTo")));
        }
    }
}

