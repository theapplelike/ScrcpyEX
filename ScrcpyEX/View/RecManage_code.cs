using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace ScrcpyEX.View
{
    public partial class RecManage//code
    {
        private void MyInit()
        {
            _vm.ScriptItems = new(MyCfg.Cfg.Scripts);
            gSuboption.Visibility = Visibility.Hidden;
            cmbMultipleScript.Visibility = Visibility.Collapsed;

            _pointDt.Columns.Add("LastTime", typeof(double));
            _pointDt.Columns.Add("XAxis", typeof(double));
            _pointDt.Columns.Add("YAxis", typeof(double));
            dgPoint.ItemsSource = _pointDt.DefaultView;
        }

        private void MyClose()
        {
            MyCfg.Cfg.Scripts = [.. _vm.ScriptItems];
            MyCfg.Save();
        }

        /// <summary>
        /// 获得当前选中的脚本列表。
        /// </summary>
        /// <returns>脚本信息列表。</returns>
        public List<ScriptInfo> GetSelectScript()
        {
            if (_currScript is ScriptInfo info)
            {
                return [info];
            }
            else if (_currScript is ScriptGroup group)
            {
                return [.. group.Scripts];
            }
            return [];
        }

        /// <summary>
        /// 编辑脚本名称，对于未编辑的项会初始化。
        /// </summary>
        /// <param name="currentItem">当前编辑项的数据源。</param>
        private void EditName(ScriptItem currentItem)
        {
            foreach (var item in lst.Items)
            {
                // 获取项的容器
                if (lst.ItemContainerGenerator.ContainerFromItem(item) is not ListBoxItem container)
                {
                    continue;
                }

                // 查找ContentPresenter（承载DataTemplate）
                ContentPresenter presenter = container.FindVisualChild<ContentPresenter>();
                if (presenter == null)
                {
                    continue;
                }

                // 应用模板确保视觉树生成
                //presenter.ApplyTemplate();

                // 按名称查找控件
                Button btn = presenter.ContentTemplate.FindName("btnEdit", presenter) as Button;
                TextBox txt = presenter.ContentTemplate.FindName("txtEdit", presenter) as TextBox;
                TextBlock tbk = presenter.ContentTemplate.FindName("tbkEdit", presenter) as TextBlock;
                
                if (item == currentItem)
                {
                    if (_lastItem == currentItem && txt.Visibility != Visibility.Hidden)
                    {
                        ItemInit(btn, txt, tbk);
                        //Debug.WriteLine("当前项，停止编辑");
                    }
                    else
                    {
                        ItemEdit(btn, txt, tbk);
                        //Debug.WriteLine("当前项，进行编辑");
                    }
                }
                else
                {
                    ItemInit(btn, txt, tbk);
                    //Debug.WriteLine("不是当前项，初始化");
                }
            }
            _lastItem = currentItem;
            //Debug.WriteLine("-----------------");
        }

        /// <summary>
        /// 脚本项的各个控件初始化。
        /// </summary>
        private void ItemInit(Button btn, TextBox txt, TextBlock tbk)
        {
            btn.Content = "\ue8ac";
            txt.Visibility = Visibility.Hidden;
            tbk.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 编辑脚本项时各个控件进行调整。
        /// </summary>
        private void ItemEdit(Button btn, TextBox txt, TextBlock tbk)
        {
            btn.Content = "\uf78c";
            txt.Visibility = Visibility.Visible;
            tbk.Visibility = Visibility.Hidden;

            txt.Focus();
            txt.Select(txt.Text.Length, 0);
        }

        /// <summary>
        /// 将指定脚本的数据赋值给界面，切换脚本时调用此方法。
        /// </summary>
        /// <param name="info">指定的脚本信息类。</param>
        private void ValToUI(ScriptInfo info)
        {
            rdoLimited.IsChecked = info.IsLimited;
            rdoHandStop.IsChecked = !info.IsLimited;
            txtCycleNum.Text = info.CycleNumber.ToString();
            rdoTime.IsChecked = info.IsTime;
            rdoImage.IsChecked = !info.IsTime;
            txtCycleInterval.Text = info.CycleInterval.ToString();
            img.Source = MyGM.GetImage(info.ImgName);
            txtSimilarity.Text = info.Similarity.ToString();
            chkAdaptive.IsChecked = info.IsAdaptive;
            _cropRect = Rect.Empty;

            //Debug.WriteLine($"选择脚本图片名：{info.ImgName}，范围：{info.CropRect}，{info.Similarity}。");
        }

        /// <summary>
        /// 进行截图。
        /// </summary>
        private async void DoScreenshot()
        {
            if (_scHandle == IntPtr.Zero)
            {
                return;
            }

            Opacity = 0;
            await Task.Delay(500);//让当前窗体有足够时间隐藏

            Screenshot screen = new(_scHandle);
            if (screen.ShowDialog() == true)
            {
                img.Source = screen.Crop;
                _cropRect = screen.CropRect;
            }

            Opacity = 1;
        }

        /// <summary>
        /// 拖动ListBox中的项。
        /// </summary>
        /// <param name="currentItem">当前拖拽的ListBox项。</param>
        /// <param name="e">MouseMove事件数据提供。</param>
        private void DragListBoxItem(ListBoxItem currentItem, MouseEventArgs e)
        {
            // 只有左键按下，并且存在有效的拖拽源时才可能开始拖拽
            if (e.LeftButton != MouseButtonState.Pressed || _dragSourceItem == null)
            {
                return;
            }

            // 当前移动的项必须与按下时的源项是同一个
            if (currentItem != _dragSourceItem)
            {
                return;
            }

            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - _dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // 只有单脚本（ScriptInfo）允许拖拽
                if (_dragSourceItem.DataContext is ScriptInfo info)
                {
                    DragDrop.DoDragDrop(_dragSourceItem, info, DragDropEffects.Copy);
                }

                // 拖拽已启动或不允许拖拽，清空源记录，避免重复
                _dragSourceItem = null;
            }
        }

        /// <summary>
        /// 创建组脚本。
        /// </summary>
        /// <param name="targetData">目标项的数据上下文。</param>
        /// <param name="sourceData">源项的数据上下文。</param>
        private void CreatGroupScript(object targetData, object sourceData)
        {
            // 源数据必须是单脚本
            if (sourceData is not ScriptInfo source)
            {
                return;
            }

            // 目标数据可以是单脚本或组脚本
            if (targetData is ScriptInfo target)
            {
                ScriptGroup newGroup = new()
                {
                    Name = $"{target.Name}&{source.Name}",
                    Scripts = [(ScriptInfo)target.Clone(), (ScriptInfo)source.Clone()]
                };
                _vm.ScriptItems.Add(newGroup);
                CopyScriptImage([..newGroup.Scripts]);
            }
            else if (targetData is ScriptGroup targetGroup)
            {
                targetGroup.Name = $"{targetGroup.Name}&{source.Name}";
                targetGroup.Scripts.Add((ScriptInfo)source.Clone());
                CopyScriptImage(targetGroup.Scripts.Last());
            }
        }

        /// <summary>
        /// 如果脚本有图片，则复制源图片为一张新图片，并且使用新图片名。
        /// </summary>
        /// <param name="info">脚本信息类。</param>
        private void CopyScriptImage(ScriptInfo info)
        {
            if (info.ImgName == "")
            {
                return;
            }

            string srcName = @$"{MV.ScriptPath}\{info.ImgName}";
            string destName = @$"{MV.ScriptPath}\Pic{DateTime.Now:yyMMddhhmm_ssfff}.png";
            File.Copy(srcName, destName, true);
            info.ImgName = Path.GetFileName(destName);
        }

        /// <summary>
        /// 如果脚本有图片，则复制源图片为一张新图片，并且使用新图片名。
        /// </summary>
        /// <param name="infos">脚本信息类列表。</param>
        private async void CopyScriptImage(List<ScriptInfo> infos)
        {
            foreach (ScriptInfo si in infos)
            {
                CopyScriptImage(si);
                await Task.Delay(50);
            }
        }

    }
}
