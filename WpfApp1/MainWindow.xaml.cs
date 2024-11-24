using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace TextAnalyzerApp
{
    public partial class MainWindow : Window
    {
        private Task _analysisTask;
        private bool _isRunning;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                MessageBox.Show("Анализ уже выполняется. Пожалуйста, подождите.");
                return;
            }
            _isRunning = true;
            btnAnalyze.IsEnabled = false;
            string inputText = txtInput.Text;
            _analysisTask = Task.Run(() => AnalyzeText(inputText));
            _analysisTask.ContinueWith(t =>
            {
                _isRunning = false;
                Dispatcher.Invoke(() => btnAnalyze.IsEnabled = true);
            });
        }
        private void AnalyzeText(string text)
        {
            int sentenceCount = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int characterCount = text.Length;
            int wordCount = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int questionCount = text.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            int exclamationCount = text.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            string report = $"Количество предложений: {sentenceCount}\n" +
                            $"Количество символов: {characterCount}\n" +
                            $"Количество слов: {wordCount}\n" +
                            $"Количество вопросительных предложений: {questionCount}\n" +
                            $"Количество восклицательных предложений: {exclamationCount}";

            Dispatcher.Invoke(() => MessageBox.Show(report, "Отчет"));
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                _isRunning = false;
                _analysisTask?.Wait();
                MessageBox.Show("Анализ остановлен.");
            }
        }
        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            string inputText = txtInput.Text;
            string report = AnalyzeTextForFile(inputText); 
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Сохранить отчет"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, report);
                MessageBox.Show("Отчет сохранен.");
            }
        }
        private string AnalyzeTextForFile(string text)
        {
            int sentenceCount = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int characterCount = text.Length;
            int wordCount = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int questionCount = text.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            int exclamationCount = text.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
            return $"Количество предложений: {sentenceCount}\n" +
                   $"Количество символов: {characterCount}\n" +
                   $"Количество слов: {wordCount}\n" +
                   $"Количество вопросительных предложений: {questionCount}\n" +
                   $"Количество восклицательных предложений: {exclamationCount}";
        }
        private void btnFindDuplicates_Click(object sender, RoutedEventArgs e)
        {
            string sourceDir = txtSourceDirectory.Text;
            string targetDir = txtTargetDirectory.Text;
            if (!Directory.Exists(sourceDir))
            {
                MessageBox.Show("Исходная директория не существует.");
                return;
            }
            var files = Directory.GetFiles(sourceDir);
            var duplicates = files.GroupBy(Path.GetFileName).Where(g => g.Count() > 1).SelectMany(g => g.Skip(1));
            foreach (var duplicate in duplicates)
            {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(duplicate));
                File.Move(duplicate, targetFile);
            }
            MessageBox.Show("Дубликаты перемещены.");
            string report = $"Перемещены дубликаты из {sourceDir} в {targetDir}.";
            File.WriteAllText(Path.Combine(targetDir, "report.txt"), report);
        }
    }
}
