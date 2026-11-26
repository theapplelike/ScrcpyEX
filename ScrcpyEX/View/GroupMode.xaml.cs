using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
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
using static ScrcpyEX.MyDefinition.MyEnum;

namespace ScrcpyEX.View
{
    /// <summary>
    /// GroupMode.xaml 的交互逻辑
    /// </summary>
    public partial class GroupMode : Window
    {
        /// <summary>
        /// 进行设置的组脚本。
        /// </summary>
        private readonly ScriptGroup _grp;

        public GroupMode(ScriptGroup grp)
        {
            InitializeComponent();
            _grp = grp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_grp.LinkMode == LinkMode.InTurn) rdoInTurn.IsChecked = true;
            else rdoKeepOn.IsChecked = true;

            if (_grp.IsAloneAdd) rdoAlone.IsChecked = true;
            else rdoGroup.IsChecked = true;
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            string desc =
                "执行组脚本时，如果其中第一个是计时脚本，则执行参数以第一个脚本为准。" +
                "每次执行都会将其中所有脚本的点击操作执行一遍。" +
                "如果第一个是图像脚本，则其中所有的脚本都需要是图像脚本，且有图像。\n" +
                "顺序轮流模式下，如果有图像脚本包含（A，B），则会按照A-B-A-B的顺序比较图像相似度，对比成功后执行对应操作。\n" +
                "成功继续模式下，如果有图像脚本包含（A，B），则会按照 [A-A...]成功->[B-B...]成功 的顺序执行，只有当前的脚本通过相似度比对后才会继续下一个脚本。";
            MyGM.ShwInfo(desc);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _grp.LinkMode = (bool)rdoInTurn.IsChecked ? LinkMode.InTurn : LinkMode.KeepOn;
            _grp.IsAloneAdd = (bool)rdoAlone.IsChecked;

            DialogResult = true;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void RdoInTurn_Checked(object sender, RoutedEventArgs e)
        {
            rdoAlone.IsChecked = true;
            rdoGroup.IsEnabled = false;
        }

        private void RdoKeepOn_Checked(object sender, RoutedEventArgs e)
        {
            rdoGroup.IsEnabled = true;
        }
    }
}
