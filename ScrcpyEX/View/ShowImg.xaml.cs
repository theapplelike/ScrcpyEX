using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScrcpyEX.View
{
    /// <summary>
    /// ShowImg.xaml 的交互逻辑
    /// </summary>
    public partial class ShowImg : Window
    {
        public ShowImg(RecInfo recInfo)
        {
            InitializeComponent();
            img.Source = MyGM.GetImage(recInfo.ImgName);
            tbkDesc.Visibility = Visibility.Collapsed;
            tbkParam.Text = 
                $"起始坐标：({recInfo.CropRect.X},{recInfo.CropRect.Y})，" +
                $"图片宽高：({recInfo.CropRect.Width},{recInfo.CropRect.Height})。";
        }

        public ShowImg(RecInfo recInfo, BitmapSource bitmap)
        {
            InitializeComponent();

            double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", bitmap);
            Image img1 = new() { Margin = new Thickness(5), Source = bitmap };

            ufg.Children.Add(img1);
            img.Source = MyGM.GetImage(recInfo.ImgName);
            tbkParam.Text =
                $"起始坐标：({recInfo.CropRect.X},{recInfo.CropRect.Y})，" +
                $"图片宽高：({recInfo.CropRect.Width},{recInfo.CropRect.Height})，" +
                $"相似程度：{ssim:F8}。";
        }
    }
}
