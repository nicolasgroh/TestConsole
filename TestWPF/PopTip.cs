using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestWPF
{
    public class PopTip : ContentControl
    {
        static PopTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopTip), new FrameworkPropertyMetadata(typeof(PopTip)));
        }
    }
}