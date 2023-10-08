using System;
using System.IO;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections.Generic;


namespace arduino
{
    public partial class MainWindow : Window
    {
        private NamedPipeClientStream arduinoPipe; 
        private StreamWriter arduinoStreamWriter; 
        private string selectedPresetPath; 
        
        public MainWindow()
        {
            InitializeComponent();
            
            comboBoxPorts.ItemsSource = new string[] { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6" };
            
            List<string> presetPaths = new List<string>
            {
                //todo: написать пути для 5 пресетов
            };
            
            comboBoxPresets.ItemsSource = presetPaths;
        }

        
        private void ComboBoxPresets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arduino Sketch Files (*.ino)|*.ino|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedPresetPath = openFileDialog.FileName; // Сохранение пути к выбранному пресету
            }
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (arduinoPipe == null)
            {
                // Проверка подключения к Arduino
                statusText.Text = "Ошибка: Arduino не подключена";
                statusText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (!string.IsNullOrEmpty(selectedPresetPath) && File.Exists(selectedPresetPath))
            {
                try
                {
                    // Чтение содержимого пресета и отправка его в Arduino через канал
                    string sketchContent = File.ReadAllText(selectedPresetPath);
                    arduinoStreamWriter.WriteLine(sketchContent);
                    arduinoStreamWriter.Flush();
                    statusText.Text = "Пресет успешно загружен";
                    statusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                catch (Exception ex)
                {
                    statusText.Text = $"Ошибка: {ex.Message}";
                    statusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                statusText.Text = "Ошибка: Пресет не выбран или файл не найден";
                statusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void LoadCustomPresetButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (arduinoPipe == null)
            {
                // Проверка подключения к Arduino
                statusText.Text = "Ошибка: Arduino не подключена";
                statusText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arduino Sketch Files (*.ino)|*.ino|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true )
            {
                selectedPresetPath = openFileDialog.FileName;
                LoadPresetButton_Click(sender, e); // Автоматически загружает выбранный пресет
            }
            
            
        }

        private void ComboBoxPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик выбора COM-порта
            if (arduinoPipe == null)
            {
                statusText.Text = "Ошибка: Arduino не подключена";
                statusText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (comboBoxPorts.SelectedItem != null)
            {
                arduinoStreamWriter.Close();
                arduinoPipe.Close(); // Закрытие канала, если он уже был открыт
                string selectedPort = comboBoxPorts.SelectedItem.ToString();
                arduinoPipe = new NamedPipeClientStream("ArduinoPipe"); 
                arduinoPipe.Connect();
                arduinoStreamWriter = new StreamWriter(arduinoPipe); // Создание потока для записи данных в канал
                arduinoStatus.Text = $"Arduino: {selectedPort}";
            }
        }

        

        private void Window_Closed(object sender, EventArgs e)
        {
            // Обработчик закрытия окна
            if (arduinoPipe != null)
            {
                arduinoStreamWriter.Close();
                arduinoPipe.Close(); // Закрытие канала при выходе из приложения
            }
        }
    }
}
