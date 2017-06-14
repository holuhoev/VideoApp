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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using VideoApp.Classes;
using Windows.Storage.Pickers;
// Документацию по шаблону элемента пустой страницы см. по адресу http://go.microsoft.com/fwlink/?LinkID=390556

namespace VideoApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EditPage : Page
    {
        MyImageBytes[] my_images;
        private List<StorageFile> m_files = new List<StorageFile>();
        private ObservableCollection<Image> m_images = new ObservableCollection<Image>();
       // CoreApplicationView view;
        public EditPage()
        {
            this.InitializeComponent();
           // view  = CoreApplication.GetCurrentView();
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AddContentAndFolder();
        }
    
        //конвертирует файлы в картинки и добавляет их в гридвью
        private async void AddImages()
        {
            if (m_files.Count > 0)
            {
                await continueAdding();
                ImageGV.DataContext = m_images;
            }
        }
        public async void AddContentAndFolder()
        {

            if (!await AddContentIsSuccessful())
            {
               //TODO : Написаать что будет если фотки не добавились
            }
        }

        public async Task<bool> AddContentIsSuccessful()
        {
            int count = await WriteReadVideoModel.FileCountInLocalFolder("ByteFolder");
            if (count > 0)
            {
                await AddButtonsToList(count);
                return true;
            }
            else
                return false;

        }
        private async Task AddButtonsToList(int count)
        {

            Button[] btns = new Button[count];
            ImageGV.Items.Clear();
            my_images = new MyImageBytes[count];
            my_images = await WriteReadVideoModel.ReadImageBytes();
           
            for (int i = 0; i < count; i++)
            {
                btns[i] = new Button();
                btns[i].Name = "Btn " + i;
               // StackPanel stack = new StackPanel();
                //stack.Children.Add()
                btns[i].BorderBrush = new SolidColorBrush(Windows.UI.Colors.White);
               // btns[i].Background = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 0, 0, 0));
                btns[i].BorderThickness = new Thickness(2);
                //добавляем грид в btn
                Grid newGrid = new Grid();
                btns[i].Content = newGrid;
                newGrid.Margin = new Thickness(0, 0, 0, 0);

              


                //Добавим элемент Image в первый столбец элемента newGrid.
                Image newImage = new Image() { Width = 100, Height = 100 };
                newGrid.Children.Add(newImage);
                BitmapImage image = await ImageConvertor.ByteArrayToImage(my_images[i].Bytes);
                newImage.Source = image;
                newImage.Stretch = Stretch.Fill;
                newImage.Margin = new Thickness(0);


                //btns[i].Click += Btn_Click;
                ImageGV.Items.Add(btns[i]);
            }
        }
        private async Task continueAdding()
        {
            foreach (StorageFile file in m_files)
            {
                //ImageGV.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    Image image = new Image();
                    image.Source = bitmapImage;
                    m_images.Add(image);
                    //Grid.SetRow(image, temp++);
                    //ImageGV.Children.Add(image);
                }

            }
        }
        

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StepsPage), m_files);
        }

        
        //private async void ContinueFileOpen(CoreApplicationView sender, IActivatedEventArgs args1)
        //{
        //    FileOpenPickerContinuationEventArgs args = args1 as FileOpenPickerContinuationEventArgs;

        //    if (args != null)
        //    {
        //        IReadOnlyList<StorageFile> files = args.Files;
        //        if (files.Count > 0)
        //        {
        //            m_files = files.ToList();
        //            AddImages();
        //        }
        //        else
        //        {
                    
        //        }
        //        view.Activated -= ContinueFileOpen;
        //    }

        //}
    }
}
