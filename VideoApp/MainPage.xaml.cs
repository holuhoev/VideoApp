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
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Storage;
using VideoApp.Classes;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=391641

namespace VideoApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    
    public sealed partial class MainPage : Page
    {
        VideoModel[] m_videomodels;     
        public MainPage()
        {
            try
            {
                this.InitializeComponent();
                this.NavigationCacheMode = NavigationCacheMode.Required;
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;   
            }
            catch(Exception ex)
            {

            }  
        }
        
        private async void AddContentAndFolder()
        {

            if (!await AddContentIsSuccessful())
            {
                VideoElement.Visibility = Visibility.Collapsed;
                
                VideoGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0,0,0,0));
                statusBlock.Visibility = Visibility.Visible;
                statusGrid.Visibility = Visibility.Visible;
            }
            else
            {
                VideoGrid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 255, 255));
                VideoElement.Visibility = Visibility.Visible;
                statusBlock.Visibility = Visibility.Collapsed;
                statusGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<bool> AddContentIsSuccessful()
        {
            int count = await WriteReadVideoModel.FileCountInLocalFolder("DataFolder");
            if (count > 0)
            {
                await AddButtonsToList(count);
                GetVideoFileInMyLibrary(m_videomodels[0].Name);
                return true;
            }
            else
                return false;
        }
        
        private async Task AddButtonsToList(int count)
        {
            Button[] btns = new Button[count];
            VideoSP.Items.Clear();
            m_videomodels = new VideoModel[count];
            m_videomodels = await WriteReadVideoModel.ReadFilesInLocalFolder();
            Array.Sort(m_videomodels);
            for (int i = 0; i < count; i++)
            {
                btns[i] = new Button();
                btns[i].Name = "Btn " + i;
                btns[i].Width = VideoSP.Width - 10;
                btns[i].Margin = new Thickness(5, 0, 5, 0);
                
                btns[i].BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
                btns[i].Background = new SolidColorBrush(Windows.UI.Color.FromArgb(60, 255, 255, 255));
                btns[i].BorderThickness = new Thickness(2);
                //добавляем грид в btn
                Grid newGrid = new Grid();
                btns[i].Content = newGrid;
                newGrid.Margin = new Thickness(0, 0, 0, 0);
                newGrid.Width = btns[i].Width;
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });

                // добавляем внутренний грид в newGrid
                Grid internalGrid = new Grid();
                internalGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                internalGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                newGrid.Children.Add(internalGrid);
                Grid.SetColumn(internalGrid, 1);


                //Добавим элемент Image в первый столбец элемента newGrid.
                Image newImage = new Image() { Width = 50, Height= 50  };
                newGrid.Children.Add(newImage);
                BitmapImage image = await ImageByteConvertor.ByteArrayToImage(m_videomodels[i].HeadPiece);
                newImage.Source = image;
                newImage.Stretch = Stretch.Fill;
                newImage.Margin=new Thickness(0);
                Grid.SetColumn(newImage, 0);

                //во внутренний грид помещаем текстблоки 
                TextBlock nameBlock = new TextBlock() { Text = m_videomodels[i].Name };
                nameBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
                TextBlock dataBlock = new TextBlock() { Text = m_videomodels[i].DateCreated };
                dataBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
                internalGrid.Children.Add(nameBlock);
                internalGrid.Children.Add(dataBlock);
                Grid.SetRow(nameBlock, 0);
                Grid.SetRow(dataBlock, 1);

                btns[i].Click += Btn_Click;
                VideoSP.Items.Add(btns[i]);
            }
        }
        
        void Btn_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                if (m_videomodels != null)
                {
                    string btn_name = (sender as Button).Name;
                    int index;
                    string[] arr = btn_name.Split(' ');
                    int.TryParse(arr[1], out index);
                    string name = m_videomodels[index].Name;
                    GetVideoFileInMyLibrary(name);
                }
            }
            catch(Exception ex)
            {

            }
        }
        private async void OpenVideoFileAndPlay(StorageFile videoFile)
        {

            var videoStream = await videoFile.OpenAsync(FileAccessMode.Read);
            VideoElement.SetSource(videoStream, videoFile.ContentType);
            nameBlock.Text = videoFile.DisplayName;

        }
       
        private async void GetVideoFileInMyLibrary(string name)
        {
            try
            {
                StorageFolder myVideoLibrary = await Windows.Storage.KnownFolders.PicturesLibrary.GetFolderAsync("ImageConvertor");
                StorageFile videoFile = await myVideoLibrary.GetFileAsync(name + ".mp4");
                OpenVideoFileAndPlay(videoFile);
            }catch(Exception ex)
            {

            }
        }
        public void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            Application.Current.Exit();
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //добавляем контент на страницу
            AddContentAndFolder();        
                 
        }
        
        private void AddBar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StepsPage));
        }
    }
}
