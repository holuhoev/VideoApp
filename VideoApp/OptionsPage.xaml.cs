using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556

namespace VideoApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class OptionsPage : Page
    {
        
        string videoName = "";
        int duration = 2;
        bool isDateNeeded=false;
        List<StorageFile> m_files = new List<StorageFile>();
        public OptionsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter!=null && e.Parameter is List<StorageFile>)
            {
                m_files = e.Parameter as List<StorageFile>;
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            isDateNeeded = (bool) dateCheckBox.IsChecked;
            Frame.Navigate(typeof(StepsPage),m_files);
        }

        private void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            videoName = nameBox.Text;
        }

        private void durationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock value = (TextBlock)durationList.SelectedItem;
            duration = int.Parse(value.Text);
        }
    }
}
