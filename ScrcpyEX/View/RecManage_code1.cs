using OpenCvSharp.ML;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScrcpyEX.View
{
    public partial class RecManage//code1
    {
        /// <summary>
        /// 执行指定脚本。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void ExecuteScript(Button btn)
        {
            if (btn.CommandParameter is ScriptInfo info)
            {
                _currScript = info;
                DialogResult = true;
            }
            else if (btn.CommandParameter is ScriptGroup group)
            {
                if (CheckScriptGroup([.. group.Scripts]))
                {
                    _currScript = group;
                    DialogResult = true;
                }
                else
                {
                    MyGM.ShwInfo("执行组脚本时，如果其中第一个是计时脚本，则执行参数以第一个脚本为准。如果第一个是图像脚本，则其中所有的脚本都需要是图像脚本，且有图像。");
                }
            }
        }

        /// <summary>
        /// 检查脚本组，如果其中第一个是计时脚本，则执行参数以第一个脚本为准。
        /// 如果第一个是图像脚本，则其中所有的脚本都需要是图像脚本，且有图像。
        /// </summary>
        /// <param name="infos">组脚本中的脚本列表。</param>
        /// <returns>true--可以执行脚本；false不能执行脚本。</returns>
        private bool CheckScriptGroup(List<ScriptInfo> infos)
        {
            if (infos[0].IsTime)
            {
                return true;
            }

            foreach (ScriptInfo i in infos)
            {
                if (i.IsTime || i.ImgName == "")
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 进行指定脚本的编辑。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void EditScript(Button btn)
        {
            if (btn.CommandParameter is ScriptItem item)
            {
                lst.SelectedItem = item;//具体操作让项改变事件触发
            }
        }

        /// <summary>
        /// 进行指定脚本的编辑。
        /// </summary>
        /// <param name="obj">脚本信息基类。</param>
        private void EditScript(object obj)
        {
            SetCurrentColor(obj);
            if (obj is ScriptInfo info)
            {
                _currScript = info;
                cmbMultipleScript.Visibility = Visibility.Hidden;
                cmbMultipleScript.ItemsSource = null;

                ValToUI(info);
                LoadPointInfos(info.PointInfos);
            }
            else if (obj is ScriptGroup group)
            {
                _currScript = group;
                cmbMultipleScript.Visibility = Visibility.Visible;
                cmbMultipleScript.ItemsSource = group.Scripts;
                cmbMultipleScript.DisplayMemberPath = "Name";

                if (group.Scripts.Any())
                {
                    cmbMultipleScript.SelectedIndex = 0;//通过索引改变事件调用加载
                }
            }
        }

        /// <summary>
        /// 删除指定的脚本。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void DeleteScript(Button btn)
        {
            if (btn.CommandParameter is ScriptItem item)
            {
                MessageBoxResult result = MessageBox.Show($"是否删除“{item.Name}”脚本？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }

                _currScript = null;
                _pointDt.Clear();
                _vm.ScriptItems.Remove(item);
                cmbMultipleScript.ItemsSource = null;

                if (item is ScriptInfo info)
                {
                    DelImg(info);
                }
                else if (item is ScriptGroup group)
                {
                    for (int i = 0; i < group.Scripts.Count; i++)
                    {
                        DelImg(group.Scripts[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 保存当前编辑的脚本。
        /// </summary>
        private void SaveScript()
        {
            if (_currScript == null)
            {
                MyGM.POP("未指定脚本，不能保存。");
                return;
            }
            
            if (_currScript is ScriptInfo info)
            {
                SaveScriptInfo(info);
            }
            else if (_currScript is ScriptGroup group)
            {
                SaveScriptGroup(group);
            }

            MyGM.POP("脚本已保存。");
        }

        /// <summary>
        /// 保存单脚本信息。
        /// </summary>
        /// <param name="si">单脚本的引用。</param>
        private void SaveScriptInfo(ScriptInfo si)
        {
            btnFouce.Focus();//移走焦点，触发DataGrid的保存
            int.TryParse(txtCycleNum.Text, out int num);
            int.TryParse(txtCycleInterval.Text, out int interval);
            double.TryParse(txtSimilarity.Text, out double similarity);

            si.IsLimited = (bool)rdoLimited.IsChecked;
            si.CycleNumber = num;
            si.IsTime = (bool)rdoTime.IsChecked;
            si.CycleInterval = interval;
            si.Similarity = similarity;
            si.IsAdaptive = (bool)chkAdaptive.IsChecked;
            si.PointInfos = SavePointInfos();

            //如果有新截取的图片，则删除旧图片，保存新图片
            if (_cropRect.Width > 0 && _cropRect.Height > 0)
            {
                string newName = $"Pic{DateTime.Now:yyMMddhhmm_ssfff}.png";

                ReplaceImg(si.ImgName, newName);
                MyGM.SavaImg(newName, (BitmapSource)img.Source);
                si.ImgName = newName;
                si.CropRect = _cropRect;
            }
        }

        /// <summary>
        /// 保存组脚本信息，除了当前项保存修改外，每组当中的循环次数参数保持一致。
        /// </summary>
        /// <param name="group">组脚本实例。</param>
        private void SaveScriptGroup(ScriptGroup group)
        {
            foreach (ScriptInfo si in group.Scripts)
            {
                if (si == cmbMultipleScript.SelectedItem)
                {
                    SaveScriptInfo(si);
                }
                else
                {
                    int.TryParse(txtCycleNum.Text, out int num);

                    si.CycleNumber = num;
                    si.IsLimited = (bool)rdoLimited.IsChecked;
                }
            }
        }

        /// <summary>
        /// 设置当前编辑的脚本UI的背景颜色。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void SetCurrentColor(object obj)
        {
            if (lst.ItemContainerGenerator.ContainerFromItem(obj) is ListBoxItem item)
            {
                _g?.Background = null;

                _g = item.FindVisualChild<Grid>();
                _g.Background = new SolidColorBrush(Color.FromArgb(128, 118, 238, 198));
            }
        }

        /// <summary>
        /// 加载点信息列表。
        /// </summary>
        /// <param name="points">坐标点信息列表。</param>
        private void LoadPointInfos(List<PointInfo> points)
        {
            _pointDt.Clear();
            foreach (PointInfo point in points)
            {
                _pointDt.Rows.Add(point.LastTime, point.Pt.X, point.Pt.Y);
            }
        }

        /// <summary>
        /// 将DataTable的值复制给实体类。
        /// </summary>
        /// <param name="dt">代表坐标点集合的DataTable。</param>
        /// <param name="recInfo">界面上的关于录制操作的信息。</param>
        private List<PointInfo> SavePointInfos()
        {
            List<PointInfo> tempInfo = [];
            foreach (DataRow dr in _pointDt.Rows)
            {
                double r1 = double.Parse(dr["XAxis"].ToString());
                double r2 = double.Parse(dr["YAxis"].ToString());
                double r3 = double.Parse(dr["LastTime"].ToString());

                tempInfo.Add(new PointInfo(r1, r2, r3));
            }

            return tempInfo;
        }

        /// <summary>
        /// 获得当前的脚本项，如果当前脚本可以转化为ScriptInfo则直接返回，否则从cmb中获得。
        /// </summary>
        /// <returns>当前选中的脚本。</returns>
        private ScriptInfo GetCurrScript()
        {
            if (_currScript is not ScriptInfo info)
            {
                info = cmbMultipleScript.SelectedItem as ScriptInfo;
            }
            return info;
        }

        /// <summary>
        /// 如果有新的图片，删除旧图片。
        /// </summary>
        /// <param name="oldName">旧的图片名。</param>
        /// <param name="newName">新的图片名。</param>
        private void ReplaceImg(string oldName, string newName)
        {
            if (oldName != newName && oldName != "")
            {
                File.Delete(@$"{MV.ScriptPath}\{oldName}");
            }
        }

        /// <summary>
        /// 删除脚本指向的图片。
        /// </summary>
        /// <param name="info">脚本信息类。</param>
        private void DelImg(ScriptInfo info)
        {
            if (info.ImgName != "")
            {
                File.Delete(@$"{MV.ScriptPath}\{info.ImgName}");
            }
        }


    }
}
